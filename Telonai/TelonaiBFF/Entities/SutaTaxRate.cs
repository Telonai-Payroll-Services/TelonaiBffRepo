namespace TelonaiWebApi.Entities;

using System.Text.Json.Serialization;

public class SutaTaxRate : BaseTracker
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public double Rate { get; set; }
    public int StateId { get; set; }
    public Company Company { get; set; }
    public State State { get; set; }
}