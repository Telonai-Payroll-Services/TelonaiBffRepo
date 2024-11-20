namespace TelonaiWebApi.Models;

using System.ComponentModel.DataAnnotations;

public enum SubscriptionTypeModel
{
    [Display(Name = "Monthly")]
    Monthly =1,
    [Display(Name = "Annually")]
    Annually
}