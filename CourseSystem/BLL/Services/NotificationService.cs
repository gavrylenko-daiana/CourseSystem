using BLL.Interfaces;
using Core.Enums;
using Core.Models;
using Core.NotificationTemplates;
using DAL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class NotificationService : GenericService<Notification>, INotificationService
    {
        public NotificationService(UnitOfWork unitOfWork)
                : base(unitOfWork, unitOfWork.NotificationRepository)
        {
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

        public async Task<Result<bool>> AddCreatedAssignmentNotification(AppUser user, Assignment assignment)
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
                    assignment, assignment.Name, assignment.Group.Name, assignment.StartDate, assignment.EndDate);

            return await SaveNotification(notification);
        }

        public async Task<Result<bool>> AddCreatedCourseNotification(AppUser user, Course course)
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
                course, null, null, course.Name);

            return await SaveNotification(notification);
        }

        public async Task<Result<bool>> AddCreatedGroupNotification(AppUser user, Group group)
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
                    group.Name, group.Course.Name, group.StartDate, group.EndDate);

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

        public async Task<Result<bool>> AddJoinedCourseNotification(AppUser user, Course course)
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
                user, course, null, null, course.Name);

            return await SaveNotification(notification);
        }

        public async Task<Result<bool>> AddJoinedGroupNotification(AppUser user, Group group)
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
                    group.Name, group.Course.Name, group.StartDate, group.EndDate);

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

        public async Task<Result<bool>> AddSubmittedAssignmentForStudentNotification(UserAssignments userAssignment)
        {
            if (userAssignment == null)
            {
                return new Result<bool>(false, "Invalid userAssignment");
            }

            var notification = await CreateNotification(NotificationType.SubmittedAssignmentForStudent, DateTime.Now, userAssignment.AppUser,
                userAssignment.Assignment.Group.Course, userAssignment.Assignment.Group, userAssignment.Assignment, userAssignment.Assignment.Name);

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

        public async Task<Result<List<Notification>>> GetNotifications(AppUser currentUser,
            NotificationsFilteringParams filteringParam = NotificationsFilteringParams.Default, string searchQuery = null)
        {
            if (currentUser == null)
            {
                return new Result<List<Notification>>(false, "Invalid user");
            }

            if (filteringParam == NotificationsFilteringParams.Default || searchQuery == null)
            {
                return new Result<List<Notification>>(true, currentUser.Notifications.OrderByDescending(a => a.Created).ToList());
            }

            var notifications = new List<Notification>();

            switch(filteringParam)
            {
                case NotificationsFilteringParams.Course:
                    notifications = currentUser.Notifications.Where(n => n.CourseId != null && n.Course.Name.Contains(searchQuery))
                        .OrderByDescending(a => a.Created).ToList();
                    break;

                case NotificationsFilteringParams.Group:
                    notifications = currentUser.Notifications.Where(n => n.GroupId != null && n.Group.Name.Contains(searchQuery))
                        .OrderByDescending(a => a.Created).ToList();
                    break;

                case NotificationsFilteringParams.Assignment:
                    notifications = currentUser.Notifications.Where(n => n.AssignmentId != null && n.Assignment.Name.Contains(searchQuery))
                        .OrderByDescending(a => a.Created).ToList();
                    break;
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
                await _repository.AddAsync(notification);
                await _unitOfWork.Save();

                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                return new Result<bool>(false, ex.Message);
            }
        }
    }
}