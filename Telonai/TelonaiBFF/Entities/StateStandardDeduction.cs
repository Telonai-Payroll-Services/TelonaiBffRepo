namespace TelonaiWebApi.Entities;

public class StateStandardDeduction
{
    public int Id { get; set; }
    public int StateId { get; set; }
    public int FilingStatusId { get; set; }
    public double Amount { get; set; }
    public State State { get; set; }
    public FilingStatus FilingStatus { get; set; }
    public int EffectiveYear { get; set; }
}