namespace TelonaiWebApi.Entities;

using System.Text.Json.Serialization;

public class Job : BaseTracker
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string LocationName { get; set; }
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }
    public string AddressLine3 { get; set; }
    public int ZipcodeId { get; set; }
    public Company Company { get; set; }
    public Zipcode Zipcode { get; set; }
}