namespace TelonaiWebApi.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using static System.Net.Mime.MediaTypeNames;

/// <summary>
/// Generates  Form 944:Employer’s ANNUAL Federal Tax Return.  
/// </summary>
public interface IFormNineFortyFourService 
{
    Task CreateAsync();
    IList<FormNineFortyFourModel> GetCurrent944FormsAsync();
    IList<FormNineFortyFourModel> GetPrevious944FormsAsync(int year);
    IList<FormNineFortyFourModel> Get();
    FormNineFortyFourModel GetById(int id);
}

public class FormNineFortyFourService : IFormNineFortyFourService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly IStaticDataService _staticDataService;
    private readonly IPayrollService _payrollService;
    private readonly ILogger<FormNineFortyOneService> _logger;

    public FormNineFortyFourService(DataContext context, IMapper mapper, ILogger<FormNineFortyOneService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public IList<FormNineFortyFourModel> Get()
    {
        var obj = _context.FormNineFortyFour
            .Include(e => e.Company).ToList();
        var result = _mapper.Map<IList<FormNineFortyFourModel>>(obj);
        return result;
    }

    public IList<FormNineFortyFourModel> GetCurrent944FormsAsync()
    {
        var obj = _context.FormNineFortyFour.Where(e => e.Year == DateTime.Now.Year)
            .Include(e => e.Company).ToList();
        var result = _mapper.Map<IList<FormNineFortyFourModel>>(obj);
        return result;
    }

    public IList<FormNineFortyFourModel> GetPrevious944FormsAsync(int year)
    {
        var obj = _context.FormNineFortyFour.Where(e => e.Year == year)
            .Include(e => e.Company).ToList();
        var result = _mapper.Map<IList<FormNineFortyFourModel>>(obj);
        return result;
    }

    public FormNineFortyFourModel GetById(int id)
    {
        var obj = GetFormNineFortyFour(id);
        var result = _mapper.Map<FormNineFortyFourModel>(obj);
        return result;
    }


    public async Task CreateAsync()
    {
        var year = DateTime.Now.Year - 1;
        var startDate = new DateOnly(year, 1, 1);
        var endDate = new DateOnly(year - 1, 12, 31);

        var telonaiSpecificFields = _context.TelonaiSpecificFieldValue.Where(e => e.EffectiveYear == year)
            .Include(e => e.TelonaiSpecificField).ToList();

        var groupedPayrolls = _context.IncomeTax.Include(e => e.PayStub).ThenInclude(e => e.Payroll).ThenInclude(e => e.Company)
            .Where(e => e.PayStub.Payroll.ScheduledRunDate >= startDate && e.PayStub.Payroll.ScheduledRunDate <= endDate)
            .GroupBy(e => e.PayStub.Payroll.CompanyId).ToList();

        groupedPayrolls.ForEach(async e =>
        {
            try
            {
                var companyId = e.Key;

                //only generate 944 if it doesn't already exist for the year
                var current944 = _context.FormNineFortyOne.Where(e => e.CompanyId == companyId && e.Year == year);

                if (current944 != null)
                    return;

                var companyFields = _context.CompanySpecificFieldValue.OrderByDescending(e => e.EffectiveDate).Include(e => e.CompanySpecificField)
                .Where(e => e.CompanyId == companyId).ToList();

                var incomeTaxRates = _context.IncomeTaxRate.ToList();
                var incomeTaxes = e.Where(e => !e.PayStub.IsCancelled).ToList();
                var allPayStubsThisYear = incomeTaxes.Select(e => e.PayStub).ToList();
                var allPayrollsThisYear = allPayStubsThisYear.Select(e => e.Payroll).Distinct().ToList();
                var grossPayThisYear = allPayStubsThisYear.Sum(e => e.GrossPay);

                var federalIncomeTaxWithheld = incomeTaxes.Where(e => e.IncomeTaxType.Name == "Federal Tax").Sum(e => e.Amount);
                var sickLeaveWages = allPayStubsThisYear.Where(e => e.OtherMoneyReceived.Note == "Qualified Sick Leave Wages").Sum(e => e.OtherMoneyReceived.OtherPay);
                var sickLeaveWagesTax = sickLeaveWages * double.Parse(telonaiSpecificFields.FirstOrDefault(e => e.TelonaiSpecificField.FieldName ==  "SickLeaveWagesTaxRate").FieldValue);
                
                var familyLeaveWages = allPayStubsThisYear.Where(e => e.OtherMoneyReceived.Note == "Qualified Family Leave Wages").Sum(e => e.OtherMoneyReceived.OtherPay);
                var familyLeaveWagesTax = familyLeaveWages * double.Parse(telonaiSpecificFields.FirstOrDefault(e => e.TelonaiSpecificField.FieldName ==  "FamilyLeaveWagesTaxRate").FieldValue);
                
                var nonRefundSickLeaveWages1 = allPayStubsThisYear.Where(e => e.OtherMoneyReceived.Note == "Non Refundable Credit For Sick Leave Wages 1").Sum(e => e.OtherMoneyReceived.OtherPay);
                var nonRefundFamilyLeaveWages1 = allPayStubsThisYear.Where(e => e.OtherMoneyReceived.Note == "Non Refundable Credit For Family Leave Wages 1").Sum(e => e.OtherMoneyReceived.OtherPay);

                var nonRefundSickLeaveWages2 = allPayStubsThisYear.Where(e => e.OtherMoneyReceived.Note == "Non Refundable Credit For Sick Leave Wages 2").Sum(e => e.OtherMoneyReceived.OtherPay);
                var nonRefundFamilyLeaveWages2 = allPayStubsThisYear.Where(e => e.OtherMoneyReceived.Note == "Non Refundable Credit For Family Leave Wages 2").Sum(e => e.OtherMoneyReceived.OtherPay);
                
                var refundableSickLeaveWages1 = allPayStubsThisYear.Where(e => e.OtherMoneyReceived.Note == "Refundable Credit For Sick Leave Wages 1").Sum(e => e.OtherMoneyReceived.OtherPay);
                var refundableFamilyLeaveWages1 = allPayStubsThisYear.Where(e => e.OtherMoneyReceived.Note == "Refundable Credit For Family Leave Wages 1").Sum(e => e.OtherMoneyReceived.OtherPay);

                var refundableSickLeaveWages2 = allPayStubsThisYear.Where(e => e.OtherMoneyReceived.Note == "Refundable Credit For Sick Leave Wages 2").Sum(e => e.OtherMoneyReceived.OtherPay);
                var refundableFamilyLeaveWages2 = allPayStubsThisYear.Where(e => e.OtherMoneyReceived.Note == "Refundable Credit For Family Leave Wages 2").Sum(e => e.OtherMoneyReceived.OtherPay);

                var taxableSocialSecurityWages = allPayStubsThisYear.Sum(e => e.RegularPay + e.OverTimePay);
                
                var taxableSocialSecurityTips = allPayStubsThisYear.Select(e => e.OtherMoneyReceived).Sum(e => e.OtherPay + e.CashTips + e.CreditCardTips);
                var taxableMedicareWagesAndTips = taxableSocialSecurityWages + taxableSocialSecurityTips;
                var wagesAndTipsSubjectToAdditionalTax = allPayStubsThisYear.Sum(e => e.AmountSubjectToAdditionalMedicareTax);

                var taxableSocialSecurityWagesTax = taxableSocialSecurityWages * incomeTaxRates.First(e => e.IncomeTaxType.Name == "Social Security").Rate;
                var taxableSocialSecurityTipsTax = taxableSocialSecurityTips * incomeTaxRates.First(e => e.IncomeTaxType.Name == "Social Security").Rate;
                var taxableMedicareWagesAndTipsTax = taxableMedicareWagesAndTips * incomeTaxRates.First(e => e.IncomeTaxType.Name == "Medicare").Rate;
                var wagesAndTipsSubjectToAdditionalTaxTax = wagesAndTipsSubjectToAdditionalTax * incomeTaxRates.First(e => e.IncomeTaxType.Name == "Additional Medicare").Rate;
                var totalSocialAndMediTax = taxableSocialSecurityWagesTax + taxableSocialSecurityTipsTax + taxableMedicareWagesAndTipsTax + wagesAndTipsSubjectToAdditionalTaxTax;

                var calculatedSocialAndMedicareTax = e.Where(e => e.IncomeTaxType.ForEmployee &&
                    (e.IncomeTaxType.Name == "Social Security" ||
                    e.IncomeTaxType.Name == "Medicare" ||
                    e.IncomeTaxType.Name == "Additional Medicare"))
                    .Sum(e => e.Amount);

                var adjustForSickPay = incomeTaxes.Where(e => e.IncomeTaxType.Name == "Sick Pay Amount Paid By Third Party").Sum(e => e.Amount);
                var adjustForTipsAndLifeInsurance = incomeTaxes.Where(e => e.IncomeTaxType.Name == "Adjustment for Tips and Life Insurance").Sum(e => e.Amount);
                var adjustForFractionsOfCents =  calculatedSocialAndMedicareTax - totalSocialAndMediTax;
                var currentYearAdjustments = adjustForFractionsOfCents + adjustForSickPay + adjustForTipsAndLifeInsurance;

                var taxLiabilityEachMonth = new List<double>();
                for (int i = 1; i < 13; i++)
                {
                    taxLiabilityEachMonth.Add(incomeTaxes.Where(e => e.PayStub.Payroll.ScheduledRunDate.Month == i)
                        .Select(e => e.PayStub.Payroll).Distinct().Sum(e => e.FederalOwed.Value));                    
                }  

                var totalDepositsAndRefundableCredits = 0.0;
                var totalNonRefundableCredit = 0.0;
                var totalTaxBeforeAdjustment = 0.0;
                var totalTaxAfterAdjustment = 0.0;
                var totalTaxAfterAdjustmentsAndCredits = 0.0;
                var creditsForResearch = 0.0;
                var totalDeposit = 0.0;
                var overPayment = 0.0;

                var form944 = new FormNineFortyFour
                {
                    CompanyId = e.Key,
                    Year = year,

                    //1 to 3
                    WagesTipsCompensation = grossPayThisYear,
                    FederalIncomeTaxWithheld = federalIncomeTaxWithheld,
                    NotSubjectToSocialSecAndMediTax = bool.Parse(companyFields.FirstOrDefault(e => e.CompanySpecificField.FieldName ==
                    "NotSubjectToSocialSecAndMediTax").FieldValue ?? "false"),

                    //4a to 4e
                    TaxableSocialSecurityWages = taxableSocialSecurityWages,
                    QualifiedSickLeaveWages = sickLeaveWages,
                    QualifiedFamilyLeaveWages = familyLeaveWages,
                    TaxableSocialSecurityTips = taxableSocialSecurityTips,
                    TaxableMedicareWagesAndTips = taxableMedicareWagesAndTips,

                    WagesAndTipsSubjectToAdditionalTax = wagesAndTipsSubjectToAdditionalTax,
                    QualifiedSickLeaveWagesTax = sickLeaveWagesTax,
                    QualifiedFamilyLeaveWagesTax = familyLeaveWagesTax,
                    TaxableSocialSecurityTipsTax = taxableSocialSecurityTipsTax,
                    TaxableMedicareWagesAndTipsTax = taxableMedicareWagesAndTipsTax,
                    WagesAndTipsSubjectToAdditionalTaxTax = wagesAndTipsSubjectToAdditionalTaxTax,
                    TotalSocialAndMediTax = totalSocialAndMediTax,

                    //5 to 7
                    TotalTaxBeforeAdjustment = totalTaxBeforeAdjustment = federalIncomeTaxWithheld + totalSocialAndMediTax,
                    CurrentYearAddjustments = currentYearAdjustments,
                    TotalTaxAfterAdjustment = totalTaxAfterAdjustment = totalTaxBeforeAdjustment - currentYearAdjustments,

                    //8a to 8g
                    TaxCreditForResearchActivities = creditsForResearch = incomeTaxes.Where(e => e.IncomeTaxType.Name == "TaxCreditForIncreasingResearchActivities").Sum(e => e.Amount),
                    NonRefundCreditForSickAndFamilyLeave1 = nonRefundFamilyLeaveWages1 + nonRefundSickLeaveWages1,
                    NonRefundCreditForSickAndFamilyLeave2 = nonRefundFamilyLeaveWages2 + nonRefundSickLeaveWages2,
                    TotalNonRefundableCredit = totalNonRefundableCredit = nonRefundFamilyLeaveWages1 + nonRefundSickLeaveWages1 + nonRefundFamilyLeaveWages2 + nonRefundSickLeaveWages2,


                    //9 to 10j
                    TotalTaxAfterAdjustmentsOfCredits = totalTaxAfterAdjustmentsAndCredits = totalTaxAfterAdjustment - totalNonRefundableCredit,
                    TotalDepositForYear = totalDeposit = allPayrollsThisYear.Sum(e => e.FederalPaid.Value),
                    RefundableCreditForSickAndFamilyLeave1 = refundableSickLeaveWages1 + refundableFamilyLeaveWages1,
                    RefundableCreditForSickAndFamilyLeave2 = refundableSickLeaveWages2 + refundableFamilyLeaveWages2,

                    TotalDepositsAndRefundableCredits = totalDepositsAndRefundableCredits = totalDeposit + refundableSickLeaveWages1 +
                    refundableFamilyLeaveWages1 + refundableSickLeaveWages2 + refundableFamilyLeaveWages2,

                    //11 to 13
                    BalanceDue = Math.Max(totalTaxAfterAdjustmentsAndCredits - totalDepositsAndRefundableCredits, 0.0),
                    Overpayment = overPayment = Math.Max(totalDepositsAndRefundableCredits - totalTaxAfterAdjustmentsAndCredits, 0.0),
                    ApplyOverpaymentToNextReturn = overPayment > 0 ? true : false,
                    TotalLiabilityIsLessThan2500 = totalTaxAfterAdjustmentsAndCredits < 2500,
                    TotalLiabilityIs2500OrMore = totalTaxAfterAdjustmentsAndCredits >= 2500,
                    TaxLiabilityEachMonth = taxLiabilityEachMonth,
                    TotalLiabilityForYear = taxLiabilityEachMonth.Sum(e=>e),

                    //14 to 26             
                    BusinessIsClosed = bool.Parse(companyFields.FirstOrDefault(e => e.CompanySpecificField.FieldName == "BusinessIsClosed").FieldValue ?? "false"),
                    BusinessStoppedPayingWages = bool.Parse(companyFields.FirstOrDefault(e => e.CompanySpecificField.FieldName == "BusinessStoppedPayingWages").FieldValue ?? "false"),
                    FinalDateWagesPaid = companyFields.FirstOrDefault(e => e.CompanySpecificField.FieldName == "FinalDateWagesPaid").FieldValue,
                    HealthPlanExpenseForSickLeaveWages1 =double.Parse(companyFields.FirstOrDefault(e => e.CompanySpecificField.FieldName == 
                                         "HealthPlanExpenseForSickLeaveWages1")?.FieldValue?? "0.0"),
                    HealthPlanExpenseForFamilyLeave1 = double.Parse(companyFields.FirstOrDefault(e => e.CompanySpecificField.FieldName ==
                                        "HealthPlanExpenseForFamilyLeaveWage2")?.FieldValue ?? "0.0"),
                    HealthPlanExpenseForSickLeaveWages2 = double.Parse(companyFields.FirstOrDefault(e => e.CompanySpecificField.FieldName ==
                                        "HealthPlanExpenseForSickLeaveWages2")?.FieldValue ?? "0.0"),
                    HealthPlanExpenseForFamilyLeaveWages2 = double.Parse(companyFields.FirstOrDefault(e => e.CompanySpecificField.FieldName ==
                                        "HealthPlanExpenseForFamilyLeaveWages2")?.FieldValue ?? "0.0"),
                    CollectivelyBargainedAmountsForSickLeaveWages =double.Parse(companyFields.FirstOrDefault(e => e.CompanySpecificField.FieldName ==
                                        "CollectivelyBargainedAmountsForSickLeaveWages")?.FieldValue ?? "0.0"),

                    CollectivelyBargainedAmountsForFamilyLeaveWages = double.Parse(companyFields.FirstOrDefault(e => e.CompanySpecificField.FieldName ==
                                        "CollectivelyBargainedAmountsForFamilyLeaveWages")?.FieldValue ?? "0.0"),
                    SickLeaveWages = double.Parse(companyFields.FirstOrDefault(e => e.CompanySpecificField.FieldName ==
                                        "SickLeaveWages")?.FieldValue ?? "0.0"),
                    FamilyLeaveWages = double.Parse(companyFields.FirstOrDefault(e => e.CompanySpecificField.FieldName ==
                                        "FamilyLeaveWages")?.FieldValue ?? "0.0"),
                    
                    //Signature
                    //Note: We will take form 8879-EMP from our customers and sign the submission ourselves using self generated pin
                    HasThirdPartyDesignee = bool.Parse(companyFields.FirstOrDefault(e => e.CompanySpecificField.FieldName == "HasThirdPartyDesignee").FieldValue ?? "false"),
                    ThirdPartyFiveDigitPin = int.Parse(telonaiSpecificFields.FirstOrDefault(e => e.TelonaiSpecificField.FieldName == "SelfGeneratedFiveDigitPin").FieldValue)

                };

                _context.FormNineFortyFour.Add(form944);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Generating Form 944 Failed. " + ex.ToString());
                return;
            }
        });

    }

    private FormNineFortyFour GetFormNineFortyFour(int id)
    {
        return _context.FormNineFortyFour.Find(id);
    }
}