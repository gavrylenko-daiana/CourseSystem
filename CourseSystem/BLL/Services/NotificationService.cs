using BLL.Interfaces;
using Castle.Core.Logging;
using Core.Enums;
using Core.Models;
using Core.NotificationTemplates;
using DAL.Repository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class NotificationService : GenericService<Notification>, INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        public NotificationService(UnitOfWork unitOfWork, ILogger<NotificationService> logger)
                : base(unitOfWork, unitOfWork.NotificationRepository)
        {
            _logger = logger;
        }

        public async Task<Result<bool>> AddAssignmentIsClosedForStudentNotification(AppUser user, Assignment assignment)
        {
            if (user == null)
            {
                return new Result<bool>(false, "Invalid user");
            }

            if (assignment == null)
            {
                return new Result<bool>(false, "Invalid assignment");
            }

            var notification = await CreateNotification(NotificationType.AssignmentIsClosedForStudent,
                assignment.EndDate, user, assignment.Group.Course, assignment.Group,
                    assignment, assignment.Name, assignment.Group.Name);

            return await SaveNotification(notification);
        }

        public async Task<Result<bool>> AddAssignmentIsClosedForTeacherNotification(AppUser user, Assignment assignment)
        {
            if (user == null)
            {
                return new Result<bool>(false, "Invalid user");
            }

            if (assignment == null)
            {
                return new Result<bool>(false, "Invalid assignment");
            }

            var notification = await CreateNotification(NotificationType.AssignmentIsClosedForTeacher,
                assignment.EndDate, user, assignment.Group.Course, assignment.Group,
                    assignment, assignment.Name, assignment.Group.Name);

            return await SaveNotification(notification);
        }

        public async Task<Result<bool>> AddAssignmentIsOpenForStudentNotification(AppUser user, Assignment assignment)
        {
            if (user == null)
            {
                return new Result<bool>(false, "Invalid user");
            }

            if (assignment == null)
            {
                return new Result<bool>(false, "Invalid assignment");
            }

            var notification = await CreateNotification(NotificationType.AssignmentIsOpenForStudent,
                assignment.StartDate, user, assignment.Group.Course, assignment.Group,
                    assignment, assignment.Name, assignment.Group.Name, assignment.EndDate);

            return await SaveNotification(notification);
        }

        public async Task<Result<bool>> AddAssignmentIsOpenForTeacherNotification(AppUser user, Assignment assignment)
        {
            if (user == null)
            {
                return new Result<bool>(false, "Invalid user");
            }

            if (assignment == null)
            {
                return new Result<bool>(false, "Invalid assignment");
            }

            var notification = await CreateNotification(NotificationType.AssignmentIsOpenForTeacher,
                assignment.StartDate, user, assignment.Group.Course, assignment.Group,
                    assignment, assignment.Name, assignment.Group.Name, assignment.EndDate);

            return await SaveNotification(notification);
        }

        public async Task<Result<bool>> AddCreatedAssignmentNotification(AppUser user, Assignment assignment, 
            string callbackForAssignment, string callbackForGroup)
        {
            if (user == null)
            {
                return new Result<bool>(false, "Invalid user");
            }

            if (assignment == null)
            {
                return new Result<bool>(false, "Invalid assignment");
            }

            var notification = await CreateNotification(NotificationType.CreatedAssignment,
                DateTime.Now, user, assignment.Group.Course, assignment.Group,
                    assignment, assignment.Name, assignment.Group.Name, callbackForAssignment, 
                callbackForGroup, assignment.StartDate, assignment.EndDate);

            return await SaveNotification(notification);
        }

        public async Task<Result<bool>> AddCreatedCourseNotification(AppUser user, Course course, string callback)
        {
            if (user == null)
            {
                return new Result<bool>(false, "Invalid user");
            }

            if (course == null)
            {
                return new Result<bool>(false, "Invalid course");
            }

            var notification = await CreateNotification(NotificationType.CreatedCourse, DateTime.Now, user, 
                course, null, null,callback, course.Name);

            return await SaveNotification(notification);
        }

        public async Task<Result<bool>> AddCreatedGroupNotification(AppUser user, Group group, string callbackForGroup, string callbackForCourse)
        {
            if (user == null)
            {
                return new Result<bool>(false, "Invalid user");
            }

            if (group == null)
            {
                return new Result<bool>(false, "Invalid group");
            }

            var notification = await CreateNotification(NotificationType.CreatedGroup,
                DateTime.Now, user, group.Course, group, null, 
                    group.Name, group.Course.Name, callbackForGroup, callbackForCourse, group.StartDate, group.EndDate);

            return await SaveNotification(notification);
        }

        public async Task<Result<bool>> AddGroupStartedForStudentNotification(AppUser user, Group group)
        {
            if (user == null)
            {
                return new Result<bool>(false, "Invalid user");
            }

            if (group == null)
            {
                return new Result<bool>(false, "Invalid group");
            }

            var notification = await CreateNotification(NotificationType.GroupStartedForStudent, group.StartDate, 
                user, group.Course, group, null, group.Name);

            return await SaveNotification(notification);
        }

        public async Task<Result<bool>> AddGroupStartedForTeacherNotification(AppUser user, Group group)
        {
            if (user == null)
            {
                return new Result<bool>(false, "Invalid user");
            }

            if (group == null)
            {
                return new Result<bool>(false, "Invalid group");
            }

            var notification = await CreateNotification(NotificationType.GroupStartedForTeacher, group.StartDate, 
                user, group.Course, group, null, group.Name);

            return await SaveNotification(notification);
        }

        public async Task<Result<bool>> AddJoinedCourseNotification(AppUser user, Course course, string callback)
        {
            if (user == null)
            {
                return new Result<bool>(false, "Invalid user");
            }

            if (course == null)
            {
                return new Result<bool>(false, "Invalid course");
            }

            var notification = await CreateNotification(NotificationType.JoinedCourse, DateTime.Now, 
                user, course, null, null, callback, course.Name);

            return await SaveNotification(notification);
        }

        public async Task<Result<bool>> AddJoinedGroupNotification(AppUser user, Group group, string callbackForGroup, string callbackForCourse)
        {
            if (user == null)
            {
                return new Result<bool>(false, "Invalid user");
            }

            if (group == null)
            {
                return new Result<bool>(false, "Invalid group");
            }

            var notification = await CreateNotification(NotificationType.JoinedGroup,
                DateTime.Now, user, group.Course, group, null, 
                    group.Name, group.Course.Name, group.StartDate, group.EndDate, callbackForGroup, callbackForCourse);

            return await SaveNotification(notification);
        }

        public async Task<Result<bool>> AddMarkedAssignmentForStudentNotification(UserAssignments userAssignment)
        {
            if (userAssignment == null)
            {
                return new Result<bool>(false, "Invalid userAssignment");
            }

            var notification = await CreateNotification(NotificationType.MarkedAssignmentForStudent,
                DateTime.Now, userAssignment.AppUser, userAssignment.Assignment.Group.Course, userAssignment.Assignment.Group,
                    userAssignment.Assignment, userAssignment.Assignment.Name, userAssignment.Grade);

            return await SaveNotification(notification);
        }

        public async Task<Result<bool>> AddMarkedAssignmentForTeacherNotification(AppUser user,
            UserAssignments userAssignment)
        {
            if (user == null)
            {
                return new Result<bool>(false, "Invalid user");
            }

            if (userAssignment == null)
            {
                return new Result<bool>(false, "Invalid userAssignment");
            }

            var notification = await CreateNotification(NotificationType.MarkedAssignmentForTeacher, DateTime.Now, user,
                userAssignment.Assignment.Group.Course, userAssignment.Assignment.Group, userAssignment.Assignment,
                    userAssignment.AppUser.LastName, userAssignment.AppUser.FirstName, userAssignment.Assignment.Name, userAssignment.Grade);

            return await SaveNotification(notification);
        }

        public async Task<Result<bool>> AddSubmittedAssignmentForStudentNotification(UserAssignments userAssignment, string callback)
        {
            if (userAssignment == null)
            {
                return new Result<bool>(false, "Invalid userAssignment");
            }

            var notification = await CreateNotification(NotificationType.SubmittedAssignmentForStudent, DateTime.Now, userAssignment.AppUser,
                userAssignment.Assignment.Group.Course, userAssignment.Assignment.Group, userAssignment.Assignment, 
                userAssignment.Assignment.Name, callback);

            return await SaveNotification(notification);
        }

        public async Task<Result<bool>> AddSubmittedAssignmentForTeacherNotification(AppUser user,
            UserAssignments userAssignment)
        {
            if (user == null)
            {
                return new Result<bool>(false, "Invalid user");
            }

            if (userAssignment == null)
            {
                return new Result<bool>(false, "Invalid userAssignment");
            }

            var notification = await CreateNotification(NotificationType.SubmittedAssignmentForTeacher, DateTime.Now, user,
                userAssignment.Assignment.Group.Course, userAssignment.Assignment.Group, userAssignment.Assignment,
                    userAssignment.AppUser.LastName, userAssignment.AppUser.FirstName, userAssignment.Assignment.Name);

            return await SaveNotification(notification);
        }

        public async Task<Result<List<Notification>>> GetNotifications(AppUser user, FilterParam findForFilter, int? findForId, SortingParam sortOrder)
        {
            if (user == null)
            {
                return new Result<List<Notification>>(false, "Invalid user");
            }

            var notifications = user.Notifications;

            if (findForFilter == FilterParam.Course && findForId != null)
            {
                notifications = notifications.Where(a => a.CourseId == findForId).ToList();
            }
            else if (findForFilter == FilterParam.Group && findForId != null)
            {
                notifications = notifications.Where(a => a.GroupId == findForId).ToList();
            }
            else if (findForFilter == FilterParam.Assignment && findForId != null)
            {
                notifications = notifications.Where(a => a.AssignmentId == findForId).ToList();
            }

            if(sortOrder == SortingParam.Created)
            {
                notifications = notifications.OrderBy(n => n.Created).ToList();
            }
            else
            {
                notifications = notifications.OrderByDescending(n => n.Created).ToList();
            }

            return new Result<List<Notification>>(true, notifications);
        }

        public async Task<Result<bool>> MarkAsRead(Notification notification)
        {
            if (notification == null)
            {
                return new Result<bool>(false, "Invalid notification");
            }

            notification.IsRead = true;

            try
            {
                await _repository.UpdateAsync(notification);
                await _unitOfWork.Save();

                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                return new Result<bool>(false, ex.Message);
            }
        }

        private Task<Notification> CreateNotification(NotificationType notificationType, DateTime created,
            AppUser user, Course? course, Group? group,
            Assignment? assignment, params object[] descriptionParams)
        {
            var notification = new Notification()
            {
                Name = NotificationTemplate.GetNotificationName(notificationType),
                Description = NotificationTemplate.GetNotificationDescription(notificationType, descriptionParams),
                Created = created,
                AppUser = user,
                IsRead = false,
                Course = course,
                Group = group,
                Assignment = assignment,
            };

            return Task.FromResult(notification);
        }

        private async Task<Result<bool>> SaveNotification(Notification notification)
        {
            try
            {
                _logger.LogInformation("Saving notification {notificationName}", notification.Name);

                await _repository.AddAsync(notification);
                await _unitOfWork.Save();

                _logger.LogInformation("Save successfully");

                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                return new Result<bool>(false, ex.Message);
            }
        }
    }
}