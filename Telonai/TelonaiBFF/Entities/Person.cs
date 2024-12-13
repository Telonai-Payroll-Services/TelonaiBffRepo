namespace TelonaiWebApi.Entities;

using System.Text.Json.Serialization;

public class Person : BaseTracker
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string MobilePhone { get; set; }
    public string OtherPhone { get; set; }
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }
    public string AddressLine3 { get; set; }
    public int? ZipcodeId { get; set; }
    public string Ssn { get; set; }
    public bool Deactivated { get; set; }
    public int CompanyId { get; set; }
    public string RoutingNumber { get; set; }
    public string BankAccountNumber { get; set; }
    public Company Company { get; set; }
    public virtual Zipcode Zipcode { get; set; }
    public virtual int INineVerificationStatusId { get; set; }
    public virtual INineVerificationStatus INineVerificationStatus { get; set; }
    public virtual int StateWithholdingDocumentStatusId { get; set; }
    public virtual StateWithholdingDocumentStatus StateWithholdingDocumentStatus { get; set; }
    public virtual int WfourWithholdingDocumentStatusId { get; set; }
    public virtual WfourWithholdingDocumentStatus WfourWithholdingDocumentStatus { get; set; }

}