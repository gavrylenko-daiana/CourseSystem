using BLL.Interfaces;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;
using Microsoft.AspNetCore.Identity;
using Core.Models;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Options;
using Core.Configuration;
using MailKit.Security;
using System.Runtime;
using Core.Enums;
using Core.EmailTemplates;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.Net.Mail;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Pkcs;

namespace BLL.Services
{
    public class EmailService : IEmailService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly EmailSettings _emailSettings;
        private readonly IUserService _userService;
        private readonly ILogger<EmailService> _logger;

        public EmailService(UserManager<AppUser> userManager, IOptions<EmailSettings> settings,
            IUserService userService, ILogger<EmailService> logger)
        {
            _userManager = userManager;
            _emailSettings = settings.Value;
            _userService = userService;
            _logger = logger;
        }

        public async Task<Result<int>> SendCodeToUser(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogError("Failed to {action}. Email is null or white space", 
                    MethodBase.GetCurrentMethod()?.Name);
                
                return new Result<int>(false, "Email is null or white space");
            }

            int randomCode = new Random().Next(1000, 9999);

            try
            {
                var subjectAndBody = EmailTemplate.GetEmailSubjectAndBody(EmailType.CodeVerification,
                    new Dictionary<string, object>() { { @"{randomcode}", randomCode } });

                var emailData = new EmailData(
                    new List<string>() { email },
                    subjectAndBody.Item1,
                    subjectAndBody.Item2
                );

                var result = await SendEmailAsync(emailData);

                if (!result.IsSuccessful)
                {
                    return new Result<int>(false, 0);
                }
                
                _logger.LogInformation("Successfully {action} for {email}", MethodBase.GetCurrentMethod()?.Name, email);

                return new Result<int>(true, randomCode);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to {action} for {email}. Error: {errorMsg}", 
                    MethodBase.GetCurrentMethod()?.Name, email, ex.Message);
                
                return new Result<int>(false, "Fail to generate code for email");
            }
        }

        public async Task<Result<bool>> SendTempPasswordToUser(EmailType emailType, AppUser appUser)
        {
            if (appUser == null)
            {
                _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(appUser));

                return new Result<bool>(false, $"{nameof(appUser)} not found");
            }

            var tempPassword = _userService.GenerateTemporaryPassword();
            var updateResult = await _userService.UpdatePasswordAsync(appUser.Email, tempPassword);

            if (!updateResult.IsSuccessful)
            {
                return new Result<bool>(false, $"{updateResult.Message}");
            }

            var sendResult = await SendEmailToAppUsers(emailType, appUser, tempPassword: tempPassword);

            if (!sendResult.IsSuccessful)
            {
                return new Result<bool>(false, $"{sendResult.Message}");
            }
            
            _logger.LogInformation("Successfully {action} for {userName} with email type {type}", 
                MethodBase.GetCurrentMethod()?.Name, appUser.FirstName, emailType);

            return new Result<bool>(true, $"{updateResult.Message}");
        }

        public async Task<Result<bool>> SendInvitationToStudents(Dictionary<string, string> studentsData, Group group)
        {
            try
            {
                foreach (var studentData in studentsData)
                {
                    var emailContent = GetEmailSubjectAndBody(EmailType.GroupInvitationToStudent, 
                        await _userManager.FindByEmailAsync(studentData.Key), group, callBackUrl: studentData.Value);
                    
                    var result = await CreateAndSendEmail(new List<string> { studentData.Key }, emailContent.Item1, emailContent.Item2);

                    if (!result.IsSuccessful)
                    {
                        return new Result<bool>(false, result.Message);
                    }
                }
                
                _logger.LogInformation("Successfully {action} in group {groupName}", MethodBase.GetCurrentMethod()?.Name, group.Name);

                return new Result<bool>(true, "Emails were sent to students");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to {action} in group {groupName}. Error: {errorMsg}", 
                    MethodBase.GetCurrentMethod()?.Name, group.Name, ex.Message);
                
                return new Result<bool>(false, "Fail to send email to students");
            }
        }
        
        public async Task<Result<bool>> SendEmailToAppUsers(EmailType emailType, AppUser appUser, string callBackUrl = null, string tempPassword = null,
            Group group = null, Course course = null, IFormFile? file = null)
        {
            if (appUser == null)
            {
                _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(appUser));

                return new Result<bool>(false, "Fail to send email");
            }

            var emailContent = GetEmailSubjectAndBody(emailType, appUser, group, course, file, callBackUrl, tempPassword);

            var allAdmins = await _userManager.GetUsersInRoleAsync(AppUserRoles.Admin.ToString());
            var toEmail = allAdmins.Select(a => a.Email).ToList();

            if (emailType.ToString().ToLower().Contains("admin"))
            {
                return await CreateAndSendEmail(toEmail, emailContent.Item1, emailContent.Item2, file);
            }
            else
            {
                return await CreateAndSendEmail(new List<string> { appUser.Email}, emailContent.Item1, emailContent.Item2, file);
            }
        }

        private async Task<Result<bool>> CreateAndSendEmail(List<string> toEmails, string subject, string body = null, IFormFile? file = null, 
            string displayName = null)
        {
            if (toEmails.IsNullOrEmpty())
            {
                _logger.LogError("Failed to {action}. No emails to send data", MethodBase.GetCurrentMethod()?.Name);

                return new Result<bool>(false, "No emails to send data");
            }

            var emailData = new EmailData(toEmails, subject, body, displayName, file);

            try
            {
                var result = await SendEmailAsync(emailData);

                if (!result.IsSuccessful)
                {
                    return new Result<bool>(false, result.Message);
                }

                _logger.LogInformation("Successfully {action} with subject {subject}", MethodBase.GetCurrentMethod()?.Name, subject);
                
                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to {action} with subject {subject}. Error: {errorMsg}", 
                    MethodBase.GetCurrentMethod()?.Name, subject, ex.Message);
                
                return new Result<bool>(false, "Fail to send email");
            }
        }

        private (string, string) GetEmailSubjectAndBody(EmailType emailType, AppUser appUser, Group group = null, 
            Course course = null, IFormFile material = null, string callBackUrl = null, string tempPassword = null)
        {
            if (appUser == null)
            {
                return (String.Empty, String.Empty);
            }

            var parameters  = new Dictionary<string, object>()
            {
                { @"{firstname}", appUser.FirstName },
                { @"{lastname}", appUser.LastName },
                { @"{email}", appUser.Email },
                { @"{userrole}", appUser.Role },
                { @"{callbackurl}", callBackUrl },
                { @"{groupname}", group?.Name },
                { @"{coursename}", course?.Name },
                { @"{materialname}", material?.FileName },
                { @"{material}", material?.ContentType },
                {@"{temppassword}", tempPassword }
                
            };

            return EmailTemplate.GetEmailSubjectAndBody(emailType, parameters);
        }
        
        private async Task<Result<bool>> SendEmailAsync(EmailData emailData)
        {
            try
            {
                var emailMessage = new MimeMessage();

                emailMessage.From.Add(new MailboxAddress(_emailSettings.DisplayName, _emailSettings.From));

                foreach (string emailToAddress in emailData.To)
                {
                    emailMessage.To.Add(MailboxAddress.Parse(emailToAddress));
                }

                #region Content
                var body = new BodyBuilder();
                emailMessage.Subject = emailData.Subject;
                body.HtmlBody = emailData.Body;

                if (emailData.Attachment != null)
                {
                    byte[] attachmentFileByteArray;

                    if (emailData.Attachment.Length > 0)
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            emailData.Attachment.CopyTo(memoryStream);
                            attachmentFileByteArray = memoryStream.ToArray();
                        }

                        body.Attachments.Add(emailData.Attachment.FileName, attachmentFileByteArray, ContentType.Parse(emailData.Attachment.ContentType));
                    }
                }

                emailMessage.Body = body.ToMessageBody();
                #endregion

                #region Email sending

                using (var client = new SmtpClient())
                {
                    if (_emailSettings.UseSSL)
                    {
                        await client.ConnectAsync(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.SslOnConnect);
                    }
                    else if (_emailSettings.UseStartTls)
                    {
                        await client.ConnectAsync(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.StartTls);
                    }

                    await client.AuthenticateAsync(_emailSettings.UserName, _emailSettings.Password); //ключ доступа от Гугл
                    await client.SendAsync(emailMessage);

                    _logger.LogInformation("Successfully send email");

                    await client.DisconnectAsync(true);
                }

                #endregion

                _logger.LogInformation("Successfully {action} with {emailData}", MethodBase.GetCurrentMethod()?.Name, nameof(emailData));
                
                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to {action} with {emailData}. Error: {errorMsg}", 
                    MethodBase.GetCurrentMethod()?.Name, nameof(emailData), ex.Message);
                
                return new Result<bool>(false, "Fail to send email");
            }
        }
    }
}