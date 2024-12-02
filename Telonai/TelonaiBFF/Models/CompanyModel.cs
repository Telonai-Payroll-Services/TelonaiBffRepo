namespace TelonaiWebApi.Models;

using System.Text.Json.Serialization;

public class CompanyModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string TradeName { get; set; }
    public string TaxId { get; set; }
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }
    public string AddressLine3 { get; set; }
    public int CityId { get; set; }
    public string City { get; set; }
    public int ZipcodeId { get; set; }
    public string ZipCode { get; set; }
    public string State { get; set; }
    public int StateId { get; set; }
    public string Country { get; set; }
    public BusinessTypeModel BusinessType { get; set; }
    public string RegistrationNumber { get; set; }
    public string ReprsentativeFirstName { get; set; }
    public string ReprsentativeLastName { get; set; }
    public string MobilePhone { get; set; }
}