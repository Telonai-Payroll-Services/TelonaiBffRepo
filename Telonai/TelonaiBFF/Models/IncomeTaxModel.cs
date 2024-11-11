using TelonaiWebApi.Entities;

namespace TelonaiWebApi.Models
{
    public class IncomeTaxModel
    {
        public int Id { get; set; }
        public int IncomeTaxTypeId { get; set; }
        public int PayStubId { get; set; }
        public double Amount { get; set; }
        public double DepositedAmount { get; set; }
        public double YtdAmount { get; set; }
    }
}
