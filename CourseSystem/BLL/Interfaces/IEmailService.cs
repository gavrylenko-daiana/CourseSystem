

using Core.Models;

namespace BLL.Interfaces
{
    public interface IEmailService
    {
        Task<int> SendCodeToUser(string email);
        Task SendUserApproveToAdmin(AppUser newUser, string callBackUrl);
        Task SendEmailAboutSuccessfulRegistration(AppUser appUser, string linkToProfile);
        Task<Result<bool>> SendEmailAsync(EmailData emailData);
        Task<Result<bool>> SendAdminEmailConfirmation(string adminEmail, string callBackUrl);
        Task<Result<bool>> ConfirmUserDeletionByAdmin(AppUser userForDelete, string callbackUrl);
        Task ConfirmUserDeletionByUser(AppUser userForDelete, string logOutLink);
        Task SendToTeacherCourseInventation(AppUser teacher,Course course, string inventationUrl);
        Task<Result<bool>> SendToAdminConfirmationForGroups(Group group, string callbackUrl);
        Task<Result<bool>> SendEmailToTeacherAboutApprovedGroup(AppUser teacher, Group group, string callbackUrl);
    }
}
