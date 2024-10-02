namespace TelonaiWebApi.Models;

using System.Text.Json.Serialization;

public abstract class BaseTracker
{
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; }
    public DateTime UpdatedDate { get; set; }
    public string UpdatedBy { get; set; }
}