namespace TelonaiWebApi.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using static System.Net.Mime.MediaTypeNames;

/// <summary>
/// Generates Employer’s QUARTERLY Federal Tax Return
/// </summary>
public interface IFormNineFortyOneService 
{
    Task CreateAsync();
    IList<FormNineFortyOneModel> GetCurrent941FormsAsync();
    IList<FormNineFortyOneModel> GetPrevious941FormsAsync(int year, int quarter);
    IList<FormNineFortyOneModel> Get();
    FormNineFortyOneModel GetById(int id);
}

public class FormNineFortyOneService : IFormNineFortyOneService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly IStaticDataService _staticDataService;
    private readonly IPayrollService _payrollService;
    private readonly ILogger<FormNineFortyOneService> _logger;

    public FormNineFortyOneService(DataContext context, IMapper mapper, ILogger<FormNineFortyOneService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public IList<FormNineFortyOneModel> Get()
    {
        var obj = _context.FormNineFortyOne
            .Include(e => e.Company).ToList();
        var result = _mapper.Map<IList<FormNineFortyOneModel>>(obj);
        return result;
    }

    public IList<FormNineFortyOneModel> GetCurrent941FormsAsync()
    {
        var quarter = GetPreviousQuarter();
        //var obj = _context.FormNineFortyOne.Where(e => e.Year == quarter.Item2.Year && e.QuarterTypeId == quarter.Item1)
            //.Include(e => e.Company).ToList();
        var irs941 = GenerateDummyData();
        var result = _mapper.Map<IList<FormNineFortyOneModel>>(irs941);
        return result;
    }

    public IList<FormNineFortyOneModel> GetPrevious941FormsAsync(int year, int quarter)
    {
        var obj = _context.FormNineFortyOne.Where(e => e.Year == year && e.QuarterTypeId == quarter)
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
        var telonaiFields = _context.TelonaiSpecificFieldValue.OrderByDescending(e => e.EffectiveDate).Include(e => e.TelonaiSpecificField)
            .ToList();

        var previousQuarter = GetPreviousQuarter();
        var startDate = previousQuarter.Item2;
        var endDate = previousQuarter.Item3;
        var twoPreviousQuarter = previousQuarter.Item1 == 4 ? Tuple.Create(1, previousQuarter.Item2.Year - 1) :
             Tuple.Create(previousQuarter.Item1 - 1, previousQuarter.Item2.Year);

        var previous941s = _context.FormNineFortyOne.Where(e => e.Year == twoPreviousQuarter.Item2 &&
            e.QuarterTypeId == twoPreviousQuarter.Item1);

        var groupedPayrolls = _context.IncomeTax.Include(e => e.PayStub).ThenInclude(e => e.Payroll).ThenInclude(e => e.Company)
            .Where(e => e.PayStub.Payroll.ScheduledRunDate >= startDate && e.PayStub.Payroll.ScheduledRunDate <= endDate)
            .GroupBy(e => e.PayStub.Payroll.CompanyId).ToList();

        groupedPayrolls.ForEach(async e =>
        {
            try
            {
                var companyId = e.Key;

                //only generate 941 if it doesn't already exist for the quarter
                var current941 = _context.FormNineFortyOne.Where(e => e.CompanyId == companyId &&
                    e.Year == previousQuarter.Item2.Year && e.QuarterTypeId == previousQuarter.Item1);

                if (current941 != null)
                    return;

                var previous941 = previous941s.FirstOrDefault(e => e.CompanyId == companyId);

                var depositSchedule = _context.DepositSchedule.OrderByDescending(e => e.EffectiveDate).FirstOrDefault(e => e.CompanyId == companyId);
                var depositScheduleType = (DepositScheduleTypeModel)depositSchedule.DepositScheduleTypeId;
                var previousDepositScheduleType = previous941.DepositScheduleTypeId;
                var previousQuarterTax = previous941.TotalTaxAfterAdjustmentsCredits;

                var companyFields = _context.CompanySpecificFieldValue.OrderByDescending(e => e.EffectiveDate).Include(e => e.CompanySpecificField)
                .Where(e => e.CompanyId == companyId).ToList();

                var incomeTaxRates = _context.IncomeTaxRate.ToList();
                var incomeTaxes = e.Where(e => !e.PayStub.IsCancelled).ToList();
                var allPayStubsThisQuarter = incomeTaxes.Select(e => e.PayStub).ToList();
                var allPayrollsThisQuarter = allPayStubsThisQuarter.Select(e => e.Payroll).Distinct().ToList();
                var grossPayThisQuarter = allPayStubsThisQuarter.Sum(e => e.GrossPay);

                var calculatedSocialAndMedicareTax = e.Where(e => e.IncomeTaxType.ForEmployee &&
                    (e.IncomeTaxType.Name == "Social Security" ||
                    e.IncomeTaxType.Name == "Medicare" ||
                    e.IncomeTaxType.Name == "Additional Medicare"))
                    .Sum(e => e.Amount);

                var federalIncomeTaxWithheld = incomeTaxes.Where(e => e.IncomeTaxType.Name == "Federal Tax").Sum(e => e.Amount);
                var taxableSocialSecurityWages = allPayStubsThisQuarter.Sum(e => e.RegularPay + e.OverTimePay);
                var taxableSocialSecurityTips = allPayStubsThisQuarter.Select(e => e.OtherMoneyReceived).Sum(e => e.OtherPay + e.CashTips + e.CreditCardTips);
                var taxableMedicareWagesAndTips = taxableSocialSecurityWages + taxableSocialSecurityTips;
                var wagesAndTipsSubjectToAdditionalTax = allPayStubsThisQuarter.Sum(e => e.AmountSubjectToAdditionalMedicareTax);
                var unreportedTipsTaxDue = incomeTaxes.Where(e => e.IncomeTaxType.Name == "Unreported Tips Tax Due").Sum(e => e.Amount);
                var taxableSocialSecurityWagesTax = taxableSocialSecurityWages * incomeTaxRates.First(e => e.IncomeTaxType.Name == "Social Security").Rate;
                var taxableSocialSecurityTipsTax = taxableSocialSecurityTips * incomeTaxRates.First(e => e.IncomeTaxType.Name == "Social Security").Rate;
                var taxableMedicareWagesAndTipsTax = taxableMedicareWagesAndTips * incomeTaxRates.First(e => e.IncomeTaxType.Name == "Medicare").Rate;
                var wagesAndTipsSubjectToAdditionalTaxTax = wagesAndTipsSubjectToAdditionalTax * incomeTaxRates.First(e => e.IncomeTaxType.Name == "Additional Medicare").Rate;
                var totalSocialAndMediTax = taxableSocialSecurityWagesTax+ taxableSocialSecurityTipsTax + taxableMedicareWagesAndTipsTax + wagesAndTipsSubjectToAdditionalTaxTax;

                var totalTaxBeforeAdjustment = 0.0;
                var totalTaxAfterAdjustment = 0.0;
                var totalTaxAfterAdjustmentsAndCredits = 0.0;
                var creditsForResearch = 0.0;
                var totalDeposit = 0.0;
                var overPayment = 0.0;

                var adjust1 = 0.0;
                var adjust2 = incomeTaxes.Where(e => e.IncomeTaxType.Name == "Sick Pay Amount Paid By Third Party").Sum(e => e.Amount);
                var adjust3 = incomeTaxes.Where(e => e.IncomeTaxType.Name == "Adjustment for Tips and Life Insurance").Sum(e => e.Amount);

                var hasOneHundredThousandNextDayObligation = bool.Parse(companyFields.FirstOrDefault(e => e.CompanySpecificField.FieldName
                == "HasOneHundredThousandUsdNextDayObligation").FieldValue ?? "false");

                var form941 = new FormNineFortyOne
                {
                    CompanyId = e.Key,
                    DepositScheduleTypeId = depositSchedule.DepositScheduleTypeId,
                    QuarterTypeId = previousQuarter.Item1,
                    Year = previousQuarter.Item2.Year,

                    //1 to 4
                    NumberOfEmployees = incomeTaxes.Select(e => e.PayStub.EmploymentId).Distinct().Count(),
                    WagesTipsCompensation = grossPayThisQuarter,
                    FederalIncomeTaxWithheld = federalIncomeTaxWithheld,
                    NotSubjectToSocialSecAndMediTax = bool.Parse(companyFields.FirstOrDefault(e => e.CompanySpecificField.FieldName ==
                    "NotSubjectToSocialSecAndMediTax").FieldValue ?? "false"),

                    //5a to 5e
                    TaxableSocialSecurityWages = taxableSocialSecurityWages,
                    TaxableSocialSecurityTips = taxableSocialSecurityTips,
                    TaxableMedicareWagesAndTips = taxableMedicareWagesAndTips,
                    WagesAndTipsSubjectToAdditionalTax = wagesAndTipsSubjectToAdditionalTax,

                    TaxableSocialSecurityWagesTax = taxableSocialSecurityWagesTax,
                    TaxableSocialSecurityTipsTax =  taxableSocialSecurityTipsTax,
                    TaxableMedicareWagesAndTipsTax = taxableMedicareWagesAndTipsTax,
                    WagesAndTipsSubjectToAdditionalTaxTax = wagesAndTipsSubjectToAdditionalTaxTax,

                    TotalSocialAndMediTax = totalSocialAndMediTax,
                    //5f to 6
                    UnreportedTipsTaxDue = unreportedTipsTaxDue,
                    TotalTaxBeforeAdjustment = totalTaxBeforeAdjustment = federalIncomeTaxWithheld + totalSocialAndMediTax + unreportedTipsTaxDue,

                    //7 to 10
                    AdjustForFractionsOfCents = adjust1 = calculatedSocialAndMedicareTax - totalSocialAndMediTax,
                    AdjustForSickPay = adjust2,
                    AdjustForTipsAndLifeInsurance = adjust3,
                    TotalTaxAfterAdjustment = totalTaxAfterAdjustment = totalTaxBeforeAdjustment + adjust1 + adjust2 + adjust3,

                    //11 to 12
                    TaxCreditForResearchActivities = creditsForResearch = incomeTaxes.Where(e => e.IncomeTaxType.Name == "TaxCreditForIncreasingResearchActivities").Sum(e => e.Amount),
                    TotalTaxAfterAdjustmentsCredits = totalTaxAfterAdjustmentsAndCredits = totalTaxAfterAdjustment - creditsForResearch,

                    //13 to 15
                    TotalDeposit = totalDeposit = allPayrollsThisQuarter.Sum(e => e.FederalPaid.Value),
                    BalanceDue = Math.Max(totalTaxAfterAdjustmentsAndCredits - totalDeposit, 0.0),
                    Overpayment = overPayment = Math.Max(totalDeposit - totalTaxAfterAdjustmentsAndCredits, 0.0),
                    ApplyOverpaymentToNextReturn = overPayment > 0 ? true : false,

                    //16 to 18
                    CheckedBoxSixteenTypeId = totalTaxAfterAdjustmentsAndCredits < 2500 ||
                        (previousQuarterTax < 2500 && totalTaxAfterAdjustmentsAndCredits < 100000 && !hasOneHundredThousandNextDayObligation) ?
                        (int)CheckedBoxSixteenTypeModel.TaxDueLessThan2500 :
                        depositScheduleType == DepositScheduleTypeModel.SemiWeekly ? (int)CheckedBoxSixteenTypeModel.IsSemiWeeklyDepositor :
                        (int)CheckedBoxSixteenTypeModel.IsMonthlyDepositor,

                    BusinessIsClosed = bool.Parse(companyFields.FirstOrDefault(e => e.CompanySpecificField.FieldName == "BusinessIsClosed").FieldValue ?? "false"),
                    BusinessStoppedPayingWages = bool.Parse(companyFields.FirstOrDefault(e => e.CompanySpecificField.FieldName == "BusinessStoppedPayingWages").FieldValue ?? "false"),
                    FinalDateWagesPaid = DateOnly.Parse(companyFields.FirstOrDefault(e => e.CompanySpecificField.FieldName == "FinalDateWagesPaid").FieldValue),
                    IsSeasonalBusiness = bool.Parse(companyFields.FirstOrDefault(e => e.CompanySpecificField.FieldName == "IsSeasonalBusiness").FieldValue ?? "false"),

                    //Signature
                    //Note: We will take form 8879-EMP from our customers and sign the submission ourselves using self generated pin
                    HasThirdPartyDesignee = bool.Parse(companyFields.FirstOrDefault(e => e.CompanySpecificField.FieldName == "HasThirdPartyDesignee").FieldValue ?? "false"),
                    ThirdPartyFiveDigitPin = int.Parse(telonaiFields.FirstOrDefault(e => e.TelonaiSpecificField.FieldName == "SelfGeneratedFiveDigitPin").FieldValue)

                };

                _context.FormNineFortyOne.Add(form941);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Generating Form 941 Failed. " + ex.ToString());
                return;
            }
        });

    }

    private FormNineFortyOne GetFormNineFortyOne(int id)
    {
        return _context.FormNineFortyOne.Find(id);
    }

    private static Tuple<int, DateOnly, DateOnly> GetPreviousQuarter()
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

    private List<FormNineFortyOne> GenerateDummyData()
    {
        var irs941 = new FormNineFortyOne()
        {
            Id = 1,
            NumberOfEmployees = 4,
            WagesTipsCompensation = 400,
            FederalIncomeTaxWithheld = 10000,
            NotSubjectToSocialSecAndMediTax = true,
            TaxableSocialSecurityTips = 1200,
            TaxableSocialSecurityWages = 400,
            TaxableMedicareWagesAndTips = 2000,
            WagesAndTipsSubjectToAdditionalTax = 130,
            TotalSocialAndMediTax = 3500,
            UnreportedTipsTaxDue = 1256,
            TotalTaxBeforeAdjustment = 30000,
            AdjustForFractionsOfCents = 4500,
            AdjustForSickPay = 1245,
            AdjustForTipsAndLifeInsurance = 799,
            TotalTaxAfterAdjustment = 6300,
            TotalTaxAfterAdjustmentsCredits = 2390,
            TotalDeposit = 3234,
            BalanceDue = 15000,
            Overpayment = 1000,
            ApplyOverpaymentToNextReturn = true,
            TaxLiabilityMonthOne = 9000,
            TaxLiabilityMonthTwo = 4000,
            TaxLiabilityMonthThree = 7500,
            TotalLiabilityForQuarter = 21500,
            DepositScheduleTypeId = 4500,
            BusinessIsClosed = false,
            BusinessStoppedPayingWages = false,
            FinalDateWagesPaid = DateOnly.FromDateTime(DateTime.Now),
            IsSeasonalBusiness = false,
            HasThirdPartyDesignee = true,
            CheckedBoxSixteenTypeId = 1,
            ThirdPartyDesigneeName = "Telonai",
            ThirdPartyDesigneePhone = "0921739313",
            ThirdPartyFiveDigitPin = 1234,
            Signature = "Biras",
            SignatureDate = DateOnly.FromDateTime(DateTime.Now),
            SignerName = "Biruk Assefa",
            SignerTitle = "Cheif Accountant",
            SignerBestDayTimePhone = TimeOnly.FromDateTime(DateTime.Now),
            CompanyId = 12,
            QuarterTypeId = 1,
            Year = 2024,
            TaxCreditForResearchActivities = 2000
        };
        List<FormNineFortyOne> irs941List = new List<FormNineFortyOne>();
        irs941List.Add(irs941);
        return irs941List;
    }
}