namespace TelonaiWebApi.Models;

using System.ComponentModel.DataAnnotations;

public enum DocumentTypeModel
{
    [Display(Name = "PayStub")]
    PayStub = 1,
    [Display(Name = "I9-Unsigned")]
    INineUnsigned,
    [Display(Name = "W4-Unsigned")]
    WFourUnsigned,
    [Display(Name = "NC4-Unsigned")]
    NCFourUnsigned,
    [Display(Name = "I-9")]
    INine,
    [Display(Name = "W-4")]
    WFour,
    [Display(Name = "NC-4")]
    NCFour,
    [Display(Name = "8879-EMP")]
    EightyEightSeventyNineEMP,
    [Display(Name = "940")]
    NineForty,
    [Display(Name = "941")]
    NineFortyOne,
    [Display(Name = "Form 8879-EMP")]
    EightyEightSeventyNineEmpUnsigned
}