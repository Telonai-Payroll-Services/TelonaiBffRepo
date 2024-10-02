namespace TelonaiWebApi.Models;

using System.ComponentModel.DataAnnotations;

public enum PayRateBasisModel
{
    [Display(Name = "Hourly")]
    Hourly =1,
    [Display(Name = "Daily")]
    Daily,
    [Display(Name = "Weekly")]
    Weekly,
    [Display(Name = "Monthly")]
    Monthly,
    [Display(Name = "Annually")]
    Annually
}