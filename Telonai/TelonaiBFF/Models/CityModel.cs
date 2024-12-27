namespace TelonaiWebApi.Models;

using System.Text.Json.Serialization;

public class CityModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public StateModel State { get; set; }
    public List<CountryModel> Countys { get; set; }
}