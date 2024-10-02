namespace TelonaiWebApi.Entities;

public class PayrollSchedule: BaseTracker
{
    public int Id { get; set; }
    public Company Company { get; set; }
    public int CompanyId { get; set; }
    public PayrollScheduleType PayrollScheduleType { get; set; }
    public int PayrollScheduleTypeId { get; set; }

    //The date this schedule will become effective. Employees will be paid for their work starting from this date.
    public DateOnly StartDate { get; set; }
    //The date on which the first payrolll must be run
    public DateOnly FirstRunDate { get; set; }

    public DateOnly? EndDate { get; set; }
}