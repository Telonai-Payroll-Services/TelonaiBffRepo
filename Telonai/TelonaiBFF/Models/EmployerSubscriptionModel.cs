using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TelonaiWebApi.Models;


public class EmployerSubscriptionModel : BaseTracker
{   
    public int Id { get; set; }
    public int InvitationId { get; set; }
    public string CompanyAddress { get; set; }
    public string Zip { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public ushort NumberOfEmployees { get; set; }
    public ulong RoutingNumber { get; set; }
    public ulong AccountNumber { get; set; }
    public ulong AccountNumber2 { get; set; }
    public bool PaymentProcessed { get; set; }
    public DateTime PaymentProcessedDate{ get; set; }
    public ushort AgentCode { get; set; }

}