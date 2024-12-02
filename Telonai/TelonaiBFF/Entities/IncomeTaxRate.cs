namespace TelonaiWebApi.Entities;


public class IncomeTaxRate
{
    public int Id { get; set; }
    public double Rate { get; set; }
    public int IncomeTaxTypeId { get; set; }
    public int? FilingStatusId { get; set; }
    public double? TentativeAmount { get; set; }
    public int EffectiveYear { get; set; }
    public double Minimum { get; set; }
    public double Maximum { get; set; }
    public IncomeTaxType IncomeTaxType { get; set; }
    public FilingStatus FilingStatus { get; set; }

}