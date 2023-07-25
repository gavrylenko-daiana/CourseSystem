using BLL.Interfaces;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;
using Microsoft.AspNetCore.Identity;
using Core.Models;
using System.Net.Http;
using System.Text;

namespace BLL.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public EmailService(IConfiguration config,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _config = config;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        
        public async Task SendEmailAsync(string toEmail, string subject, string message) 
        {
            var adminSettings = GetAdminSettings();
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Course System", adminSettings.Item1));
            emailMessage.To.Add(new MailboxAddress("", toEmail));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 587, false);
                await client.AuthenticateAsync(adminSettings.Item1, adminSettings.Item2); //ключ доступа от Гугл
                await client.SendAsync(emailMessage);

                await client.DisconnectAsync(true);
            }
        }

        private (string , string) GetAdminSettings() 
        {
            //метод для получение данных администрации
            //сайта для рассылки аппрувов админам, студентам и учетилям
            
            var adminEmail = _config["AdminEmail"];
            var adminAccessCode = _config["AdminAccessCode"];

            return (adminEmail, adminAccessCode);
        }
        
        public async Task<int> SendCodeToUser(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) 
                throw new ArgumentNullException(nameof(email));
        
            try
            {
                int randomCode = new Random().Next(1000, 9999);
                var emailBody = $"<html><body> Your code: {randomCode} </body></html>";
                var emailSubject = "Verify code for update password.";

                await SendEmailAsync(email, emailSubject, emailBody);

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
            var userRole = await _roleManager.FindByIdAsync(newUser.Id);

            var userData = new StringBuilder()
                .AppendLine($"<h4>User data overview</h4>" +
                $"<hr/>" +
                $"<p>User first name: {newUser.FirstName}</p>" +
                $"<p>User last name: {newUser.LastName}</p>" +
                $"<p>Date of birth: {newUser.BirthDate}</p>" +
                $"<p>User email: {newUser.Email}</p>" +
                $"<p>User role: {userRole.Name}</p>");

            userData.AppendLine($"<h5>Confirm registration of {newUser.UserName}, follow the link: <a href='{callBackUrl}'>link</a></h5>");

            try
            {
                foreach (var admin in allAdmins)
                {
                    await SendEmailAsync(admin.Email, "Confirm user account", userData.ToString());
                }
            }
            catch(Exception ex)
            {
                throw new Exception($"Fail to send email to {newUser.Email}");
            }
            
        }

        public async Task SendEmailAboutSuccessfulRegistration(AppUser appUser)
        {
            var emailBody = new StringBuilder().AppendLine($"<h4>Dear {appUser.UserName}, you have been successfully registered into system");

            try
            {
                await SendEmailAsync(appUser.Email, "Successful registration", emailBody.ToString());
            }
            catch( Exception ex)
            {
                throw new Exception($"Fail to send email about successful registration to user {appUser.Email} ");
            }
        }
    }
}
