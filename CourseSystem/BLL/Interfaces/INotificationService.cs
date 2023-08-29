using Core.Enums;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface INotificationService : IGenericService<Notification>
    {
        Task<Result<List<Notification>>> GetNotifications(AppUser user, FilterParam findForFilter, int? findForId, SortingParam sortOrder);
        Task<Result<bool>> MarkAsRead(Notification notification);
        Task<Result<bool>> AddCreatedCourseNotification(AppUser user, Course course, string callback);
        Task<Result<bool>> AddJoinedCourseNotification(AppUser user, Course course, string callback); //ready
        Task<Result<bool>> AddCreatedGroupNotification(AppUser user, Group group); //ready
        Task<Result<bool>> AddJoinedGroupNotification(AppUser user, Group group); //ready
        Task<Result<bool>> AddGroupStartedForTeacherNotification(AppUser user, Group group); //ready
        Task<Result<bool>> AddGroupStartedForStudentNotification(AppUser user, Group group); //ready
        Task<Result<bool>> AddCreatedAssignmentNotification(AppUser user, Assignment assignment); //ready
        Task<Result<bool>> AddSubmittedAssignmentForStudentNotification(UserAssignments userAssignment); //ready
        Task<Result<bool>> AddSubmittedAssignmentForTeacherNotification(AppUser user, UserAssignments userAssignment); //ready
        Task<Result<bool>> AddMarkedAssignmentForTeacherNotification(AppUser user, UserAssignments userAssignment); //ready
        Task<Result<bool>> AddMarkedAssignmentForStudentNotification(UserAssignments userAssignment); //ready
        Task<Result<bool>> AddAssignmentIsOpenForTeacherNotification(AppUser user, Assignment assignment); //ready
        Task<Result<bool>> AddAssignmentIsOpenForStudentNotification(AppUser user, Assignment assignment); //ready
        Task<Result<bool>> AddAssignmentIsClosedForTeacherNotification(AppUser user, Assignment assignment); //ready
        Task<Result<bool>> AddAssignmentIsClosedForStudentNotification(AppUser user, Assignment assignment); //ready
    }
}
