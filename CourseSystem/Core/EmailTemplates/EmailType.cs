using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.EmailTemplates
{
    public enum EmailType
    {
        CodeVerification = 0,
        AccountApproveByAdmin,
        AccountApproveByUser,
        ApprovedGroupCreation,
        UserRegistration,
        ConfirmAdminRegistration,
        ConfirmDeletionByAdmin,
        ConfirmDeletionByUser,
        CourseInvitation,
        GroupConfirmationByAdmin,
        GroupInvitationToStudent,
        GetTempPasswordToAdmin,
        EducationMaterialApproveByAdmin,
        ApprovedUploadEducationalMaterial,
        DeniedUploadEducationalMaterial
    }
}
