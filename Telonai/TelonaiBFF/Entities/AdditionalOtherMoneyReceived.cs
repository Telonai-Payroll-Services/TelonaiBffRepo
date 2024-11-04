namespace TelonaiWebApi.Entities
{
    public class AdditionalOtherMoneyReceived : BaseTracker
    {
        public int Id { get; set; }
        public int PayStubId { get; set; }
        public double Amount { get; set; }
        public double YtdAmount { get; set; }
        public string Note { get; set; }
        public int ExemptFromFutaTaxTypeId { get; set; }

    }
}
