namespace TelonaiWebApi.Models;

public class FAQModel
{
    public int Id { get; set; }
    public string Question { get; set; }
    public string Answer { get; set; }

}

public partial class HelpPageResponse
{
    public List<FAQModel> Faqs { get; set; }

    public List<TelonaiSpecificFieldValueModel> TelonaiContact { get; set; }
}

