namespace TelonaiWebApi.Entities;

public class PayrollRun: BaseTracker
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int PayrollScheduleId { get; set; }

    public Company Company { get; set; }
    public DateOnly ScheduledRunDate { get; set; }
    public DateOnly StartDate { get; set; }
    public DateTime? TrueRunDate { get; set; }
    public PayrollSchedule PayrollSchedule { get; set; }

}