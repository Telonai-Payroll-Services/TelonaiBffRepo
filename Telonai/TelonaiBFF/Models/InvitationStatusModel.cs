namespace TelonaiWebApi.Models;

using System.Text.Json.Serialization;
using TelonaiWebApi.Entities;

public class InvitationStatusModel
{
    public string Email { get; set; }
    public string FullName { get; set; }
    public string Status { get; set; }

}