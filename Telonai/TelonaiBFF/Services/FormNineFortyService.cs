namespace TelonaiWebApi.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
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

    public FormNineFortyService(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public IList<FormNineFortyModel> Get()
    {
        var obj = _context.FormNineForty
            .Include(e => e.Company).ToList();
        var result=_mapper.Map<IList<FormNineFortyModel>>(obj);
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

        var form940s = groupedPayrolls.Select(e => {
            var companyId = e.Key;

            var depositSchedule = _context.DepositSchedule.OrderByDescending(e => e.EffectiveDate).FirstOrDefault(e => e.CompanyId == companyId);
            var depositScheduleType = (DepositScheduleTypeModel)depositSchedule.DepositScheduleTypeId;
         
            var companySpecificFields = _context.CompanySpecificFieldValue.OrderByDescending(e => e.EffectiveDate).Include(e => e.CompanySpecificField)
            .Where(e => e.CompanyId==companyId).ToList();

            var incomeTaxRates = _context.IncomeTaxRate.ToList();
            var incomeTaxes = e.Where(e => !e.PayStub.IsCancelled).ToList();
            var allPayStubsThisYear = incomeTaxes.Select(e => e.PayStub).ToList();
            var allPayrollsThisYear = allPayStubsThisYear.Select(e => e.Payroll).Distinct().ToList();
            var grossPayThisYear = allPayStubsThisYear.Sum(e => e.GrossPay);
            var paymentExemptFromFutaTax = allPayStubsThisYear.Where(e => e.OtherMoneyReceived.PaymentExemptFromFutaTaxTypeId > 0).Select(e => e.OtherMoneyReceived);
           
            var statesTax = incomeTaxes.Where(e => e.IncomeTaxType.Name == "State Tax")
            .Select(e=> new { State = e.IncomeTaxType.State.StateCode, TaxAmount=e.Amount, Wages=e.PayStub.GrossPay }).ToList();

            var creditReductionStates = stateSpecificFields.Where(e => e.StateSpecificField.FieldName == "StateIsCreditReductionState")
            .Select(e=>e.State.Name).ToList();

            var stateTaxCreditReductionApplies = statesTax.Where(e => creditReductionStates.Contains(e.State)).ToList();

            var federalIncomeTaxWithheld = incomeTaxes.Where(e => e.IncomeTaxType.Name == "Federal Tax").Sum(e => e.Amount);
            var taxableSocialSecurityWages = allPayStubsThisYear.Sum(e => e.RegularPay + e.OverTimePay);
            var taxableSocialSecurityTips = allPayStubsThisYear.Select(e=>e.OtherMoneyReceived).Sum(e => e.OtherPay + e.CashTips+ e.CreditCardTips);
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

            var totalDeposit = 0.0;
            var overPayment = 0.0;

            return new FormNineForty
            {
                CompanyId = e.Key,
                Year = year,
                //1 to 2
                FormNineFortyTypeOfReturn = (companySpecificFields.FirstOrDefault(e => e.CompanySpecificField.FieldName == "FormNineFortyTypeOfReturn").FieldValue ?? "0").Split(",").Select(e => int.Parse(e)).ToList(),
                InvolvedStates = (companySpecificFields.FirstOrDefault(e => e.CompanySpecificField.FieldName == "InvolvedStates").FieldValue ?? "NC").Split(",").ToList(),
                PaysMultiStateUnemploymentTax = bool.Parse(companySpecificFields.FirstOrDefault(e => e.CompanySpecificField.FieldName == "PaysMultiStateUnemploymentTax").FieldValue ?? "false"),
                PaidWagesInCreditReductionState = stateTaxCreditReductionApplies!=null && stateTaxCreditReductionApplies.Count>0,

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
                FutaTaxBeforeAdjust = totalTaxableFutaWages * double.Parse(telonaiSpecificFields.FirstOrDefault(e => e.TelonaiSpecificField.FieldName == year.ToString() + "FUTATaxRate").FieldValue),

                //9 to 11
                AdjustIfAllExcludedFromStateUnemploymentTax = allFutaWagesWereExcludedFromStateUnemploymentTax ? totalTaxableFutaWages * futaTaxIfAllExcludedFromStateUnemploymentTax : 0,
                AdjustIfSomeExcludedFromStateUnemploymentTax = someFutaWagesWereExcludedFromStateUnemploymentTax ? totalTaxableFutaWages * futaTaxIfSomeExcludedFromStateUnemploymentTax : 0,
                CreditReductionAmount = stateTaxCreditReductionApplies.Sum(e=>e.Wages),

                //11 to 17
                //TO DO: Will continue tomorrow

                //Signature
                //Note: We will take form 8879-EMP from our customers and sign the submission ourselves using self generated pin
                HasThirdPartyDesignee = bool.Parse(companySpecificFields.FirstOrDefault(e => e.CompanySpecificField.FieldName == "HasThirdPartyDesignee").FieldValue ?? "false"),
                ThirdPartyFiveDigitPin = int.Parse(telonaiSpecificFields.FirstOrDefault(e => e.TelonaiSpecificField.FieldName == "SelfGeneratedFiveDigitPin").FieldValue)

            };
        }).ToList();

    }

    private FormNineForty GetFormNineForty(int id)
    {
        return _context.FormNineForty.Find(id);
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