namespace TelonaiWebApi.Models;

public enum SignUpStatusTypeModel
{
    None = 0,
    CompanyInvitationSent,
    CompanyInvitationAccepted,
    CompanyProfileCreationStarted,
    CompanyProfileCreationCompleted,
    PayrollScheduleCreationStarted,
    PayrollScheduleCreationCompleted,
    EmployeeAdditionStarted,
    EmployeeAddtionCompleted,
    UserAccountCreationStarted,
    UserAccountCreationCompleted,
    UserProfileCreationStarted,
    UserProfileCreationCompleted,
    UserSubmittedINine,
    UserSubmittedWFour,
    UserSubmittedStateFour
}