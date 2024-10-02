namespace TelonaiWebApi.Models;

using System.ComponentModel.DataAnnotations;

public enum PayrollScheduleTypeModel
{
    [Display(Name = "Monthly")]
    Monthly = 1,
    [Display(Name = "Semi-Monthly")]
    SemiMonthly,
    [Display(Name = "Biweekly")]
    Biweekly,
    [Display(Name = "Weekly")]
    Weekly
}