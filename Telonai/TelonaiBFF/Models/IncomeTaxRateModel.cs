namespace TelonaiWebApi.Entities;


public class IncomeTaxRateModel
{
    public int Id { get; set; }
    public double Rate { get; set; }
    public int IncomeTaxTypeId { get; set; }
    public int? FilingStatusId { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public double MinimumGrossPay { get; set; }
    public double MaximumGrossPay { get; set; }
    public string IncomeTaxType { get; set; }
    public string FilingStatus { get; set; }

}