using System.Xml.Serialization;

namespace TelonaiWebApi.Models.IRS
{
    public class MedicareWageTipsAndTaxGrp
    {
        public decimal taxableMedicareWagesTipsAmt { get; set; }
        public decimal taxOnMedicareWagesTipsAmt { get; set; }
    }
}
