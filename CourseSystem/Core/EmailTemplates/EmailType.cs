using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.EmailTemplates
{
    public enum EmailType
    {
        CodeVerification,
        AccountApproveByAdmin,
        ApprovedGroup,
        Registration,
        Confirm,
        ConfirmDeletionByAdmin,
        ConfirmDeletionByUser,
        CourseInvitation,
        GroupConfirmationByAdmin,
        GroupConfirmationByTeacher,
        GroupInvitation
    }
}
