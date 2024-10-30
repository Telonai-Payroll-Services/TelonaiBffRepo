using System.Xml.Serialization;

namespace TelonaiWebApi.Models.IRS
{
    public class SocialSecurityWageAndTaxGrp
    {
        public decimal socialSecurityTaxCashWagesAmt { get; set; }
        public decimal socialSecurityTaxAmt { get; set; }
    }
}
