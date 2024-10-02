namespace TelonaiWebApi.Models;

using System.Text.Json.Serialization;

public class PayrollRunModel
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public PayrollScheduleModel PayrollSchedule { get; set; }
    public DateTime RunDate { get; set; }
    public DateTime? NextRunDate { get; set; }
}