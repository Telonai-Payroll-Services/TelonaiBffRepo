namespace TelonaiWebApi.Models;

using System.Text.Json.Serialization;

public class PayrollModel
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public PayrollScheduleModel PayrollSchedule { get; set; }
    public DateTime? TrueRunDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime ScheduledRunDate { get; set; }
    public double? EmployeesOwed { get; set; }
    public double? StatesOwed { get; set; }
    public double? FederalOwed { get; set; }
    public double? EmployeesPaid { get; set; }
    public double? StatesPaid { get; set; }
    public double? FederalPaid { get; set; }
    public string ExpenseTrackingHexColor { get; set; }
    public double? GrossPay { get; set; }

}