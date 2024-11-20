
namespace TelonaiWebApi.Entities;


public class EmployerSubscription : BaseTracker
{   
    public int Id { get; set; }
    public Guid InvitationId { get; set; }
    public string CompanyAddress { get; set; }
    public string Zip { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public int SubscriptionTypeId { get; set; }
    public double Amount { get; set; }
    public ushort NumberOfEmployees { get; set; }
    public ulong RoutingNumber { get; set; }
    public ulong AccountNumber { get; set; }
    public ulong AccountNumber2 { get; set; }
    public bool PaymentProcessed { get; set; }
    public DateTime PaymentProcessedDate{ get; set; }
    public bool IsCancelled { get; set; }
    public string CancellationReason { get; set; }
    public ushort AgentCode { get; set; }

    public Invitation Invitation { get; set; }

}