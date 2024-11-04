namespace TelonaiWebApi.Entities
{
    public class OtherMoneyReceived : BaseTracker
    {
        public int Id { get; set; }
        public bool IsCancelled { get; set; }

        public double CreditCardTips { get; set; }
        public double YtdCreditCardTips { get; set; }
        public int[] AdditionalOtherMoneyReceivedId { get; set; }
        public double CashTips { get; set; }
        public double YtdCashTips { get; set; }
        public double Reimbursement { get; set; }
        public double YtdReimbursement { get; set; }
    }
}
