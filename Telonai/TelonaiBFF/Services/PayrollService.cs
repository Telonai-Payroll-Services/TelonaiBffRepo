namespace TelonaiWebApi.Services;

using Amazon.SimpleEmail.Model;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.Design;
using System.Diagnostics.Eventing.Reader;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;

public interface IPayrollService
{
    List<PayrollModel> GetLatestByCount(int companyId, int count);
    List<PayrollModel> GetReport(int companyId, DateOnly from, DateOnly to);
    PayrollModel GetCurrentPayroll( int companyId);
    PayrollModel GetPreviousPayroll(int companyId);
    DateOnly GetFirstPayrollRunDate(PayrollScheduleTypeModel frequency, DateOnly scheduleStartDate, int countryId);
    List<DateOnly> GetFirstPaycheckDate(PayrollScheduleTypeModel frequency, DateOnly scheduleStartDate, int countryId);

    PayrollModel GetById(int id);
    Task<int> CreateNextPayrollForAll(int countryId);
    Task CreateNextPaystubForAllCurrentPayrollsAsync();
    void Create(int companyId);
    void Update(int id, int companyId);
    void Delete(int id);

    public Task<PayrollSummary> GetPayrollSummanryByPayrollId(int payrollId);

}

public class PayrollService : IPayrollService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly IPayStubService _payStubService;
    private readonly IMailSender _mailSender;
    private readonly ILogger<PayrollService> _logger;
    private readonly IDayOffRequestService<DayOffRequestModel, DayOffRequest> _dayOffRequestService;

    public PayrollService(DataContext context, IMapper mapper, IMailSender mailSender, ILogger<PayrollService> logger, IDayOffRequestService<DayOffRequestModel, DayOffRequest> dayOffRequestService)
    {
        _context = context;
        _mapper = mapper;
        _mailSender = mailSender;
        _logger = logger;
        _dayOffRequestService = dayOffRequestService;
    }

    public List<PayrollModel> GetLatestByCount(int companyId, int count)
    {
        var payrolls = _context.Payroll.OrderByDescending(e => e.ScheduledRunDate)
            .Where(e => e.CompanyId == companyId && e.StartDate < DateOnly.FromDateTime(DateTime.UtcNow))
            .Take(count).ToList();

        var result = _mapper.Map<List<PayrollModel>>(payrolls);
        var averageAmount = result.Where(e => e.ScheduledRunDate < DateTime.Now).Sum(e => e.GrossPay?? 0) / (count - 1);
        
        double buffer1 = averageAmount;
        double buffer2 = buffer1 * 1.2;

        result.ForEach(e => {
         
            var diff = e.GrossPay?? 0 - averageAmount;
            if (e.ScheduledRunDate > DateTime.Now && e.GrossPay.HasValue)
                diff = GetForecastedPayrollExpense(e.GrossPay.Value, e.StartDate, e.ScheduledRunDate)-averageAmount;

            e.GrossPay = e.GrossPay ?? 0;
            e.GrossPay = Math.Round(e.GrossPay.Value, 2);

            if (diff < buffer1)
            {
                e.ExpenseTrackingHexColor = "#3CA612";
                return;
            }
            if (diff < buffer2)
            {
                e.ExpenseTrackingHexColor = "#FCCC44";
                return;
            }
            e.ExpenseTrackingHexColor = "#D20103";
        });
        return result;
    }

    private static double GetForecastedPayrollExpense(double grossPayToDate, DateTime payrollStartDate, DateTime payrollEndDate)
    {
        var span = DateTime.UtcNow - payrollStartDate;
        var projectedSpan = payrollEndDate - payrollStartDate;
        var dailyAmount = grossPayToDate / span.TotalDays;
        var forecasted = dailyAmount * projectedSpan.TotalDays;
        return forecasted;
    }

    public List<PayrollModel> GetReport(int companyId, DateOnly from, DateOnly to)
    {
        var obj = _context.Payroll.Where(e => e.CompanyId == companyId && e.ScheduledRunDate >= from && e.ScheduledRunDate < to.AddDays(1)).ToList();
        var result = _mapper.Map<List<PayrollModel>>(obj);
        return result;
    }

    public PayrollModel GetCurrentPayroll(int companyId)
    {
        Payroll obj;
        var payrolls = _context.Payroll.Include(e => e.PayrollSchedule).OrderByDescending(e => e.ScheduledRunDate).Where(e =>
        e.StartDate <= DateOnly.FromDateTime(DateTime.Now) && e.TrueRunDate == null && e.CompanyId == companyId).ToList();

        if (payrolls == null || payrolls.Count == 0)
            return null;

        if (payrolls.Count == 2)
            obj = payrolls.Last();
        else
            obj = payrolls.First();

        return _mapper.Map<PayrollModel>(obj);
    }

    public PayrollModel GetPreviousPayroll(int companyId)
    {
        var obj = _context.Payroll.OrderByDescending(e=>e.ScheduledRunDate).Skip(1).FirstOrDefault(e=> e.CompanyId == companyId);
        if (obj == null)
            return null;

        return _mapper.Map<PayrollModel>(obj);
    }

    public PayrollModel GetById(int id)
    {
        var obj = GetPayroll(id);
        if (obj == null) return null;
        var result = _mapper.Map<PayrollModel>(obj);
        return result;
    }

    /// <summary>
    /// This needs to be run daily by aws lambda
    /// </summary>
    /// <returns></returns>
    /// <exception cref="AppException"></exception>
    public async Task<int> CreateNextPayrollForAll(int countryId)
    {
        var newPayrollsList = new List<Payroll>();

        var today = DateOnly.FromDateTime(DateTime.Now);

        //Get the latest payroll schedule for all companies.
        //This gets the schedules that have already started or will start in the next 3 days

        var threeDaysFromNow = today.AddDays(3);

        var paySchedules = _context.PayrollSchedule.OrderByDescending(e => e.FirstRunDate)
            .Where(e => e.StartDate <= threeDaysFromNow
            && (e.EndDate == null || e.EndDate > threeDaysFromNow))
            .GroupBy(e => e.CompanyId)
            .Select(g => g.First()).ToList();

        var currentPayrolls = _context.Payroll.OrderByDescending(e => e.ScheduledRunDate)
            .Include(e => e.PayrollSchedule)
            .Where(e => e.ScheduledRunDate >= today)
            .GroupBy(e => e.CompanyId)
            .Where(g => g.Count() == 1) //This line will filter out those already created in the previous day
            .Select(g => g.First()).ToList();

        foreach (var payroll in currentPayrolls)
        {
            var newSchedule = paySchedules.Where(e => e.CompanyId == payroll.CompanyId)
                .OrderByDescending(e => e.Id).FirstOrDefault();
            var existingSchedule = payroll.PayrollSchedule;
            var scheduleChanged = newSchedule.Id != existingSchedule.Id;

            var nextPayrollRunDate = payroll.PayrollSchedule.StartDate;
            var nextPayrollStartDate = payroll.PayrollSchedule.StartDate;

            if (scheduleChanged)
            {
                nextPayrollRunDate = newSchedule.FirstRunDate;
                nextPayrollStartDate = newSchedule.StartDate;
            }
            else
            {
                var freq = (PayrollScheduleTypeModel)payroll.PayrollSchedule.PayrollScheduleTypeId;

                switch (freq)
                {
                    case PayrollScheduleTypeModel.Monthly:
                        nextPayrollRunDate = payroll.ScheduledRunDate.AddMonths(1);
                        nextPayrollStartDate = new DateOnly(nextPayrollRunDate.Year, nextPayrollRunDate.Month, 1);
                        nextPayrollRunDate = nextPayrollStartDate.AddMonths(1).AddDays(-1);

                        //move date backward by one day as ACH takes 1 day to process
                        nextPayrollRunDate = nextPayrollRunDate.AddDays(-1);
                        nextPayrollRunDate = AvoidHolidaysAndWeekends(nextPayrollRunDate, countryId);
                        break;
                    case PayrollScheduleTypeModel.SemiMonthly:
                        nextPayrollRunDate = payroll.ScheduledRunDate.AddDays(13);
                        if (nextPayrollRunDate.Day < 16)
                        {
                            nextPayrollRunDate = new DateOnly(nextPayrollRunDate.Year, nextPayrollRunDate.Month, 15);
                            nextPayrollStartDate = new DateOnly(nextPayrollRunDate.Year, nextPayrollRunDate.Month, 1);
                        }
                        else
                        {
                            nextPayrollStartDate = new DateOnly(nextPayrollRunDate.Year, nextPayrollRunDate.Month, 16);
                            var firstDayOfMonth= new DateOnly(nextPayrollRunDate.Year, nextPayrollRunDate.Month, 1);
                            nextPayrollRunDate = firstDayOfMonth.AddMonths(1).AddDays(-1);
                        }
                        nextPayrollRunDate = nextPayrollRunDate.AddDays(-1);
                        nextPayrollRunDate = AvoidHolidaysAndWeekends(nextPayrollRunDate, countryId);
                        break;
                    case PayrollScheduleTypeModel.Biweekly: // Run date should be every other Wednesday. It is not affected by holidays
                        nextPayrollRunDate = payroll.ScheduledRunDate.AddDays(14);
                        nextPayrollStartDate = payroll.ScheduledRunDate.AddDays(1);
                        break;
                    case PayrollScheduleTypeModel.Weekly:// Run date should be every Wednesday. Not affected by holidays
                        nextPayrollRunDate = payroll.ScheduledRunDate.AddDays(7);
                        nextPayrollStartDate = payroll.ScheduledRunDate.AddDays(1);
                        break;
                    case 0:
                        throw new AppException("Payroll run frequency not defined");
                }
            }
            var nextPayroll = new Payroll
            {
                PayrollScheduleId = newSchedule.Id,
                ScheduledRunDate = nextPayrollRunDate,
                StartDate = nextPayrollStartDate,
                CompanyId = payroll.CompanyId
            };

            newPayrollsList.Add(nextPayroll);
        }

        if (newPayrollsList.Count > 0)
        {
            _context.Payroll.AddRange(newPayrollsList);
            _context.SaveChanges();
        }

        return newPayrollsList.Count;
    }
    private async Task Send()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);

        //send email to the owner
        var payrolls = _context.Payroll.Include(e => e.PayrollSchedule)
            .Where(e => e.ScheduledRunDate >= today && e.ScheduledRunDate<today.AddDays(3))
            .GroupBy(e => e.CompanyId)
            .Select(g => g.First()).ToList();
        foreach (var payroll in payrolls)
        {
            var admins = (from person in _context.Person
                          join employment in _context.Employment
                          on person.Id equals employment.PersonId
                          where person.CompanyId == payroll.CompanyId && employment.IsPayrollAdmin
                          select new
                          {
                              person.Email,
                              person.FirstName,
                              person.LastName,
                              payroll.ScheduledRunDate
                          }).ToList();

            foreach (var admin in admins)
            {
                if (payroll.ScheduledRunDate == today)
                {
                    _ = SendReminderForCurrentPayroll(admin.Email, admin.FirstName, admin.LastName, admin.ScheduledRunDate.ToString("MMM/dd/yyyy"));
                }
                else
                {
                    _ = SendReminderForLatePayroll(admin.Email,admin.FirstName,admin.LastName,admin.ScheduledRunDate.ToString("MMM/dd/yyyy"));
                }
            }
        }
       
    }

    private async Task SendReminderForCurrentPayroll(string email, string firstName, string lastName, string scheduledRunDate)
    {
        _ = _mailSender.SendUsingAwsClientAsync(
                      email, $"Time to run payroll",
                      CreateHtmlEmailBody(scheduledRunDate, $"{firstName} {lastName}"),
                      CreateTextEmailBody(scheduledRunDate, $"{firstName} {lastName}")
                  ).ContinueWith(task =>
                  {
                      if (task.IsFaulted)
                      {
                          _logger.LogError($"Failed to send email to {email}: {task.Exception}");
                      }
                  });
    }
        private async Task SendReminderForLatePayroll(string email, string firstName, string lastName, string scheduledRunDate)
    {
        _ = _mailSender.SendUsingAwsClientAsync(
            email,
            $"Time to run payroll",
            CreateHtmlEmailBodyForLatePayroll(scheduledRunDate, $"{firstName} {lastName}"),
            CreateTextEmailBodyForLatePayroll(scheduledRunDate, $"{firstName} {lastName}")
        ).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                _logger.LogError($"Failed to send email to {email}: {task.Exception}");
            }
        });
    }

       
    private static string CreateTextEmailBody(string scheduledDate, string receiverName)
    {
        return $"Time to Run Payroll"
            + $"Dear {receiverName},\r\n"
            + $"Today, {scheduledDate}, is your payroll run date. To ensure your employees get paid on time, please run payroll, "
            + $"after close of business, today. If you know the hours your employees will work today, you may enter" 
            + "those hours and run payroll right now. \r\n"
            + $"Please do not reply to this email as it is not monitored.";
    }

    private static string CreateHtmlEmailBody(string scheduledDate, string receiverName)
    {
        return $"<h2>Time to Run Payroll</h2>"
         + $"Dear {receiverName}, </br>"
         + $"<p>Today, <strong>{scheduledDate}<strong>, is your payroll run date. To ensure your employees get paid on time, "
         + $"please run payroll today, after close of business. If you know the hours your employees will work today, you may "
         + "enter those hours and run payroll right now."
         + $"<br/>Please do not reply to this email as it is not monitored.";
    }
    private static string CreateTextEmailBodyForLatePayroll(string scheduledDate, string receiverName)
    {
        return $"Time to Run Payroll"
            + $"Dear {receiverName},\r\n"
            + $"Your payroll run date has passed. It was on {scheduledDate}. Please run payroll, using our mobile app, as soon as possible."
            + "Please note that your employees will not get paid unless you complete running payroll. \r\n\n"
            + $"Please do not reply to this email as it is not monitored. If you need assistance, please call our support team at 601-608-7025";
    }

    private static string CreateHtmlEmailBodyForLatePayroll(string scheduledDate, string receiverName)
    {
        return $"<h2>Time to Run Payroll</h2>"
         + $"Dear {receiverName}, </br>"
         + $"Your payroll run date has passed. It was on <strong>{scheduledDate}</strong>. Please run payroll, using our mobile app, "
         + "as soon as possible."
         + "Please note that your employees will not get paid unless you complete running payroll."
         + $"<br/><br/>Please do not reply to this email as it is not monitored. If you need assistance, please call our support team at <strong>601-608-7025</strong>";
    }

    /// <summary>
    /// Creates  the first payroll that will be run by a manager after a week,month, etc,
    /// as determined by the PayrollScheduleType.
    /// </summary>
    /// <param name="companyId"></param>
    /// <exception cref="AppException"></exception>
    public void Create(int companyId)
    {
        var paySchedule = _context.PayrollSchedule.FirstOrDefault(e => e.CompanyId == companyId && e.EndDate == null) ?? 
            throw new AppException("Payroll Schedule has not been setup yet.");

        var lastScheduledRun = _context.Payroll.OrderByDescending(e => e.ScheduledRunDate).FirstOrDefault();
        Payroll currentPayroll = null;
        if (lastScheduledRun == null) //This is the first payroll
        {
            currentPayroll = new Payroll
            {
                PayrollScheduleId= paySchedule.Id,
                StartDate = paySchedule.StartDate,
                ScheduledRunDate= paySchedule.FirstRunDate,
                CompanyId = companyId
            };
            _context.Payroll.Add(currentPayroll);
            _context.SaveChanges();
            return;
        }

    }

    /// <summary>
    /// Called when an admin  runs payroll.
    /// </summary>
    /// <param name="id"></param>
    /// <exception cref="KeyNotFoundException"></exception>
    public void Update(int id, int companyId)
    {
        var dto = GetPayroll(id) ?? throw new KeyNotFoundException("Payroll not found");
        if (companyId != dto.CompanyId)
            throw new UnauthorizedAccessException();

        if (dto.TrueRunDate != null)
            throw new AppException("Payroll has already been run");

        var stubs = CreateOrUpdatePayStubsForCurrentPayrollAsync(dto,true).Result;

        dto.TrueRunDate = DateTime.UtcNow;

        _context.Payroll.Update(dto);
        if (stubs.Item1.Count > 0)
            _context.PayStub.UpdateRange(stubs.Item1);

        if (stubs.Item2.Count > 0)
            _context.PayStub.AddRange(stubs.Item2);
        _context.SaveChanges();
    }
    

    public void Delete(int id)
    {
        var dto = GetPayroll(id);
        _context.Payroll.Remove(dto);
        _context.SaveChanges();
    }

    public DateOnly GetFirstPayrollRunDate(PayrollScheduleTypeModel frequency, DateOnly scheduleStartDate, int countryId)
    {
        var daysInMonth = DateTime.DaysInMonth(scheduleStartDate.Year, scheduleStartDate.Month);

        DateOnly nextRunDate;

        switch (frequency)
        {
            case PayrollScheduleTypeModel.Monthly:
                nextRunDate = scheduleStartDate.AddDays(daysInMonth - scheduleStartDate.Day);
                
                //move date backward by one day as ACH takes 1 day to process
                nextRunDate = nextRunDate.AddDays(-1);
                return AvoidHolidaysAndWeekends(nextRunDate, countryId);                
            case PayrollScheduleTypeModel.SemiMonthly:
                if (scheduleStartDate.Day < 16)
                    nextRunDate = scheduleStartDate.AddDays(15 - scheduleStartDate.Day);
                else
                    nextRunDate = scheduleStartDate.AddDays(daysInMonth - scheduleStartDate.Day);
                
                //move date backward by one day as ACH takes 1 day to process
                nextRunDate = nextRunDate.AddDays(-1);
                return AvoidHolidaysAndWeekends(nextRunDate, countryId);
            case PayrollScheduleTypeModel.Biweekly:
                nextRunDate = GetNextWednesday(scheduleStartDate).AddDays(7);
                break;
            case PayrollScheduleTypeModel.Weekly:                
                    nextRunDate = GetNextWednesday(scheduleStartDate);
                break;
            case 0:
                throw new AppException("Payroll schedule not defined");
        }
        return nextRunDate; //Note: for weekly and bi-weekly this date falls on Wednesday, but the check date will be on Friday
    }

    /// <summary>
    /// For weekly and bi-weekly this date falls on Wednesday, but the check date will be on Friday
    /// </summary>
    /// <param name="frequency"></param>
    /// <param name="scheduleStartDate"></param>
    /// <param name="countryId"></param>
    /// <returns>List of possible first paycheck dates</returns>
    /// <exception cref="AppException"></exception>
    public List<DateOnly> GetFirstPaycheckDate(PayrollScheduleTypeModel frequency, DateOnly scheduleStartDate, int countryId)
    {
        var daysInMonth = DateTime.DaysInMonth(scheduleStartDate.Year, scheduleStartDate.Month);

        DateOnly nextPaycheckDate;

        switch (frequency)
        {
            case PayrollScheduleTypeModel.Monthly:
                nextPaycheckDate = scheduleStartDate.AddDays(daysInMonth - scheduleStartDate.Day);
                return new List<DateOnly> { AvoidHolidaysAndWeekends(nextPaycheckDate, countryId) };
            case PayrollScheduleTypeModel.SemiMonthly:
                if (scheduleStartDate.Day < 16)
                    nextPaycheckDate = scheduleStartDate.AddDays(15 - scheduleStartDate.Day);
                else
                    nextPaycheckDate = scheduleStartDate.AddDays(daysInMonth - scheduleStartDate.Day);
                return new List<DateOnly> { AvoidHolidaysAndWeekends(nextPaycheckDate, countryId) };
            case PayrollScheduleTypeModel.Biweekly:
                nextPaycheckDate = GetNextWednesday(scheduleStartDate).AddDays(2);//paycheck date is 2 days after payroll is run
                var nextPaycheckDate2 = nextPaycheckDate.AddDays(7);
                return new List<DateOnly> {
                    AvoidHolidaysAndWeekends(nextPaycheckDate, countryId),
                    AvoidHolidaysAndWeekends(nextPaycheckDate2, countryId)
                };
            case PayrollScheduleTypeModel.Weekly:
                nextPaycheckDate = GetNextWednesday(scheduleStartDate).AddDays(2); //paycheck date is normally 2 days later
                return new List<DateOnly> { AvoidHolidaysAndWeekends(nextPaycheckDate, countryId) };                
            case 0:
                throw new AppException("Payroll schedule not defined");
        }
        return null;
    }

    public async Task CreateNextPaystubForAllCurrentPayrollsAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var payrolls = _context.Payroll.Include(e => e.PayrollSchedule)
            .Where(e => e.StartDate <= today && e.ScheduledRunDate >= today
                && e.TrueRunDate == null).ToList();

        for (int i = 0; i < payrolls.Count; i++)
        {
            var payroll = payrolls[i];
            var stubs = await CreateOrUpdatePayStubsForCurrentPayrollAsync(payroll,false);
            payroll.GrossPay = stubs.Item1.Sum(e => e.GrossPay) + stubs.Item2.Sum(e => e.GrossPay);

            if (stubs.Item1.Count > 0)
                _context.PayStub.UpdateRange(stubs.Item1);

            if (stubs.Item2.Count > 0)
                _context.PayStub.AddRange(stubs.Item2);

            if (stubs.Item1.Count > 0 || stubs.Item2.Count > 0)
            {
                _context.Payroll.Update(payroll);
                _ = _context.SaveChanges();
            }
        }
    }
    private static DateOnly GetNextWednesday(DateOnly date)
    {
        var numberOfdaysFromWednesday = date.DayOfWeek - DayOfWeek.Wednesday;
        
        if (numberOfdaysFromWednesday > 0)
            return date.AddDays(7 - numberOfdaysFromWednesday);

        return date.AddDays(-numberOfdaysFromWednesday);
    }

    private DateOnly AvoidHolidaysAndWeekends(DateOnly date, int countryId)
    {
        var holidays = _context.Holiday.Where(e => e.CountryId == countryId && e.Date.Year == date.Year).ToList();

        if (holidays.Any(e => e.Date == date))
            date = date.AddDays(-1);

        //Do this again in case there are two holidays next to each other
        if (holidays.Any(e => e.Date == date))
            date = date.AddDays(-1);

        if (date.DayOfWeek == DayOfWeek.Saturday)
            date = date.AddDays(-1);

        else if (date.DayOfWeek == DayOfWeek.Sunday)
            date = date.AddDays(-2);

        //Repeat holiday checks because moving it from the weekend may have brought it to a holiday
        if (holidays.Any(e => e.Date == date))
            date = date.AddDays(-1);

        //Do this again in case there are two holidays next to each other
        if (holidays.Any(e => e.Date == date))
            date = date.AddDays(-1);

        return date;
    }
    private Payroll GetPayroll(int id)
    {
        var dto = _context.Payroll.Include(e=>e.PayrollSchedule).FirstOrDefault(e=>e.Id==id);
        return dto;
    }

    /// <summary>
    /// This method creates paystubs if they don't exist, it updates them if they exist.
    /// It must be invoked via lambda once a day so that the expense tracker displays correct data. 
    /// </summary>
    /// <param name="currentPayroll"></param>
    /// <returns>List of Updated paystubs as Item1, and list of new paystubs as item 2 of a Tuple</returns>
    /// <exception cref="AppException"></exception>
    private async Task<Tuple<List<PayStub>, List<PayStub>>> CreateOrUpdatePayStubsForCurrentPayrollAsync(Payroll currentPayroll, bool isFinal)
    {
        var companyId = currentPayroll.CompanyId;
        var currentYear = currentPayroll.ScheduledRunDate.Year;
        var timecards = _context.TimecardUsa.Where(e => e.Job.CompanyId == companyId &&
            DateOnly.FromDateTime(e.ClockIn) >= currentPayroll.StartDate &&
            DateOnly.FromDateTime(e.ClockIn) <= currentPayroll.ScheduledRunDate).ToList();

        if (isFinal && timecards.Any(e => !e.IsApproved))
            throw new AppException("You have not approved all timecards");

        var currentPayStubs = _context.PayStub.Where(e => e.PayrollId == currentPayroll.Id && e.Employment.PayRateBasisId!=null).ToList();
        var newPaystubs = new List<PayStub>();

        var previousPayrollId = _context.Payroll.OrderByDescending(e => e.Id).FirstOrDefault(e => e.Id < currentPayroll.Id
            && e.CompanyId == companyId)?.Id;
      
        var previousPayStubs = _context.PayStub.OrderByDescending(e => e.Id).Where(e => e.Payroll.CompanyId == companyId &&
        !e.IsCancelled && e.Payroll.ScheduledRunDate.Year==currentYear)
            .GroupBy(e => e.EmploymentId).Select(g=>g.First()).ToList();

        var frequency = (PayrollScheduleTypeModel)currentPayroll.PayrollSchedule.PayrollScheduleTypeId;

        var employments = _context.Employment.Where(e => e.Job.CompanyId == companyId && e.PayRateBasisId!=null &&
        (!e.Deactivated || (e.EndDate != null && e.EndDate >= currentPayroll.StartDate))).ToList();

        var dayOffRequest = _dayOffRequestService.GetUnpaidDaysOffForPayrollSchedule(companyId,
            currentPayroll.StartDate, currentPayroll.ScheduledRunDate);


        var additionalWithholdingTaxWageLimit = double.Parse(_context.CountrySpecificFieldValue.FirstOrDefault(e => e.EffectiveYear == currentYear &&
        e.CountrySpecificField.FieldName == "AdditionalMedicareTaxWithholdingWageLimit")?.FieldValue ?? "0.0");

        foreach (var emp in employments)
        {            
            var payrate = emp.PayRate;
            var payRateBasis = emp.PayRateBasisId;
            var numberofDayOff = dayOffRequest.Count(x => x.EmploymentId == emp.Id);

            if (payRateBasis == null)
                continue;

            var regularPay = 0.0;  var regularHours = 0.0; var otPay = 0.0; var otHours = 0.0;
            Tuple<double, double, double, double> calculatedPay = null;

            var currentPaystub = currentPayStubs.FirstOrDefault(e => e.EmploymentId == emp.Id);
            var previousPayStub = previousPayStubs.FirstOrDefault(e => e.EmploymentId == emp.Id);
            
            if (payRateBasis == (int)PayRateBasisModel.Daily)
            {
                var pay = CalculatePayForDailyRatedEmployee(timecards, currentPayroll, emp);
                regularPay = pay.Item1;
                regularHours = pay.Item2 * 8;
                otPay = 0;
                otHours = 0;              
            }
            else
            {
                switch (frequency)
                {
                    case PayrollScheduleTypeModel.Monthly:
                        {
                            if (payRateBasis == (int)PayRateBasisModel.Annually) //(BusinessTypeModel)src.BusinessTypeId"Annually")
                            {
                                if (numberofDayOff > 0)
                                {
                                    var dailyPayrate = payrate / 260;
                                    regularPay = (payrate / 12) - (dailyPayrate * numberofDayOff);
                                }
                                else
                                {
                                    regularPay = payrate / 12;
                                }
                                break;
                            }
                            if (payRateBasis == (int)PayRateBasisModel.Monthly)
                            {
                                if (numberofDayOff > 0)
                                {
                                    var dailyPayrate = (payrate * 12) / 260;
                                    regularPay = payrate - (dailyPayrate * numberofDayOff);
                                }
                                else
                                {
                                    regularPay = payrate;
                                }
                                break;
                            }
                            if (payRateBasis == (int)PayRateBasisModel.Hourly)
                            {
                                calculatedPay = CalculatePayForHourlyRatedEmployees(timecards, currentPayroll, emp, frequency);
                                regularPay = calculatedPay.Item1;
                                regularHours = calculatedPay.Item2;
                                otPay = calculatedPay.Item3;
                                otHours = calculatedPay.Item4;
                                break;
                            }
                            throw new AppException($"Employee {emp.Person.FirstName} {emp.Person.LastName} has Invalid pay rate for Monthly payroll");
                        }
                    case PayrollScheduleTypeModel.SemiMonthly:
                        {
                            if (payRateBasis == (int)PayRateBasisModel.Annually)
                            {
                                if (numberofDayOff > 0)
                                {
                                    var dailyPayrate = payrate / 260;
                                    regularPay = (payrate / 24) - (dailyPayrate * numberofDayOff);
                                }
                                else
                                {
                                    regularPay = payrate / 24;
                                }
                                break;
                            }
                            if (payRateBasis == (int)PayRateBasisModel.Monthly)
                            {
                                if (numberofDayOff > 0)
                                {
                                    var dailyPayrate = (payrate * 12) / 260;
                                    regularPay = (payrate / 2) - (dailyPayrate * numberofDayOff);
                                }
                                else
                                {
                                    regularPay = payrate / 2;
                                }
                                break;
                            }
                            if (payRateBasis == (int)PayRateBasisModel.Weekly)
                            {
                                if (numberofDayOff > 0)
                                {
                                    var dailyPayrate = payrate / 5;
                                    regularPay = (payrate * 52 / 24) - (dailyPayrate * numberofDayOff);
                                }
                                else
                                {
                                    regularPay = (payrate * 52 / 24);
                                }

                                break;
                            }
                            if (payRateBasis == (int)PayRateBasisModel.Hourly)
                            {
                                calculatedPay = CalculatePayForHourlyRatedEmployees(timecards, currentPayroll, emp, frequency);
                                regularPay = calculatedPay.Item1;
                                regularHours = calculatedPay.Item2;
                                otPay = calculatedPay.Item3;
                                otHours = calculatedPay.Item4;
                                break;
                            }

                            throw new AppException($"Employee {emp.Person.FirstName} {emp.Person.LastName} has Invalid pay rate for Semi-Monthly payroll");
                        }
                    case PayrollScheduleTypeModel.Biweekly:
                        {
                            if (payRateBasis == (int)PayRateBasisModel.Annually)
                            {
                                if (numberofDayOff > 0)
                                {
                                    var dailyPayrate = payrate / 260;
                                    regularPay = (payrate / 26) - (dailyPayrate * numberofDayOff);
                                }
                                else
                                {
                                    regularPay = payrate / 26;
                                }

                                break;
                            }
                            if (payRateBasis == (int)PayRateBasisModel.Monthly)
                            {
                                if (numberofDayOff > 0)
                                {
                                    var dailyPayrate = (payrate * 12) / 260;
                                    regularPay = (payrate * 12 / 26) - (dailyPayrate * numberofDayOff);
                                }
                                else
                                {
                                    regularPay = payrate * 12 / 26;
                                }
                                break;
                            }
                            if (payRateBasis == (int)PayRateBasisModel.Weekly)
                            {
                                if (numberofDayOff > 0)
                                {
                                    var dailyPayrate = payrate / 5;
                                    regularPay = (payrate * 2) - (dailyPayrate * numberofDayOff);
                                }
                                else
                                {
                                    regularPay = payrate * 2;
                                }
                                break;
                            }
                            if (payRateBasis == (int)PayRateBasisModel.Hourly)
                            {
                                calculatedPay = CalculatePayForHourlyRatedEmployees(timecards, currentPayroll, emp, frequency);
                                regularPay = calculatedPay.Item1;
                                regularHours = calculatedPay.Item2;
                                otPay = calculatedPay.Item3;
                                otHours = calculatedPay.Item4;
                                break;
                            }
                            throw new AppException($"Employee {emp.Person.FirstName} {emp.Person.LastName} has Invalid pay rate for Bi-Weekly payroll");
                        }
                    case PayrollScheduleTypeModel.Weekly:
                        {
                            if (payRateBasis == (int)PayRateBasisModel.Annually)
                            {
                                if (numberofDayOff > 0)
                                {
                                    var dailyPayrate = payrate / 260;
                                    regularPay = (payrate / 52) - (dailyPayrate * numberofDayOff);
                                }
                                else
                                {
                                    regularPay = payrate / 52;
                                }
                                break;
                            }
                            if (payRateBasis == (int)PayRateBasisModel.Monthly)
                            {
                                if (numberofDayOff > 0)
                                {
                                    var dailyPayrate = payrate * 12 / 260;
                                    regularPay = (payrate * 12 / 52) - (dailyPayrate * numberofDayOff);
                                }
                                else
                                {
                                    regularPay = (payrate * 12 / 52);
                                }

                                break;
                            }
                            if (payRateBasis == (int)PayRateBasisModel.Weekly)
                            {
                                if (numberofDayOff > 0)
                                {
                                    var dailyPayrate = payrate / 5;
                                    regularPay = payrate - (dailyPayrate * numberofDayOff);
                                }
                                else
                                {
                                    regularPay = payrate;
                                }
                                break;
                            }
                            if (payRateBasis == (int)PayRateBasisModel.Hourly)
                            {
                                calculatedPay = CalculatePayForHourlyRatedEmployees(timecards, currentPayroll, emp, frequency);
                                regularPay = calculatedPay.Item1;
                                regularHours = calculatedPay.Item2;
                                otPay = calculatedPay.Item3;
                                otHours = calculatedPay.Item4;
                                break;
                            }

                            break;
                        }
                    default:
                        break;
                }
            }
           
            if (currentPaystub == null)
            {
                var paystub = new PayStub
                {
                    EmploymentId = emp.Id,
                    RegularHoursWorked = regularHours,
                    OverTimeHoursWorked = otHours,
                    OverTimePay = Math.Round(otPay, 2),
                    PayrollId = currentPayroll.Id,
                    RegularPay = Math.Round(regularPay, 2),
                    GrossPay = Math.Round(otPay + regularPay, 2),
                    NetPay = Math.Round(regularPay, 2), //This is initializing the netpay. It will be adjusted when we deduct tax
                    YtdGrossPay = Math.Round(otPay + regularPay + (previousPayStub?.YtdGrossPay ?? 0), 2),
                    YtdOverTimeHoursWorked = Math.Round(otHours + (previousPayStub?.YtdOverTimeHoursWorked ?? 0), 2),
                    YtdOverTimePay = Math.Round(otPay + (previousPayStub?.YtdOverTimePay ?? 0), 2),
                    YtdRegularHoursWorked = Math.Round(regularHours + (previousPayStub?.YtdRegularHoursWorked ?? 0), 2),
                    YtdRegularPay = Math.Round(regularPay + (previousPayStub?.RegularPay ?? 0), 2),
                    AmountSubjectToAdditionalMedicareTax = previousPayStub?.AmountSubjectToAdditionalMedicareTax > 0 ?
                    Math.Round(otPay + regularPay, 2) :
                    Math.Max(Math.Round(otPay + regularPay + previousPayStub?.YtdGrossPay ?? 0, 2) - additionalWithholdingTaxWageLimit, 0.0)

                };
                newPaystubs.Add(paystub);
            }
            else
            {
                currentPaystub.OverTimeHoursWorked = otHours;
                currentPaystub.OverTimePay = Math.Round(otPay, 2);
                currentPaystub.RegularHoursWorked = regularHours;
                currentPaystub.RegularPay = Math.Round(regularPay, 2);
                currentPaystub.GrossPay = Math.Round(regularPay,2);
                currentPaystub.NetPay = Math.Round(regularPay,2);
                currentPaystub.YtdGrossPay = Math.Round(otPay + regularPay + (previousPayStub?.YtdGrossPay?? 0),2);
                currentPaystub.YtdOverTimeHoursWorked = Math.Round(otHours + (previousPayStub?.YtdOverTimeHoursWorked ?? 0),2);
                currentPaystub.YtdOverTimePay = Math.Round(otPay + (previousPayStub?.YtdOverTimePay ?? 0),2);
                currentPaystub.YtdRegularHoursWorked = Math.Round(regularHours + (previousPayStub?.YtdRegularHoursWorked ?? 0),2);
                currentPaystub.YtdRegularPay = Math.Round(regularPay + (previousPayStub?.RegularPay ?? 0),2);
                currentPaystub.AmountSubjectToAdditionalMedicareTax = previousPayStub?.AmountSubjectToAdditionalMedicareTax > 0 ?
                    Math.Round(otPay + regularPay, 2) :
                    Math.Max(Math.Round(otPay + regularPay + previousPayStub?.YtdGrossPay ?? 0, 2) - additionalWithholdingTaxWageLimit, 0.0);

            }
        }

        if (isFinal)
        {
            timecards.ForEach(e => e.IsLocked = true);
            _context.UpdateRange(timecards);
        }

        return Tuple.Create(currentPayStubs, newPaystubs);
    }

    private static Tuple<double,double,double,double> CalculatePayForHourlyRatedEmployees(List<TimecardUsa> timecards , Payroll currentPayroll, 
        Employment emp, PayrollScheduleTypeModel frequency)
    {
        var myTimeCards = timecards.Where(e => e.PersonId == emp.PersonId);
        if(myTimeCards.Count()<1)
            return Tuple.Create(0.0, 0.0, 0.0, 0.0);

        var totalHoursWorked = myTimeCards.Select(e => Math.Round(e.HoursWorked.Value.TotalSeconds/60 / 60,2)).ToList();

        var firstWeekHours = myTimeCards.Where(e => e.ClockIn < currentPayroll.StartDate.AddDays(7).ToDateTime(TimeOnly.MinValue))
            .Select(e => Math.Round(e.HoursWorked.Value.TotalSeconds / 60 / 60,2)).ToList().Sum();

        var firstWeekOverTime = Math.Max(firstWeekHours - 40,0); //TODO: the 40 hours should come form database

        var secondWeekHours=0.0;
        var secondWeekOverTime = 0.0;
        var thirdWeekHours = 0.0;
        var thirdWeekOverTime = 0.0;
        var fourthWeekHours = 0.0;
        var fourthWeekOverTime = 0.0;

        if (frequency == PayrollScheduleTypeModel.SemiMonthly || frequency==PayrollScheduleTypeModel.Biweekly
            || frequency == PayrollScheduleTypeModel.Monthly)
        { 
            secondWeekHours = myTimeCards.Where(e => e.ClockIn >= currentPayroll.StartDate.AddDays(7).ToDateTime(TimeOnly.MinValue)
            && e.ClockIn < currentPayroll.StartDate.AddDays(14).ToDateTime(TimeOnly.MinValue))
            .Select(e => Math.Round(e.HoursWorked.Value.TotalSeconds / 60 / 60, 2)).ToList().Sum();

            secondWeekOverTime = Math.Max(secondWeekHours - 40, 0);
        }

        if (frequency == PayrollScheduleTypeModel.Monthly)
        {
            thirdWeekHours = myTimeCards.Where(e => e.ClockIn >= currentPayroll.StartDate.AddDays(14).ToDateTime(TimeOnly.MinValue)
           && e.ClockIn < currentPayroll.StartDate.AddDays(21).ToDateTime(TimeOnly.MinValue))
               .Select(e => Math.Round(e.HoursWorked.Value.TotalSeconds / 60 / 60,2)).ToList().Sum();

            thirdWeekOverTime = Math.Max(thirdWeekHours - 40, 0);

            fourthWeekHours = myTimeCards.Where(e => e.ClockIn >= currentPayroll.StartDate.AddDays(21)
            .ToDateTime(TimeOnly.MinValue) && e.ClockIn < currentPayroll.StartDate.AddDays(28)
            .ToDateTime(TimeOnly.MinValue)).Select(e => Math.Round(e.HoursWorked.Value.TotalSeconds / 60 / 60, 2)).ToList().Sum();
            
            fourthWeekOverTime = Math.Max(fourthWeekHours - 40, 0);
        }

        var overTimeHours = firstWeekOverTime + secondWeekOverTime + thirdWeekOverTime + fourthWeekOverTime;
        var regularHours = totalHoursWorked.Sum() - overTimeHours;

        var regularPay = Math.Round(emp.PayRate * regularHours,2);
        var overTimePayAmount = Math.Round(emp.PayRate * 1.5 * overTimeHours,2); //TODO: get the 1.5 value from CountrySpecificFields table in DB
        return Tuple.Create(regularPay, regularHours, overTimePayAmount,overTimeHours);
    }
    private static Tuple<double, int> CalculatePayForDailyRatedEmployee(List<TimecardUsa> timecards, 
        Payroll currentPayroll, Employment emp)
    {
        var myTimeCards = timecards.Where(e => e.PersonId == emp.PersonId);

        var totalDaysWorked = myTimeCards.Select(e => e.ClockIn.Day).Distinct().Count();

        var regularPay = Math.Round(emp.PayRate * totalDaysWorked, 2);

        return Tuple.Create(regularPay, totalDaysWorked);
    }

    public async Task<PayrollSummary> GetPayrollSummanryByPayrollId(int payrollId)
    {
        try
        {
            var payrollSummary = new PayrollSummary();
            var payroll = await _context.Payroll.FirstAsync(p => p.Id == payrollId);
            if (payroll != null)
            {
                var incometax = _context.IncomeTax.Include(e => e.PayStub).Where(e => e.PayStub.PayrollId == payrollId);
                if (incometax != null)
                {
                    payrollSummary.EmployeeSocialSecurity = incometax.Where(e => e.IncomeTaxType.Name == "Social Security" && e.IncomeTaxType.ForEmployee == true).Sum(e => e.Amount);
                    payrollSummary.EmployerSocialSecurity = incometax.Where(e => e.IncomeTaxType.Name == "Social Security" && e.IncomeTaxType.ForEmployee == false).Sum(e => e.Amount);
                    payrollSummary.EmployeeMediCare = incometax.Where(e => e.IncomeTaxType.Name == "Medicare" && e.IncomeTaxType.ForEmployee == true).Sum(e => e.Amount);
                    payrollSummary.EmployerMediCare = incometax.Where(e => e.IncomeTaxType.Name == "Medicare" && e.IncomeTaxType.ForEmployee == false).Sum(e => e.Amount);

                    if (payroll.EmployeesOwed.HasValue)
                    {
                        payrollSummary.WagesAndTIps = payroll.EmployeesOwed.Value;
                    }

                    if (payroll.FederalOwed.HasValue)
                    {
                        payrollSummary.FederalTaxWithHeld = payroll.FederalOwed.Value;
                    }

                    if (payroll.StatesOwed.HasValue)
                    {
                        payrollSummary.StateTaxWithHeld = payroll.StatesOwed.Value;
                    }

                    return payrollSummary;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        catch(Exception ex) 
        {

            return null;
        }

    }
}

