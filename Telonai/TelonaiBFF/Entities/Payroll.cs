using System.Runtime.CompilerServices;

namespace TelonaiWebApi.Entities;

public class Payroll: BaseTracker
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int PayrollScheduleId { get; set; }

    public Company Company { get; set; }
    public DateOnly ScheduledRunDate { get; set; }   //This is the last date of this payroll period
    public DateOnly StartDate { get; set; }
    public DateTime? TrueRunDate { get; set; }
    public PayrollSchedule PayrollSchedule { get; set; }
    public double? EmployeesOwed { get; set; }
    public double? StatesOwed { get; set; }
    public double? FederalOwed { get; set; }
    public double? EmployeesPaid { get; set; }
    public double? StatesPaid { get; set; }
    public double? FederalPaid { get; set; }

}