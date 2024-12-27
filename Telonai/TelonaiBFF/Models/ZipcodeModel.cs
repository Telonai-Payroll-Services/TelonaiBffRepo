namespace TelonaiWebApi.Models;

using System.Text.Json.Serialization;
using TelonaiWebApi.Entities;

public class ZipcodeModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public List<CountyModel> Counties { get; set; }
    public CityModel City { get; set; }
}