namespace TelonaiWebApi.Models;

using System.Text.Json.Serialization;

public class LoginResult
{
    public string FullName { get; set; }
    public List<EmploymentModel> Employments { get; set; }
    public TimecardUsaModel OpenTimeCard { get; set; }
    public List<DayOffRequestModel> DayOffRequests { get; set; }

    public SignInManagerResponse Error { get; set; }
}