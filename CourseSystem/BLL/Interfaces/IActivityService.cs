﻿using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IActivityService : IGenericService<UserActivity>
    {
        Task<Result<bool>> AddCreatedCourseActivity(AppUser user, Course course); //ready
        Task<Result<bool>> AddJoinedCourseActivity(AppUser user, Course course); //ready
        Task<Result<bool>> AddCreatedGroupActivity(AppUser user, Group group); //ready
        Task<Result<bool>> AddJoinedGroupActivity(AppUser user, Group group); //ready
        Task<Result<bool>> AddCreatedAssignmentActivity(AppUser user, Assignment assignment); //ready
        Task<Result<bool>> AddMarkedAssignmentActivity(AppUser user, UserAssignments userAssignment); //ready
        Task<Result<bool>> AddSubmittedAssignmentAnswerActivity(AppUser user, Assignment assignment); //ready
        Task<Result<bool>> AddAttachedEducationalMaterialForGroupActivity(AppUser user, Group group); //ready
        Task<Result<bool>> AddAttachedEducationalMaterialForCourseActivity(AppUser user, Course course); //ready
        Task<Result<bool>> AddAttachedGeneralEducationalMaterialActivity(AppUser user); //ready
    }
}
