using Org.BouncyCastle.Asn1.X509;

namespace TelonaiWebApi.Entities;

public class PayStub: BaseTracker
{
    public int Id { get; set; }
    public int PayrollId { get; set; }
    public int EmploymentId { get; set; }
    public Guid? DocumentId { get; set; }
    public int? OtherMoneyReceivedId { get; set; }

    public double RegularHoursWorked { get; set; }
    public double OverTimeHoursWorked { get; set; }
    public double GrossPay { get; set; }
    public double NetPay { get; set; }
    public double OverTimePay { get; set; }
    public double RegularPay { get; set; }
    public double AmountSubjectToAdditionalMedicareTax { get; set; }

    public double YtdRegularHoursWorked { get; set; }
    public double YtdOverTimeHoursWorked { get; set; }
    public double YtdGrossPay { get; set; }
    public double YtdNetPay { get; set; }
    public double YtdOverTimePay { get; set; }
    public double YtdRegularPay
    {
        get; set;
    }
    public bool IsCancelled { get; set; }
    public Employment Employment { get; set; }
    public Payroll Payroll { get; set; }
    public virtual Document Document { get; set; }
    public virtual OtherMoneyReceived OtherMoneyReceived { get; set; }

}