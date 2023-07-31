﻿using BLL.Interfaces;
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

        public async Task SendToTeacherCourseInventation(AppUser teacher, Course course, string inventationUrl)
        {
            if(teacher != null )
            {
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

                    //if (!result.IsSuccessful)
                    //    return new Result<bool>(false, result.Message);
                }
                catch(Exception ex)
                {
                    throw new Exception($"Fail to send email inventation to the techer");
                }

                #endregion
            }
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
            #region Email body
            var emailBody = new StringBuilder();
            emailBody.AppendLine($"<h4>You get inventation to the group {group.Name}");
            #endregion
            #region Email sending
            try
            {
                foreach(var studentData in studentsData)
                {
                    var emailData = new EmailData(
                        new List<string> { studentData.Key },
                        "Group inventation",
                        emailBody.AppendLine($", follow the link: <a href='{studentData.Value}'>link</a></h4>").ToString());

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

            #endregion
        }
    }
}
