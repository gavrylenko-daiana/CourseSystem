using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Core.ActivityTemplates
{
    public static class ActivityTemplate
    {
        private static readonly Dictionary<ActivityType, string> _nameDictionary = new()
        {
            {ActivityType.CreatedCourse, "You created a new course" },
            {ActivityType.JoinedCourse, "You joined a new course" },
            {ActivityType.CreatedGroup, "You created a new group" },
            {ActivityType.JoinedGroup, "You joined a new group" },
            {ActivityType.CreatedAssignment, "You created a new assignment" },
            {ActivityType.MarkedAssignment, "You marked an assignment" },
            {ActivityType.SubmittedAssignmentAnswer, "You submitted an assignment answer" },
            {ActivityType.AttachedEducationalMaterialForGroup, "You attached a new educational material" },
            {ActivityType.AttachedEducationalMaterialForCourse, "You attached a new educational material" },
            {ActivityType.AttachedGeneralEducationalMaterial, "You attached a new educational material" }
        };

        private static readonly Dictionary<ActivityType, string> _descriptionDictionary = new()
        {
            {ActivityType.CreatedCourse, "You created a new course - \"{0}\"." },
            {ActivityType.JoinedCourse, "You joined a new course - \"{0}\"." },
            {ActivityType.CreatedGroup, "You created a new group, \"{0}\", in course \"{1}\". " +
                "Education in it starts on {2:dddd, dd MMMM yyyy}. " +
                "It ends on {3:dddd, dd MMMM yyyy}." },
            {ActivityType.JoinedGroup, "You joined a new group, \"{0}\", in course \"{1}\"." +
                "Education in it starts on {2:dddd, dd MMMM yyyy}, at {2:HH:mm}. " +
                "It ends on {3:dddd, dd MMMM yyyy}, at {3:HH:mm}." },
            {ActivityType.CreatedAssignment, "You created a new assignment \"{0}\" for group \"{1}\". " +
                "Assignment activity starts on {2:dddd, dd MMMM yyyy}, at {2:HH:mm}." +
                "It ends on {3:dddd, dd MMMM yyyy}, at {3:HH:mm}." },
            {ActivityType.MarkedAssignment, "You marked the solution {0} {1} submitted for assignment \"{2}\". " +
                "His/her grade: {3}/100." },
            {ActivityType.SubmittedAssignmentAnswer, "You submitted a solution for assignment \"{0}\"." },
            {ActivityType.AttachedEducationalMaterialForGroup, "You attached a new educational material for group \"{0}\"." },
            {ActivityType.AttachedEducationalMaterialForCourse, "You attached a new educational material for course \"{0}\"." },
            {ActivityType.AttachedGeneralEducationalMaterial, "You attached a new educational material for general access" }
        };


        public static string GetActivityName(ActivityType activityType)
        {
            return _nameDictionary[activityType];
        }

        public static string GetActivityDescription(ActivityType activityType, params object[] obs)
        {
            var result = string.Format(_descriptionDictionary[activityType], obs);
            
            return result;
        }
    }
}
