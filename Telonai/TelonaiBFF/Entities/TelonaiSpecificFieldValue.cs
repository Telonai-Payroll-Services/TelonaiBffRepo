namespace TelonaiWebApi.Entities;

using System.Text.Json.Serialization;

public class TelonaiSpecificFieldValue:BaseTracker
{
    public int Id { get; set; }
    public int TelonaiSpecificFieldId { get; set; }
    public string FieldValue { get; set; }
    public int EffectiveYear { get; set; }
    public TelonaiSpecificField TelonaiSpecificField { get; set; }

}