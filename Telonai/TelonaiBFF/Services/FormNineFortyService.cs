namespace TelonaiWebApi.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Globalization;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;

/// <summary>
/// Generates Employer’s Annual Federal Unemployment (FUTA) Tax Return
/// </summary>
public interface IFormNineFortyService 
{
    Task CreateAsync();
    IList<FormNineFortyModel> GetCurrent940FormsAsync();
    IList<FormNineFortyModel> GetPrevious940FormsAsync(int year);
    IList<FormNineFortyModel> Get();
    FormNineFortyModel GetById(int id);
    FormNineFortyModel GetTestScenario(int id);
}

public class FormNineFortyService : IFormNineFortyService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly IStaticDataService _staticDataService;
    private readonly IPayrollService _payrollService;
    private readonly ILogger<FormNineFortyService> _logger;

    public FormNineFortyService(DataContext context, IMapper mapper, ILogger<FormNineFortyService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public IList<FormNineFortyModel> Get()
    {
        var obj = _context.FormNineForty
            .Include(e => e.Company).ToList();
        var result = _mapper.Map<IList<FormNineFortyModel>>(obj);
        return result;
    }

    public IList<FormNineFortyModel> GetCurrent940FormsAsync()
    {
        var obj = _context.FormNineForty.Where(e => e.Year == DateTime.Now.Year)
            .Include(e => e.Company).ToList();
        var result = _mapper.Map<IList<FormNineFortyModel>>(obj);
        return result;
    }

    public IList<FormNineFortyModel> GetPrevious940FormsAsync(int year)
    {
        var obj = _context.FormNineForty.Where(e => e.Year == year)
            .Include(e => e.Company).ToList();
        var result = _mapper.Map<IList<FormNineFortyModel>>(obj);
        return result;
    }

    public FormNineFortyModel GetById(int id)
    {
        var obj = GetFormNineForty(id);
        var result = _mapper.Map<FormNineFortyModel>(obj);
        return result;
    }


    public async Task CreateAsync()
    {
        var year = DateTime.Now.Year - 1;
        var startDate = new DateOnly(year, 1, 1);
        var endDate = new DateOnly(year - 1, 12, 31);

        var countrySpecificFields = _context.CountrySpecificFieldValue.Where(e => e.EffectiveYear == year)
            .Include(e => e.CountrySpecificField).ToList();

        var stateSpecificFields = _context.StateSpecificFieldValue.Where(e => e.EffectiveYear == year)
            .Include(e => e.StateSpecificField).ToList();

        var groupedPayrolls = _context.IncomeTax.Include(e => e.PayStub).ThenInclude(e => e.Payroll).ThenInclude(e => e.Company)
            .Where(e => e.PayStub.Payroll.ScheduledRunDate >= startDate && e.PayStub.Payroll.ScheduledRunDate <= endDate)
            .GroupBy(e => e.PayStub.Payroll.CompanyId).ToList();

        groupedPayrolls.ForEach(async e =>
        {
            try
            {
                var companyId = e.Key;

                var current940 = _context.FormNineFortyOne.Where(e => e.CompanyId == companyId && e.Year == year);

                if (current940 != null)
                    return;

                var companySpecificFields = _context.CompanySpecificFieldValue.OrderByDescending(e => e.EffectiveDate).Include(e => e.CompanySpecificField)
                .Where(e => e.CompanyId == companyId).ToList();

                var incomeTaxRates = _context.IncomeTaxRate.ToList();
                var incomeTaxes = e.Where(e => !e.PayStub.IsCancelled).ToList();
                var allPayStubsThisYear = incomeTaxes.Select(e => e.PayStub).ToList();
                var allPayrollsThisYear = allPayStubsThisYear.Select(e => e.Payroll).Distinct().ToList();
                var grossPayThisYear = allPayStubsThisYear.Sum(e => e.GrossPay);
                var additionalMoneyReceivedIds = allPayStubsThisYear.SelectMany(e => e.OtherMoneyReceived.AdditionalOtherMoneyReceivedId).ToArray();

                var additionalMoneyReceived = _context.AdditionalOtherMoneyReceived.Where(e => additionalMoneyReceivedIds.Contains(e.Id)).ToList();

                var paymentExemptFromFutaTax = additionalMoneyReceived.Where(e => e.ExemptFromFutaTaxTypeId>0);

                var statesTax = incomeTaxes.Where(e => e.IncomeTaxType.Name == "State Tax")
                .Select(e => new { State = e.IncomeTaxType.State.StateCode, TaxAmount = e.Amount, Wages = e.PayStub.GrossPay }).ToList();

                var creditReductionStates = stateSpecificFields.Where(e => e.StateSpecificField.FieldName == "StateIsCreditReductionState")
                .Select(e => e.State.Name).ToList();

                var stateTaxCreditReductionApplies = statesTax.Where(e => creditReductionStates.Contains(e.State)).ToList();

                var futaTax = incomeTaxes.Where(e => e.IncomeTaxType.Name == "FUTA");
                
                var totalPaymentsAbove7K = allPayStubsThisYear.Where(e => e.GrossPay > 7000).Sum(e => e.GrossPay - 7000);
                var paymentsExemptFromFutaTaxAmount = paymentExemptFromFutaTax.Sum(e => e.Amount);
                var subTotal = paymentsExemptFromFutaTaxAmount + totalPaymentsAbove7K;
                var totalTaxableFutaWages = grossPayThisYear - subTotal;

                var allFutaWagesWereExcludedFromStateUnemploymentTax = bool.Parse(companySpecificFields.FirstOrDefault(e => e.CompanySpecificField.FieldName == "allFutaWagesWereExcludedFromStateUnemploymentTax").FieldValue ?? "false");
                var someFutaWagesWereExcludedFromStateUnemploymentTax = bool.Parse(companySpecificFields.FirstOrDefault(e => e.CompanySpecificField.FieldName == "someFutaWagesWereExcludedFromStateUnemploymentTax").FieldValue ?? "false");
                var futaTaxIfAllExcludedFromStateUnemploymentTax = double.Parse(countrySpecificFields.FirstOrDefault(e => e.CountrySpecificField.FieldName == "FUTATaxRateIfAllExcludedFromStateUnemploymentTax").FieldValue);
                var futaTaxIfSomeExcludedFromStateUnemploymentTax = double.Parse(countrySpecificFields.FirstOrDefault(e => e.CountrySpecificField.FieldName == "FUTATaxRateIfSomeExcludedFromStateUnemploymentTax").FieldValue);

                var futaTaxBeforeAdjust = 0.0;
                var adjustIfAllExcludedFromStateUnemploymentTax = 0.0;
                var adjustIfSomeExcludedFromStateUnemploymentTax = 0.0;
                var creditReductionAmount = 0.0;
                var totalFutaTaxDeposited = 0.0;
                var totalFutaTaxAfterAdjust = 0.0;
                var overPayment = 0.0;
                var firstQuarterEndDate = new DateOnly(year, 3, 31);
                var secondQuarterEndDate = new DateOnly(year, 6, 30);
                var thirdQuarterEndDate = new DateOnly(year, 9, 30);

                var form940 = new FormNineForty
                {
                    CompanyId = e.Key,
                    Year = year,
                    //1 to 2
                    FormNineFortyTypeOfReturn = (companySpecificFields.FirstOrDefault(e => e.CompanySpecificField.FieldName == "FormNineFortyTypeOfReturn").FieldValue ?? "0").Split(",").Select(e => int.Parse(e)).ToList(),
                    InvolvedStates = (companySpecificFields.FirstOrDefault(e => e.CompanySpecificField.FieldName == "InvolvedStates").FieldValue ?? "NC").Split(",").ToList(),
                    PaysMultiStateUnemploymentTax = bool.Parse(companySpecificFields.FirstOrDefault(e => e.CompanySpecificField.FieldName == "PaysMultiStateUnemploymentTax").FieldValue ?? "false"),
                    PaidWagesInCreditReductionState = stateTaxCreditReductionApplies != null && stateTaxCreditReductionApplies.Count > 0,
                    //TO DO: In the above case, where multi state and credit reduction apply, we need to create Schedule A (Form 940)

                    //3 to 4e
                    TotalPaymentsToAllEmployees = grossPayThisYear,
                    PaymentsExemptFromFutaTax = paymentsExemptFromFutaTaxAmount,
                    ExemptFromFutaFringeBenefits = paymentExemptFromFutaTax.Any(e => e.ExemptFromFutaTaxTypeId == (int)ExemptFromFutaTaxTypeModel.FringeBenefits),
                    ExemptFromFutaGroupTermLifeInsurance = paymentExemptFromFutaTax.Any(e => e.ExemptFromFutaTaxTypeId == (int)ExemptFromFutaTaxTypeModel.GroupTermLifeInsurance),
                    ExemptFromFutaRetirementOrPension = paymentExemptFromFutaTax.Any(e => e.ExemptFromFutaTaxTypeId == (int)ExemptFromFutaTaxTypeModel.RetirementOrPension),
                    ExemptFromFutaDependentCare = paymentExemptFromFutaTax.Any(e => e.ExemptFromFutaTaxTypeId == (int)ExemptFromFutaTaxTypeModel.DependentCare),
                    ExemptFromFutaOther = paymentExemptFromFutaTax.Any(e => e.ExemptFromFutaTaxTypeId == (int)ExemptFromFutaTaxTypeModel.Other),

                    //5f to 8
                    TotalPaymentsAbove7K = totalPaymentsAbove7K,
                    SubTotal = subTotal,
                    TotalTaxableFutaWages = totalTaxableFutaWages,
                    FutaTaxBeforeAdjust = futaTaxBeforeAdjust = totalTaxableFutaWages * double.Parse(countrySpecificFields.FirstOrDefault(e => e.CountrySpecificField.FieldName == year.ToString() + "FUTATaxRate").FieldValue),

                    
                    //9 to 11
                    AdjustIfAllExcludedFromStateUnemploymentTax = adjustIfAllExcludedFromStateUnemploymentTax =
                        allFutaWagesWereExcludedFromStateUnemploymentTax ? totalTaxableFutaWages * futaTaxIfAllExcludedFromStateUnemploymentTax : 0,
                    AdjustIfSomeExcludedFromStateUnemploymentTax = adjustIfSomeExcludedFromStateUnemploymentTax =
                        someFutaWagesWereExcludedFromStateUnemploymentTax ? totalTaxableFutaWages * futaTaxIfSomeExcludedFromStateUnemploymentTax : 0,

                    CreditReductionAmount = creditReductionAmount = stateTaxCreditReductionApplies.Sum(e => e.Wages),

                    //12 to 15
                    TotalFutaTaxAfterAdjust = totalFutaTaxAfterAdjust = futaTaxBeforeAdjust + adjustIfAllExcludedFromStateUnemploymentTax
                        + adjustIfSomeExcludedFromStateUnemploymentTax + creditReductionAmount,
                    FutaTaxDepositedForTheYear = totalFutaTaxDeposited = futaTax.Sum(e => e.DepositedAmount),
                    BalanceDue = Math.Max(totalFutaTaxAfterAdjust - totalFutaTaxDeposited, 0.0),
                    OverPayment = Math.Max(totalFutaTaxDeposited - totalFutaTaxAfterAdjust, 0.0),
                    ApplyOverpaymentToNextReturn = overPayment > 0 ? true : false,


                    //16a to 16d  
                    TaxLiabilityFirstQuarter = futaTax.Where(e => e.PayStub.Payroll.ScheduledRunDate >= startDate &&
                        e.PayStub.Payroll.ScheduledRunDate <= firstQuarterEndDate).Sum(e => e.Amount),
                    TaxLiabilitySecondQuarter = futaTax.Where(e => e.PayStub.Payroll.ScheduledRunDate > firstQuarterEndDate &&
                        e.PayStub.Payroll.ScheduledRunDate <= secondQuarterEndDate).Sum(e => e.Amount),
                    TaxLiabilityThirdQuarter = futaTax.Where(e => e.PayStub.Payroll.ScheduledRunDate > secondQuarterEndDate &&
                        e.PayStub.Payroll.ScheduledRunDate <= thirdQuarterEndDate).Sum(e => e.Amount),
                    TaxLiabilityFourthQuarter = futaTax.Where(e => e.PayStub.Payroll.ScheduledRunDate > thirdQuarterEndDate &&
                        e.PayStub.Payroll.ScheduledRunDate <= endDate).Sum(e => e.Amount),

                    //Signature
                    //Note: We will take form 8879-EMP from our customers and sign the submission ourselves using self generated pin
                    HasThirdPartyDesignee = bool.Parse(companySpecificFields.FirstOrDefault(e => e.CompanySpecificField.FieldName == "HasThirdPartyDesignee").FieldValue ?? "false"),
                    ThirdPartyFiveDigitPin = int.Parse(countrySpecificFields.FirstOrDefault(e => e.CountrySpecificField.FieldName == "SelfGeneratedFiveDigitPin").FieldValue)

                };

                _context.FormNineForty.Add(form940);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Generating Form 940 Failed. " + ex.ToString());
                return;
            }
        });

    }

    private FormNineForty GetFormNineForty(int id)
    {
        return _context.FormNineForty.Find(id);
    }
    public FormNineFortyModel GetTestScenario(int id)
    {
        return id switch
        {
            1 => GetTestScenarioOne(),
            2 => GetTestScenarioTwo(),
            3 => GetTestScenarioThree(),
            _ => null
        };
    }
    private FormNineFortyModel GetTestScenarioOne()
    {
        var testScinarioOneData = new FormNineFortyModel
        {
            Id = 003000011,
            TypeOfReturn = null,
            PaysMultiStateUnemploymentTax = false,
            InvolvedStates = new List<string>() { "AZ" },
            PaidWagesInCreditReductionState = false,
            TotalPaymentsToAllEmployees = 200000.0,
            PaymentsExemptFromFutaTax = 0.0,
            ExemptFromFutaFringeBenefits = false,
            ExemptFromFutaGroupTermLifeInsurance = false,
            ExemptFromFutaRetirementOrPension = false,
            ExemptFromFutaDependentCare = false,
            ExemptFromFutaOther = false,
            TotalPaymentsAbove7K = 4000.0,
            SubTotal = 4000.0,
            TotalTaxableFutaWages = 196000.0,
            FutaTaxBeforeAdjust = 1176.0,
            AdjustIfAllExcludedFromStateUnemploymentTax = 0.0,
            AdjustIfSomeExcludedFromStateUnemploymentTax = 0.0,
            PaidStateUnemploymentTaxLate = false,
            CreditReductionAmount = 0.0,
            TotalFutaTaxAfterAdjust = 1176.0,
            FutaTaxDepositedForTheYear = 2886.0,
            BalanceDue = 0.0,
            OverPayment = 1710.0,
            ApplyOverpaymentToNextReturn = false,
            TaxLiabilityFirstQuarter = 300.0,
            TaxLiabilitySecondQuarter = 300.0,
            TaxLiabilityThirdQuarter = 300.0,
            TaxLiabilityFourthQuarter = 276.0,
            TotalLiabilityForTheYear = 1176.0,
            HasThirdPartyDesignee = true,
            ThirdPartyDesigneeName = null,
            ThirdPartyDesigneePhone = "520-555-1212",
            ThirdPartyFiveDigitPin = 12345,
            Signature = null,
            SignerName = null,
            SignerTitle = "Owner",
            SignerBestDayTimePhone = "520-555-0000",
            Year = 2024
        };
        return testScinarioOneData;
    }
    private FormNineFortyModel GetTestScenarioTwo()
    {
         
        var testScinarioOneData = new FormNineFortyModel
        {
            Id = 3000001,
            TypeOfReturn = null,
            PaysMultiStateUnemploymentTax = false,
            InvolvedStates = new List<string>() { "CA"},
            PaidWagesInCreditReductionState = true,
            TotalPaymentsToAllEmployees = 350000.0,
            PaymentsExemptFromFutaTax = 100000.0,
            ExemptFromFutaFringeBenefits = false,
            ExemptFromFutaGroupTermLifeInsurance = false,
            ExemptFromFutaRetirementOrPension = true,
            ExemptFromFutaDependentCare = false,
            ExemptFromFutaOther = false,
            TotalPaymentsAbove7K = 0.0,
            SubTotal = 100000.0,
            TotalTaxableFutaWages = 250000.0,
            FutaTaxBeforeAdjust = 1500.0,
            AdjustIfAllExcludedFromStateUnemploymentTax = 0.0,
            AdjustIfSomeExcludedFromStateUnemploymentTax = 0.0,
            PaidStateUnemploymentTaxLate = false,
            CreditReductionAmount = 18.0,
            TotalFutaTaxAfterAdjust = 1518.0,
            FutaTaxDepositedForTheYear = 4000.0,
            BalanceDue = 0.0,
            OverPayment = 2482.0,
            ApplyOverpaymentToNextReturn = true,
            TaxLiabilityFirstQuarter = 500.0,
            TaxLiabilitySecondQuarter = 500.0,
            TaxLiabilityThirdQuarter = 500.0,
            TaxLiabilityFourthQuarter = 18.0,
            TotalLiabilityForTheYear = 1518.0,
            HasThirdPartyDesignee = false,
            ThirdPartyDesigneeName = null,
            ThirdPartyDesigneePhone = null,
            ThirdPartyFiveDigitPin = 0,
            Signature = null,
            SignerName = null,
            SignerTitle = "Controller",
            SignerBestDayTimePhone = "718-000-1212",
            Year = 2024
        };
        return testScinarioOneData;
    }
    private FormNineFortyModel GetTestScenarioThree()
    {

        var testScinarioOneData = new FormNineFortyModel
        {
            Id = 3000002,
            TypeOfReturn = null,
            PaysMultiStateUnemploymentTax = false,
            InvolvedStates = new List<string>() { "TX" },
            PaidWagesInCreditReductionState = false,
            TotalPaymentsToAllEmployees = 560000.0,
            PaymentsExemptFromFutaTax = 0.0,
            ExemptFromFutaFringeBenefits = false,
            ExemptFromFutaGroupTermLifeInsurance = false,
            ExemptFromFutaRetirementOrPension = false,
            ExemptFromFutaDependentCare = false,
            ExemptFromFutaOther = false,
            TotalPaymentsAbove7K = 0.0,
            SubTotal = 0.0,
            TotalTaxableFutaWages = 560000.0,
            FutaTaxBeforeAdjust = 3360.0,
            AdjustIfAllExcludedFromStateUnemploymentTax = 0.0,
            AdjustIfSomeExcludedFromStateUnemploymentTax = 0.0,
            PaidStateUnemploymentTaxLate = false,
            CreditReductionAmount = 0.0,
            TotalFutaTaxAfterAdjust = 3360.0,
            FutaTaxDepositedForTheYear = 1428.0,
            BalanceDue = 1932.0,
            OverPayment = 0.0,
            ApplyOverpaymentToNextReturn = false,
            TaxLiabilityFirstQuarter = 800.0,
            TaxLiabilitySecondQuarter = 800.0,
            TaxLiabilityThirdQuarter = 800.0,
            TaxLiabilityFourthQuarter = 960.0,
            TotalLiabilityForTheYear = 3360.0,
            HasThirdPartyDesignee = false,
            ThirdPartyDesigneeName = null,
            ThirdPartyDesigneePhone = null,
            ThirdPartyFiveDigitPin = 0,
            Signature = null,
            SignerName = null,
            SignerTitle = null,
            Year = 2024           
        };
        return testScinarioOneData;
    }
}