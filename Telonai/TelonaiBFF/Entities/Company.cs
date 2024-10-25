namespace TelonaiWebApi.Entities;

using System.Text.Json.Serialization;

public class Company:BaseTracker
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string TradeName { get; set; }
    public string TaxId { get; set; }
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }
    public string AddressLine3 { get; set; }
    public int ZipcodeId { get; set; }
    public int BusinessTypeId { get; set; }
    public string RegistrationNumber { get; set; }
    public Zipcode Zipcode { get; set; }
    public BusinessType BusinessType { get; set; }
}