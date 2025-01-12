namespace TelonaiWebApi.Models;

public class QuoteModel:BaseTracker
{
    public int AgentId { get; set; }

    public string CustomerEmail { get; set; }
    public string CustomerName { get; set; }

    public int DiscountPercentage { get; set; }
    public int NumberOfEmployees { get; set; }
    public int MonthlyCost { get; set; }

}