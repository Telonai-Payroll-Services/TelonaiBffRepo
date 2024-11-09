namespace TelonaiWebApi.Entities;

public class Document : BaseTracker
{
    public Guid Id { get; set; }
    public string FileName { get; set; }
    public int DocumentTypeId { get; set; }
    public DocumentType DocumentType { get; set; }
    public bool IsDeleted { get; set; }
    public int PersonId { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public bool? IsConfirmed { get; set; }
}