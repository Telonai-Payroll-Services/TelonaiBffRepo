namespace TelonaiWebApi.Models;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public enum ContactTypeModel
{
    [Display(Name = "First Contact")]
    FirstContact =1,
    [Display(Name = "Second Contact")]
    SecondContact,
    [Display(Name = "Third Contact")]
    ThirdContact,
    [Display(Name = "Fourth Contact")]
    FourthContact,
    [Display(Name = "Fifth Contact")]
    FifthContact
}