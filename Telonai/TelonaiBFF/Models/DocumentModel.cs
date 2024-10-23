namespace TelonaiWebApi.Models;

public class DocumentModel: BaseTracker
{
    public Guid Id { get; set; }
    public string FileName { get; set; }
    public DocumentTypeModel DocumentType { get; set; }
    public int PersonId { get; set; }
    public DateOnly EffectiveDate { get; set; }
}