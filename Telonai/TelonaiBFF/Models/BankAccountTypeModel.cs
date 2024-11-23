namespace TelonaiWebApi.Models;

using System.ComponentModel.DataAnnotations;

public enum BankAccountTypeModel
{
    [Display(Name = "Checking")]
    Checking =1,
    [Display(Name = "Saving")]
    Saving
}