namespace TelonaiWebApi.Entities;

using System.Text.Json.Serialization;

public class CompanyContact: BaseTracker
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public int CompanyId { get; set; }
    public int ContacttypeId { get; set; }
    public Person person { get; set; }
    public Company Company { get; set; }
    public ContactType ContactType { get; set; }
}