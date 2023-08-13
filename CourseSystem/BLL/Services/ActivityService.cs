using BLL.Interfaces;
using Core.ActivityTemplates;
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

        public async Task<Result<bool>> AddCreatedAssignmentActivity(AppUser user, Assignment assignment)
        {
            if (user == null)
            {
                return new Result<bool>(false, $"Invalid ${nameof(user)}");
            }

            if (assignment == null)
            {
                return new Result<bool>(false, $"Invalid {nameof(assignment)}");
            }

            var activity = new UserActivity()
            {
                Name = ActivityTemplate.GetActivityName(ActivityType.CreatedAssignment),
                Description = ActivityTemplate.GetActivityDescription(ActivityType.CreatedAssignment, new object[] { assignment.Name, assignment.Group.Name, assignment.StartDate, assignment.EndDate }),
                Created = DateTime.Now,
                AppUser = user,
            };

            return await SaveActivity(activity);
        }

        public async Task<Result<bool>> AddCreatedCourseActivity(AppUser user, Course course)
        {
            if (user == null)
            {
                return new Result<bool>(false, $"Invalid ${nameof(user)}");
            }

            if (course == null)
            {
                return new Result<bool>(false, $"Invalid {nameof(course)}");
            }

            var activity = new UserActivity()
            {
                Name = ActivityTemplate.GetActivityName(ActivityType.CreatedCourse),
                Description = ActivityTemplate.GetActivityDescription(ActivityType.CreatedCourse, new object[] { course.Name }),
                Created = DateTime.Now,
                AppUser = user,
            };

            return await SaveActivity(activity);
        }

        public async Task<Result<bool>> AddCreatedGroupActivity(AppUser user, Group group)
        {
            if (user == null)
            {
                return new Result<bool>(false, $"Invalid ${nameof(user)}");
            }

            if (group == null)
            {
                return new Result<bool>(false, $"Invalid {nameof(group)}");
            }

            var activity = new UserActivity()
            {
                Name = ActivityTemplate.GetActivityName(ActivityType.CreatedGroup),
                Description = ActivityTemplate.GetActivityDescription(ActivityType.CreatedGroup, new object[] { group.Name, group.Course.Name, group.StartDate, group.EndDate }),
                Created = DateTime.Now,
                AppUser = user,
            };

            return await SaveActivity(activity);
        }

        public async Task<Result<bool>> AddJoinedCourseActivity(AppUser user, Course course)
        {
            if (user == null)
            {
                return new Result<bool>(false, $"Invalid ${nameof(user)}");
            }

            if (course == null)
            {
                return new Result<bool>(false, $"Invalid {nameof(course)}");
            }

            var activity = new UserActivity()
            {
                Name = ActivityTemplate.GetActivityName(ActivityType.JoinedCourse),
                Description = ActivityTemplate.GetActivityDescription(ActivityType.JoinedCourse, new object[] { course.Name }),
                Created = DateTime.Now,
                AppUser = user,
            };

            return await SaveActivity(activity);
        }

        public async Task<Result<bool>> AddJoinedGroupActivity(AppUser user, Group group)
        {
            if (user == null)
            {
                return new Result<bool>(false, $"Invalid ${nameof(user)}");
            }

            if (group == null)
            {
                return new Result<bool>(false, $"Invalid {nameof(group)}");
            }

            var activity = new UserActivity()
            {
                Name = ActivityTemplate.GetActivityName(ActivityType.JoinedGroup),
                Description = ActivityTemplate.GetActivityDescription(ActivityType.JoinedGroup, new object[] { group.Name, group.Course.Name, group.StartDate, group.EndDate }),
                Created = DateTime.Now,
                AppUser = user,
            };

            return await SaveActivity(activity);
        }

        public async Task<Result<bool>> AddMarkedAssignmentActivity(AppUser user, UserAssignments userAssignment)
        {
            if (user == null)
            {
                return new Result<bool>(false, $"Invalid ${nameof(user)}");
            }

            if (userAssignment == null)
            {
                return new Result<bool>(false, $"Invalid {nameof(userAssignment)}");
            }

            var activity = new UserActivity()
            {
                Name = ActivityTemplate.GetActivityName(ActivityType.MarkedAssignment),
                Description = ActivityTemplate.GetActivityDescription(ActivityType.MarkedAssignment,
                    new object[]
                    {
                        userAssignment.AppUser.LastName, userAssignment.AppUser.FirstName,
                        userAssignment.Assignment.Name, userAssignment.Grade
                    }),
                Created = DateTime.Now,
                AppUser = user,
            };

            return await SaveActivity(activity);
        }

        public async Task<Result<bool>> AddSubmittedAssignmentActivity(AppUser user, Assignment assignment)
        {
            if (user == null)
            {
                return new Result<bool>(false, $"Invalid ${nameof(user)}");
            }

            if (assignment == null)
            {
                return new Result<bool>(false, $"Invalid {nameof(assignment)}");
            }

            var activity = new UserActivity()
            {
                Name = ActivityTemplate.GetActivityName(ActivityType.SubmittedAssignment),
                Description = ActivityTemplate.GetActivityDescription(ActivityType.SubmittedAssignment, new object[] { assignment.Name }),
                Created = DateTime.Now,
                AppUser = user,
            };

            return await SaveActivity(activity);
        }

        public async Task<Result<bool>> AddAttachedEducationalMaterialForCourseActivity(AppUser user, Course course)
        {
            if (user == null)
            {
                return new Result<bool>(false, $"Invalid ${nameof(user)}");
            }

            if (course == null)
            {
                return new Result<bool>(false, $"Invalid {nameof(course)}");
            }

            var activity = new UserActivity()
            {
                Name = ActivityTemplate.GetActivityName(ActivityType.AttachedEducationalMaterialForCourse),
                Description = ActivityTemplate.GetActivityDescription(ActivityType.AttachedEducationalMaterialForCourse, new object[] { course.Name }),
                Created = DateTime.Now,
                AppUser = user,
            };

            return await SaveActivity(activity);
        }

        public async Task<Result<bool>> AddAttachedEducationalMaterialForGroupActivity(AppUser user, Group group)
        {
            if (user == null)
            {
                return new Result<bool>(false, $"Invalid ${nameof(user)}");
            }

            if (group == null)
            {
                return new Result<bool>(false, $"Invalid {nameof(group)}");
            }

            var activity = new UserActivity()
            {
                Name = ActivityTemplate.GetActivityName(ActivityType.AttachedEducationalMaterialForCourse),
                Description = ActivityTemplate.GetActivityDescription(ActivityType.AttachedEducationalMaterialForCourse, new object[] { group.Name }),
                Created = DateTime.Now,
                AppUser = user,
            };

            return await SaveActivity(activity);
        }

        private async Task<Result<bool>> SaveActivity(UserActivity activity)
        {
            try
            {
                await _repository.AddAsync(activity);
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