namespace TelonaiWebApi.Models
{
    public class OtherMoneyReceivedModel : BaseTracker
    {
        public int Id { get; set; }
        public int PayStubId { get; set; }
        public double CreditCardTips { get; set; }
        public double CashTips { get; set; }
        public double OtherPay { get; set; }
        public string Note { get; set; }
        public double Reimbursement { get; set; }
        
    }
}
