

using Core.EmailTemplates;
using Core.Models;

namespace BLL.Interfaces
{
    public interface IEmailService
    {
        Task<int> SendCodeToUser(string email);
        Task<Result<bool>> SendEmailAppUsers(EmailType emailType, AppUser appUser, string callBackUrl = null);
        Task<Result<bool>> ConfirmUserDeletionByAdmin(AppUser userForDelete, string callbackUrl);
        Task<Result<bool>> ConfirmUserDeletionByUser(AppUser userForDelete, string logOutLink);
        Task<Result<bool>> SendToTeacherCourseInventation(AppUser teacher,Course course, string inventationUrl);
        Task<Result<bool>> SendToAdminConfirmationForGroups(Group group, string callbackUrl);
        Task<Result<bool>> SendEmailToTeacherAboutApprovedGroup(AppUser teacher, Group group, string callbackUrl);
        Task<Result<bool>> SendInventationToStudents(Dictionary<string, string> studentData, Group group);
    }
}
