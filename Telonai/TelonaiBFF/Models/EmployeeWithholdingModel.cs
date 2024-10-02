namespace TelonaiWebApi.Models;

public class EmployeeWithholdingModel: BaseTracker
{
    public Guid DocumentId { get; set; }

    public int FieldId { get; set; }
    public int EmploymentId { get; set; }

    public int WithholdingYear { get; set; }

    public string FieldValue { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public DocumentModel Document { get; set; }

}