namespace TelonaiWebApi.Entities;

public class TimecardUsaNote:BaseTracker
{
    public int Id { get; set; }
    public int TimecardUsaId { get; set; }
    public TimecardUsa TimecardUsa { get; set; }
    public string Note { get; set; }
}