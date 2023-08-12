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
        Task<Result<bool>> MarkAsRead(Notification notification);
        Task<Result<bool>> AddCreatedCourseNotification(AppUser user, Course course);
        Task<Result<bool>> AddJoinedCourseNotification(AppUser user, Course course);
        Task<Result<bool>> AddCreatedGroupNotification(AppUser user, Group group);
        Task<Result<bool>> AddJoinedGroupNotification(AppUser user, Group group);
        Task<Result<bool>> AddGroupStartedForTeacherNotification(AppUser user, Group group);
        Task<Result<bool>> AddGroupStartedForStudentNotification(AppUser user, Group group);
        Task<Result<bool>> AddCreatedAssignmentNotification(AppUser user, Assignment assignment);
        Task<Result<bool>> AddSubmittedAssignmentForStudentNotification(UserAssignments userAssignment);
        Task<Result<bool>> AddSubmittedAssignmentForTeacherNotification(AppUser user, UserAssignments userAssignment);
        Task<Result<bool>> AddMarkedAssignmentForTeacherNotification(AppUser user, UserAssignments userAssignment);
        Task<Result<bool>> AddMarkedAssignmentForStudentNotification(UserAssignments userAssignment);
        Task<Result<bool>> AddAssignmentIsOpenForTeacherNotification(AppUser user, Assignment assignment);
        Task<Result<bool>> AddAssignmentIsOpenForStudentNotification(AppUser user, Assignment assignment);
        Task<Result<bool>> AddAssignmentIsClosedForTeacherNotification(AppUser user, Assignment assignment);
        Task<Result<bool>> AddAssignmentIsClosedForStudentNotification(AppUser user, Assignment assignment);
    }
}
