using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TelonaiWebApi.Models;


public class EmployerInvitationModel : BaseTracker
{   
    public int Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string TaxId { get; set; }
    public string Company { get; set; }
    public string CompanyAddress { get; set; }
    public string Zip { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public ushort NumberOfEmployees { get; set; }
    public ulong RoutingNumber { get; set; }
    public ulong AccountNumber { get; set; }
    public ulong AccountNumber2 { get; set; }
    public ushort AgentCode { get; set; }

}