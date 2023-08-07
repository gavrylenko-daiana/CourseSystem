using BLL.Interfaces;
using Core.Models;
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

        public async Task AddAssignmentClosedNotificationForStudent(AppUser student, Assignment assignment)
        {
            try
            {
                if (student == null)
                {
                    throw new ArgumentNullException("Student was null");
                }

                if (assignment == null)
                {
                    throw new ArgumentNullException("Assignment was null");
                }

                var notification = new Notification()
                {
                    Name = "Assignment is closed",
                    Description = $"<p>One of your assignments, \"{assignment.Name}\", was closed on {assignment.EndDate.ToString("D")}, at {assignment.EndDate.ToString("t")}.</p>" +
                    $"<p>If you haven't submitted your solution in time, please, contact your teacher.</p>",
                    Created = assignment.EndDate,
                    AppUser = student,
                    IsRead = false
                };

                await _repository.AddAsync(notification);

            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add notification. Exception: {ex.Message}");
            }
        }

        public async Task AddAssignmentClosedNotificationForTeacher(AppUser teacher, Assignment assignment)
        {
            try
            {
                if (teacher == null)
                {
                    throw new ArgumentNullException("Teacher was null");
                }

                if (assignment == null)
                {
                    throw new ArgumentNullException("Assignment was null");
                }

                var notification = new Notification()
                {
                    Name = "Assignment is closed",
                    Description = $"<p>One of your assignments, \"{assignment.Name}\", for group \"{assignment.Group.Name}\", was closed on {assignment.EndDate.ToString("D")}, at {assignment.EndDate.ToString("t")}.</p>" +
                    $"<p>Please, mark solutions submitted by your students.</p>",
                    Created = assignment.EndDate,
                    AppUser = teacher,
                    IsRead = false
                };

                await _repository.AddAsync(notification);

            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add notification. Exception: {ex.Message}");
            }
        }

        public async Task AddAssignmentCreatedNotification(AppUser teacher, Assignment assignment)
        {
            try
            {
                if (teacher == null)
                {
                    throw new ArgumentNullException("Teacher was null");
                }

                if (assignment == null)
                {
                    throw new ArgumentNullException("Assignment was null");
                }

                var notification = new Notification()
                {
                    Name = "You created new assignment",
                    Description = $"<p>You created a new assignment \"{assignment.Name}\" for group \"{assignment.Group.Name}\".</p>" +
                    $"<p>Assignment activity starts on {assignment.StartDate.ToString("D")}, at {assignment.StartDate.ToString("t")}." +
                    $"It ends on {assignment.EndDate.ToString("D")}, at {assignment.EndDate.ToString("t")}.</p>",
                    Created = DateTime.Now,
                    AppUser = teacher,
                    IsRead = false
                };

                await _repository.AddAsync(notification);

            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add notification. Exception: {ex.Message}");
            }
        }

        public async Task AddAssignmentMarkedNotificationForStudent(UserAssignments userAssignments)
        {
            try
            {
                if (userAssignments == null)
                {
                    throw new ArgumentNullException("Assignment was null");
                }

                var notification = new Notification()
                {
                    Name = "Assignment was marked",
                    Description = $"<p>Your solution for assignment \"{userAssignments.Assignment.Name}\" was marked {userAssignments.Grade}/100.</p>",
                    Created = DateTime.Now,
                    AppUser = userAssignments.AppUser,
                    IsRead = false
                };

                await _repository.AddAsync(notification);

            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add notification. Exception: {ex.Message}");
            }
        }

        public async Task AddAssignmentMarkedNotificationForTeacher(AppUser teacher, UserAssignments userAssignments)
        {
            try
            {
                if (teacher == null)
                {
                    throw new ArgumentNullException("Teacher was null");
                }

                if (userAssignments == null)
                {
                    throw new ArgumentNullException("Assignment was null");
                }

                var notification = new Notification()
                {
                    Name = "You marked an assignment",
                    Description = $"<p>You marked the solution {userAssignments.AppUser.FirstName} {userAssignments.AppUser.LastName} submited for assignment \"{userAssignments.Assignment.Name}\".</p>" +
                    $"<p>His/her grade: {userAssignments.Grade}/100.</p>",
                    Created = DateTime.Now,
                    AppUser = teacher,
                    IsRead = false
                };

                await _repository.AddAsync(notification);

            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add notification. Exception: {ex.Message}");
            }
        }

        public async Task AddAssignmentOpenNotificationForStudent(AppUser student, Assignment assignment)
        {
            try
            {
                if (student == null)
                {
                    throw new ArgumentNullException("Student was null");
                }

                if (assignment == null)
                {
                    throw new ArgumentNullException("Assignment was null");
                }

                var notification = new Notification()
                {
                    Name = "Assignment is open",
                    Description = $"<p>A new assignment, \"{assignment.Name}\", was opened on {assignment.StartDate.ToString("D")}, at {assignment.StartDate.ToString("t")}.</p>" +
                    $"<p>Please, submit your solution befor it is closed on {assignment.EndDate.ToString("D")}, at {assignment.EndDate.ToString("t")}.</p>",
                    Created = assignment.StartDate,
                    AppUser = student,
                    IsRead = false
                };

                await _repository.AddAsync(notification);

            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add notification. Exception: {ex.Message}");
            }
        }

        public async Task AddAssignmentOpenNotificationForTeacher(AppUser teacher, Assignment assignment)
        {
            try
            {
                if (teacher == null)
                {
                    throw new ArgumentNullException("Teacher was null");
                }

                if (assignment == null)
                {
                    throw new ArgumentNullException("Assignment was null");
                }

                var notification = new Notification()
                {
                    Name = "Assignment is open",
                    Description = $"<p>One of your assignments, \"{assignment.Name}\", for group \"{assignment.Group.Name}\", was opened on {assignment.StartDate.ToString("D")}, at {assignment.StartDate.ToString("t")}.</p>" +
                    $"<p>Your students have acess to it until it is closed on {assignment.EndDate.ToString("D")}, at {assignment.EndDate.ToString("t")}..</p>",
                    Created = assignment.StartDate,
                    AppUser = teacher,
                    IsRead = false
                };

                await _repository.AddAsync(notification);

            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add notification. Exception: {ex.Message}");
            }
        }

        public async Task AddAssignmentSubmitedNotification(AppUser student, Assignment assignment)
        {
            try
            {
                if (student == null)
                {
                    throw new ArgumentNullException("Student was null");
                }

                if (assignment == null)
                {
                    throw new ArgumentNullException("Assignment was null");
                }

                var notification = new Notification()
                {
                    Name = "You submitted an assignment",
                    Description = $"<p>You subbmited a solution for assignment \"{assignment.Name}\". Wait till your teacher marks it.</p>",
                    Created = DateTime.Now,
                    AppUser = student,
                    IsRead = false
                };

                await _repository.AddAsync(notification);

            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add notification. Exception: {ex.Message}");
            }
        }

        public async Task AddJoinedCourseNotification(AppUser user, Course course)
        {
            try
            {
                if (user == null)
                {
                    throw new ArgumentNullException("User was null");
                }

                if (course == null)
                {
                    throw new ArgumentNullException("Course was null");
                }

                var notification = new Notification()
                {
                    Name = "You joined a new course",
                    Description = $"<p>You joined a new course - \"{course.Name}\".</p>",
                    Created = DateTime.Now,
                    AppUser = user,
                    IsRead = false
                };

                await _repository.AddAsync(notification);

            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add notification. Exception: {ex.Message}");
            }
        }

        public async Task MarkAsRead(Notification notification)
        {
            try
            {
                if (notification == null)
                {
                    throw new Exception("Notification was null");
                }

                notification.IsRead = true;

                await _repository.UpdateAsync(notification);
                await _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to mark notification as read. Exception: {ex.Message}");
            }
        }
    }
}
