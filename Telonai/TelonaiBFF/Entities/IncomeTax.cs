namespace TelonaiWebApi.Entities;

public class IncomeTax:BaseTracker
{
    public int Id { get; set; }
    public int IncomeTaxTypeId { get; set; }
    public int PayStubId { get; set; }
    public double Amount { get; set; }
    public double DepositedAmount { get; set; }
    public double YtdAmount { get; set; }
    public IncomeTaxType IncomeTaxType { get; set; }
    public PayStub PayStub { get; set; }
}