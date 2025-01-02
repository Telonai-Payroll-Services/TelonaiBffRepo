using System.ComponentModel.DataAnnotations.Schema;

namespace TelonaiWebApi.Entities;

public class TelonaiSpecificFieldValue
{
    public int Id { get; set; }
    public int FieldId { get; set; }
    public string FieldValue { get; set; }

    [ForeignKey("FieldId")]
    public TelonaiSpecificField TelonaiSpecificField { get; set; }
}
