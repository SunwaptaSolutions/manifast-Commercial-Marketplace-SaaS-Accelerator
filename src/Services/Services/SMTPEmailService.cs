using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using Marketplace.SaaS.Accelerator.DataAccess.Contracts;
using Marketplace.SaaS.Accelerator.Services.Contracts;
using Marketplace.SaaS.Accelerator.Services.Helpers;
using Marketplace.SaaS.Accelerator.Services.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using ILogger = Marketplace.SaaS.Accelerator.Services.Contracts.ILogger;

namespace Marketplace.SaaS.Accelerator.Services.Services;

/// <summary>
/// Service to send emails using SMTP settings.
/// </summary>
/// <seealso cref="IEmailService" />
public class SMTPEmailService : IEmailService
{
  /// <summary>
  /// The application configuration repository.
  /// </summary>
  // private readonly IApplicationConfigRepository applicationConfigRepository;

  private readonly ILogger<SMTPEmailService> Logger;

  /// <summary>
  /// Initializes a new instance of the <see cref="SMTPEmailService"/> class.
  /// </summary>
  /// <param name="logger"></param>
  public SMTPEmailService()
  {
    var loggerFactory = LoggerFactory.Create(builder =>
    {
      builder
        .AddConsole();
    });


    // this.applicationConfigRepository = applicationConfigRepository;
    Logger = loggerFactory.CreateLogger<SMTPEmailService>();
  }

  /// <summary>
  /// Sends the email.
  /// </summary>
  /// <param name="emailContent">Content of the email.</param>
  public void SendEmail(EmailContentModel emailContent)
  {
    SendViaSendGrid(emailContent);

    // MailMessage mail = new MailMessage();
    // if (!string.IsNullOrEmpty(emailContent.ToEmails) || !string.IsNullOrEmpty(emailContent.BCCEmails))
    // {
    //   mail.From = new MailAddress(emailContent.FromEmail);
    //   mail.IsBodyHtml = true;
    //   mail.Subject = emailContent.Subject;
    //   mail.Body = emailContent.Body;
    //
    //   string[] toEmails = emailContent.ToEmails.Split(';');
    //   foreach (string multimailid in toEmails)
    //   {
    //     mail.To.Add(new MailAddress(multimailid));
    //   }
    //
    //   if (!string.IsNullOrEmpty(emailContent.BCCEmails))
    //   {
    //     foreach (string multimailid1 in toEmails)
    //     {
    //       mail.Bcc.Add(new MailAddress(multimailid1));
    //     }
    //   }
    //
    //   SmtpClient smtp = new SmtpClient();
    //   smtp.Host = emailContent.SMTPHost;
    //   smtp.Port = emailContent.Port;
    //   smtp.UseDefaultCredentials = true;
    //   smtp.Timeout = 10000; // 10 seconds
    //   smtp.Credentials = new NetworkCredential(
    //       emailContent.UserName, emailContent.Password);
    //   smtp.EnableSsl = emailContent.SSL;
    //   try
    //   {
    //     smtp.Send(mail);
    //   }
    //   catch (Exception ex)
    //   {
    //     throw new Exception($"Email failed: {ex.Message}");
    //   }
    //   finally
    //   {
    //     mail.Dispose();
    //     smtp.Dispose();
    //   }
    // }
  }

  private bool SendViaSendGrid(EmailContentModel emailContent) // string sendGridApiKey, MailMessage message, out string errorMessage)
  {
    var sendGridApiKey = emailContent.Password;

    var msg = new SendGridMessage
    {
      From = emailContent.FromEmail.AsSendGridEmailAddress(),
      Subject = emailContent.Subject,
      HtmlContent = emailContent.Body  //htmlBody
    };
    msg.AddTos(emailContent.ToEmails.Split(new []{',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(a => a.AsSendGridEmailAddress()).ToList());

    var ccList = emailContent.CCEmails.Split(new[] {',', ';' }, StringSplitOptions.RemoveEmptyEntries);
    if (ccList.Length > 0)
    {
      msg.AddCcs(ccList.Select(a => a.AsSendGridEmailAddress()).ToList());
    }

    var bccList = emailContent.BCCEmails.Split(new[] {',', ';' }, StringSplitOptions.RemoveEmptyEntries);
    if (bccList.Length > 0)
    {
      msg.AddBccs(bccList.Select(a => a.AsSendGridEmailAddress()).ToList());
    }

    var sendGridClient = new SendGridClient(sendGridApiKey);
    var response = sendGridClient.SendEmailAsync(msg).Result;

    if (response.IsSuccessStatusCode)
    {
      return true;
    }

    // failed
    var details = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(response.Body.ReadAsStringAsync().Result);
    var errorList = details?.Select(x => $"{x.Key} = {x.Value}").JoinedAsString("\r\n");

    var fullSubject = $"EmailApi Failed ({Environment.MachineName})";
    var errorMessage = $"{fullSubject}\r\n{response.StatusCode}\r\n\r\n{errorList}";

    Logger.LogError(errorMessage);
    return false;

  }
}