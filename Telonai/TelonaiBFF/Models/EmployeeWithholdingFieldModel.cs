namespace TelonaiWebApi.Models;

public class EmployeeWithholdingFieldModel
{
    public int Id { get; set; }
    public int CountryId { get; set; }
    public int? StateId { get; set; }
    public int WithholdingYear { get; set; }
    public int DocumentTypeId { get; set; }
    public string DocumentType { get; set; }
}