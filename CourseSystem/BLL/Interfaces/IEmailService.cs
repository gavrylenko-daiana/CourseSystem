

using Core.EmailTemplates;
using Core.Models;
using Microsoft.AspNetCore.Http;

namespace BLL.Interfaces
{
    public interface IEmailService
    {
        Task<Result<int>> SendCodeToUser(string email);
        Task<Result<bool>> SendTempPasswordToUser(EmailType emailType, AppUser appUser);
        Task<Result<bool>> SendEmailToAppUsers(EmailType emailType, AppUser appUser, string callBackUrl = null, string tempPassword = null, Group group = null, Course course = null, IFormFile? formFile = null);
        Task<Result<bool>> SendInvitationToStudents(Dictionary<string, string> studentData, Group group);
    }
}
