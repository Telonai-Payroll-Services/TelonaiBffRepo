using System.Xml.Serialization;

namespace TelonaiWebApi.Models.IRS
{
    public class PayrollTaxCreditAmt
    {
        public string[] referenceDocumentId { get; set; }
        public string referenceDocumentName { get; set; }
        public decimal value { get; set; }
    }
}
