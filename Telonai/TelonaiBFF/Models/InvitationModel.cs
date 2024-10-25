namespace TelonaiWebApi.Models;


public class InvitationModel:BaseTracker
{
    public int? JobId { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string TaxId { get; set; }
    public string ActivationCode { get; set; }
    public string Job { get; set; }
    public string Company { get; set; }
    public string Country { get; set; }
    public int CountryId { get; set; }
    public string PhoneCountryCode { get; set; }
    public EmploymentModel Employment { get; set; }
    public DateTime CreatedDate { get; set; }
}