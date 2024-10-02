namespace TelonaiWebApi.Entities;

public class EmployeeWithholding: BaseTracker
{
    public int Id { get; set; }
    public Guid DocumentId { get; set; }

    public int FieldId { get; set; }
    public int EmploymentId { get; set; }
    public int WithholdingYear { get; set; }

    public string FieldValue { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public EmployeeWithholdingField Field { get; set; }
    public Employment Employment { get; set; }
    public Document Document { get; set; }
    public bool Deactivated { get; set; }

}