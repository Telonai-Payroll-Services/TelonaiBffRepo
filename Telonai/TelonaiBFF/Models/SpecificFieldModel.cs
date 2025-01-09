namespace TelonaiWebApi.Models;

public class SpecificFieldModel
{
    public int Id { get; set; }
    public string FieldName { get; set; }   
}

public sealed class StateSpecificFieldModel : SpecificFieldModel { }
public sealed class CompanySpecificFieldModel : SpecificFieldModel { }
public sealed class CountrySpecificFieldModel : SpecificFieldModel { }
public sealed class AgentFieldModel : SpecificFieldModel { }
public sealed class TelonaiSpecificFieldModel : SpecificFieldModel { }
