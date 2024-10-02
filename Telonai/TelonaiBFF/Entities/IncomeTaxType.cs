namespace TelonaiWebApi.Entities;

using System.Text.Json.Serialization;

public class IncomeTaxType
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int CountryId { get; set; }
    public int? StateId { get; set; }
    public bool ForEmployee { get; set; }
    public Country Country { get; set; }
    public virtual State State { get; set; }

}