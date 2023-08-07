using BLL.Interfaces;
using Core.Models;
using DAL.Interfaces;
using DAL.Repository;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class ActivityService : GenericService<UserActivity>, IActivityService
    {
        public ActivityService(UnitOfWork unitOfWork) 
            : base(unitOfWork, unitOfWork.UserActivityRepository)
        {
        }

        public async Task AddAssignmentCreatedActivity(AppUser teacher, Assignment assignment)
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

                var userActivity = new UserActivity()
                {
                    Name = "You created new assignment",
                    Description = $"<p>You created a new assignment \"{assignment.Name}\" for group \"{assignment.Group.Name}\".</p>" +
                    $"<p>Assignment activity starts on {assignment.StartDate.ToString("D")}, at {assignment.StartDate.ToString("t")}." +
                    $"It ends on {assignment.EndDate.ToString("D")}, at {assignment.EndDate.ToString("t")}.</p>",
                    Created = DateTime.Now,
                    AppUser = teacher,
                };

                await _repository.AddAsync(userActivity);

            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add activity record. Exception: {ex.Message}");
            }
        }

        public async Task AddAssignmentMarkedActivity(AppUser teacher, UserAssignments userAssignments)
        {
            try
            {
                if (teacher == null)
                {
                    throw new ArgumentNullException("Teacher was null");
                }

                if (userAssignments == null)
                {
                    throw new ArgumentNullException("UserAssignment was null");
                }

                var userActivity = new UserActivity()
                {
                    Name = "You marked an assignment",
                    Description = $"<p>You marked the solution {userAssignments.AppUser.FirstName} {userAssignments.AppUser.LastName} submited for assignment \"{userAssignments.Assignment.Name}\".</p>" +
                    $"<p>His/her grade: {userAssignments.Grade}/100.</p>",
                    Created = DateTime.Now,
                    AppUser = teacher,
                };

                await _repository.AddAsync(userActivity);

            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add activity record. Exception: {ex.Message}");
            }
        }

        public async Task AddAssignmentSubmitedActivity(AppUser student, Assignment assignment)
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

                var userActivity = new UserActivity()
                {
                    Name = "You submitted an assignment",
                    Description = $"<p>You subbmited a solution for assignment \"{assignment.Name}\".</p>",
                    Created = DateTime.Now,
                    AppUser = student,
                };

                await _repository.AddAsync(userActivity);

            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add activity record. Exception: {ex.Message}");
            }
        }

        public async Task AddJoinedCourseActivity(AppUser user, Course course)
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

                var userActivity = new UserActivity()
                {
                    Name = "You joined a new course",
                    Description = $"<p>You joined a new course - \"{course.Name}\".</p>",
                    Created = DateTime.Now,
                    AppUser = user,
                };

                await _repository.AddAsync(userActivity);

            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add activity record. Exception: {ex.Message}");
            }
        }
    }
}
