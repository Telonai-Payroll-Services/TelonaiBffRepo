namespace TelonaiWebApi.Models;

using System.Text.Json.Serialization;

public class CountryModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string CountryCode { get; set; }
    public string PhoneCountryCode { get; set; }

}