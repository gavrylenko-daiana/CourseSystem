using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ActivityTemplates
{
    public enum ActivityType
    {
        CreatedCourse = 0,
        JoinedCourse,
        CreatedGroup,
        JoinedGroup,
        CreatedAssignment,
        MarkedAssignment,
        SubmittedAssignmentAnswer,
        AttachedEducationalMaterialForGroup,
        AttachedEducationalMaterialForCourse
    }
}
