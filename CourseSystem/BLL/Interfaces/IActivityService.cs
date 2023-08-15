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
        Task<Result<bool>> AddCreatedCourseActivity(AppUser user, Course course);
        Task<Result<bool>> AddJoinedCourseActivity(AppUser user, Course course);
        Task<Result<bool>> AddCreatedGroupActivity(AppUser user, Group group);
        Task<Result<bool>> AddJoinedGroupActivity(AppUser user, Group group);
        Task<Result<bool>> AddCreatedAssignmentActivity(AppUser user, Assignment assignment);
        Task<Result<bool>> AddMarkedAssignmentActivity(AppUser user, UserAssignments userAssignment);
        Task<Result<bool>> AddSubmittedAssignmentAnswerActivity(AppUser user, Assignment assignment); //ready
        Task<Result<bool>> AddAttachedEducationalMaterialForGroupActivity(AppUser user, Group group);
        Task<Result<bool>> AddAttachedEducationalMaterialForCourseActivity(AppUser user, Course course);
    }
}
