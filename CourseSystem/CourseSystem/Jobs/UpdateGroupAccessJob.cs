using BLL.Interfaces;
using Core.Enums;
using Core.Models;
using Quartz;
using static Dropbox.Api.TeamLog.EventCategory;

namespace UI.Jobs
{
    public class UpdateGroupAccessJob : IJob
    {
        private readonly IGroupService _groupService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<UpdateGroupAccessJob> _logger;

        public UpdateGroupAccessJob(IGroupService groupService, INotificationService notificationService, ILogger<UpdateGroupAccessJob> logger)
        {
            _groupService = groupService;
            _notificationService = notificationService;
            _logger = logger;
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
                        if (user.Role == AppUserRoles.Student)
                        {
                            await _notificationService.AddGroupStartedForStudentNotification(user, group);
                        }
                        else if (user.Role == AppUserRoles.Teacher)
                        {
                            await _notificationService.AddGroupStartedForTeacherNotification(user, group);
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
