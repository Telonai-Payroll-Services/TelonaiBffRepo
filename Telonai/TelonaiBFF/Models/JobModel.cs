namespace TelonaiWebApi.Models;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class JobModel 
{
    [Required]
    public int Id { get; set; }
    [Required]
    public string LocationName { get; set; }
    [Required]
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }
    public string AddressLine3 { get; set; }
    [Required]
    public int ZipcodeId { get; set; }
    [Required]
    public int CompanyId { get; set; }
    public string Company;
}