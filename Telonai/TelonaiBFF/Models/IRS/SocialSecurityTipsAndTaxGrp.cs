using System.Xml.Serialization;

namespace TelonaiWebApi.Models.IRS
{
    public class SocialSecurityTipsAndTaxGrp
    {
        public decimal taxableSocSecTipsAmt { get; set; }
        public decimal taxOnSocialSecurityTipsAmt { get; set; }
    }
}
