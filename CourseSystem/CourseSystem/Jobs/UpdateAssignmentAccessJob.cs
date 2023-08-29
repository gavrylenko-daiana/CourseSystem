using BLL.Interfaces;
using Core.Enums;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Routing;
using Quartz;
using UI.Controllers;
using LinkGenerator = Core.Helpers.LinkGenerator;

namespace UI.Jobs
{
    public class UpdateAssignmentAccessJob : IJob
    {
        private readonly IAssignmentService _assignmentService;
        private readonly IGroupService _groupService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<UpdateGroupAccessJob> _logger;
        private readonly IUserService _userService; 
        private readonly IUserAssignmentService _userAssignmentService;
        private readonly IActivityService _activityService;
        private readonly ILogger<AssignmentController> _loggerForAssignment;
        private readonly IAssignmentFileService _assignmentFileService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly ICourseService _courseService;
        private readonly IEmailService _emailService;
        private readonly IUserGroupService _userGroupService;
        private readonly IUserCourseService _userCourseService;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<GroupController> _loggerForGroup;

        public UpdateAssignmentAccessJob(IAssignmentService assignmentService, IGroupService groupService,
            INotificationService notificationService, ILogger<UpdateGroupAccessJob> logger, IUserService userService,
            IUserAssignmentService userAssignmentService, IActivityService activityService, ILogger<AssignmentController> loggerForAssignment, 
            IAssignmentFileService assignmentFileService, IUrlHelperFactory urlHelperFactory, ICourseService courseService, UserManager<AppUser> userManager, 
            IEmailService emailService, IUserGroupService userGroupService, IUserCourseService userCourseService, ILogger<GroupController> loggerForGroup)
        {
            _assignmentService = assignmentService;
            _notificationService = notificationService;
            _groupService = groupService;
            _logger = logger;
            _userService = userService;
            _userAssignmentService = userAssignmentService;
            _activityService = activityService;
            _loggerForAssignment = loggerForAssignment;
            _assignmentFileService = assignmentFileService;
            _urlHelperFactory = urlHelperFactory;
            _courseService = courseService;
            _emailService = emailService;
            _userGroupService = userGroupService;
            _userCourseService = userCourseService;
            _userManager = userManager;
            _loggerForGroup = loggerForGroup;
        }

        public async Task Execute(IJobExecutionContext context)
        {
           var assignmentResult = await _assignmentService.GetByPredicate();

            if (!assignmentResult.IsSuccessful)
            {
                _logger.LogError("Failed to add time-related notifications for assignments! Error: {errorMessage}", assignmentResult.Message);

                return;
            }

            _logger.LogInformation("Updating assignment access");

            foreach (var assignment in assignmentResult.Data)
            {
                if (assignment.StartDate > DateTime.Now)
                {
                    assignment.AssignmentAccess = AssignmentAccess.Planned;
                }

                #region Controllers
                var assignmentController = new AssignmentController(
                    _assignmentService,
                    _userService,
                    _userAssignmentService,
                    _activityService,
                    _notificationService,
                    _loggerForAssignment,
                    _assignmentFileService,
                    _urlHelperFactory
                );
                
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
                #endregion

                if (assignment.StartDate <= DateTime.Now && assignment.AssignmentAccess == AssignmentAccess.Planned)
                {
                    assignment.AssignmentAccess = AssignmentAccess.InProgress;

                    var groupResult = await _groupService.GetById(assignment.GroupId);

                    if (!groupResult.IsSuccessful)
                    {
                        _logger.LogError("Failed to add time-related notification for assignment {assignmentId} - related group not found!" +
                            "Error: {errorMessage}", assignment.Id, assignmentResult.Message);

                        continue;
                    }

                    foreach (var user in groupResult.Data.UserGroups.Select(ug => ug.AppUser).ToList())
                    {
                        if (user.Role == AppUserRoles.Student)
                        {
                            await _notificationService.AddAssignmentIsOpenForStudentNotification(user, assignment,
                                LinkGenerator.GenerateAssignmentLink(_urlHelperFactory, assignmentController, assignment),
                                LinkGenerator.GenerateGroupLink(_urlHelperFactory, groupController, assignment.Group));
                        }
                        else if (user.Role == AppUserRoles.Teacher)
                        {
                            await _notificationService.AddAssignmentIsOpenForTeacherNotification(user, assignment,
                                LinkGenerator.GenerateAssignmentLink(_urlHelperFactory, assignmentController, assignment),
                                LinkGenerator.GenerateGroupLink(_urlHelperFactory, groupController, assignment.Group));
                        }
                    }
                }

                if (assignment.EndDate < DateTime.Now && assignment.AssignmentAccess == AssignmentAccess.InProgress)
                {
                    assignment.AssignmentAccess = AssignmentAccess.Completed;

                    var groupResult = await _groupService.GetById(assignment.GroupId);

                    if (!groupResult.IsSuccessful)
                    {
                        _logger.LogError("Failed to add time-related notification for assignment {assignmentId} - related group not found!" +
                            "Error: {errorMessage}", assignment.Id, assignmentResult.Message);

                        continue;
                    }

                    foreach (var user in groupResult.Data.UserGroups.Select(ug => ug.AppUser).ToList())
                    {
                        if (user.Role == AppUserRoles.Student)
                        {
                            await _notificationService.AddAssignmentIsClosedForStudentNotification(user, assignment,
                                LinkGenerator.GenerateAssignmentLink(_urlHelperFactory, assignmentController, assignment),
                                LinkGenerator.GenerateGroupLink(_urlHelperFactory, groupController, assignment.Group));
                        }
                        else if (user.Role == AppUserRoles.Teacher)
                        {
                            await _notificationService.AddAssignmentIsClosedForTeacherNotification(user, assignment,
                                LinkGenerator.GenerateAssignmentLink(_urlHelperFactory, assignmentController, assignment),
                                LinkGenerator.GenerateGroupLink(_urlHelperFactory, groupController, assignment.Group));
                        }
                    }
                }

                await _assignmentService.UpdateAssignment(assignment);
            }
        }
    }
}
