namespace TelonaiWebApi.Models;

using System.Text.Json.Serialization;

public class TelonaiSpecificFieldValueModel: BaseTracker
{
    public int Id { get; set; }
    public int FieldId { get; set; }
    public string FieldValue { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public TelonaiSpecificFieldModel TelonaiSpecificField { get; set; }

}