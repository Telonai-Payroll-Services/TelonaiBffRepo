namespace TelonaiWebApi.Models;

using System.Text.Json.Serialization;

public class DepositScheduleModel:BaseTracker
{
    public int Id { get; set; }
    public int DepositScheduleTypeId { get; set; }
    public int CompanyId { get; set; }
    public DateOnly EffectiveDate { get; set; }

    public DepositScheduleTypeModel DepositScheduleType { get; set; }
    public CompanyModel Company { get; set; }

}