namespace TelonaiWebApi.Services;

using AutoMapper;
using iTextSharp.text;
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



    PayrollModel GetById(int id);
    Task<int> CreateNextPayrollForAll();
    void Create(int companyId);
    void Update(int id, int companyId);
    void Delete(int id);

    public Task<List<PayrollSummary>> GetPayrollSummanryByPayrollId(int payrollId);

}

public class PayrollService : IPayrollService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public PayrollService(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public List<PayrollModel> GetLatestByCount(int companyId, int count)
    {
        var obj = _context.Payroll.OrderByDescending(e => e.ScheduledRunDate)
            .Where(e => e.CompanyId == companyId && e.StartDate < DateOnly.FromDateTime(DateTime.UtcNow))
            .Take(count).ToList();

        var result = _mapper.Map<List<PayrollModel>>(obj);
        var averageAmount = result.Where(e => e.ScheduledRunDate < DateTime.Now).Sum(e=>e.EmployeesOwed)/(count-1);
        var dailyRates = result.Where(e => e.ScheduledRunDate < DateTime.Now)
            .Select(e => GetDailyPayrollExpense(e.EmployeesOwed.Value, e.ScheduledRunDate, e.StartDate));

        double buffer1 = 100;
        double buffer2 = 500;
        result.ForEach(e => {
            var diff = e.EmployeesOwed - averageAmount;
            if (e.ScheduledRunDate > DateTime.Now && e.EmployeesOwed.HasValue &&
            GetForecastedPayrollExpense(e.EmployeesOwed.Value, e.StartDate, e.ScheduledRunDate) - averageAmount > buffer2)
            {
                e.ExpenseTrackingHexColor = "#D20103";
                return;
            }
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

    private static double GetDailyPayrollExpense(double totalEmployeesOwed, DateTime startDate, DateTime endDate)
    {
        var span = endDate - startDate;
        var dailyAmount = totalEmployeesOwed / span.TotalDays;
        return dailyAmount;
    }

    private static double GetForecastedPayrollExpense(double totalEmployeesOwed, DateTime startDate, DateTime endDate)
    {
        var span = DateTime.UtcNow - startDate;
        var projectedSpan = endDate - startDate;
        var dailyAmount = totalEmployeesOwed / span.TotalDays;
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
    public async Task<int> CreateNextPayrollForAll()
    {
        var newPayrollsList = new List<Payroll>();

        //Get the latest payroll schedule for all companies.
        //This gets the schedules that have already started or will start in the next 2 days
        var startDate = DateOnly.FromDateTime(DateTime.Now.AddDays(2));
        var paySchedules = _context.PayrollSchedule.OrderByDescending(e => e.FirstRunDate).Where(e => e.StartDate < startDate
            && (e.EndDate == null || e.EndDate >= startDate.AddDays(-2)))
            .GroupBy(e => e.CompanyId).Select(g => g.First()).ToList();

        var currentPayrolls = _context.Payroll.Include(e => e.PayrollSchedule)
        .Where(e => e.ScheduledRunDate >= startDate && e.TrueRunDate == null)
        .GroupBy(e => e.CompanyId)
        .Where(g => g.Count() ==1) //This line will filter out those already created in the previous day
        .Select(g => g.First()).ToList();

        foreach (var payroll in currentPayrolls)
        {
            var newSchedule = paySchedules.Where(e => e.CompanyId == payroll.CompanyId).FirstOrDefault();
            var existingSchedule = payroll.PayrollSchedule;
            var scheduleChanged = newSchedule.Id != existingSchedule.Id;
                        
            var nextRunDate = payroll.PayrollSchedule.StartDate;

            var freq = (PayrollScheduleTypeModel)newSchedule.PayrollScheduleTypeId;
            switch (freq)
            {
                case PayrollScheduleTypeModel.Monthly:
                    nextRunDate = scheduleChanged ? newSchedule.StartDate.AddDays(-1).AddMonths(1) : payroll.ScheduledRunDate.AddDays(1).AddMonths(1);
                    break;
                case PayrollScheduleTypeModel.SemiMonthly:
                    nextRunDate = scheduleChanged ? newSchedule.StartDate.AddDays(13) : payroll.ScheduledRunDate.AddDays(14);
                    if (nextRunDate.Day < 15)
                        nextRunDate = DateOnly.Parse($"{nextRunDate.Year}-{nextRunDate.Month}-15");
                    else
                        nextRunDate = DateOnly.Parse($"{nextRunDate.Year}-{nextRunDate.Month}-{DateTime.DaysInMonth(nextRunDate.Year, nextRunDate.Month)}");
                    break;
                case PayrollScheduleTypeModel.Biweekly:
                    nextRunDate = scheduleChanged ? newSchedule.StartDate.AddDays(13) : payroll.ScheduledRunDate.AddDays(14);
                    break;
                case PayrollScheduleTypeModel.Weekly:
                    nextRunDate = scheduleChanged ? newSchedule.StartDate.AddDays(6) : payroll.ScheduledRunDate.AddDays(7);
                    break;
                case 0:
                    throw new AppException("Payroll run frequency not defined");
            }

            if (nextRunDate.DayOfWeek == DayOfWeek.Saturday)
                nextRunDate = nextRunDate.AddDays(-1);
            else if (nextRunDate.DayOfWeek == DayOfWeek.Sunday)
                nextRunDate = nextRunDate.AddDays(-2);

            var nextPayroll = new Payroll
            {
                PayrollScheduleId = newSchedule.Id,
                ScheduledRunDate = nextRunDate,
                StartDate = scheduleChanged ? newSchedule.StartDate : payroll.ScheduledRunDate.AddDays(1),
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


    /// <summary>
    /// Creates  the current (and theoretically  the first) payroll that will be run by a manager after a week,month, etc,
    /// as determined by the PayrollScheduleType.
    /// </summary>
    /// <param name="companyId"></param>
    /// <exception cref="AppException"></exception>
    public void Create(int companyId)
    {
        var paySchedule = _context.PayrollSchedule.FirstOrDefault(e => e.CompanyId == companyId && e.EndDate == null) ?? 
            throw new AppException("Payroll Schedule has not been setup yet.");

        var freq = (PayrollScheduleTypeModel)paySchedule.PayrollScheduleTypeId;

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

        var stubs = CreatePaystubs(dto);

        if (dto.TrueRunDate == null)
        {
            dto.TrueRunDate = DateTime.UtcNow;

            _context.Payroll.Update(dto);
            _context.PayStub.AddRange(stubs);
            
            _context.SaveChanges();
        }
    }

    public void Delete(int id)
    {
        var dto = GetPayroll(id);
        _context.Payroll.Remove(dto);
        _context.SaveChanges();
    }

    private Payroll GetPayroll(int id)
    {
        var dto = _context.Payroll.Find(id);
        return dto;
    }
       
    private List<PayStub>CreatePaystubs(Payroll currentPayroll)
    {
        var payStubs= new List<PayStub>();
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
        (!e.Deactivated || (e.EndDate != null && e.EndDate < currentPayroll.ScheduledRunDate
        && e.EndDate > currentPayroll.StartDate))).ToList();

        timecards.ForEach(e => e.IsLocked = true);
        _context.UpdateRange(timecards);

        var additionalWithholdingTaxWageLimit = double.Parse(_context.CountrySpecificFieldValue.FirstOrDefault(e => e.EffectiveYear == currentYear && 
        e.CountrySpecificField.FieldName== "AdditionalMedicareTaxWithholdingWageLimit")?.FieldValue?? "0.0");

        foreach (var  emp in employments) 
        {
            var payrate = emp.PayRate;
            var payRateBasis = emp.PayRateBasisId;

            var regularPay = 0.0;
            var regularHours = 0.0;
            var otPay = 0.0;
            var otHours = 0.0;
            Tuple<double,double,double,double> calculatedPay = null;

            var previousPayStub = _context.PayStub.OrderByDescending(e => e.Id).FirstOrDefault(e => e.EmploymentId == emp.Id
            && !e.IsCancelled);

            if (payRateBasis == (int)PayRateBasisModel.Daily)
            {
                var pay = CalculatePayForDailyRatedEmployees(timecards, currentPayroll, emp);
                regularPay = pay.Item1;
                regularHours = pay.Item2 * 8;

                var paystub1 = new PayStub
                {
                    EmploymentId = emp.Id,
                    RegularHoursWorked = regularHours,                    
                    PayrollId = currentPayroll.Id,
                    RegularPay = regularPay,
                    GrossPay =  regularPay
                };

                if (previousPayStub != null)
                {
                    paystub1.YtdGrossPay = regularPay + previousPayStub.YtdGrossPay;
                    paystub1.YtdOverTimeHoursWorked = previousPayStub.YtdOverTimeHoursWorked;
                    paystub1.YtdOverTimePay = previousPayStub.YtdOverTimePay;
                    paystub1.YtdRegularHoursWorked = regularHours + previousPayStub.YtdRegularHoursWorked;
                    paystub1.YtdRegularPay = regularPay + previousPayStub.RegularPay;
                    paystub1.AmountSubjectToAdditionalMedicareTax = previousPayStub.AmountSubjectToAdditionalMedicareTax > 0 ? otPay + regularPay :
                        Math.Max(regularPay + previousPayStub.YtdGrossPay - additionalWithholdingTaxWageLimit, 0.0);
                }
                payStubs.Add(paystub1);

                continue;
            }

            switch (frequency)
            {
                case PayrollScheduleTypeModel.Monthly:
                    {
                        if(payRateBasis== (int)PayRateBasisModel.Annually) //(BusinessTypeModel)src.BusinessTypeId"Annually")
                        {
                            regularPay = payrate/12;
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
                            regularPay = payrate/2;
                            break;
                        }
                        if (payRateBasis == (int)PayRateBasisModel.Weekly)
                        {
                            regularPay = payrate*52 / 24;
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
            var paystub = new PayStub
            {
                EmploymentId = emp.Id,
                RegularHoursWorked = regularHours,
                OverTimeHoursWorked = otHours,
                OverTimePay = otPay,
                PayrollId = currentPayroll.Id,
                RegularPay = regularPay,
                GrossPay = otPay + regularPay                
            };

            if (previousPayStub != null)
            {
                paystub.YtdGrossPay = otPay + regularPay + previousPayStub.YtdGrossPay;
                paystub.YtdOverTimeHoursWorked = otHours + previousPayStub.YtdOverTimeHoursWorked;
                paystub.YtdOverTimePay = otPay + previousPayStub.YtdOverTimePay;
                paystub.YtdRegularHoursWorked = regularHours + previousPayStub.YtdRegularHoursWorked;
                paystub.YtdRegularPay = regularPay + previousPayStub.RegularPay;
                paystub.AmountSubjectToAdditionalMedicareTax = previousPayStub.AmountSubjectToAdditionalMedicareTax > 0 ? otPay + regularPay :
                    Math.Max(otPay + regularPay + previousPayStub.YtdGrossPay - additionalWithholdingTaxWageLimit, 0.0);
            }

            payStubs.Add(paystub);
        }
       
        
        return payStubs;
    }

    private static Tuple<double,double,double,double> CalculatePayForHourlyRatedEmployees(List<TimecardUsa> timecards , Payroll currentPayroll, 
        Employment emp, PayrollScheduleTypeModel frequency)
    {
        var myTimeCards = timecards.Where(e => e.PersonId == emp.PersonId &&
                            e.ClockIn > currentPayroll.StartDate.ToDateTime(TimeOnly.MinValue) &&
                            e.ClockIn < currentPayroll.ScheduledRunDate.ToDateTime(TimeOnly.MaxValue));

        var totalHoursWorked = myTimeCards.Select(e => Math.Round(e.HoursWorked.Value.TotalSeconds/60 / 60)).ToList();

        var firstWeekHours = myTimeCards.Where(e => e.ClockIn < currentPayroll.StartDate.AddDays(7).ToDateTime(TimeOnly.MinValue))
            .Select(e => Math.Round(e.HoursWorked.Value.TotalSeconds / 60 / 60,2)).ToList().Sum();

        var firstWeekOverTime = firstWeekHours - 40;

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
    private static Tuple<double, int> CalculatePayForDailyRatedEmployees(List<TimecardUsa> timecards, 
        Payroll currentPayroll, Employment emp)
    {
        var myTimeCards = timecards.Where(e => e.PersonId == emp.PersonId &&
                            e.ClockIn > currentPayroll.StartDate.ToDateTime(TimeOnly.MinValue) &&
                            e.ClockIn < currentPayroll.ScheduledRunDate.ToDateTime(TimeOnly.MaxValue));

        var totalDaysWorked = myTimeCards.Select(e => e.ClockIn.Day).Distinct().Count();

        var regularPay = Math.Round(emp.PayRate * totalDaysWorked,2);

        return Tuple.Create(regularPay, totalDaysWorked);
    }

    public Task<List<PayrollSummary>> GetPayrollSummanryByPayrollId(int payrollId)
    {
        var payroll = _context.Payroll.Find
    }
}

