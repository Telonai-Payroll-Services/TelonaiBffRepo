using System.ComponentModel.DataAnnotations.Schema;

namespace TelonaiWebApi.Entities
{
    public class AgentFieldValue : BaseTracker
    {
        public int Id { get; set; }
        public int FieldId { get; set; }
        public int PersonId { get; set; }
        public string FieldValue { get; set; }
        public DateOnly EffectiveDate { get; set; }
        public DateOnly EndDate { get; set; }
        public Person Person { get; set; }

        [ForeignKey("FieldId")]
        public AgentField AgentField { get; set; }
    }
}
