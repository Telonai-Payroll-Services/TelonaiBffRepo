namespace TelonaiWebApi.Models;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public enum FilingStatusTypeModel
{
    [Display(Name = "Single")]
    Single =1,
    [Display(Name = "Married Fliling Separately")]
    MarriedFlilingSeparately,
    [Display(Name = "Married Fliling Jointly")]
    MarriedFlilingJointly,
    [Display(Name = "Head of Household")]
    HeadOfHousehold,
    [Display(Name = "Qualifying Widow-er with Dependent Child")]
    QualifyingWidower
}