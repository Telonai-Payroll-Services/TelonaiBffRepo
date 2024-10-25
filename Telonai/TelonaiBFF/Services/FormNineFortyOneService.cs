namespace TelonaiWebApi.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;

public interface IFormNineFortyOneService 
{
    Task CreateAsync();
    IList<FormNineFortyOneModel> GetCurrent941FormsAsync();
    IList<FormNineFortyOneModel> Get();
    FormNineFortyOneModel GetById(int id);
}

public class FormNineFortyOneService : IFormNineFortyOneService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly IStaticDataService _staticDataService;
    private readonly IPayrollService _payrollService;

    public FormNineFortyOneService(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public IList<FormNineFortyOneModel> Get()
    {
        var obj = _context.FormNineFortyOne
            .Include(e => e.Company).ToList();
        var result=_mapper.Map<IList<FormNineFortyOneModel>>(obj);
        return result;
    }

    public IList<FormNineFortyOneModel> GetCurrent941FormsAsync()
    {
        var quarter = GetPreviousQuarter();
        var obj = _context.FormNineFortyOne.Where(e => e.Year == quarter.Item2.Year && e.QuarterTypeId == quarter.Item1)
            .Include(e => e.Company).ToList();
        var result = _mapper.Map<IList<FormNineFortyOneModel>>(obj);
        return result;
    }

    public FormNineFortyOneModel GetById(int id)
    {
        var obj = GetFormNineFortyOne(id);
        var result = _mapper.Map<FormNineFortyOneModel>(obj);
        return result;
    }


    public async Task CreateAsync()
    {
        var previousQuarter = GetPreviousQuarter();
        var startDate = previousQuarter.Item2;
        var endDate = previousQuarter.Item3;

        var groupedPayrolls = _context.IncomeTax.Include(e => e.PayStub).ThenInclude(e => e.Payroll).ThenInclude(e => e.Company)
            .Where(e => e.PayStub.Payroll.ScheduledRunDate >= startDate && e.PayStub.Payroll.ScheduledRunDate <= endDate)
            .GroupBy(e => e.PayStub.Payroll.CompanyId).ToList();

        var Form941s = groupedPayrolls.Select(e => {
            var companyId = e.Key;
            var depositSchedule = _context.DepositSchedule.OrderByDescending(e => e.EffectiveDate).FirstOrDefault();
            var companyFields = _context.CompanySpecificFieldValue.OrderByDescending(e => e.EffectiveDate).Include(e => e.CompanySpecificField)
            .GroupBy(e => e.CompanySpecificField).Select(group => new KeyValuePair<string, string>(group.Key.FieldName, group.First().FieldValue)).ToList();

            var distinctPayrolls = e.Select(e => e.PayStub.Payroll).Distinct();

            return new FormNineFortyOne
            {
                CompanyId = e.Key,
                DepositScheduleTypeId = depositSchedule.DepositScheduleTypeId,
                QuarterTypeId = previousQuarter.Item1,
                Year = previousQuarter.Item2.Year,
                BusinessIsClosed = bool.Parse(companyFields.FirstOrDefault(e => e.Key == "BusinessIsClosed").Value ?? "false"),
                BusinessStoppedPayingWages = bool.Parse(companyFields.FirstOrDefault(e => e.Key == "BusinessStoppedPayingWages").Value ?? "false"),
                HasThirdPartyDesignee = bool.Parse(companyFields.FirstOrDefault(e => e.Key == "HasThirdPartyDesignee").Value ?? "false"),
                IsSeasonalBusiness = bool.Parse(companyFields.FirstOrDefault(e => e.Key == "IsSeasonalBusiness").Value ?? "false"),
                NotSubjectToSocialSecAndMediTax = bool.Parse(companyFields.FirstOrDefault(e => e.Key == "NotSubjectToSocialSecAndMediTax").Value ?? "false"),
                FederalIncomeTaxWithheld = e.Where(e => e.IncomeTaxType.Name == "Federal Tax").Sum(e => e.Amount),
                NumberOfEmployees = e.Select(e => e.PayStub.EmploymentId).Distinct().Count(),
                BalanceDue = distinctPayrolls.Sum(e => e.FederalOwed).Value - distinctPayrolls.Sum(e => e.FederalPaid).Value,
                Overpayment = distinctPayrolls.Sum(e => e.FederalPaid).Value - distinctPayrolls.Sum(e => e.FederalOwed).Value,
                WagesTipsCompensation = e.Sum(e => e.PayStub.GrossPay),
                WagesAndTipsSubjectToAdditionalTax = e.Where(e => e.IncomeTaxType.Name == "Additional Medicare").Sum(e => e.Amount)

                //TO DO: Populate more fields here tomorrow
            };
        }).ToList();

    }

    private FormNineFortyOne GetFormNineFortyOne(int id)
    {
        return _context.FormNineFortyOne.Find(id);
    }

    private static Tuple<int,DateOnly,DateOnly> GetPreviousQuarter()
    {
        int currentMonth = DateTime.Now.Month;
        int currentYear = DateTime.Now.Year;
        int previousYear = currentYear - 1;

        if (currentMonth >= 1 && currentMonth <= 3)
            return new Tuple<int, DateOnly, DateOnly>(
                4,
                DateOnly.Parse($"{previousYear.ToString()}-10-01"),
                DateOnly.Parse($"{previousYear.ToString()}-12-31")
                );
        else if (currentMonth >= 4 && currentMonth <= 6)
            return new Tuple<int, DateOnly, DateOnly>(
                1,
                DateOnly.Parse($"{currentYear.ToString()}-01-01"),
                DateOnly.Parse($"{currentYear.ToString()}-03-31")
                );
        else if (currentMonth >= 7 && currentMonth <= 9)
            return new Tuple<int, DateOnly, DateOnly>(
                2,
                DateOnly.Parse($"{currentYear.ToString()}-04-01"),
                DateOnly.Parse($"{currentYear.ToString()}-06-30")
                );
        else
            return new Tuple<int, DateOnly, DateOnly>(
                3,
                DateOnly.Parse($"{currentYear.ToString()}-07-01"),
                DateOnly.Parse($"{currentYear.ToString()}-09-30")
                );
    }
}