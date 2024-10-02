namespace TelonaiWebApi.Models;

using System.Text.Json.Serialization;


public class WorkScheduleModel: BaseTracker
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public int JobId { get; set; }
    public DateOnly ScheduledDate { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get; set; }

    public bool? Accepted { get; set; }
    public string Job { get; set; }
    public string Person { get; set; }

}