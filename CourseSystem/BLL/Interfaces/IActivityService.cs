using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IActivityService : IGenericService<UserActivity>
    {
        Task AddAssignmentSubmitedActivity(AppUser student, Assignment assignment);

        Task AddJoinedCourseActivity(AppUser user, Course course);

        Task AddAssignmentCreatedActivity(AppUser teacher, Assignment assignment);

        Task AddAssignmentMarkedActivity(AppUser teacher, UserAssignments userAssignments);
    }
}
