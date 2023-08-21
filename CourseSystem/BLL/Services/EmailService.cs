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
using Org.BouncyCastle.Asn1.Pkcs;

namespace BLL.Services
{
    public class EmailService : IEmailService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly EmailSettings _emailSettings;
        private readonly IUserService _userService;

        public EmailService(UserManager<AppUser> userManager,
            IOptions<EmailSettings> settings,
            IUserService userService)
        {
            _userManager = userManager;
            _emailSettings = settings.Value;
            _userService = userService;
        }

        public async Task<Result<int>> SendCodeToUser(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return new Result<int>(false, "Fail to generate code for email");
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

                return new Result<int>(true, randomCode);
            }
            catch (Exception ex)
            {
                return new Result<int>(false, "Fail to generate code for email");
            }
        }

        public async Task<Result<bool>> SendTempPasswordToUser(EmailType emailType, AppUser appUser)
        {
            if (appUser == null)
            {
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

            return new Result<bool>(true, $"{updateResult.Message}");
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

                    await client.DisconnectAsync(true);
                }

                #endregion

                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                return new Result<bool>(false, "Fail to send email");
            }
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

                return new Result<bool>(true, "Emails were sent to students");
            }
            catch (Exception ex)
            {
                return new Result<bool>(false, "Fail to send email to students");
            }
        }

        private async Task<Result<bool>> CreateAndSendEmail(List<string> toEmails, string subject, string body = null, IFormFile? file = null, string displayName = null)
        {
            if (toEmails.IsNullOrEmpty())
            {
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

                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
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

        public async Task<Result<bool>> SendEmailToAppUsers(EmailType emailType, AppUser appUser, string callBackUrl = null, string tempPassword = null,
             Group group = null, Course course = null, IFormFile? file = null)
        {
            if (appUser == null)
            {
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
    }
}