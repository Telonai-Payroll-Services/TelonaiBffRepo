
using System.Xml.Serialization;

namespace TelonaiWebApi.Models.IRS
{
    public class BusinessClosedGrp
    {
        public string futureFilingNotRequiredInd { get; set; }
        public DateTime finalWagesPaidDt { get; set; }
    }
}
