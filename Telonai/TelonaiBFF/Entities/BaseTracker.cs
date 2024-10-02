namespace TelonaiWebApi.Entities;

using System.Text.Json.Serialization;

public interface IBaseTracker
{
    DateTime CreatedDate { get; set; }
    string CreatedBy { get; set; }
    DateTime? UpdatedDate { get; set; }
    string UpdatedBy { get; set; }
}

public abstract class BaseTracker: IBaseTracker
{
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string UpdatedBy { get; set; }
}