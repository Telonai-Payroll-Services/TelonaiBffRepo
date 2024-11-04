namespace TelonaiWebApi.Models;

using System.Text.Json.Serialization;

public class CountrySpecificFieldValueModel: BaseTracker
{
    public int Id { get; set; }
    public int FieldId { get; set; }
    public string FieldValue { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public CountrySpecificFieldModel CountrySpecificField { get; set; }

}