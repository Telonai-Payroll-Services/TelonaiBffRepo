namespace TelonaiWebApi.Entities;

using System.Text.Json.Serialization;

public class Invitation : BaseTracker
{
    public Guid Id { get; set; }
    public int? JobId { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string TaxId { get; set; }
    public virtual Job Job { get; set; }
    public DateTime ExpirationDate { get; set; }
    public int CountryId { get; set; }
    public Country Country { get; set; }
    public string CompanyName { get; set; }

}