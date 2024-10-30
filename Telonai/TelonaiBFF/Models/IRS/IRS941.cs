using System.Xml.Serialization;
namespace TelonaiWebApi.Models.IRS
{
    public class IRS941
    {
        public string spanishLanguageInd { get; set; }
        public string employeeCnt { get; set; }
        public decimal wagesAmt { get; set; }
        public decimal federalIncomeTaxWithheldAmt { get; set; }
        public string wagesNotSubjToSSMedcrTaxInd { get; set; }
        public SocialSecurityWageAndTaxGrp socialSecurityWageAndTaxGrp { get; set; }
        public SocialSecurityTipsAndTaxGrp socialSecurityTipsAndTaxGrp { get; set; }
        public MedicareWageTipsAndTaxGrp medicareWageTipsAndTaxGrp { get; set; }
        public AddnlMedicareWageTipsAndTaxGrp addnlMedicareWageTipsAndTaxGrp { get; set; }
        public decimal totalSSMdcrTaxAmt { get; set; }
        public decimal taxOnUnreportedTips3121qAmt { get; set; }
        public decimal totalTaxBeforeAdjustmentAmt { get; set; }
        public decimal currentQtrFractionsCentsAmt { get; set; }
        public decimal currentQuarterSickPaymentAmt { get; set; }
        public decimal currQtrTipGrpTermLifeInsAdjAmt{ get; set; }
        public decimal totalTaxAfterAdjustmentAmt{ get; set; }
        public PayrollTaxCreditAmt payrollTaxCreditAmt{ get; set; }
        public decimal totalTaxAmt{ get; set; }
        public decimal totalTaxDepositAmt{ get; set; }
        public decimal balanceDueAmt { get; set; }
        public OverpaymentGrp overpaymentGrp { get; set; }
        public decimal totalTaxLessThanLimitAmtInd { get; set;}
        public MonthlyScheduleDepositorGrp monthlyScheduleDepositorGrp { get; set; }
        public decimal semiweeklyScheduleDepositorInd { get; set; }
        public BusinessClosedGrp businessClosedGrp{ get; set; }
        public string seasonalEmployerInd{ get; set; }
    }
}
