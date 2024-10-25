namespace TelonaiWebApi.Models;

using System.Text.Json.Serialization;

public class CompanySpecificFieldValueModel:BaseTracker
{
    public int Id { get; set; }
    public int FieldId { get; set; }
    public int CompanyId { get; set; }
    public string FieldValue { get; set; }
    public DateOnly EffectiveDate { get; set; }
}