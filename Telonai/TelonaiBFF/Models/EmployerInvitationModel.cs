using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TelonaiWebApi.Models;


public class EmployerInvitationModel
{   
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string TaxId { get; set; }
    public string Company { get; set; }
    public string Address { get; set; }
    public ushort Zip { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public ushort NumberOfEmployees { get; set; }
    public SubscriptionTypeModel SubscriptionType { get; set; }
    public ulong RoutingNumber { get; set; }
    public ulong AccountNumber { get; set; }
    public ulong AccountNumber2 { get; set; }
}