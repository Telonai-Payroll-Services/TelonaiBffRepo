namespace TelonaiWebApi.Helpers;

using Amazon.SecretsManager.Extensions.Caching;
using Amazon.SimpleEmail.Model;
using Amazon.SimpleEmail;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Net.Mail;
using Amazon;
using TelonaiWebApi.Helpers.Configuration;

public interface IMailSender
{
    Task SendUsingSmtpClientAsync(string to, string subjuct, string body);
    Task SendUsingAwsClientAsync(string to, string subject, string htmlBody, string textBody);
}

public class MailSender : IMailSender
{
    private readonly EmailSettings _emailOptions;
    private readonly SecretsManagerCache cache = new SecretsManagerCache();
    private readonly ILogger<MailSender> _logger;

    public MailSender(ILogger<MailSender> logger)
    {
        var secret = cache.GetSecretString("EmailSenderSettings").Result;
        _emailOptions = JsonConvert.DeserializeObject<EmailSettings>(secret);
        _logger = logger;
    }

    public async Task SendUsingSmtpClientAsync(string to, string subject, string body)
    {
        var message = new MailMessage
        {
            IsBodyHtml = true,
            From = new MailAddress(_emailOptions.FromEmail),
            Subject = subject,
            Body = body
        };

        message.To.Add(new MailAddress(to));

        using (var client = new SmtpClient(_emailOptions.SmtpHost, _emailOptions.SmtpPort))
        {
            client.Credentials = new NetworkCredential(_emailOptions.Username, _emailOptions.Password);
            client.EnableSsl = true;
            await client.SendMailAsync(message);
        }
    }

    public async Task SendUsingAwsClientAsync(string to, string subject, string htmlBody, string textBody)
    {
        var sendRequest = CreateRequest(to, subject, htmlBody, textBody);
        using var client = new AmazonSimpleEmailServiceClient(RegionEndpoint.USEast2);
        try
        {
            var result = await client.SendEmailAsync(sendRequest);
            _logger.LogInformation($"Mail Sent to: {to}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Mail Not Sent to {to} ." + ex.Message + ex.InnerException);
        }
    }


    private SendEmailRequest CreateRequest(string to, string subject, string htmlBody, string textBody)
    {
        return new SendEmailRequest
        {
            Source = _emailOptions.FromEmail,
            Destination = new Destination
            {
                ToAddresses = new List<string> { to }
            },
            Message = new Message
            {
                Subject = new Content(subject),
                Body = new Body
                {
                    Html = new Content
                    {
                        Charset = "UTF-8",
                        Data = htmlBody
                    },
                    Text = new Content
                    {
                        Charset = "UTF-8",
                        Data = textBody
                    }
                }
            },
            // If you are not using a configuration set, comment
            // or remove the following line 
            //ConfigurationSetName = configSet
        };
    }
}


    