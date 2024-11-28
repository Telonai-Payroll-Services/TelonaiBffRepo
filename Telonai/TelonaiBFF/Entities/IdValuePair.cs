namespace TelonaiWebApi.Entities;

using System.Text.Json.Serialization;

public abstract class IdValuePair
{
    public int Id { get; set; }
    public string Value { get; set; }    
}

public class ContactType:IdValuePair{ }
public class BusinessType : IdValuePair { }
public class RoleType : IdValuePair { }
public class PayrollScheduleType : IdValuePair { }
public class PayrollDeductionType : IdValuePair { }
public class PayrollDeductionOwnerType : IdValuePair { }
public class PayRateBasis : IdValuePair { }
public class FilingStatus : IdValuePair { }
public class DocumentType : IdValuePair { }
public class SignUpStatusType : IdValuePair { }
public class INineVerificationStatus : IdValuePair { }
public class StateWithholdingDocumentStatus : IdValuePair { }
public class WfourWithholdingDocumentStatus : IdValuePair { }
public class DepositScheduleType : IdValuePair { }
public class CheckedBoxSixteenType : IdValuePair { }
public class QuarterType : IdValuePair { }
public class ExemptFromFutaTaxType : IdValuePair { }

public class BankAccountType : IdValuePair { }
