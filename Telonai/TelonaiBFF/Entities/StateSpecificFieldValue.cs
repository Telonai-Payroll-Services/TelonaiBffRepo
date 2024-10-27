namespace TelonaiWebApi.Entities;


public class StateSpecificFieldValue:BaseTracker
{
    public int Id { get; set; }
    public int FieldId { get; set; }
    public int StateId { get; set; }
    public string FieldValue { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public State State { get; set; }
    public StateSpecificField StateSpecificField { get; set; }

}