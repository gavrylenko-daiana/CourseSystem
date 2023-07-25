

using Core.Models;

namespace BLL.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail,string subject, string message);
        Task<int> SendCodeToUser(string email);
        Task SendUserApproveToAdmin(AppUser newUser, string callBackUrl);
        Task SendEmailAboutSuccessfulRegistration(AppUser appUser, string linkToProfile);
    }
}
