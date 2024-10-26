namespace TelonaiWebApi.Entities;

/// <summary>
/// Form 941: Employer’s QUARTERLY Federal Tax Return
/// </summary>
public class FormNineFortyOne:BaseTracker
{
    public int Id { get; set; }
    public int NumberOfEmployees { get; set; }
    public double WagesTipsCompensation { get; set; }
    public double FederalIncomeTaxWithheld { get; set; }
    public bool NotSubjectToSocialSecAndMediTax { get; set; }
    public double TaxableSocialSecurityWages { get; set; }
    public double TaxableSocialSecurityTips { get; set; }
    public double TaxableMedicareWagesAndTips { get; set; }
    public double WagesAndTipsSubjectToAdditionalTax { get; set; }
    public double TotalSocialAndMediTax { get; set; }
    public double UnreportedTipsTaxDue { get; set; }
    public double TotalTaxBeforeAdjustment { get; set; }
    public double AdjustForFractionsOfCents { get; set; }
    public double AdjustForSickPay { get; set; }
    public double AdjustForTipsAndLifeInsurance { get; set; }
    public double TotalTaxAfterAdjustment { get; set; }
    public double TotalTaxAfterAdjustmentsCredits { get; set; }

    public double TotalDeposit { get; set; }
    public double BalanceDue { get; set; }
    public double Overpayment { get; set; }
    public bool ApplyOverpaymentToNextReturn { get; set; }
    public double TaxLiabilityMonthOne { get; set; }
    public double TaxLiabilityMonthTwo { get; set; }
    public double TaxLiabilityMonthThree { get; set; }
    public double TotalLiabilityForQuarter { get; set; }
    public int DepositScheduleTypeId { get; set; }
    public bool BusinessIsClosed { get; set; }
    public bool BusinessStoppedPayingWages { get; set; }
    public string FinalDateWagesPaid { get; set; }
    public bool IsSeasonalBusiness { get; set; }
    public bool HasThirdPartyDesignee { get; set; }
    public int CheckedBoxSixteenTypeId { get; set; }
    public string ThirdPartyDesigneeName { get; set; }
    public string ThirdPartyDesigneePhone { get; set; }
    public int ThirdPartyFiveDigitPin { get; set; }
    public string Signature { get; set; }
    public DateOnly SignatureDate { get; set; }
    public string SignerName { get; set; }
    public string SignerTitle { get; set; }
    public string SignerBestDayTimePhone { get; set; }
    public int CompanyId { get; set; }
    public int QuarterTypeId { get; set; }
    public int Year { get; set; }
    public double TaxCreditForResearchActivities { get; set; }
    public Company Company { get; set; }


}