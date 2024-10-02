namespace TelonaiWebApi.Models;

using System.Text.Json.Serialization;


public class TimecardUsaModel: BaseTracker
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public int JobId { get; set; }
    public DateTime? ClockOut { get; set; }
    public DateTime ClockIn { get; set; }
    public string Job { get; set; }
    public TimeSpan? HoursWorked { get; set; }
    public bool IsApproved { get; set; }
    public bool IsLocked { get; set; }
}