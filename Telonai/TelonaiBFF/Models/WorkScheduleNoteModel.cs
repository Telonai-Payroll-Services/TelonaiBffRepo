namespace TelonaiWebApi.Models;


public class WorkScheduleNoteModel: BaseTracker
{
    public int Id { get; set; }
    public int WorkScheduleId { get; set; }
    public string WorkSchedule { get; set; }
    public string Note { get; set; }
}