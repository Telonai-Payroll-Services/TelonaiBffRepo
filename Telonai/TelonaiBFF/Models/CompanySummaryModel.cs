namespace TelonaiWebApi.Models;

using System.Text.Json.Serialization;

public class CompanySummaryModel
{
    public int Id { get; set; }
    public string CompanyName { get; set; }
    public int NumberOfEmployees { get; set; }
    public int NumberOfLocations { get; set; }
    public List<PayrollModel> Payrolls { get; set; }
}