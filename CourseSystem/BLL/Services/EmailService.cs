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
using Microsoft.IdentityModel.Tokens;

namespace BLL.Services
{
    public class EmailService : IEmailService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly EmailSettings _emailSettings;


        public EmailService(UserManager<AppUser> userManager,
            IOptions<EmailSettings> settings)
        {
            _userManager = userManager;
            _emailSettings = settings.Value;
        }
              
        public async Task<int> SendCodeToUser(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) 
                throw new ArgumentNullException(nameof(email));

            int randomCode = new Random().Next(1000, 9999);

            try
            {
                var subjectAndBody = EmailTemplate.GetEmailSubjectAndBody(EmailType.CodeVerification, 
                    new Dictionary<string, object>() { { @"{randomCode}", randomCode } });
                
                var emailData = new EmailData(
                    new List<string>() { email},
                    subjectAndBody.Item1,
                    subjectAndBody.Item2
                    );

                var result = await SendEmailAsync(emailData);

                if(!result.IsSuccessful)
                {
                    return 0;
                }

                return randomCode;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task<Result<bool>> SendEmailAsync(EmailData emailData)
        {
            try
            {
                var emailMessage = new MimeMessage();

                emailMessage.From.Add(new MailboxAddress(_emailSettings.DisplayName, _emailSettings.From));

                foreach (string emailToAdress in emailData.To)
                    emailMessage.To.Add(MailboxAddress.Parse(emailToAdress));

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


        public async Task<Result<bool>> ConfirmUserDeletionByAdmin(AppUser userForDelete, string callbackUrl)
        {
            var allAdmins = await _userManager.GetUsersInRoleAsync(AppUserRoles.Admin.ToString());

            if(allAdmins.Any())
            {
                #region Email body creation

                var userData = new StringBuilder()
                    .AppendLine($"<h4>User data overview</h4>" +
                    $"<hr/>" +
                    $"<p>User first name: {userForDelete.FirstName}</p>" +
                    $"<p>User last name: {userForDelete.LastName}</p>" +
                    $"<p>Date of birth: {userForDelete.BirthDate.ToString("d")}</p>" +
                    $"<p>User email: {userForDelete.Email}</p>" +
                    $"<p>User role: {userForDelete.Role}</p>");

                userData.AppendLine($"<h4>Confirm deletion of {userForDelete.FirstName} {userForDelete.LastName}, follow the link: <a href='{callbackUrl}'>link</a></h4>");
                #endregion
                try
                {
                    var allAdminsEmails = allAdmins.Select(a => a.Email).ToList();
                    var emailData = new EmailData(
                        allAdminsEmails,
                        "Confirm deletion of user account",
                        userData.ToString());

                    var result = await SendEmailAsync(emailData);

                    if (!result.IsSuccessful)
                        return new Result<bool>(false, result.Message);

                    return new Result<bool>(true);
                }
                catch (Exception ex)
                {
                    return new Result<bool>(false, $"Fail to send email to {userForDelete.Email}");
                }
            }

            return new Result<bool>(false, $"Fail to send email to {userForDelete.Email}");
        }

        public async Task<Result<bool>> ConfirmUserDeletionByUser(AppUser userForDelete, string logOutLink)
        {
            if (userForDelete == null)
                return new Result<bool>(false, "Invalid user for delete");

            #region Email body creation
            var emailBody = new StringBuilder().AppendLine($"<h4>Dear {userForDelete.FirstName}, your deletion was successfully approved by admin</h4>");
            var buttonToUserProfileDetails = $"<h4>Confirm your deletion, follow the link: <a href='{logOutLink}'>link</a></h4>";

            emailBody.AppendLine(buttonToUserProfileDetails);
            #endregion

            var emailData = new EmailData(
                new List<string> { userForDelete.Email },
                "Successful deletion approve",
                emailBody.ToString());
            try
            {
                var result = await SendEmailAsync(emailData);

                if (!result.IsSuccessful)
                    return new Result<bool>(false, result.Message);

                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Fail to send email about successful registration to user {userForDelete.Email} ");
            }
        }

        public async Task<Result<bool>> SendToTeacherCourseInventation(AppUser teacher, Course course, string inventationUrl)
        {
            if (teacher == null || course == null)
                return new Result<bool>(false, $"Fail to send email inventation to the techer");

            #region Body email creation
            var emailBody = new StringBuilder().AppendLine($"<h4>Dear {teacher.FirstName}, you were invited to the course {course.Name}</h4>");
            emailBody.AppendLine("<h5>Сourse data overview</h5>");
            emailBody.AppendLine($"<p>Course name: {course.Name}</p>");
            var linkToConfirm = $"<h4>Сonfirm your participation in the course, follow the link: <a href='{inventationUrl}'>link</a></h4>";

            emailBody.AppendLine(linkToConfirm);
            #endregion

            var emailData = new EmailData(
                new List<string> { teacher.Email },
                "Course Invitation",
                emailBody.ToString());

            #region Email sending
            try
            {
                var result = await SendEmailAsync(emailData);

                if (!result.IsSuccessful)
                    return new Result<bool>(false, result.Message);

                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                return new Result<bool>(false, $"Fail to send email inventation to the techer");
            }

            #endregion
        }

        public async Task<Result<bool>> SendToAdminConfirmationForGroups(Group group, string callbackUrl)
        {
            if (group == null)
                return new Result<bool>(false, "Fail to send email");

            var allAdmins = await _userManager.GetUsersInRoleAsync("Admin");

            if (!allAdmins.Any())
                return new Result<bool>(false, "No admins for sending emails");

            var allAdminsEmails = allAdmins.Select(a => a.Email).ToList();

            #region Email body
            var emailBody = new StringBuilder();
            emailBody.AppendLine($"<h4>Confirm the creation of a group {group.Name} of more than 20 people, follow the link: <a href='{callbackUrl}'>link</a></h4>");
            #endregion

            var emailData = new EmailData(
                    allAdminsEmails ,
                    "Group confirmation",
                    emailBody.ToString());

            #region Email sending
            try
            {
                var result = await SendEmailAsync(emailData);

                if (!result.IsSuccessful)
                    return new Result<bool>(false, result.Message);

                return new Result<bool>(true, "Emails were sent to admins");
            }
            catch (Exception ex)
            {
                return new Result<bool>(false, "Fail to send email to admins");
            }

            #endregion
        }

        public async Task<Result<bool>> SendEmailToTeacherAboutApprovedGroup(AppUser teacher, Group group, string callbackUrl)
        {
            if (group == null || teacher == null)
                return new Result<bool>(false, "Fail to send email");

            #region Email body
            var emailBody = new StringBuilder();
            emailBody.AppendLine($"<h4>You get approve fot the creation of a group {group.Name} of more than 20 people, follow the link: <a href='{callbackUrl}'>link</a></h4>");
            #endregion

            var emailData = new EmailData(
                    new List<string> { teacher.Email},
                    "Group confirmation",
                    emailBody.ToString());

            #region Email sending
            try
            {
                var result = await SendEmailAsync(emailData);

                if (!result.IsSuccessful)
                    return new Result<bool>(false, result.Message);

                return new Result<bool>(true, "Emails were sent to admins");
            }
            catch (Exception ex)
            {
                return new Result<bool>(false, "Fail to send email to admins");
            }

            #endregion
        }

        public async Task<Result<bool>> SendInventationToStudents(Dictionary<string, string> studentsData, Group group)
        {
            try
            {
                foreach(var studentData in studentsData)
                {
                    var emailBody = new StringBuilder();
                    emailBody.AppendLine($"<h4>You get inventation to the group {group.Name}");
                    var emailData = new EmailData(
                        new List<string> { studentData.Key },
                        "Group inventation",
                        emailBody.Append($", follow the link: <a href='{studentData.Value}'>link</a></h4>").ToString());

                    var result = await SendEmailAsync(emailData);

                    if (!result.IsSuccessful)
                        return new Result<bool>(false, result.Message);
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
                return new Result<bool>(false, "No emails to send data");

            var emailData = new EmailData(toEmails, subject, body, displayName);

            try
            {
                var result = await SendEmailAsync(emailData);

                if (!result.IsSuccessful)
                    return new Result<bool>(false, result.Message);

                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                return new Result<bool>(false, "Fail to send email");
            }

        }

        public (string, string) GetEmailSubjectAndBody(EmailType emailType, AppUser appUser,  string callBackUrl = null) 
        {
            if(appUser == null)
                return (String.Empty, String.Empty);

            var parameters = new Dictionary<string, object>();
            switch(emailType)
            {
                case EmailType.AccountApproveByAdmin:
                    parameters = new Dictionary<string, object>()
                    {
                        {@"{firstname}", appUser.FirstName },
                        {@"{lastname}", appUser.LastName },
                        {@"{birthdate}", appUser.BirthDate },
                        {@"{email}", appUser.Email },
                        {@"{userrole}", appUser.Role},
                        {@"{callbackurl}", callBackUrl }
                    };
                    break;
                case EmailType.UserRegistration:
                    parameters = new Dictionary<string, object>()
                    {
                        {@"{firstname}", appUser.FirstName },
                        {@"{callbackurl}", callBackUrl }
                    };
                    break;
                case EmailType.ConfirmAdminRegistration:
                    parameters = new Dictionary<string, object>()
                    {
                        {@"{callbackurl}", callBackUrl }
                    };
                    break;
                case EmailType.ConfirmDeletionByAdmin:
                    parameters = new Dictionary<string, object>()
                    {
                        {@"{firstname}", appUser.FirstName },
                        {@"{lastname}", appUser.LastName },
                        {@"{birthdate}", appUser.BirthDate },
                        {@"{email}", appUser.Email },
                        {@"{userrole}", appUser.Role},
                        {@"{callbackurl}", callBackUrl }
                    };
                    break;
                case EmailType.ConfirmDeletionByUser:
                    parameters = new Dictionary<string, object>()
                    {
                        {@"{firstname}", appUser.FirstName },
                        {@"{callbackurl}", callBackUrl }
                    };
                    break;
                default:
                    return (String.Empty, String.Empty);
            }

            return EmailTemplate.GetEmailSubjectAndBody(emailType, parameters);
        }

        public (string, string) GetEmailSubjectAndBody(EmailType emailType, AppUser appUser, Group group, string callBackUrl = null)
        {
            if (appUser == null || group == null)
                return (String.Empty, String.Empty);

            var parameters = new Dictionary<string, object>();
            var groupEmailTypes = new List<EmailType>() {
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
                        {@"{groupname}", group.Name },
                        {@"{callbackurl}", callBackUrl }
                    };           

            return EmailTemplate.GetEmailSubjectAndBody(emailType, parameters);
        }

        public (string, string) GetEmailSubjectAndBody(EmailType emailType, AppUser appUser, Course course, string callBackUrl = null)
        {
            if (appUser == null || course == null)
                return (String.Empty, String.Empty);

            var parameters = new Dictionary<string, object>();
            switch (emailType)
            {
                case EmailType.CourseInvitation:
                    if (course == null)
                        break;

                    parameters = new Dictionary<string, object>()
                    {
                        {@"{firstname}", appUser.FirstName },
                        {@"{coursename}", course.Name },
                        {@"{callbackurl}", callBackUrl }
                    };
                    break;
                default:
                    return (String.Empty, String.Empty);
            }

            return EmailTemplate.GetEmailSubjectAndBody(emailType, parameters);
        }

        public async Task<Result<bool>> SendEmailAppUsers(EmailType emailType, AppUser appUser, string callBackUrl = null)
        {
            if (appUser == null)
                return new Result<bool>(false, "Fail to send email");

            var emailContent = GetEmailSubjectAndBody(emailType, appUser, callBackUrl);

            var toEmail = new List<string>();
            var allAdmins = await _userManager.GetUsersInRoleAsync(AppUserRoles.Admin.ToString());
            
            switch (emailType)
            {               
                case EmailType.ConfirmDeletionByAdmin:                   
                    toEmail  = allAdmins.Select(a => a.Email).ToList();
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
    }
}
