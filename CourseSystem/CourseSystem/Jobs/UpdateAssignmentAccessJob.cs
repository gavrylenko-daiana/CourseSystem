using BLL.Interfaces;
using Core.Enums;
using Core.Models;
using Quartz;

namespace UI.Jobs
{
    public class UpdateAssignmentAccessJob : IJob
    {
        private readonly IAssignmentService _assignmentService;
        private readonly IGroupService _groupService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<UpdateGroupAccessJob> _logger;

        public UpdateAssignmentAccessJob(IAssignmentService assignmentService, IGroupService groupService,
            INotificationService notificationService, ILogger<UpdateGroupAccessJob> logger)
        {
            _assignmentService = assignmentService;
            _notificationService = notificationService;
            _groupService = groupService;
            _logger = logger;
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
                            await _notificationService.AddAssignmentIsOpenForStudentNotification(user, assignment);
                        }
                        else if (user.Role == AppUserRoles.Teacher)
                        {
                            await _notificationService.AddAssignmentIsOpenForTeacherNotification(user, assignment);
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
                            await _notificationService.AddAssignmentIsClosedForStudentNotification(user, assignment);
                        }
                        else if (user.Role == AppUserRoles.Teacher)
                        {
                            await _notificationService.AddAssignmentIsClosedForTeacherNotification(user, assignment);
                        }
                    }
                }

                await _assignmentService.UpdateAssignment(assignment);
            }
        }
    }
}
