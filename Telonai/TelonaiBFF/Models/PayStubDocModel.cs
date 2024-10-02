namespace TelonaiWebApi.Models;

public class PayStubDocModel: BaseTracker
{
    public Guid Id { get; set; }
    public int PayStubId { get; set; }
    public string FileName { get; set; }
}