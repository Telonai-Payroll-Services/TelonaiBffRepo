namespace TelonaiWebApi.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
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

    public PayrollService(DataContext context, IMapper mapper, IMailSender mailSender)
    {
        _context = context;
        _mapper = mapper;
        _mailSender = mailSender;
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
        var payrolls = _context.Payroll.OrderByDescending(e => e.ScheduledRunDate).Where(e =>
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

        var now = DateOnly.FromDateTime(DateTime.Now);

        //Get the latest payroll schedule for all companies.
        //This gets the schedules that have already started or will start in the next 2 days

        var twoDaysFromNow = now.AddDays(2);

        var paySchedules = _context.PayrollSchedule.OrderByDescending(e => e.FirstRunDate).Where(e => e.StartDate <= twoDaysFromNow
            && (e.EndDate == null || e.EndDate > twoDaysFromNow))
            .GroupBy(e => e.CompanyId)
            .Select(g => g.First()).ToList();

        var currentPayrolls = _context.Payroll.Include(e => e.PayrollSchedule)
            .Where(e => e.ScheduledRunDate >= now)
            .GroupBy(e => e.CompanyId)
            .Where(g => g.Count() ==1) //This line will filter out those already created in the previous day
            .Select(g => g.First()).ToList();

        foreach (var payroll in currentPayrolls)
        {
            var newSchedule = paySchedules.Where(e => e.CompanyId == payroll.CompanyId).OrderByDescending(e=>e.Id).FirstOrDefault();
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
                var freq = (PayrollScheduleTypeModel)newSchedule.PayrollScheduleTypeId;

                switch (freq)
                {
                    case PayrollScheduleTypeModel.Monthly:
                        nextPayrollRunDate = payroll.ScheduledRunDate.AddMonths(1);
                        nextPayrollStartDate = new DateOnly(nextPayrollRunDate.Year, nextPayrollRunDate.Month, 1);
                        nextPayrollRunDate = nextPayrollStartDate.AddMonths(1).AddDays(-1);

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
                            nextPayrollRunDate = nextPayrollStartDate.AddMonths(1).AddDays(17);
                        }
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


            //send email to the owner
            var admins = (from person in _context.Person
                          join employment in _context.Employment
                          on person.Id equals employment.PersonId
                          where person.CompanyId == payroll.CompanyId && employment.IsPayrollAdmin
                          select new
                          {
                              person.Email,
                              person.FirstName,
                              person.LastName,
                              ScheduledRunDate = payroll.ScheduledRunDate
                          }).ToList();
           
            foreach (var admin in admins) {
                _ = _mailSender.SendUsingAwsClientAsync(
                    admin.Email,
                    $"Time to run payroll",
                    CreateHtmlEmailBody(admin.ScheduledRunDate.ToString("MMM/dd/yyyy"), $"{admin.FirstName} {admin.LastName}"),
                    CreateTextEmailBody(admin.ScheduledRunDate.ToString("MMM/dd/yyyy"), $"{admin.FirstName} {admin.LastName}")
                ).ContinueWith(task => {
                    if (task.IsFaulted){
                        Console.Error.WriteLine($"Failed to send email to {admin.Email}: {task.Exception}");
                    }
                });
            }
        }

        _ = SendReminderForLatePayrolls();

        if (newPayrollsList.Count > 0)
        {
            _context.Payroll.AddRange(newPayrollsList);
            _context.SaveChanges();
        }

        return newPayrollsList.Count;
    }

    public async Task SendReminderForLatePayrolls()
    {
        var now = DateOnly.FromDateTime(DateTime.Now);

        var latePayrolls = _context.Payroll.Include(e => e.PayrollSchedule)
            .Where(e => e.ScheduledRunDate < now && e.TrueRunDate == null)
            .GroupBy(e => e.CompanyId)
            .Where(g => g.Count() == 1) //This line will filter out those already created in the previous day
            .Select(g => g.First()).ToList();

        foreach (var payroll in latePayrolls)
        {
            var admins = (from person in _context.Person
                          join employment in _context.Employment
                          on person.Id equals employment.Id
                          where person.CompanyId == payroll.CompanyId && employment.IsPayrollAdmin
                          select new
                          {
                              person.Email,
                              person.FirstName,
                              person.LastName,
                              ScheduledRunDate = payroll.ScheduledRunDate
                          }).ToList();

            foreach (var admin in admins)
            {
                _ = _mailSender.SendUsingAwsClientAsync(
                    admin.Email,
                    $"Time to run payroll",
                    CreateHtmlEmailBodyForLatePayroll(admin.ScheduledRunDate.ToString("MMM/dd/yyyy"), $"{admin.FirstName} {admin.LastName}"),
                    CreateTextEmailBodyForLatePayroll(admin.ScheduledRunDate.ToString("MMM/dd/yyyy"), $"{admin.FirstName} {admin.LastName}")
                ).ContinueWith(task => {
                    if (task.IsFaulted)
                    {
                        Console.Error.WriteLine($"Failed to send email to {admin.Email}: {task.Exception}");
                    }
                });
            }

        }
    }

    private static string CreateTextEmailBody(string scheduledDate, string receiverName)
    {
        return $"Time to Run Payroll"
            + $"Dear {receiverName},\r\n"
            + $"Your payroll run date is on {scheduledDate}. To ensure your employees get paid on time, please run payroll, "
            + $"using our mobile app, on {scheduledDate}. \r\n"
            + $"Please do not reply to this email as it is not monitored.";
    }

    private static string CreateHtmlEmailBody(string scheduledDate, string receiverName)
    {
        return $"<h2>Time to Run Payroll</h2>"
         + $"Dear {receiverName}, </br>"
         + $"<p>Your payroll run date is on <strong>{scheduledDate}</strong>. To ensure your employees get paid on time, "
         + $"please run payroll, using our mobile app, on <strong>{scheduledDate}</strong>. "
         + $"<br/>Please do not reply to this email as it is not monitored.";
    }
    private static string CreateTextEmailBodyForLatePayroll(string scheduledDate, string receiverName)
    {
        return $"Time to Run Payroll"
            + $"Dear {receiverName},\r\n"
            + $"Your payroll run date has passed. It was on {scheduledDate}. Using our mobile app, please run payroll as soon as possible. Please note that your employees will not get paid until you run payroll. \r\n\n"
            + $"Please do not reply to this email as it is not monitored. If you need assistance, please call our support team at 601-608-7025";
    }

    private static string CreateHtmlEmailBodyForLatePayroll(string scheduledDate, string receiverName)
    {
        return $"<h2>Time to Run Payroll</h2>"
         + $"Dear {receiverName}, </br>"
         + $"Your payroll run date has passed. It was on <strong>{scheduledDate}</strong>. Using our mobile app, please run payroll as soon as possible. Please note that your employees will not get paid until you run payroll."
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

        var stubs = CompletePayStubsForCurrentPayroll(dto);

        if (dto.TrueRunDate == null)
        {
            dto.TrueRunDate = DateTime.UtcNow;

            _context.Payroll.Update(dto);
            if (stubs.Item1.Count > 0)
                _context.PayStub.UpdateRange(stubs.Item1);

            if (stubs.Item2.Count > 0)
                _context.PayStub.AddRange(stubs.Item2);            
            _context.SaveChanges();
        }
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
                return AvoidHolidaysAndWeekends(nextRunDate, countryId);                
            case PayrollScheduleTypeModel.SemiMonthly:
                if (scheduleStartDate.Day < 16)
                    nextRunDate = scheduleStartDate.AddDays(15 - scheduleStartDate.Day);
                else
                    nextRunDate = scheduleStartDate.AddDays(daysInMonth - scheduleStartDate.Day);
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
        var payrolls = _context.Payroll.Include(e => e.PayrollSchedule).Where(e => e.StartDate <= today && e.ScheduledRunDate >= today
        && e.TrueRunDate == null).ToList();

        for (int i = 0; i < payrolls.Count; i++)
        {
            var payroll = payrolls[i];
            var stubs = await CreateOrUpdatePayStubsForCurrentPayrollAsync(payroll);
            payroll.GrossPay = stubs.Item1.Sum(e => e.GrossPay) + stubs.Item2.Sum(e => e.GrossPay);

            if (stubs.Item1.Count > 0)
                _context.PayStub.UpdateRange(stubs.Item1);

            if (stubs.Item2.Count > 0)
                _context.PayStub.AddRange(stubs.Item2);

            _context.Payroll.Update(payroll);
            _ = _context.SaveChanges();
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

        //Repeat holiday checks because moving it from the weekend may have brought it a holiday
        if (holidays.Any(e => e.Date == date))
            date = date.AddDays(-1);

        //Do this again in case there are two holidays next to each other
        if (holidays.Any(e => e.Date == date))
            date = date.AddDays(-1);

        return date;
    }
    private Payroll GetPayroll(int id)
    {
        var dto = _context.Payroll.Find(id);
        return dto;
    }

    /// <summary>
    /// This method creates paystubs if they don't exist, it updates them if they exist.
    /// It must be invoked via lambda once a day so that the expense tracker displays correct data. 
    /// </summary>
    /// <param name="currentPayroll"></param>
    /// <returns>List of Updated paystubs as Item1, and list of new paystubs as item 2 of a Tuple</returns>
    /// <exception cref="AppException"></exception>
    private async Task<Tuple<List<PayStub>, List<PayStub>>> CreateOrUpdatePayStubsForCurrentPayrollAsync(Payroll currentPayroll)
    {

        var payStubs = _context.PayStub.Where(e => e.PayrollId == currentPayroll.Id).ToList();
        var newPaystubs = new List<PayStub>();

        var companyId = currentPayroll.CompanyId;
        var currentYear = currentPayroll.ScheduledRunDate.Year;

        var timecards = _context.TimecardUsa.Where(e => e.Job.CompanyId == companyId &&
        DateOnly.FromDateTime(e.ClockIn) >= currentPayroll.StartDate &&
        DateOnly.FromDateTime(e.ClockIn) <= currentPayroll.ScheduledRunDate).ToList();

        var frequency = (PayrollScheduleTypeModel)currentPayroll.PayrollSchedule.PayrollScheduleTypeId;

        var employments = _context.Employment.Where(e => e.Job.CompanyId == companyId &&
        (!e.Deactivated || (e.EndDate != null && e.EndDate >= currentPayroll.StartDate
        && e.EndDate > currentPayroll.StartDate))).ToList();

        foreach (var emp in employments)
        {
            var payrate = emp.PayRate;
            var payRateBasis = emp.PayRateBasisId;

            var regularPay = 0.0;
            var regularHours = 0.0;
            var otPay = 0.0;
            var otHours = 0.0;
            Tuple<double, double, double, double> calculatedPay = null;

            var currentPaystub = payStubs.FirstOrDefault(e => e.EmploymentId == emp.Id);

            if (payRateBasis == (int)PayRateBasisModel.Daily)
            {
                var pay = CalculatePayForDailyRatedEmployee(timecards, currentPayroll, emp);
                regularPay = Math.Round(pay.Item1, 2);
                regularHours = pay.Item2 * 8;

                if (currentPaystub == null)
                {
                    var paystub1 = new PayStub
                    {
                        EmploymentId = emp.Id,
                        RegularHoursWorked = regularHours,
                        PayrollId = currentPayroll.Id,
                        RegularPay = regularPay,
                        GrossPay = regularPay,
                        NetPay = regularPay,
                    };

                    newPaystubs.Add(paystub1);
                }
                else
                {
                    currentPaystub.EmploymentId = emp.Id;
                    currentPaystub.RegularHoursWorked = regularHours;
                    currentPaystub.PayrollId = currentPayroll.Id;
                    currentPaystub.RegularPay = regularPay;
                    currentPaystub.GrossPay = regularPay;
                    currentPaystub.NetPay = regularPay;
                }

                continue;
            }

            switch (frequency)
            {
                case PayrollScheduleTypeModel.Monthly:
                    {
                        if (payRateBasis == (int)PayRateBasisModel.Annually) //(BusinessTypeModel)src.BusinessTypeId"Annually")
                        {
                            regularPay = payrate / 12;
                            break;
                        }
                        if (payRateBasis == (int)PayRateBasisModel.Monthly)
                        {
                            regularPay = payrate;
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
                            regularPay = payrate / 24;
                            break;
                        }
                        if (payRateBasis == (int)PayRateBasisModel.Monthly)
                        {
                            regularPay = payrate / 2;
                            break;
                        }
                        if (payRateBasis == (int)PayRateBasisModel.Weekly)
                        {
                            regularPay = payrate * 52 / 24;
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
                            regularPay = payrate / 26;
                            break;
                        }
                        if (payRateBasis == (int)PayRateBasisModel.Monthly)
                        {
                            regularPay = payrate * 12 / 26;
                            break;
                        }
                        if (payRateBasis == (int)PayRateBasisModel.Weekly)
                        {
                            regularPay = payrate * 2;
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
                            regularPay = payrate / 52;
                            break;
                        }
                        if (payRateBasis == (int)PayRateBasisModel.Monthly)
                        {
                            regularPay = payrate * 12 / 52;
                            break;
                        }
                        if (payRateBasis == (int)PayRateBasisModel.Weekly)
                        {
                            regularPay = payrate;
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

            if (currentPaystub == null)
            {
                var paystub = new PayStub
                {
                    EmploymentId = emp.Id,
                    RegularHoursWorked = regularHours,
                    OverTimeHoursWorked = otHours,
                    OverTimePay = otPay,
                    PayrollId = currentPayroll.Id,
                    RegularPay = regularPay,
                    GrossPay = otPay + regularPay,
                };
                newPaystubs.Add(paystub);
            }
            else
            {
                currentPaystub.EmploymentId = emp.Id;
                currentPaystub.RegularHoursWorked = regularHours;
                currentPaystub.PayrollId = currentPayroll.Id;
                currentPaystub.RegularPay = regularPay;
                currentPaystub.GrossPay = regularPay;
                currentPaystub.NetPay = regularPay;
            }
        }
        
        return Tuple.Create(payStubs, newPaystubs);
    }

    /// <summary>
    /// This method is invoked when Payrolls are run. Paystubs will not change after this method call
    /// </summary>
    /// <param name="currentPayroll"></param>
    /// <returns>List of Updated paystubs as Item1, and list of new paystubs as item 2 of a Tuple</returns>
    /// <exception cref="AppException"></exception>
    private Tuple<List<PayStub>, List<PayStub>> CompletePayStubsForCurrentPayroll(Payroll currentPayroll)
    {
        var previousPayrollId = _context.Payroll.OrderByDescending(e=>e.Id).FirstOrDefault(e => e.Id < currentPayroll.Id)?.Id;
        var previousPayStubs = _context.PayStub.Where(e => e.PayrollId == previousPayrollId).ToList();

        var currentPayStubs = _context.PayStub.Where(e => e.PayrollId == currentPayroll.Id).ToList();
        var newPayStubs = new List<PayStub>();
        var companyId = currentPayroll.CompanyId;
        var company= currentPayroll.Company;
        var currentYear = currentPayroll.ScheduledRunDate.Year;

        var timecards = _context.TimecardUsa.Where(e => e.Job.CompanyId == companyId &&
        DateOnly.FromDateTime(e.ClockIn) >= currentPayroll.StartDate &&
        DateOnly.FromDateTime(e.ClockIn) <= currentPayroll.ScheduledRunDate).ToList();

        if (timecards.Any(e => !e.IsApproved))
            throw new AppException("You have not approved all timecards");
               
        var schedule = _context.PayrollSchedule.FirstOrDefault(e => e.CompanyId == companyId && (e.EndDate == null ||
        e.EndDate < currentPayroll.ScheduledRunDate));        
        var frequency = (PayrollScheduleTypeModel)schedule?.PayrollScheduleTypeId;

        var employments = _context.Employment.Where(e => e.Job.CompanyId == companyId &&
        (!e.Deactivated || (e.EndDate != null && e.EndDate >= currentPayroll.StartDate))).ToList();

        timecards.ForEach(e => e.IsLocked = true);
        _context.UpdateRange(timecards);

        var additionalWithholdingTaxWageLimit = double.Parse(_context.CountrySpecificFieldValue.FirstOrDefault(e => e.EffectiveYear == currentYear && 
        e.CountrySpecificField.FieldName== "AdditionalMedicareTaxWithholdingWageLimit")?.FieldValue?? "0.0");

        foreach (var emp in employments)
        {
            var payrate = emp.PayRate;
            var payRateBasis = emp.PayRateBasisId;

            var regularPay = 0.0;
            var regularHours = 0.0;
            var otPay = 0.0;
            var otHours = 0.0;
            Tuple<double, double, double, double> calculatedPay = null;

            var currentPaystub = currentPayStubs.FirstOrDefault(e => e.EmploymentId == emp.Id);
            var previousPayStub = previousPayStubs.FirstOrDefault(e => e.EmploymentId == emp.Id);

            if (payRateBasis == (int)PayRateBasisModel.Daily)
            {
                var pay = CalculatePayForDailyRatedEmployee(timecards, currentPayroll, emp);
                regularPay = pay.Item1;
                regularHours = pay.Item2 * 8;

                if (currentPaystub == null)
                {
                    currentPaystub = new PayStub
                    {
                        EmploymentId = emp.Id,
                        RegularHoursWorked = regularHours,
                        PayrollId = currentPayroll.Id,
                        RegularPay = regularPay,
                        GrossPay = regularPay,
                        NetPay = regularPay,
                    };
                    newPayStubs.Add(currentPaystub);
                }
                else
                {
                    currentPaystub.RegularHoursWorked = regularHours;
                    currentPaystub.RegularPay = regularPay;
                    currentPaystub.GrossPay = regularPay;
                    currentPaystub.NetPay = regularPay;
                }
                if (previousPayStub != null)
                {
                    currentPaystub.YtdGrossPay = regularPay + previousPayStub.YtdGrossPay;
                    currentPaystub.YtdOverTimeHoursWorked = previousPayStub.YtdOverTimeHoursWorked;
                    currentPaystub.YtdOverTimePay = previousPayStub.YtdOverTimePay;
                    currentPaystub.YtdRegularHoursWorked = regularHours + previousPayStub.YtdRegularHoursWorked;
                    currentPaystub.YtdRegularPay = regularPay + previousPayStub.RegularPay;
                    currentPaystub.AmountSubjectToAdditionalMedicareTax = previousPayStub.AmountSubjectToAdditionalMedicareTax > 0 ? otPay + regularPay :
                        Math.Max(regularPay + previousPayStub.YtdGrossPay - additionalWithholdingTaxWageLimit, 0.0);
                    currentPaystub.YtdNetPay = regularPay + previousPayStub.YtdNetPay;
                }

                continue;
            }

            switch (frequency)
            {
                case PayrollScheduleTypeModel.Monthly:
                    {
                        if (payRateBasis == (int)PayRateBasisModel.Annually) //(BusinessTypeModel)src.BusinessTypeId"Annually")
                        {
                            regularPay = payrate / 12;
                            break;
                        }
                        if (payRateBasis == (int)PayRateBasisModel.Monthly)
                        {
                            regularPay = payrate;
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
                            regularPay = payrate / 24;
                            break;
                        }
                        if (payRateBasis == (int)PayRateBasisModel.Monthly)
                        {
                            regularPay = payrate / 2;
                            break;
                        }
                        if (payRateBasis == (int)PayRateBasisModel.Weekly)
                        {
                            regularPay = payrate * 52 / 24;
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
                            regularPay = payrate / 26;
                            break;
                        }
                        if (payRateBasis == (int)PayRateBasisModel.Monthly)
                        {
                            regularPay = payrate * 12 / 26;
                            break;
                        }
                        if (payRateBasis == (int)PayRateBasisModel.Weekly)
                        {
                            regularPay = payrate * 2;
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
                            regularPay = payrate / 52;
                            break;
                        }
                        if (payRateBasis == (int)PayRateBasisModel.Monthly)
                        {
                            regularPay = payrate * 12 / 52;
                            break;
                        }
                        if (payRateBasis == (int)PayRateBasisModel.Weekly)
                        {
                            regularPay = payrate;
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
            
            regularPay= Math.Round(regularPay,2);
            otPay = Math.Round(otPay, 2);

            if (currentPaystub == null)
            {
                currentPaystub = new PayStub
                {
                    EmploymentId = emp.Id,
                    RegularHoursWorked = regularHours,
                    OverTimeHoursWorked = otHours,
                    OverTimePay = otPay,
                    PayrollId = currentPayroll.Id,
                    RegularPay = regularPay,
                    GrossPay = otPay + regularPay,
                };
                newPayStubs.Add(currentPaystub);
            }
            else
            {
                currentPaystub.RegularHoursWorked = regularHours;
                currentPaystub.RegularPay = regularPay;
                currentPaystub.GrossPay = otPay + regularPay;
                currentPaystub.NetPay = otPay + regularPay;
                currentPaystub.OverTimePay = otPay;
                currentPaystub.OverTimeHoursWorked = otHours;
            }
            if (previousPayStub != null)
            {
                currentPaystub.YtdGrossPay = otPay + regularPay + previousPayStub.YtdGrossPay;
                currentPaystub.YtdOverTimeHoursWorked = otHours + previousPayStub.YtdOverTimeHoursWorked;
                currentPaystub.YtdOverTimePay = otPay + previousPayStub.YtdOverTimePay;
                currentPaystub.YtdRegularHoursWorked = regularHours + previousPayStub.YtdRegularHoursWorked;
                currentPaystub.YtdRegularPay = regularPay + previousPayStub.RegularPay;
                currentPaystub.AmountSubjectToAdditionalMedicareTax = previousPayStub.AmountSubjectToAdditionalMedicareTax > 0 ? otPay + regularPay :
                    Math.Max(otPay + regularPay + previousPayStub.YtdGrossPay - additionalWithholdingTaxWageLimit, 0.0);
            }
        }

        return Tuple.Create(currentPayStubs, newPayStubs);
    }

    private static Tuple<double,double,double,double> CalculatePayForHourlyRatedEmployees(List<TimecardUsa> timecards , Payroll currentPayroll, 
        Employment emp, PayrollScheduleTypeModel frequency)
    {
        var myTimeCards = timecards.Where(e => e.PersonId == emp.PersonId);

        var totalHoursWorked = myTimeCards.Select(e => Math.Round(e.HoursWorked.Value.TotalSeconds/60 / 60)).ToList();

        var firstWeekHours = myTimeCards.Where(e => e.ClockIn < currentPayroll.StartDate.AddDays(7).ToDateTime(TimeOnly.MinValue))
            .Select(e => Math.Round(e.HoursWorked.Value.TotalSeconds / 60 / 60,2)).ToList().Sum();

        var firstWeekOverTime = Math.Max(firstWeekHours - 40,0);

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
            secondWeekOverTime = secondWeekHours - 40;
        }

        if (frequency == PayrollScheduleTypeModel.Monthly)
        {
            thirdWeekHours = myTimeCards.Where(e => e.ClockIn >= currentPayroll.StartDate.AddDays(14).ToDateTime(TimeOnly.MinValue)
           && e.ClockIn < currentPayroll.StartDate.AddDays(21).ToDateTime(TimeOnly.MinValue))
               .Select(e => Math.Round(e.HoursWorked.Value.TotalSeconds / 60 / 60,2)).ToList().Sum();
            thirdWeekOverTime = thirdWeekHours - 40;

            fourthWeekHours = myTimeCards.Where(e => e.ClockIn >= currentPayroll.StartDate.AddDays(21)
            .ToDateTime(TimeOnly.MinValue) && e.ClockIn < currentPayroll.StartDate.AddDays(28)
            .ToDateTime(TimeOnly.MinValue)).Select(e => Math.Round(e.HoursWorked.Value.TotalSeconds / 60 / 60, 2)).ToList().Sum();
            fourthWeekOverTime = fourthWeekHours - 40;
        }

        var overTimeHours = firstWeekOverTime + secondWeekOverTime + thirdWeekOverTime + fourthWeekOverTime;
        var regularHours = totalHoursWorked.Sum() - overTimeHours;

        var regularPay = Math.Round(emp.PayRate * regularHours,2);
        var overTimePayAmount = Math.Round(emp.PayRate * 1.5 * overTimeHours,2); //TO DO: get the 1.5 value from CountrySpecificFields table in DB
        return Tuple.Create(regularPay, regularHours, overTimePayAmount,overTimeHours);
    }
    private static Tuple<double, int> CalculatePayForDailyRatedEmployee(List<TimecardUsa> timecards, 
        Payroll currentPayroll, Employment emp)
    {
        var myTimeCards = timecards.Where(e => e.PersonId == emp.PersonId &&
                            e.ClockIn > currentPayroll.StartDate.ToDateTime(TimeOnly.MinValue) &&
                            e.ClockIn < currentPayroll.ScheduledRunDate.ToDateTime(TimeOnly.MaxValue));

        var totalDaysWorked = myTimeCards.Select(e => e.ClockIn.Day).Distinct().Count();

        var regularPay = Math.Round(emp.PayRate * totalDaysWorked,2);

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

