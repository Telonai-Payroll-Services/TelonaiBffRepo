namespace TelonaiWebApi.Entities;

using System.Text.Json.Serialization;

public class WorkSchedule: BaseTracker
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public Person Person { get; set; }
    public Job Job { get; set; }
    public int JobId { get; set; }
    public bool? Accepted { get; set; }
    public DateOnly ScheduledDate { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get; set; }

}