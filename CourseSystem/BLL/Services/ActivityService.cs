using BLL.Interfaces;
using Castle.Core.Logging;
using Core.ActivityTemplates;
using Core.Models;
using DAL.Interfaces;
using DAL.Repository;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dropbox.Api.TeamLog.TimeUnit;

namespace BLL.Services
{
    public class ActivityService : GenericService<UserActivity>, IActivityService
    {
        private readonly ILogger<ActivityService> _logger;
        private readonly IGroupService _groupService;
        public ActivityService(UnitOfWork unitOfWork, ILogger<ActivityService> logger, IGroupService groupService)
            : base(unitOfWork, unitOfWork.UserActivityRepository)
        {
            _logger = logger;
            _groupService = groupService;
        }

        public async Task<Result<bool>> AddCreatedAssignmentActivity(AppUser user, Assignment assignment)
        {
            if (user == null)
            {
                _logger.LogError("Failed to add {activityType} activity - user was null!", ActivityType.CreatedAssignment);

                return new Result<bool>(false, $"Invalid ${nameof(user)}");
            }

            if (assignment == null)
            {
                _logger.LogError("Failed to add {activityType} activity - assignment was null!", ActivityType.CreatedAssignment);

                return new Result<bool>(false, $"Invalid {nameof(assignment)}");
            }

            _logger.LogInformation("Forming {activityType} activity for user {userId} about assignment {assignmentId}",
                ActivityType.CreatedAssignment, user.Id, assignment.Id);

            var groupResult = await _groupService.GetById(assignment.GroupId);

            if (!groupResult.IsSuccessful)
            {
                _logger.LogError("Failed to add activity - corresponding group by Id {groupId} not found! Error: {errorMessage}",
                    assignment.GroupId, groupResult.Message);

                return new Result<bool>(false, "Failed to add activity - corresponding group not found!");
            }

            var activity = new UserActivity()
            {
                Name = ActivityTemplate.GetActivityName(ActivityType.CreatedAssignment),
                Description = ActivityTemplate.GetActivityDescription(ActivityType.CreatedAssignment, new object[] { assignment.Name, groupResult.Data.Name, assignment.StartDate, assignment.EndDate }),
                Created = DateTime.Now,
                AppUser = user,
            };

            return await SaveActivity(activity);
        }

        public async Task<Result<bool>> AddCreatedCourseActivity(AppUser user, Course course)
        {
            if (user == null)
            {
                _logger.LogError("Failed to add {activityType} activity - user was null!", ActivityType.CreatedCourse);

                return new Result<bool>(false, $"Invalid ${nameof(user)}");
            }

            if (course == null)
            {
                _logger.LogError("Failed to add {activityType} activity - course was null!", ActivityType.CreatedCourse);

                return new Result<bool>(false, $"Invalid {nameof(course)}");
            }

            _logger.LogInformation("Forming {activityType} activity for user {userId} about course {courseId}",
                ActivityType.CreatedCourse, user.Id, course.Id);

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
                _logger.LogError("Failed to add {activityType} activity - user was null!", ActivityType.CreatedGroup);

                return new Result<bool>(false, $"Invalid ${nameof(user)}");
            }

            if (group == null)
            {
                _logger.LogError("Failed to add {activityType} activity - group was null!", ActivityType.CreatedGroup);

                return new Result<bool>(false, $"Invalid {nameof(group)}");
            }

            _logger.LogInformation("Forming {activityType} activity for user {userId} about group {groupId}",
                ActivityType.CreatedGroup, user.Id, group.Id);

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
                _logger.LogError("Failed to add {activityType} activity - user was null!", ActivityType.JoinedCourse);

                return new Result<bool>(false, $"Invalid ${nameof(user)}");
            }

            if (course == null)
            {
                _logger.LogError("Failed to add {activityType} activity - course was null!", ActivityType.JoinedCourse);

                return new Result<bool>(false, $"Invalid {nameof(course)}");
            }

            _logger.LogInformation("Forming {activityType} activity for user {userId} about course {courseId}",
                ActivityType.JoinedCourse, user.Id, course.Id);

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
                _logger.LogError("Failed to add {activityType} activity - user was null!", ActivityType.JoinedGroup);

                return new Result<bool>(false, $"Invalid ${nameof(user)}");
            }

            if (group == null)
            {
                _logger.LogError("Failed to add {activityType} activity - group was null!", ActivityType.JoinedGroup);

                return new Result<bool>(false, $"Invalid {nameof(group)}");
            }

            _logger.LogInformation("Forming {activityType} activity for user {userId} about group {groupId}",
                ActivityType.JoinedGroup, user.Id, group.Id);

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
                _logger.LogError("Failed to add {activityType} activity - user was null!", ActivityType.MarkedAssignment);

                return new Result<bool>(false, $"Invalid ${nameof(user)}");
            }

            if (userAssignment == null)
            {
                _logger.LogError("Failed to add {activityType} activity - userAssignment was null!", ActivityType.MarkedAssignment);

                return new Result<bool>(false, $"Invalid {nameof(userAssignment)}");
            }

            _logger.LogInformation("Forming {activityType} activity for user {userId} about userAssignment {userAssignmentId}",
                ActivityType.MarkedAssignment, user.Id, userAssignment.Id);

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

        public async Task<Result<bool>> AddSubmittedAssignmentAnswerActivity(AppUser user, Assignment assignment)
        {
            if (user == null)
            {
                _logger.LogError("Failed to add {activityType} activity - user was null!", ActivityType.SubmittedAssignmentAnswer);

                return new Result<bool>(false, $"Invalid ${nameof(user)}");
            }

            if (assignment == null)
            {
                _logger.LogError("Failed to add {activityType} activity - assignment was null!", ActivityType.SubmittedAssignmentAnswer);

                return new Result<bool>(false, $"Invalid {nameof(assignment)}");
            }

            _logger.LogInformation("Forming {activityType} activity for user {userId} about assignment {assignmentId}",
                ActivityType.SubmittedAssignmentAnswer, user.Id, assignment.Id);

            var activity = new UserActivity()
            {
                Name = ActivityTemplate.GetActivityName(ActivityType.SubmittedAssignmentAnswer),
                Description = ActivityTemplate.GetActivityDescription(ActivityType.SubmittedAssignmentAnswer, new object[] { assignment.Name }),
                Created = DateTime.Now,
                AppUser = user,
            };

            return await SaveActivity(activity);
        }

        public async Task<Result<bool>> AddAttachedEducationalMaterialForCourseActivity(AppUser user, Course course)
        {
            if (user == null)
            {
                _logger.LogError("Failed to add {activityType} activity - user was null!", ActivityType.AttachedEducationalMaterialForCourse);

                return new Result<bool>(false, $"Invalid ${nameof(user)}");
            }

            if (course == null)
            {
                _logger.LogError("Failed to add {activityType} activity - course was null!", ActivityType.AttachedEducationalMaterialForCourse);

                return new Result<bool>(false, $"Invalid {nameof(course)}");
            }

            _logger.LogInformation("Forming {activityType} activity for user {userId} about course {courseId}",
                ActivityType.AttachedEducationalMaterialForCourse, user.Id, course.Id);

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
                _logger.LogError("Failed to add {activityType} activity - user was null!", ActivityType.AttachedEducationalMaterialForGroup);

                return new Result<bool>(false, $"Invalid ${nameof(user)}");
            }

            if (group == null)
            {
                _logger.LogError("Failed to add {activityType} activity - group was null!", ActivityType.AttachedEducationalMaterialForGroup);

                return new Result<bool>(false, $"Invalid {nameof(group)}");
            }

            _logger.LogInformation("Forming {activityType} activity for user {userId} about group {groupId}",
                ActivityType.AttachedEducationalMaterialForGroup, user.Id, group.Id);

            var activity = new UserActivity()
            {
                Name = ActivityTemplate.GetActivityName(ActivityType.AttachedEducationalMaterialForGroup),
                Description = ActivityTemplate.GetActivityDescription(ActivityType.AttachedEducationalMaterialForGroup, new object[] { group.Name }),
                Created = DateTime.Now,
                AppUser = user,
            };

            return await SaveActivity(activity);
        }

        public async Task<Result<bool>> AddAttachedGeneralEducationalMaterialActivity(AppUser user)
        {
            if (user == null)
            {
                _logger.LogError("Failed to add {activityType} activity - user was null!", ActivityType.AttachedGeneralEducationalMaterial);

                return new Result<bool>(false, $"Invalid ${nameof(user)}");
            }

            _logger.LogInformation("Forming {activityType} activity for user {userId}",
                ActivityType.AttachedGeneralEducationalMaterial, user.Id);

            var activity = new UserActivity()
            {
                Name = ActivityTemplate.GetActivityName(ActivityType.AttachedEducationalMaterialForCourse),
                Description = ActivityTemplate.GetActivityDescription(ActivityType.AttachedGeneralEducationalMaterial, new object[] { }),
                Created = DateTime.Now,
                AppUser = user,
            };

            return await SaveActivity(activity);
        }

        private async Task<Result<bool>> SaveActivity(UserActivity activity)
        {
            try
            {
                _logger.LogInformation("Saving activity");

                await _repository.AddAsync(activity);
                await _unitOfWork.Save();

                _logger.LogInformation("Activity saved successfully");

                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to save activity! Error: {errorMessage}", ex.Message);

                return new Result<bool>(false, ex.Message);
            }
        }
    }
}