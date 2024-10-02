namespace TelonaiWebApi.Entities;

public class WorkScheduleNote:BaseTracker
{
    public int Id { get; set; }
    public string Note { get; set; }
    public string WorkScheduleId { get; set; }
    public WorkSchedule WorkSchedule { get; set; }

}