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
        Task MarkAsRead(Notification notification);

        Task AddAssignmentSubmitedNotification(AppUser student, Assignment assignment);

        Task AddJoinedCourseNotification(AppUser user, Course course);

        Task AddAssignmentCreatedNotification(AppUser teacher, Assignment assignment);

        Task AddAssignmentMarkedNotificationForTeacher(AppUser teacher, UserAssignments userAssignments);

        Task AddAssignmentMarkedNotificationForStudent(UserAssignments userAssignments);

        Task AddAssignmentOpenNotificationForStudent(AppUser student, Assignment assignment);

        Task AddAssignmentOpenNotificationForTeacher(AppUser teacher, Assignment assignment);

        Task AddAssignmentClosedNotificationForStudent(AppUser student, Assignment assignment);

        Task AddAssignmentClosedNotificationForTeacher(AppUser teacher, Assignment assignment);
    }
}
