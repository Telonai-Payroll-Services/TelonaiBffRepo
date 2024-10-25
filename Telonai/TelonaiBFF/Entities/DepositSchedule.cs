namespace TelonaiWebApi.Entities;

using System.Text.Json.Serialization;

public class DepositSchedule:BaseTracker
{
    public int Id { get; set; }
    public int DepositScheduleTypeId { get; set; }
    public int CompanyId { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public DepositScheduleType DepositScheduleType { get; set; }
    public Company Company { get; set; }

}