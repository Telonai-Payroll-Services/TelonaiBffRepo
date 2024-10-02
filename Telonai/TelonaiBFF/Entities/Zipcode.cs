namespace TelonaiWebApi.Entities;

using System.Text.Json.Serialization;

public class Zipcode
{
    public int Id { get; set; }
    public string Code { get; set; }
    public int CityId { get; set; }
    public City City { get; set; }
}