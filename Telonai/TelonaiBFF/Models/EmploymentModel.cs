
namespace TelonaiWebApi.Models;

public class EmploymentModel:BaseTracker
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public int JobId { get; set; }
    public int CompanyId { get; set; }

    public int? PayRateBasisId { get; set; }
    public double PayRate { get; set; }

    public bool IsPayrollAdmin { get; set; }
    //Employee receives salary, and is exempt from overtime pay
    public bool IsSalariedOvertimeExempt { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }   
    public string Person { get; set; }
    public JobModel Job { get; set; }
    public string RoleType { get; set; }
    public string Company { get; set; }
    public bool IsTenNinetyNine { get; set; }
    public SignUpStatusTypeModel SignUpStatusType { get; set; }
    public bool Deactivated { get; set; }
    public int WithholdingDueInDays { get; set; } = 3;
    public string InternalEmployeeID { get; set; }

}