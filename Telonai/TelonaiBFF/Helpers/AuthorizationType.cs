namespace TelonaiWebApi.Helpers;

using System.ComponentModel.DataAnnotations;

public enum AuthorizationType
{
    [Display(Name = "User")]
    User = 1,
    [Display(Name = "Admin")]
    Admin,
    [Display(Name = "SystemAdmin")]
    SystemAdmin
}