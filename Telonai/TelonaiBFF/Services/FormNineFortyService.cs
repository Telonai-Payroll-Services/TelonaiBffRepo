namespace TelonaiWebApi.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using TelonaiWebApi.Controllers;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using static System.Net.Mime.MediaTypeNames;

public interface IFormNineFortyService 
{
    Task CreateAsync();
    IList<FormNineFortyModel> GetCurrent940FormsAsync();
    IList<FormNineFortyModel> GetPrevious940FormsAsync(int year);
    IList<FormNineFortyModel> Get();
    FormNineFortyModel GetById(int id);
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
        var telonaiSpecificFields = _context.TelonaiSpecificFieldValue.OrderByDescending(e => e.EffectiveDate).Include(e => e.TelonaiSpecificField)
            .ToList();

        var stateSpecificFields = _context.StateSpecificFieldValue.OrderByDescending(e => e.EffectiveDate).Include(e => e.StateSpecificField)
            .ToList();

        var year = DateTime.Now.Year - 1;
        var startDate = new DateOnly(year, 1, 1);
        var endDate = new DateOnly(year - 1, 12, 31);

        var groupedPayrolls = _context.IncomeTax.Include(e => e.PayStub).ThenInclude(e => e.Payroll).ThenInclude(e => e.Company)
            .Where(e => e.PayStub.Payroll.ScheduledRunDate >= startDate && e.PayStub.Payroll.ScheduledRunDate <= endDate)
            .GroupBy(e => e.PayStub.Payroll.CompanyId).ToList();

        groupedPayrolls.ForEach(async e =>
        {
            try
            {
                var companyId = e.Key;

                var depositSchedule = _context.DepositSchedule.OrderByDescending(e => e.EffectiveDate).FirstOrDefault(e => e.CompanyId == companyId);
                var depositScheduleType = (DepositScheduleTypeModel)depositSchedule.DepositScheduleTypeId;

                var companySpecificFields = _context.CompanySpecificFieldValue.OrderByDescending(e => e.EffectiveDate).Include(e => e.CompanySpecificField)
                .Where(e => e.CompanyId == companyId).ToList();

                var incomeTaxRates = _context.IncomeTaxRate.ToList();
                var incomeTaxes = e.Where(e => !e.PayStub.IsCancelled).ToList();
                var allPayStubsThisYear = incomeTaxes.Select(e => e.PayStub).ToList();
                var allPayrollsThisYear = allPayStubsThisYear.Select(e => e.Payroll).Distinct().ToList();
                var grossPayThisYear = allPayStubsThisYear.Sum(e => e.GrossPay);
                var paymentExemptFromFutaTax = allPayStubsThisYear.Where(e => e.OtherMoneyReceived.PaymentExemptFromFutaTaxTypeId > 0).Select(e => e.OtherMoneyReceived);

                var statesTax = incomeTaxes.Where(e => e.IncomeTaxType.Name == "State Tax")
                .Select(e => new { State = e.IncomeTaxType.State.StateCode, TaxAmount = e.Amount, Wages = e.PayStub.GrossPay }).ToList();

                var creditReductionStates = stateSpecificFields.Where(e => e.StateSpecificField.FieldName == "StateIsCreditReductionState")
                .Select(e => e.State.Name).ToList();

                var stateTaxCreditReductionApplies = statesTax.Where(e => creditReductionStates.Contains(e.State)).ToList();

                var futaTax = incomeTaxes.Where(e => e.IncomeTaxType.Name == "FUTA");
                var federalIncomeTaxWithheld = incomeTaxes.Where(e => e.IncomeTaxType.Name == "Federal Tax").Sum(e => e.Amount);
                var taxableSocialSecurityWages = allPayStubsThisYear.Sum(e => e.RegularPay + e.OverTimePay);
                var taxableSocialSecurityTips = allPayStubsThisYear.Select(e => e.OtherMoneyReceived).Sum(e => e.OtherPay + e.CashTips + e.CreditCardTips);
                var taxableMedicareWagesAndTips = taxableSocialSecurityWages + taxableSocialSecurityTips;
                var wagesAndTipsSubjectToAdditionalTax = allPayStubsThisYear.Sum(e => e.AmountSubjectToAdditionalMedicareTax);
                var unreportedTipsTaxDue = incomeTaxes.Where(e => e.IncomeTaxType.Name == "Unreported Tips Tax Due").Sum(e => e.Amount);

                var totalPaymentsAbove7K = allPayStubsThisYear.Where(e => e.GrossPay > 7000).Sum(e => e.GrossPay - 7000);
                var paymentsExemptFromFutaTaxAmount = paymentExemptFromFutaTax.Sum(e => e.OtherPay);
                var subTotal = paymentsExemptFromFutaTaxAmount + totalPaymentsAbove7K;
                var totalTaxableFutaWages = grossPayThisYear - subTotal;

                var allFutaWagesWereExcludedFromStateUnemploymentTax = bool.Parse(companySpecificFields.FirstOrDefault(e => e.CompanySpecificField.FieldName == "allFutaWagesWereExcludedFromStateUnemploymentTax").FieldValue ?? "false");
                var someFutaWagesWereExcludedFromStateUnemploymentTax = bool.Parse(companySpecificFields.FirstOrDefault(e => e.CompanySpecificField.FieldName == "someFutaWagesWereExcludedFromStateUnemploymentTax").FieldValue ?? "false");
                var futaTaxIfAllExcludedFromStateUnemploymentTax = double.Parse(telonaiSpecificFields.FirstOrDefault(e => e.TelonaiSpecificField.FieldName == year.ToString() + "FUTATaxRateIfAllExcludedFromStateUnemploymentTax").FieldValue);
                var futaTaxIfSomeExcludedFromStateUnemploymentTax = double.Parse(telonaiSpecificFields.FirstOrDefault(e => e.TelonaiSpecificField.FieldName == year.ToString() + "FUTATaxRateIfSomeExcludedFromStateUnemploymentTax").FieldValue);

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

                    //3 to 4e
                    TotalPaymentsToAllEmployees = grossPayThisYear,
                    PaymentsExemptFromFutaTax = paymentsExemptFromFutaTaxAmount,
                    ExemptFromFutaFringeBenefits = paymentExemptFromFutaTax.Any(e => e.PaymentExemptFromFutaTaxTypeId == (int)PaymentExemptFromFutaTaxTypeModel.FringeBenefits),
                    ExemptFromFutaGroupTermLifeInsurance = paymentExemptFromFutaTax.Any(e => e.PaymentExemptFromFutaTaxTypeId == (int)PaymentExemptFromFutaTaxTypeModel.GroupTermLifeInsurance),
                    ExemptFromFutaRetirementOrPension = paymentExemptFromFutaTax.Any(e => e.PaymentExemptFromFutaTaxTypeId == (int)PaymentExemptFromFutaTaxTypeModel.RetirementOrPension),
                    ExemptFromFutaDependentCare = paymentExemptFromFutaTax.Any(e => e.PaymentExemptFromFutaTaxTypeId == (int)PaymentExemptFromFutaTaxTypeModel.DependentCare),
                    ExemptFromFutaOther = paymentExemptFromFutaTax.Any(e => e.PaymentExemptFromFutaTaxTypeId == (int)PaymentExemptFromFutaTaxTypeModel.Other),

                    //5f to 8
                    TotalPaymentsAbove7K = totalPaymentsAbove7K,
                    SubTotal = subTotal,
                    TotalTaxableFutaWages = totalTaxableFutaWages,
                    FutaTaxBeforeAdjust = futaTaxBeforeAdjust = totalTaxableFutaWages * double.Parse(telonaiSpecificFields.FirstOrDefault(e => e.TelonaiSpecificField.FieldName == year.ToString() + "FUTATaxRate").FieldValue),

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
                    ThirdPartyFiveDigitPin = int.Parse(telonaiSpecificFields.FirstOrDefault(e => e.TelonaiSpecificField.FieldName == "SelfGeneratedFiveDigitPin").FieldValue)

                };

                _context.FormNineForty.Add(form940);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Generating Form 941 Failed. " + ex.ToString());
                return;
            }
        });

    }

    private FormNineForty GetFormNineForty(int id)
    {
        return _context.FormNineForty.Find(id);
    }
}