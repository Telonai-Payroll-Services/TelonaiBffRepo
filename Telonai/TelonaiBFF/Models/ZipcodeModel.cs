namespace TelonaiWebApi.Models;

using System.Text.Json.Serialization;

public class ZipcodeModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public CityModel City { get; set; }
}