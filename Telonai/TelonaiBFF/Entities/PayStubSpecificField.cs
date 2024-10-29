namespace TelonaiWebApi.Entities;

using System.Text.Json.Serialization;

public class PayStubSpecificField
{
    public int Id { get; set; }
    public string FieldName { get; set; }
    public string Description { get; set; }
}