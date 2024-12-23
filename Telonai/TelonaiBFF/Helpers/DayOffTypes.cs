using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TelonaiWebApi.Helpers;


public enum DayOffTypes
{
    [Description("Sick Leave")]
    SickLeave = 1,
    [Description("Bereavement")]
    Bereavement,
    [Description("Parental Leave")]
    ParentalLeave,
    [Description("Vacation")]
    Vacation,
    [Description("Holiday")]
    Holiday,
    [Description("Jury Duty")]
    JuryDuty,
    [Description("Voting Day")]
    VotingDay,
    [Description("Military Leave")]
    MilitaryLeave,
    [Description("Other")]
    Other
}