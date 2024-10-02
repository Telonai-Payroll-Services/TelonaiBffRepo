namespace TelonaiWebApi.Models;

public class PayStubModel: BaseTracker
{
    public int Id { get; set; }
    public int PayrollId { get; set; }
    public int EmploymentId { get; set; }

    public double RegularHoursWorked { get; set; }
    public double OverTimeHoursWorked { get; set; }
    public double GrossPay { get; set; }
    public double NetPay { get; set; }
    public double OverTimePay { get; set; }
    public double RegularPay { get; set; }
    
    public EmploymentModel Employment { get; set; }
    public PayrollModel Payroll { get; set; }
}