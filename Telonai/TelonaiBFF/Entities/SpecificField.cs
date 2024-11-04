namespace TelonaiWebApi.Entities;

using System.Text.Json.Serialization;
using TelonaiWebApi.Models;

public class SpecificField
{
    public int Id { get; set; }
    public string FieldName { get; set; }
}

public sealed class StateSpecificField : SpecificField { }
public sealed class CompanySpecificField : SpecificField { }
public sealed class CountrySpecificField : SpecificField { }
