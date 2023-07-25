using BLL.Interfaces;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace BLL.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        
        public EmailService(IConfiguration config)
        {
            _config = config;
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
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentNullException(nameof(email));
        
            try
            {
                Random rand = new Random();
                int emailCode = rand.Next(1000, 9999);
                string fromMail = "dayana01001@gmail.com";
                string fromPassword = "oxizguygokwxgxgb";

                MailMessage message = new MailMessage();
                message.From = new MailAddress(fromMail);
                message.Subject = "Verify code for update password.";
                message.To.Add(new MailAddress($"{email}"));
                message.Body = $"<html><body> Your code: {emailCode} </body></html>";
                message.IsBodyHtml = true;

                // var smtpClient = new SmtpClient("smtp.gmail.com")
                // {
                //     Port = 587,
                //     Credentials = new NetworkCredential(fromMail, fromPassword),
                //     EnableSsl = true,
                // };

                // smtpClient.Send(message);

                return emailCode;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
