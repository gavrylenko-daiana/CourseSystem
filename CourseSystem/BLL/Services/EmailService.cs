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

                emailMessage.Subject = emailData.Subject;
                emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = emailData.Body
                };

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

        public async Task<Result<bool>> SendToTeacherCourseInvitation(AppUser teacher, Course course, string invitationUrl)
        {
            if (teacher == null || course == null)
            {
                return new Result<bool>(false, $"Fail to send email invitation to the teacher");
            }

            var emailContent = GetEmailSubjectAndBody(EmailType.CourseInvitation, teacher, course, invitationUrl);

            return await CreateAndSendEmail(new List<string> { teacher.Email }, emailContent.Item1, emailContent.Item2);
        }

        public async Task<Result<bool>> SendInvitationToStudents(Dictionary<string, string> studentsData, Group group)
        {
            try
            {
                foreach (var studentData in studentsData)
                {
                    var emailContent = GetEmailSubjectAndBody(EmailType.GroupInvitationToStudent, group,
                        studentData.Value, await _userManager.FindByEmailAsync(studentData.Key));
                    
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

        private async Task<Result<bool>> CreateAndSendEmail(List<string> toEmails, string subject, string body = null, string displayName = null)
        {
            if (toEmails.IsNullOrEmpty())
            {
                return new Result<bool>(false, "No emails to send data");
            }

            var emailData = new EmailData(toEmails, subject, body, displayName);

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

        private (string, string) GetEmailSubjectAndBody(EmailType emailType, AppUser appUser, string callBackUrl = null, string tempPassword = null)
        {
            if (appUser == null)
            {
                return (String.Empty, String.Empty);
            }

            var parameters = new Dictionary<string, object>();
            
            switch (emailType)
            {
                case EmailType.AccountApproveByAdmin:
                    parameters = new Dictionary<string, object>()
                    {
                        { @"{firstname}", appUser.FirstName },
                        { @"{lastname}", appUser.LastName },
                        { @"{email}", appUser.Email },
                        { @"{userrole}", appUser.Role },
                        { @"{callbackurl}", callBackUrl }
                    };
                    break;
                case EmailType.UserRegistration:
                    parameters = new Dictionary<string, object>()
                    {
                        { @"{firstname}", appUser.FirstName },
                        { @"{callbackurl}", callBackUrl }
                    };
                    break;
                case EmailType.ConfirmAdminRegistration:
                    parameters = new Dictionary<string, object>()
                    {
                        { @"{callbackurl}", callBackUrl }
                    };
                    break;
                case EmailType.ConfirmDeletionByAdmin:
                    parameters = new Dictionary<string, object>()
                    {
                        { @"{firstname}", appUser.FirstName },
                        { @"{lastname}", appUser.LastName },
                        { @"{email}", appUser.Email },
                        { @"{userrole}", appUser.Role },
                        { @"{callbackurl}", callBackUrl }
                    };
                    break;
                case EmailType.ConfirmDeletionByUser:
                    parameters = new Dictionary<string, object>()
                    {
                        { @"{firstname}", appUser.FirstName },
                        { @"{callbackurl}", callBackUrl }
                    };
                    break;
                case EmailType.AccountApproveByUser:
                    parameters = new Dictionary<string, object>()
                    {
                        { @"{firstname}", appUser.FirstName },
                        { @"{lastname}", appUser.LastName },
                        { @"{callbackurl}", callBackUrl }
                    };
                    break;
                case EmailType.GetTempPasswordToAdmin:
                    parameters = new Dictionary<string, object>()
                    {
                        { @"{firstname}", appUser.FirstName },
                        { @"{lastname}", appUser.LastName },
                        { @"{email}", appUser.Email },
                        { @"{userrole}", appUser.Role },
                        { @"{temppassword}", tempPassword },
                    };
                    break;
                default:
                    return (String.Empty, String.Empty);
            }

            return EmailTemplate.GetEmailSubjectAndBody(emailType, parameters);
        }

        private (string, string) GetEmailSubjectAndBody(EmailType emailType, Group group, string callBackUrl = null, AppUser appUser = null)
        {
            if (group == null)
            {
                return (String.Empty, String.Empty);
            }

            var parameters = new Dictionary<string, object>();
            
            var groupEmailTypes = new List<EmailType>()
            {
                EmailType.GroupConfirmationByAdmin,
                EmailType.ApprovedGroupCreation,
                EmailType.GroupInvitationToStudent
            };

            if (!groupEmailTypes.Contains(emailType))
            {
                return (String.Empty, String.Empty);
            }

            parameters = new Dictionary<string, object>()
            {
                { @"{groupname}", group.Name },
                { @"{callbackurl}", callBackUrl }
            };

            return EmailTemplate.GetEmailSubjectAndBody(emailType, parameters);
        }

        private (string, string) GetEmailSubjectAndBody(EmailType emailType, AppUser appUser, Course course, string callBackUrl = null)
        {
            if (appUser == null || course == null)
            {
                return (String.Empty, String.Empty);
            }

            var parameters = new Dictionary<string, object>();
            
            switch (emailType)
            {
                case EmailType.CourseInvitation:
                    if (course == null)
                    {
                        break;
                    }

                    parameters = new Dictionary<string, object>()
                    {
                        { @"{firstname}", appUser.FirstName },
                        { @"{coursename}", course.Name },
                        { @"{callbackurl}", callBackUrl }
                    };
                    break;
                default:
                    return (String.Empty, String.Empty);
            }

            return EmailTemplate.GetEmailSubjectAndBody(emailType, parameters);
        }
        
        private (string, string) GetEmailSubjectAndBody(EmailType emailType, AppUser appUser, IFormFile material, string callBackUrl = null)
        {
            if (appUser == null || material == null)
            {
                return (String.Empty, String.Empty);
            }

            var parameters = new Dictionary<string, object>();
            
            switch (emailType)
            {
                case EmailType.EducationMaterialApproveByAdmin:
                    parameters = new Dictionary<string, object>()
                    {
                        { @"{firstname}", appUser.FirstName },
                        { @"{lastname}", appUser.LastName },
                        { @"{email}", appUser.Email!},
                        { @"{materialname}", material.FileName },
                        { @"{material}", material.ContentType },
                        { @"{callbackurl}", callBackUrl }
                    };
                    break;
                case EmailType.ApprovedUploadEducationalMaterial:
                    break;
                default:
                    return (String.Empty, String.Empty);
            }

            return EmailTemplate.GetEmailSubjectAndBody(emailType, parameters);
        }

        public async Task<Result<bool>> SendEmailToAppUsers(EmailType emailType, AppUser appUser, string callBackUrl = null, string tempPassword = null)
        {
            if (appUser == null)
            {
                return new Result<bool>(false, "Fail to send email");
            }

            var emailContent = GetEmailSubjectAndBody(emailType, appUser, callBackUrl, tempPassword);

            var toEmail = new List<string>();
            var allAdmins = await _userManager.GetUsersInRoleAsync(AppUserRoles.Admin.ToString());

            switch (emailType)
            {
                case EmailType.ConfirmDeletionByAdmin:
                    toEmail = allAdmins.Select(a => a.Email).ToList();
                    break;
                case EmailType.AccountApproveByAdmin:
                    toEmail = allAdmins.Select(a => a.Email).ToList();
                    break;
                default:
                    toEmail.Add(appUser.Email);
                    break;
            }

            return await CreateAndSendEmail(toEmail, emailContent.Item1, emailContent.Item2);
        }

        public async Task<Result<bool>> SendEmailGroups(EmailType emailType, Group group, string callBackUrl = null, AppUser appUser = null)
        {
            if (group == null)
            {
                return new Result<bool>(false, "Fail to send email");
            }

            var emailContent = GetEmailSubjectAndBody(emailType, group, callBackUrl, appUser);

            var toEmail = new List<string>();
            var allAdmins = await _userManager.GetUsersInRoleAsync(AppUserRoles.Admin.ToString());

            switch (emailType)
            {
                case EmailType.GroupConfirmationByAdmin:
                    toEmail = allAdmins.Select(a => a.Email).ToList();
                    break;
                default:
                    toEmail.Add(appUser.Email);
                    break;
            }

            return await CreateAndSendEmail(toEmail, emailContent.Item1, emailContent.Item2);
        }
        
        public async Task<Result<bool>> SendEmailMaterial(EmailType emailType, AppUser appUser, IFormFile material, string callBackUrl = null!)
        {
            if (appUser == null)
            {
                return new Result<bool>(false, "Fail to send email");
            }

            var emailContent = GetEmailSubjectAndBody(emailType, appUser, material, callBackUrl);
            
            var toEmail = new List<string>();
            var allAdmins = await _userManager.GetUsersInRoleAsync(AppUserRoles.Admin.ToString());

            switch (emailType)
            {
                case EmailType.EducationMaterialApproveByAdmin:
                    toEmail = allAdmins.Select(a => a.Email).ToList();
                    break;
                default:
                    toEmail.Add(appUser.Email);
                    break;
            }
            
            return await CreateAndSendEmail(toEmail, emailContent.Item1, emailContent.Item2);
        }

    }
}