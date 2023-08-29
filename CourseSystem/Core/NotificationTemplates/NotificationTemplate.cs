using Core.ActivityTemplates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.NotificationTemplates
{
    public static class NotificationTemplate
    {
        private static readonly Dictionary<NotificationType, string> _nameDictionary = new()
        {
            {NotificationType.CreatedCourse, "You created a new course" },
            {NotificationType.JoinedCourse, "You joined a new course" },
            {NotificationType.CreatedGroup, "You created a new group" },
            {NotificationType.JoinedGroup, "You joined a new group" },
            {NotificationType.GroupStartedForTeacher, "Education is starting in a group" },
            {NotificationType.GroupStartedForStudent, "Education is starting in a group" },
            {NotificationType.CreatedAssignment, "You created a new assignment" },
            {NotificationType.SubmittedAssignmentForStudent, "You submitted an assignment" },
            {NotificationType.SubmittedAssignmentForTeacher, "Someone submitted an assignment" },
            {NotificationType.MarkedAssignmentForTeacher, "You marked an assignment" },
            {NotificationType.MarkedAssignmentForStudent, "Your assignment was marked" },
            {NotificationType.AssignmentIsOpenForTeacher, "Assignment is open for submitting solutions" },
            {NotificationType.AssignmentIsOpenForStudent, "Assignment is open for submitting solutions" },
            {NotificationType.AssignmentIsClosedForTeacher, "Assignment is closed for submitting solutions" },
            {NotificationType.AssignmentIsClosedForStudent, "Assignment is closed for submitting solutions" }
        };

        private static readonly Dictionary<NotificationType, string> _descriptionDictionary = new()
        {
            {NotificationType.CreatedCourse, "You created a new course - <a href=\"{0}\">\"{1}\"</a>." },
            {NotificationType.JoinedCourse, "You joined a new course - <a href=\"{0}\">\"{1}\"</a>." },
            {NotificationType.CreatedGroup, "You created a new group <a href=\"{2}\">\"{0}\"</a>, in course <a href=\"{3}\">\"{1}\"</a>. " +
                "Education in it starts on {4:dddd, dd MMMM yyyy}. " +
                "It ends on {5:dddd, dd MMMM yyyy}." },
            {NotificationType.JoinedGroup, "You joined a new group, \"{0}\", in course \"{1}\"." +
                "Education in it starts on {2:dddd, dd MMMM yyyy}, at {2:HH:mm}. " +
                "It ends on {3:dddd, dd MMMM yyyy}, at {3:HH:mm}." },
            {NotificationType.GroupStartedForTeacher, "Education is starting in the group \"{0}\". " +
                "Now your students have access to its educational materials and assignments." },
            {NotificationType.GroupStartedForStudent, "Education is starting in the group \"{0}\". " +
                "Now you have access to its educational materials and assignments." },
            {NotificationType.CreatedAssignment, "You created a new assignment \"{0}\" for group \"{1}\". " +
                "Assignment activity starts on {2:dddd, dd MMMM yyyy}, at {2:HH:mm}. " +
                "It ends on {3:dddd, dd MMMM yyyy}, at {3:HH:mm}. " +
                "While assignment is active your students will be able to attach their solutions to it for you to grade." },
            {NotificationType.SubmittedAssignmentForStudent, "You submitted a solution for assignment \"{0}\". " +
                "Please, wait till your teacher marks it - you will receive corresponding notification as soon as it happens." },
            {NotificationType.SubmittedAssignmentForTeacher, "{0} {1} submitted a solution for your assignment \"{2}\". " +
                "Please, check and mark it." },
            {NotificationType.MarkedAssignmentForTeacher, "You marked the solution {0} {1} submitted for assignment \"{2}\". " +
                "His/her grade: {3}/100." },
            {NotificationType.MarkedAssignmentForStudent, "The solution you submitted for assignment \"{0}\" was marked. " +
                "Your grade: {1}/100." },
            {NotificationType.AssignmentIsOpenForTeacher, "Your assignment \"{0}\" for group \"{1}\" is open for submitting solutions. " +
                "It will be open until {2:dddd, dd MMMM yyyy}, {2:HH:mm}. " +
                "After that your students won't be able to submit their solutions, but you will still be able to grade them." },
            {NotificationType.AssignmentIsOpenForStudent, "Assignment \"{0}\" in group \"{1}\" is open for submitting solutions. " +
                "It will be open until {2:dddd, dd MMMM yyyy}, {2:HH:mm}. " +
                "After that you won't be able to submit your solutions." },
            {NotificationType.AssignmentIsClosedForTeacher, "Your assignment \"{0}\" for group \"{1}\" is closed for submitting solutions. " +
                "Now your students are unable to submit new solutions, but you can still grade them if you haven't done so yet." },
            {NotificationType.AssignmentIsClosedForStudent, "Assignment \"{0}\" in group \"{1}\" is closed for submitting solutions. " +
                "If you had successfully submitted your solution, but didn't get your grade yet - try to wait some more, as your teacher can still be checking it. " +
                "If you haven't submitted your solution in time, please, contact your teacher." }
        };


        public static string GetNotificationName(NotificationType notificationType)
        {
            return _nameDictionary[notificationType];
        }

        public static string GetNotificationDescription(NotificationType notificationType, params object[] obs)
        {
            var result = string.Format(_descriptionDictionary[notificationType], obs);
            
            return result;
        }
    }
}
