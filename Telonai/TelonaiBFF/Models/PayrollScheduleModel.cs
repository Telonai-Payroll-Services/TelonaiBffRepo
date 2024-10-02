namespace TelonaiWebApi.Models;

public class PayrollScheduleModel
{
    public int Id { get; set; }
    public string Compnay { get; set; }
    public int CompanyId { get; set; }
    public string PayrollScheduleType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime FirstRunDate { get; set; }
    public DateTime? EndDate { get; set; }
}