namespace TelonaiWebApi.Entities;

public class EmployeeWithholdingField
{
    public int Id { get; set; }
    public int CountryId { get; set; }
    public int? StateId { get; set; }
    public int WithholdingYear { get; set; }
    public int DocumentTypeId { get; set; }
    public string FieldName { get; set; }
    public Country Country { get; set; }
    public virtual State State { get; set; }
    public DocumentType DocumentType { get; set; }    
}