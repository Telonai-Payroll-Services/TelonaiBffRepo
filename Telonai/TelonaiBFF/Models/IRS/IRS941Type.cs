namespace TelonaiWebApi.Models.IRS
{
    public class IRS941Type
    {
        //private CheckboxType spanishLanguageInd { get; set; }

        private string employeeCnt { get; set; }

        private decimal wagesAmt { get; set; }

        private bool wagesAmtSpecified { get; set; }

        private decimal federalIncomeTaxWithheldAmt { get; set; }

        private CheckboxType wagesNotSubjToSSMedcrTaxInd { get; set; }

        private bool wagesNotSubjToSSMedcrTaxIndSpecified { get; set; }

        private SocialSecurityWageAndTaxGrp socialSecurityWageAndTaxGrp { get; set; }

        private SocialSecurityTipsAndTaxGrp socialSecurityTipsAndTaxGrp { get; set; }

        private MedicareWageTipsAndTaxGrp medicareWageTipsAndTaxGrp { get; set; }

        private AddnlMedicareWageTipsAndTaxGrp addnlMedicareWageTipsAndTaxGrp { get; set; }

        private decimal totalSSMdcrTaxAmt { get; set; }

        private decimal taxOnUnreportedTips3121qAmt { get; set; }

        private decimal totalTaxBeforeAdjustmentAmt { get; set; }

        private decimal currentQtrFractionsCentsAmt { get; set; }

        private decimal currentQuarterSickPaymentAmt { get; set; }

        private decimal currQtrTipGrpTermLifeInsAdjAmt{ get; set; }

        private decimal totalTaxAfterAdjustmentAmt{ get; set; }

        private PayrollTaxCreditAmt payrollTaxCreditAmt{ get; set; }

        private decimal totalTaxAmt{ get; set; }

        private decimal totalTaxDepositAmt{ get; set; }

        private object item{ get; set; }

        private object item1{ get; set; }

        private Item1ChoiceType item1ElementName{ get; set; }

        private BusinessClosedGrp businessClosedGrp{ get; set; }

        private CheckboxType seasonalEmployerInd{ get; set; }

        private bool seasonalEmployerIndSpecified{ get; set; }
    }
}
