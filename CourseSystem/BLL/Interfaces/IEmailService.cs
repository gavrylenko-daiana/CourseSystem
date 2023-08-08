

using Core.EmailTemplates;
using Core.Models;

namespace BLL.Interfaces
{
    public interface IEmailService
    {
        Task<int> SendCodeToUser(string email);
        Task<Result<bool>> SendEmailToAppUsers(EmailType emailType, AppUser appUser, string callBackUrl = null);
        Task<Result<bool>> SendEmailGroups(EmailType emailType,Group group, string callBackUrl = null, AppUser appUser = null);
        Task<Result<bool>> SendToTeacherCourseInventation(AppUser teacher,Course course, string inventationUrl);
        Task<Result<bool>> SendInventationToStudents(Dictionary<string, string> studentData, Group group);
    }
}
