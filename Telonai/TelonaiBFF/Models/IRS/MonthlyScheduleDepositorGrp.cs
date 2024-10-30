using System.Xml.Serialization;

namespace TelonaiWebApi.Models.IRS
{
    public class MonthlyScheduleDepositorGrp
    {
        public string MonthlyScheduleDepositorInd { get; set; }
        public decimal TaxLiabilityMonth1Amt { get; set; }
        public decimal TaxLiabilityMonth2Amt { get; set; }
        public decimal TaxLiabilityMonth3Amt { get; set; }
    }
}
