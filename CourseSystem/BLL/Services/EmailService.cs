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
                var emailData = new EmailData(
                    new List<string>() { email},
                    "Verify code for update password.",
                    $"<html><body> Your code: {randomCode} </body></html>"
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

        public async Task SendUserApproveToAdmin(AppUser newUser, string callBackUrl)
        {
            var allAdmins = await _userManager.GetUsersInRoleAsync("Admin");

            if(allAdmins.Any())
            {
                #region Email body creation
                var userRole = newUser.Role;

                var userData = new StringBuilder()
                    .AppendLine($"<h4>User data overview</h4>" +
                    $"<hr/>" +
                    $"<p>User first name: {newUser.FirstName}</p>" +
                    $"<p>User last name: {newUser.LastName}</p>" +
                    $"<p>Date of birth: {newUser.BirthDate.ToString("d")}</p>" +
                    $"<p>User email: {newUser.Email}</p>" +
                    $"<p>User role: {userRole}</p>");

                userData.AppendLine($"<h4>Confirm registration of {newUser.FirstName} {newUser.LastName}, follow the link: <a href='{callBackUrl}'>link</a></h4>");
                #endregion
                try
                {
                    var allAdminsEmails = allAdmins.Select(a => a.Email).ToList();
                    var emailData = new EmailData(
                        allAdminsEmails,
                        "Confirm user account",
                        userData.ToString());

                    await SendEmailAsync(emailData);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Fail to send email to {newUser.Email}");
                }
            }           
        }

        public async Task SendEmailAboutSuccessfulRegistration(AppUser appUser, string linkToProfile)
        {
            if(appUser != null)
            {
                #region Email body creation
                var emailBody = new StringBuilder().AppendLine($"<h4>Dear {appUser.FirstName}, you have been successfully registered into system</h4>");
                var buttonToUserProfileDetails = $"<form action=\"{linkToProfile}\">\r\n   " +
                    $" <input type=\"submit\" style=\"color: red\" " +
                    $"value=\"Your profile details\" />\r\n</form>";

                emailBody.AppendLine(buttonToUserProfileDetails);
                #endregion

                var emailData = new EmailData(
                    new List<string> { appUser.Email},
                    "Successful registration",
                    emailBody.ToString());

                try
                {
                    await SendEmailAsync(emailData);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Fail to send email about successful registration to user {appUser.Email} ");
                }
            }          
        }

        public async Task<Result<bool>> SendEmailAsync(EmailData emailData)
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

        public async Task<Result<bool>> SendAdminEmailConfirmation(string adminEmail, string callBackUrl) 
        {
            var emailData = new EmailData(new List<string> { adminEmail },
                     "Confirm your account",
                     $"Confirm registration, follow the link: <a href='{callBackUrl}'>link</a>");

           return await SendEmailAsync(emailData);
        }

        public async Task<Result<bool>> ConfirmUserDeletionByAdmin(AppUser userForDelete, string callbackUrl)
        {
            var allAdmins = await _userManager.GetUsersInRoleAsync("Admin");

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

                    await SendEmailAsync(emailData);

                    return new Result<bool>(true);
                }
                catch (Exception ex)
                {
                    return new Result<bool>(false, $"Fail to send email to {userForDelete.Email}");
                }
            }

            return new Result<bool>(false, $"Fail to send email to {userForDelete.Email}");
        }

        public async Task ConfirmUserDeletionByUser(AppUser userForDelete, string logOutLink)
        {
            if (userForDelete != null)
            {
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
                    await SendEmailAsync(emailData);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Fail to send email about successful registration to user {userForDelete.Email} ");
                }
            }
        }
    }
}
