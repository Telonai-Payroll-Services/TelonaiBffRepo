using Amazon.SimpleEmail.Model;
using System.Net.Mail;
using System.Text.Json.Serialization;
using TelonaiWebApi.Services;

namespace TelonaiWebApi.Helpers
{
    public class EmailSettings
    {
        [JsonPropertyName("fromEmail")]
        public string FromEmail { get; set; }
        [JsonPropertyName("smtpHost")]
        public string SmtpHost { get; set; }
        [JsonPropertyName("smtpPort")]
        public int SmtpPort { get; set; }
        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("password")]
        public string Password { get; set; }

    }
}
