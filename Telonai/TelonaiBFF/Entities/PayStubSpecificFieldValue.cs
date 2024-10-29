namespace TelonaiWebApi.Entities;

public class PayStubSpecificFieldValue : BaseTracker
{
    public int Id { get; set; }
    public int FieldId { get; set; }
    public int PayStubId { get; set; }
    public string FieldValue { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public PayStub PayStub { get; set; }
    public PayStubSpecificField PayStubSpecificField { get; set; }

}