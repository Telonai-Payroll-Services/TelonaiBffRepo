namespace TelonaiWebApi.Models;


public class TimecardUsaNoteModel: BaseTracker
{
    public int Id { get; set; }
    public int TimecardUsaId { get; set; }
    public string Note { get; set; }
}