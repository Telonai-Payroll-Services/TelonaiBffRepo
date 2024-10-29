namespace TelonaiWebApi.Entities;

using System.Text.Json.Serialization;

public class CompanySpecificFieldValue:BaseTracker
{
    public int Id { get; set; }
    public int FieldId { get; set; }
    public int CompanyId { get; set; }
    public string FieldValue { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public Company Company { get; set; }
    public CompanySpecificField CompanySpecificField { get; set; }

}