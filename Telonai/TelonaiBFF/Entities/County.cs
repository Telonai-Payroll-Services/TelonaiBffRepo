namespace TelonaiWebApi.Entities;

public class County
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int StateId { get; set; }
    public int CountryId{ get; set; }

}