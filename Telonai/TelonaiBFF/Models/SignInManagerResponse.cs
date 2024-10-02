namespace TelonaiWebApi.Models;

using System.Text.Json.Serialization;

public enum SignInManagerResponse
{
    LoginSucceeded,
    RequiresTwoFactor,
    RequiresPasswordReset,
    RequiresPasswordChange,
    InvalidCredentials,
    UserIsNotConfirmed,
    UnknownError
}