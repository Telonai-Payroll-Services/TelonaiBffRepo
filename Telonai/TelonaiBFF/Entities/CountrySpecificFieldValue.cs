namespace TelonaiWebApi.Entities;

using System.Text.Json.Serialization;

public class CountrySpecificFieldValue:BaseTracker
{
    public int Id { get; set; }
    public int CountrySpecificFieldId { get; set; }
    public string FieldValue { get; set; }
    public int EffectiveYear { get; set; }
    public CountrySpecificField CountrySpecificField { get; set; }

}