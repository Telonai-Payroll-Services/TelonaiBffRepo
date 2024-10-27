using System.Xml.Serialization;

namespace TelonaiWebApi.Models.IRS
{
    public class AddnlMedicareWageTipsAndTaxGrp
    {
        public decimal txblWageTipsSubjAddnlMedcrAmt { get; set; }
        public decimal taxOnWageTipsSubjAddnlMedcrAmt { get; set; }
    }
}
