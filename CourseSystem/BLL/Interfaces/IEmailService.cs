

namespace BLL.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail,string subject, string message);

        Task<int> SendCodeToUser(string email);
    }
}
