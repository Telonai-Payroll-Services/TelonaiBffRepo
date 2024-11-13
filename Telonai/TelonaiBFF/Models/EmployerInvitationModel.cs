namespace TelonaiWebApi.Models;


public class EmployerInvitationModel : BaseTracker
{   public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string TaxId { get; set; }
    public string Company { get; set; }
    public string CompanyAddress { get; set; }
    public string Zip { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Country { get; set; }
    public int CountryId { get; set; }
    public int NumberOfEmployees { get; set; }
    public int RoutingNumber { get; set; }
    public int AccountNumber { get; set; }
    public int AccountNumber2 { get; set; }

}