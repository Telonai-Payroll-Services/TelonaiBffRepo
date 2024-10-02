namespace TelonaiWebApi.Entities;

using System.Text.Json.Serialization;

public class Holiday:BaseTracker
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateOnly Date { get; set; }
    public int CountryId { get; set; }
    public Country Country { get; set; }
}