namespace TelonaiWebApi.Models;

using System.ComponentModel.DataAnnotations;

public class PersonModel
{
    public int Id { get; set; }

    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    public string FullName { get; set; }

    [EmailAddress]
    public string Email { get; set; }
    [MinLength(9)]
    public string MobilePhone { get; set; }

    public string OtherPhone { get; set; }

    public string AddressLine1 { get; set; }

    public string AddressLine2 { get; set; }
    public string AddressLine3 { get; set; }

    public int CityId { get; set; }
    public string City { get; set; }

    public int StateId { get; set; }
    public string State { get; set; }

    public int CountyId { get; set; }
    public string County { get; set; }

    public int CountryId { get; set; }
    public string Country { get; set; }
    public string Ssn { get; set; }

    public bool Deactivated { get; set; }
    public int CompanyId { get; set; }
    public string Company { get; set; }
    public int ZipcodeId { get; set; }
    public string Zipcode { get; set; }
    public bool IsTwopercentshareholder { get; set; }
    public string RoutingNumber { get; set; }
    public string BankAccountNumber { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public INineVerificationStatusModel INineVerificationStatus { get; set; }
    public StateWithholdingDocumentStatusModel StateWithholdingDocumentStatus { get; set; }
    public WFourWithholdingDocumentStatusModel WFourWithholdingDocumentStatus { get; set; }
    public string InternalEmployeeId { get; set; }

}