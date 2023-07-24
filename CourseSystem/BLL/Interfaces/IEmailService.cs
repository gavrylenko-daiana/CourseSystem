

namespace BLL.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string topic, string subject, string message);
    }
}
