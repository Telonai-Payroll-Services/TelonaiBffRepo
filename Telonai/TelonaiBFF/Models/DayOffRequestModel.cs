namespace TelonaiWebApi.Models;

using System.Text.Json.Serialization;
using TelonaiWebApi.Helpers;

public class DayOffRequestModel: BaseTracker
{
    public int Id { get; set; }
    public DayOffTypes DayOffType { get; set; }
    public DayOffPayTypeModel DayOffPayType { get; set; }
    public string Comment { get; set; }
    public int EmploymentId { get; set; }
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public bool? IsApproved { get; set; }
    public bool IsCancelled { get; set; }
    public string Name { get; set; }

    public string IsCancellable { get; set; }
}