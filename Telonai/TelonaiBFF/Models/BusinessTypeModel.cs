namespace TelonaiWebApi.Models;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public enum BusinessTypeModel
{
    [Display(Name = "Sole Trader")]
    SoleTrader =1,
    [Display(Name = "Partnership")]
    Partnership,
    [Display(Name = "LLC")]
    LLC,
    [Display(Name = "S-CORP")]
    SCORP,
    [Display(Name = "C-CORP")]
    CCORP
}