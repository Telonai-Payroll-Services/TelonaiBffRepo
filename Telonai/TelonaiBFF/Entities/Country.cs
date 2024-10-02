namespace TelonaiWebApi.Entities;

using System.Text.Json.Serialization;

public class Country
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string CountryCode { get; set; }
    public string PhoneCountryCode { get; set; }

}