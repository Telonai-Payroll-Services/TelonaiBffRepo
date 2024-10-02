namespace TelonaiWebApi.Models;

using System.ComponentModel.DataAnnotations;

public enum RoleTypeModel
{

    [Display(Name = "Owner")]
    Owner = 1,
    [Display(Name = "CEO")]
    CEO,
    [Display(Name = "President")]
    President,
    [Display(Name = "Vice-President")]
    VicePresident,
    [Display(Name = "Executive Director")]
    ExecutiveDirector,
    [Display(Name = "Managing Director")]
    ManagingDirector,
    [Display(Name = "Director")]
    Director,
    [Display(Name = "Manger")]
    Manager,
    [Display(Name = "Lead")]
    Lead,
    [Display(Name = "Non-managerial Employee")]
    Employee,
    [Display(Name = "Contractor")]
    Contractor
}