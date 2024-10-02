namespace TelonaiWebApi.Models;

public class CompanyContactModel
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public int CompanyId { get; set; }
    public PersonModel Person { get; set; }
    public CompanyModel Company { get; set; }
    public string ContactType { get; set; }
}