namespace TelonaiWebApi.Models;

using System.Text.Json.Serialization;

public class StateSpecificFieldValueModel: BaseTracker
{
    public int Id { get; set; }
    public int FieldId { get; set; }
    public int StateId { get; set; }
    public string FieldValue { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public StateModel State { get; set; }
    public StateSpecificFieldModel StateSpecificField { get; set; }

}