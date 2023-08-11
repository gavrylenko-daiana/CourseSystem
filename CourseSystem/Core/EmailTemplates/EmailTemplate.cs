using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.EmailTemplates
{
    public static class EmailTemplate
    {
        private static Dictionary<EmailType, string> SubjectGetter = new()
        {
            {EmailType.CodeVerification, "Verify code for update password."},
            {EmailType.AccountApproveByAdmin, "Confirm user account" },
            {EmailType.UserRegistration,  "Successful registration"},
            {EmailType.ConfirmAdminRegistration, "Confirm your account" },
            {EmailType.ConfirmDeletionByAdmin,  "Confirm deletion of user account"},
            {EmailType.ConfirmDeletionByUser, "Successful deletion approve" },
            {EmailType.CourseInvitation, "Course Invitation" },
            {EmailType.GroupConfirmationByAdmin,  "Group confirmation" },
            {EmailType.ApprovedGroupCreation,  "Group confirmation"},
            {EmailType.GroupInvitationToStudent, "Group inventation" }
        };

        private static Dictionary<EmailType, string> BodyGetter = new()
        {
            {EmailType.CodeVerification,  @"<html><body> Your code: {randomcode} </body></html>" },
            {EmailType.AccountApproveByAdmin,  @"<h4>User data overview</h4>" + "<hr/>" +
                    "<p>User first name: {firstname}</p>" +
                    "<p>User last name: {lastname}</p>" +
                    "<p>User email: {email}</p>" +
                    "<p>User role: {userrole}</p>"+
                    "<h4>Confirm registration of {firstname} {lastname}, follow the link: <a href='{callbackurl}'>link</a></h4>"
            },
            {EmailType.UserRegistration, @"<h4>Dear {firstname}, you have been successfully registered into system</h4>"+
                "<form action=\"{callbackurl}\">\r\n   " +
                " <input type=\"submit\" style=\"color: red\" " +
                "value=\"Your profile details\" />\r\n</form>"},
            {EmailType.ConfirmAdminRegistration, @"Confirm registration, follow the link: <a href='{callbackurl}'>link</a>" },
            {EmailType.ConfirmDeletionByAdmin, @"<h4>User data overview</h4>" +
                    "<hr/>" +
                    "<p>User first name: {firstname}</p>" +
                    "<p>User last name: {lastname}</p>" +
                    "<p>User email: {email}</p>" +
                    "<p>User role: {userrole}</p>"+
                    "<h4>Confirm deletion of {firstname} {lastname}, follow the link: <a href='{callbackurl}'>link</a></h4>" },
            {EmailType.ConfirmDeletionByUser, @"<h4>Dear {firstname}, your deletion was successfully approved by admin</h4>"+
                @"<h4>Confirm your deletion, follow the link: <a href='{callbackurl}'>link</a></h4>"},
            {EmailType.CourseInvitation, @"<h4>Dear {firstname}, you were invited to the course {coursename}</h4>"+
                "<h5>Сourse data overview</h5>"+
                @"<p>Course name: {coursename}</p>"+
                @"<h4>Сonfirm your participation in the course, follow the link: <a href='{callbackurl}'>link</a></h4>"},
            {EmailType.GroupConfirmationByAdmin,  @"<h4>Confirm the creation of a group {groupname} of more than 20 people, follow the link: <a href='{callbackurl}'>link</a></h4>"},
            {EmailType.ApprovedGroupCreation, @"<h4>You get approve fot the creation of a group {groupname} of more than 20 people, follow the link: <a href='{callbackurl}'>link</a></h4>" },
            {EmailType.GroupInvitationToStudent, @"<h4>You get inventation to the group {groupname}"+
                ", follow the link: <a href='{callbackurl}'>link</a></h4>"},           
        };

        public static (string, string) GetEmailSubjectAndBody(EmailType emailType, Dictionary<string, object> placeholderNamesandValues)
        {           
            if(!SubjectGetter.ContainsKey(emailType) || !BodyGetter.ContainsKey(emailType))
            {
                return (String.Empty, String.Empty);
            }

            var parameters = FillParameters(placeholderNamesandValues);

            var subject = SubjectGetter[emailType];
            var body = BodyGetter[emailType];
            var resultBody = parameters.Aggregate(body, (s, kv) => s.Replace(kv.Key, kv.Value.ToString()));

            return (subject, resultBody);
        }

        private static Dictionary<string, string> FillParameters(Dictionary<string, object> placeholderNamesandValues)
        {
            var parameters = new Dictionary<string, string>();
            foreach (var parameter in placeholderNamesandValues)
            {
                var parameterName = parameter.Key;

                if (parameterName.Equals(String.Empty))
                    break;

                var value = parameter.Value;

                if(value is DateTime date)
                {
                    parameters.Add(parameterName, date.ToString("d"));
                }
                else
                {
                    parameters.Add(parameterName, value.ToString());
                }                
            }

            return parameters;
        }
    }
}
