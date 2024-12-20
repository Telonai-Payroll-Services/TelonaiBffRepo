namespace TelonaiWebApi.Entities;

using System.Text.Json.Serialization;

public class DayOffRequest: BaseTracker
{
    public int Id { get; set; }
    public int DayOffTypeId { get; set; }
    public int DayOffPayTypeId { get; set; }
    public string Comment { get; set; }
    public int EmploymentId { get; set; }
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public bool? IsApproved { get; set; }
    public bool IsCancelled { get; set; }
    public Employment Employment { get; set; }
    public DayOffType dayOffType { get; set; }
}