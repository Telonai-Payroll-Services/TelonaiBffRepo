namespace TelonaiWebApi.Models;

using System.Text.Json.Serialization;

public class WorkScheduleRequestModel
{
    public int JobId { get; set; }
    public int PersonId { get; set; }
    public DateOnly ScheduledDate { get; set; }
    public string  StartTime { get; set; }
    public string EndTime { get; set; }

}