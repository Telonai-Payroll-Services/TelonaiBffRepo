using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Models;
using TelonaiWebApi.Models.IRS;
using TelonaiWebApi.Services;

namespace TelonaiWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IRS941Controller : ControllerBase
    {
        private readonly IFormNineFortyOneService _irsService;
        public IRS941Controller(IFormNineFortyOneService irsService)
        {
            _irsService = irsService;
        }

        [HttpGet]
        public ActionResult<List<FormNineFortyOneModel>> GenerateXML()
        {
            var irs941Data  = _irsService.GetCurrent941FormsAsync();
            //var objSocialSecurityWageAndTaxGrp = new SocialSecurityWageAndTaxGrp()
            //{
            //    socialSecurityTaxAmt = 89,
            //    socialSecurityTaxCashWagesAmt = 1200,
            //};

            //var objSocialSecurityTipsAndTaxGrp = new SocialSecurityTipsAndTaxGrp()
            //{
            //    taxableSocSecTipsAmt = 35,
            //    taxOnSocialSecurityTipsAmt = 300
            //};
            //var objMedicareWageTipsAndTaxGrp = new MedicareWageTipsAndTaxGrp()
            //{
            //    taxableMedicareWagesTipsAmt = 890,
            //    taxOnMedicareWagesTipsAmt = 7992,
            //};
            //var objAddnlMedicareWageTipsAndTaxGrp = new AddnlMedicareWageTipsAndTaxGrp()
            //{
            //    taxOnWageTipsSubjAddnlMedcrAmt = 7522,
            //    txblWageTipsSubjAddnlMedcrAmt = 2322
            //};
            //var objPayrollTaxCreditAmt = new PayrollTaxCreditAmt()
            //{
            //    referenceDocumentId = new string[]
            //    {
            //        "Biruk INineDocument"
            //    },
            //    referenceDocumentName = "INineDocument",
            //    value = 12
            //};
            //var objOverpaymentGrp = new OverpaymentGrp()
            //{
            //    applyOverpaymentNextReturnInd = "test",
            //    refundOverpaymentInd = "test",
            //};
            //var objMonthlyScheduleDepositorGrp = new MonthlyScheduleDepositorGrp()
            //{
            //    MonthlyScheduleDepositorInd = "test",
            //    TaxLiabilityMonth1Amt = 12,
            //    TaxLiabilityMonth2Amt = 24,
            //    TaxLiabilityMonth3Amt = 36,
            //};
            //var objBusinessClosedGrp = new BusinessClosedGrp()
            //{
            //    finalWagesPaidDt = DateTime.Now,
            //    futureFilingNotRequiredInd = "test",
            //};
            //var irs941Data = new IRS941()
            //{
            //    spanishLanguageInd = "123",
            //    employeeCnt = "121.45",
            //    wagesAmt =56,
            //    federalIncomeTaxWithheldAmt = 900,
            //    wagesNotSubjToSSMedcrTaxInd = "test",
            //    socialSecurityWageAndTaxGrp = objSocialSecurityWageAndTaxGrp,
            //    socialSecurityTipsAndTaxGrp = objSocialSecurityTipsAndTaxGrp,
            //    medicareWageTipsAndTaxGrp = objMedicareWageTipsAndTaxGrp,
            //    addnlMedicareWageTipsAndTaxGrp = objAddnlMedicareWageTipsAndTaxGrp,
            //    totalSSMdcrTaxAmt = 8992,
            //    taxOnUnreportedTips3121qAmt = 8111,
            //    currentQtrFractionsCentsAmt = 7200,
            //    currentQuarterSickPaymentAmt = 112,
            //    currQtrTipGrpTermLifeInsAdjAmt = 0,
            //    totalTaxAfterAdjustmentAmt = 2400,
            //    payrollTaxCreditAmt = objPayrollTaxCreditAmt,
            //    totalTaxAmt = 2300,
            //    totalTaxDepositAmt = 300,
            //    balanceDueAmt = 800,
            //    overpaymentGrp = objOverpaymentGrp,
            //    totalTaxLessThanLimitAmtInd = 1233,
            //    monthlyScheduleDepositorGrp = objMonthlyScheduleDepositorGrp,
            //    semiweeklyScheduleDepositorInd = 2322,
            //    businessClosedGrp = objBusinessClosedGrp,
            //    seasonalEmployerInd = "test"
            //};
            //var generatedXML = _irsService.GenerateIRS491XML(irs941Data);
            //_irsService.ZipXMLData(generatedXML);
            return Ok(irs941Data);
        }

    }
}
