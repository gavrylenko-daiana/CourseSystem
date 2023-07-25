using BLL.Interfaces;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;

namespace BLL.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config)
        {
            _config = config;
        }
        public async Task SendEmailAsync(string toEmail, string topic, string subject, string message) 
        {
            var adminSettings = GetAdminSettings();
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(topic, adminSettings.Item1));
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
    }
}
