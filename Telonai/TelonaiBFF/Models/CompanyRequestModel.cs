namespace TelonaiWebApi.Models;

using System.Text.Json.Serialization;

public class CompanyRequestModel
{
    public string Name { get; set; }
    public string TaxId { get; set; }
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }
    public string AddressLine3 { get; set; }
    public int ZipCodeId { get; set; }
    public BusinessTypeModel BusinessType { get; set; }
    public string RegistrationNumber { get; set; }
}