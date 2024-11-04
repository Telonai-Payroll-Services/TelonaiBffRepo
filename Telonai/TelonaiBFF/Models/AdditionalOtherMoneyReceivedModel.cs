namespace TelonaiWebApi.Models
{
    public class AdditionalOtherMoneyReceivedModel : BaseTracker
    {
        public int Id { get; set; }
        public int PayStubId { get; set; }
        public double Amount { get; set; }
        public string Note { get; set; }
        public int ExemptFromFutaTaxTypeId { get; set; }
        public ExemptFromFutaTaxTypeModel ExemptFromFutaTaxType { get; set; }
    }
}
