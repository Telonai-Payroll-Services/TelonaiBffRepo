namespace TelonaiWebApi.Models
{
    public class AgentFieldValueModel : BaseTracker
    {
        public int Id { get; set; }
        public int FieldId { get; set; }
        public int PersonId { get; set; }
        public string FieldValue { get; set; }
        public DateOnly EffectiveDate { get; set; }
        public DateOnly EndDate { get; set; }
    }
}
