using TelonaiWebApi.Models;

namespace TelonaiWebApi.Entities;

public class Employment: BaseTracker
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public int JobId { get; set; }
    public int? PayRateBasisId { get; set; }
    public double PayRate { get; set; }

    public bool IsPayrollAdmin { get; set; }
    //Employee is paid salary, and is exempt from overtime pay
    public bool IsSalariedOvertimeExempt { get; set; }
    public bool IsTenNinetyNine { get; set; }
    public Person Person { get; set; }
    public Job Job { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public virtual PayRateBasis PayRateBasis { get; set; }
    public int? SignUpStatusTypeId { get; set; }

    public virtual SignUpStatusType SignUpStatusType { get; set; }
    public bool Deactivated { get; set; }
    public string InternalEmployeeID { get; set; }

}