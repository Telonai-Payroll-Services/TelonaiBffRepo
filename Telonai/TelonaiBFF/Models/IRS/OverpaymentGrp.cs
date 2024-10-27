using System.Xml.Serialization;
namespace TelonaiWebApi.Models.IRS
{
    public class OverpaymentGrp
    { 
        public string applyOverpaymentNextReturnInd { get; set; }
        public string refundOverpaymentInd { get; set; }
    }
}
