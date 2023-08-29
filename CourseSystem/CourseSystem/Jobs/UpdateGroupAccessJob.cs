using BLL.Interfaces;
using Core.Enums;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Routing;
using Quartz;
using UI.Controllers;
using static Dropbox.Api.TeamLog.EventCategory;
using LinkGenerator = Core.Helpers.LinkGenerator;

namespace UI.Jobs
{
    public class UpdateGroupAccessJob : IJob
    {
        private readonly IGroupService _groupService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<UpdateGroupAccessJob> _logger;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly ICourseService _courseService;
        private readonly IEmailService _emailService;
        private readonly IUserGroupService _userGroupService;
        private readonly IUserCourseService _userCourseService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserService _userService;
        private readonly ILogger<GroupController> _loggerForGroup;
        private readonly IActivityService _activityService;

        public UpdateGroupAccessJob(IGroupService groupService, INotificationService notificationService, ILogger<UpdateGroupAccessJob> logger,
            IUrlHelperFactory urlHelperFactory, ICourseService courseService, UserManager<AppUser> userManager, IEmailService emailService,
            IUserGroupService userGroupService, IUserCourseService userCourseService, IUserService userService, IActivityService activityService,
            ILogger<GroupController> loggerForGroup)
        {
            _groupService = groupService;
            _notificationService = notificationService;
            _logger = logger;
            _urlHelperFactory = urlHelperFactory;
            _loggerForGroup = loggerForGroup;
            _courseService = courseService;
            _emailService = emailService;
            _userGroupService = userGroupService;
            _userManager = userManager;
            _userService = userService;
            _activityService = activityService;
            _userCourseService = userCourseService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var groupsResult = await _groupService.GetAllGroupsAsync();

            if (!groupsResult.IsSuccessful)
            {
                _logger.LogError("Failed to add time-related notifications for groups! Error: {errorMessage}", groupsResult.Message);

                return;
            }

            _logger.LogInformation("Updating groups access");

            foreach (var group in groupsResult.Data)
            {
                if (group.StartDate > DateTime.Now)
                {
                    group.GroupAccess = GroupAccess.Planned;
                }

                if (group.StartDate <= DateTime.Now && group.GroupAccess == GroupAccess.Planned)
                {
                    group.GroupAccess = GroupAccess.InProgress;

                    foreach (var user in group.UserGroups.Select(ug => ug.AppUser).ToList())
                    {
                        var groupController = new GroupController(
                            _groupService,
                            _courseService,
                            _userManager,
                            _emailService,
                            _userGroupService,
                            _userCourseService,
                            _userService,
                            _activityService,
                            _notificationService,
                            _loggerForGroup,
                            _urlHelperFactory);
                        
                        if (user.Role == AppUserRoles.Student)
                        {
                            await _notificationService.AddGroupStartedForStudentNotification(user, group,
                                LinkGenerator.GenerateGroupLink(_urlHelperFactory, groupController, group));
                        }
                        else if (user.Role == AppUserRoles.Teacher)
                        {
                            await _notificationService.AddGroupStartedForTeacherNotification(user, group,
                                LinkGenerator.GenerateGroupLink(_urlHelperFactory, groupController, group));
                        }
                    }
                }

                if (group.EndDate < DateTime.Now)
                {
                    group.GroupAccess = GroupAccess.Completed;
                }

                await _groupService.UpdateGroup(group);
            }
        }
    }
}
