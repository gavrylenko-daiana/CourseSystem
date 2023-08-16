

using Core.EmailTemplates;
using Core.Models;
using Microsoft.AspNetCore.Http;

namespace BLL.Interfaces
{
    public interface IEmailService
    {
        Task<Result<int>> SendCodeToUser(string email);
        Task<Result<bool>> SendTempPasswordToUser(EmailType emailType, AppUser appUser);
        Task<Result<bool>> SendEmailToAppUsers(EmailType emailType, AppUser appUser, string callBackUrl = null, string tempPassword = null);
        Task<Result<bool>> SendEmailMaterial(EmailType emailType, AppUser appUser, IFormFile material, string callBackUrl = null!);
        Task<Result<bool>> SendEmailGroups(EmailType emailType,Group group, string callBackUrl = null, AppUser appUser = null);
        Task<Result<bool>> SendToTeacherCourseInvitation(AppUser teacher,Course course, string inventationUrl);
        Task<Result<bool>> SendInvitationToStudents(Dictionary<string, string> studentData, Group group);
    }
}
