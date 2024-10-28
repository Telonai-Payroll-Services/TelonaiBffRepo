namespace TelonaiWebApi.Entities;

/// <summary>
/// Form 944:Employer’s ANNUAL Federal Tax Return.  
/// It is for employers with annual liability of $1,000 or less for social security, Medicare, and withheld federal income taxes.
/// </summary>
public class FormNineFortyFour:BaseTracker
{
    public int Id { get; set; }
    public double WagesTipsCompensation { get; set; }
    public double FederalIncomeTaxWithheld { get; set; }
    public bool NotSubjectToSocialSecAndMediTax { get; set; }
    public double TaxableSocialSecurityWages { get; set; }
    public double QualifiedSickLeaveWages { get; set; }
    public double QualifiedFamilyLeaveWages { get; set; }
    public double TaxableSocialSecurityTips { get; set; }
    public double TaxableMedicareWagesAndTips { get; set; }
    public double WagesAndTipsSubjectToAdditionalTax { get; set; }
    public double TaxableSocialSecurityWagesTax { get; set; }
    public double QualifiedSickLeaveWagesTax { get; set; }
    public double QualifiedFamilyLeaveWagesTax { get; set; }
    public double TaxableSocialSecurityTipsTax { get; set; }
    public double TaxableMedicareWagesAndTipsTax { get; set; }
    public double WagesAndTipsSubjectToAdditionalTaxTax { get; set; }
    public double TotalSocialAndMediTax { get; set; }
    public double TotalTaxBeforeAdjustment { get; set; }
    public double CurrentYearAddjustments { get; set; }
    public double TotalTaxAfterAdjustment { get; set; }
    public double TaxCreditForResearchActivities { get; set; }

    //public double NonRefundCreditForReasearchActivity { get; set; }
    public double NonRefundCreditForSickAndFamilyLeave1 { get; set; }
    public double NonRefundCreditForSickAndFamilyLeave2 { get; set; }
    public double TotalNonRefundableCredit { get; set; }

    public double TotalTaxAfterAdjustmentsOfCredits { get; set; }
    public double TotalDepositForYear { get; set; }
    public double RefundableCreditForSickAndFamilyLeave1 { get; set; }
    public double RefundableCreditForSickAndFamilyLeave2 { get; set; }
    public double TotalDepositsAndRefundableCredits { get; set; }

    public double BalanceDue { get; set; }
    public double Overpayment { get; set; }
    public bool ApplyOverpaymentToNextReturn { get; set; }

    public bool TotalLiabilityIsLessThan2500 { get; set; }
    public bool TotalLiabilityIs2500OrMore { get; set; }

    public List<double> TaxLiabilityEachMonth { get; set; }
    public double TotalLiabilityForYear { get; set; }
    public bool BusinessIsClosed { get; set; }
    public bool BusinessStoppedPayingWages { get; set; }
    public string FinalDateWagesPaid { get; set; }
    public double HealthPlanExpenseForSickLeaveWages1 { get; set; }
    public double HealthPlanExpenseForFamilyLeave1 { get; set; }
    public double SickLeaveWages { get; set; }
    public double HealthPlanExpenseForSickLeaveWages2 { get; set; }
    public double CollectivelyBargainedAmountsForSickLeaveWages { get; set; }
    public double FamilyLeaveWages { get; set; }
    public double HealthPlanExpenseForFamilyLeaveWages2 { get; set; }
    public double CollectivelyBargainedAmountsForFamilyLeaveWages { get; set; }
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