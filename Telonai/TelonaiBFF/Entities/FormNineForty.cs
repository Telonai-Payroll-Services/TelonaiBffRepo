namespace TelonaiWebApi.Entities;

/// <summary>
/// Form 940: Employer’s Annual Federal Unemployment Tax
/// </summary>
public class FormNineForty : BaseTracker
{
    public int Id { get; set; }
    public List<int> FormNineFortyTypeOfReturn { get; set; }
    public bool PaysMultiStateUnemploymentTax { get; set; }
    public List<string> InvolvedStates { get; set; }
    public bool PaidWagesInCreditReductionState { get; set; }
    public double TotalPaymentsToAllEmployees { get; set; }
    public double PaymentsExemptFromFutaTax { get; set; }
    public bool ExemptFromFutaFringeBenefits { get; set; }
    public bool ExemptFromFutaGroupTermLifeInsurance { get; set; }
    public bool ExemptFromFutaRetirementOrPension { get; set; }
    public bool ExemptFromFutaDependentCare { get; set; }
    public bool ExemptFromFutaOther { get; set; }
    public double TotalPaymentsAbove7K { get; set; }
    public double SubTotal { get; set; }
    public double TotalTaxableFutaWages { get; set; }
    public double FutaTaxBeforeAdjust{ get; set; }
    public double AdjustIfAllExcludedFromStateUnemploymentTax { get; set; }
    public double AdjustIfSomeExcludedFromStateUnemploymentTax { get; set; }
    public bool PaidStateUnemploymentTaxLate { get; set; }
    public double CreditReductionAmount { get; set; }
    public double TotalFutaTaxAfterAdjust { get; set; }
    public double FutaTaxDepositedForTheYear { get; set; }
    public double BalanceDue { get; set; }
    public double OverPayment { get; set; }
    public bool ApplyOverpaymentToNextReturn { get; set; }
    public double TaxLiabilityFirstQuarter { get; set; }
    public double TaxLiabilitySecondQuarter { get; set; }
    public double TaxLiabilityThirdQuarter { get; set; }
    public double TaxLiabilityFourthQuarter { get; set; }
    public double TotalLiabilityForTheYear { get; set; }
    public bool HasThirdPartyDesignee { get; set; }
    public string ThirdPartyDesigneeName { get; set; }
    public string ThirdPartyDesigneePhone { get; set; }
    public int ThirdPartyFiveDigitPin { get; set; }
    public string Signature { get; set; }
    public DateOnly SignatureDate { get; set; }
    public string SignerName { get; set; }
    public string SignerTitle { get; set; }
    public string SignerBestDayTimePhone { get; set; }
    public int CompanyId { get; set; }
    public int Year { get; set; }
    public Company Company { get; set; }
}