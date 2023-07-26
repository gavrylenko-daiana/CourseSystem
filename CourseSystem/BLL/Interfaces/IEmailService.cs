

using Core.Models;

namespace BLL.Interfaces
{
    public interface IEmailService
    {
        //Task SendEmailAsync(EmailData emailData);
        Task<int> SendCodeToUser(string email);
        Task SendUserApproveToAdmin(AppUser newUser, string callBackUrl);
        Task SendEmailAboutSuccessfulRegistration(AppUser appUser, string linkToProfile);
        Task<Result<bool>> SendEmailAsync(EmailData emailData);
    }
}
