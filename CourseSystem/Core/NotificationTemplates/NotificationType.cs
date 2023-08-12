using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.NotificationTemplates
{
    public enum NotificationType
    {
        CreatedCourse,
        JoinedCourse,
        CreatedGroup,
        JoinedGroup,
        GroupStartedForTeacher,
        GroupStartedForStudent,
        CreatedAssignment,
        SubmittedAssignmentForStudent,
        SubmittedAssignmentForTeacher,
        MarkedAssignmentForTeacher,
        MarkedAssignmentForStudent,
        AssignmentIsOpenForTeacher,
        AssignmentIsOpenForStudent,
        AssignmentIsClosedForTeacher,
        AssignmentIsClosedForStudent
    }
}
