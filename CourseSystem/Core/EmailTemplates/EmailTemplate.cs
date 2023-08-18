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
            { EmailType.CodeVerification, "Verify code for update password."},
            { EmailType.AccountApproveByAdmin, "Confirm user account" },
            { EmailType.AccountApproveByUser, "Confirm user account" },
            { EmailType.UserRegistration,  "Successful registration"},
            { EmailType.ConfirmAdminRegistration, "Confirm your account" },
            { EmailType.ConfirmDeletionByAdmin,  "Confirm deletion of user account"},
            { EmailType.ConfirmDeletionByUser, "Successful deletion approve" },
            { EmailType.CourseInvitation, "Course Invitation" },
            { EmailType.GroupConfirmationByAdmin,  "Group confirmation" },
            { EmailType.ApprovedGroupCreation,  "Group confirmation"},
            { EmailType.GroupInvitationToStudent, "Group invitation" },
            { EmailType.GetTempPasswordToAdmin, "Login with temporary password" },
            { EmailType.EducationMaterialApproveByAdmin, "Confirm upload educational material" },
            { EmailType.ApprovedUploadEducationalMaterial, "Educational material confirmation" },
            {EmailType.DeniedUploadEducationalMaterial, "Educational material denied"  }
        };

        private static Dictionary<EmailType, string> BodyGetter = new()
        {
            { EmailType.CodeVerification,  @"<html><body> Your code: {randomcode} </body></html>" },
            { EmailType.AccountApproveByAdmin,  @"<h4>User data overview</h4>" + "<hr/>" +
                    "<p>User first name: {firstname}</p>" +
                    "<p>User last name: {lastname}</p>" +
                    "<p>User email: {email}</p>" +
                    "<p>User role: {userrole}</p>"+
                    "<h4>Confirm registration of {firstname} {lastname}, follow the link: <a href='{callbackurl}'>link</a></h4>"
            },
            { EmailType.AccountApproveByUser,  @"<h4>Confirm that you are {firstname} {lastname} and it is your new email, follow the link: <a href='{callbackurl}'>link</a></h4>"},
            { EmailType.UserRegistration, @"<h4>Dear {firstname}, you have been successfully registered into system</h4>"+
                "<form action=\"{callbackurl}\">\r\n   " +
                " <input type=\"submit\" style=\"color: red\" " +
                "value=\"Your profile details\" />\r\n</form>"},
            { EmailType.ConfirmAdminRegistration, @"Confirm registration, follow the link: <a href='{callbackurl}'>link</a>" },
            { EmailType.ConfirmDeletionByAdmin, @"<h4>User data overview</h4>" +
                    "<hr/>" +
                    "<p>User first name: {firstname}</p>" +
                    "<p>User last name: {lastname}</p>" +
                    "<p>User email: {email}</p>" +
                    "<p>User role: {userrole}</p>"+
                    "<h4>Confirm deletion of {firstname} {lastname}, follow the link: <a href='{callbackurl}'>link</a></h4>" },
            { EmailType.ConfirmDeletionByUser, @"<h4>Dear {firstname}, your deletion was successfully approved by admin</h4>"+
                @"<h4>Confirm your deletion, follow the link: <a href='{callbackurl}'>link</a></h4>"},
            { EmailType.CourseInvitation, @"<h4>Dear {firstname}, you were invited to the course {coursename}</h4>"+
                "<h5>Course data overview</h5>"+
                @"<p>Course name: {coursename}</p>"+
                @"<h4>Confirm your participation in the course, follow the link: <a href='{callbackurl}'>link</a></h4>"},
            { EmailType.GroupConfirmationByAdmin,  @"<h4>Confirm the creation of a group {groupname} of more than 20 people, follow the link: <a href='{callbackurl}'>link</a></h4>"},
            { EmailType.ApprovedGroupCreation, @"<h4>You get approve for the creation of a group {groupname} of more than 20 people, follow the link: <a href='{callbackurl}'>link</a></h4>" },
            { EmailType.GroupInvitationToStudent, @"<h4>You get invitation to the group {groupname}"+
                ", follow the link: <a href='{callbackurl}'>link</a></h4>"},     
            { EmailType.GetTempPasswordToAdmin, @"<h4>You get information about your account</h4>" +
                                               "<hr/>" +
                                               "<p>First name: {firstname}</p>" +
                                               "<p>Last name: {lastname}</p>" +
                                               "<p>Email: {email}</p>" +
                                               "<p>Role: {userrole}</p>" +
                                               "<bolt>Temporary Password: {temppassword}</bolt>" + 
                                               "<h3>You need to change password at the first visit</h3>"},
            {EmailType.EducationMaterialApproveByAdmin, @"<h4>Confirm uploading of educational material by {firstname} {lastname}, follow the link: <a href='{callbackurl}'>link</a></h4></h4>" },
            { EmailType.ApprovedUploadEducationalMaterial, @"<h4>Educational material that you added was successfully approved by Admin</h4>" },
            { EmailType.DeniedUploadEducationalMaterial, @"<h4>Educational material that you added was denied by Admin</h4>" },
        };

        public static (string, string) GetEmailSubjectAndBody(EmailType emailType, Dictionary<string, object> placeholderNameSandValues)
        {           
            if(!SubjectGetter.ContainsKey(emailType) || !BodyGetter.ContainsKey(emailType))
            {
                return (String.Empty, String.Empty);
            }

            if(placeholderNameSandValues.Count == 0)
            {
                return (SubjectGetter[emailType], BodyGetter[emailType]);
            }

            var parameters = FillParameters(placeholderNameSandValues);

            var subject = SubjectGetter[emailType];
            var body = BodyGetter[emailType];
            var resultBody = parameters.Aggregate(body, (s, kv) => s.Replace(kv.Key, kv.Value.ToString()));

            return (subject, resultBody);
        }

        private static Dictionary<string, string> FillParameters(Dictionary<string, object> placeholderNameSandValues)
        {
            var parameters = new Dictionary<string, string>();
            
            foreach (var parameter in placeholderNameSandValues)
            {
                var parameterName = parameter.Key;

                if (parameterName.Equals(String.Empty))
                {
                    break;
                }

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
