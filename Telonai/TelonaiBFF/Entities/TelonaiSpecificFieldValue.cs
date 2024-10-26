namespace TelonaiWebApi.Entities;

using System.Text.Json.Serialization;

public class TelonaiSpecificFieldValue:BaseTracker
{
    public int Id { get; set; }
    public int FieldId { get; set; }
    public string FieldValue { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public TelonaiSpecificField TelonaiSpecificField { get; set; }

}