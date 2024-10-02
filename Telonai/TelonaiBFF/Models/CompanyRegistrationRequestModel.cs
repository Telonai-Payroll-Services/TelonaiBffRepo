namespace TelonaiWebApi.Models;

using System.Text.Json.Serialization;

public class CompanyRegistrationRequestModel
{
    public CompanyRequestModel Company { get; set; }
    public User Manager { get; set; }   
}