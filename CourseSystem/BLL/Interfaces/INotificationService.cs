﻿using Core.Enums;
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
        Task<Result<bool>> AddJoinedCourseNotification(AppUser user, Course course, string callback); 
        Task<Result<bool>> AddCreatedGroupNotification(AppUser user, Group group, string callbackForGroup, string callbackForCourse); 
        Task<Result<bool>> AddJoinedGroupNotification(AppUser user, Group group, string callbackForGroup, string callbackForCourse); 
        Task<Result<bool>> AddGroupStartedForTeacherNotification(AppUser user, Group group, string callback); 
        Task<Result<bool>> AddGroupStartedForStudentNotification(AppUser user, Group group, string callback); 
        Task<Result<bool>> AddCreatedAssignmentNotification(AppUser user, Assignment assignment, string callbackForAssignment, string callbackForGroup); 
        Task<Result<bool>> AddSubmittedAssignmentForStudentNotification(UserAssignments userAssignment, string callback);
        Task<Result<bool>> AddSubmittedAssignmentForTeacherNotification(AppUser user, UserAssignments userAssignment, string callback); 
        Task<Result<bool>> AddMarkedAssignmentForTeacherNotification(AppUser user, UserAssignments userAssignment, string callback); 
        Task<Result<bool>> AddMarkedAssignmentForStudentNotification(UserAssignments userAssignment, string callback); 
        Task<Result<bool>> AddAssignmentIsOpenForTeacherNotification(AppUser user, Assignment assignment, string callbackForAssignment, string callbackForGroup); 
        Task<Result<bool>> AddAssignmentIsOpenForStudentNotification(AppUser user, Assignment assignment, string callbackForAssignment, string callbackForGroup);
        Task<Result<bool>> AddAssignmentIsClosedForTeacherNotification(AppUser user, Assignment assignment, string callbackForAssignment, string callbackForGroup); 
        Task<Result<bool>> AddAssignmentIsClosedForStudentNotification(AppUser user, Assignment assignment, string callbackForAssignment, string callbackForGroup); 
    }
}
