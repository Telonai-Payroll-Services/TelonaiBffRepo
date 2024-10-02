namespace TelonaiWebApi.Models;

using System.Text.Json.Serialization;

public class StateModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int CountryId { get; set; }
    public string Country { get; set; }
}