using Core.Enums;

namespace UI.ViewModels.EmailViewModels
{
    public class ConfirmEducationMaterial
    {
        public string TeacherId { get; set; }
        public string FileUrl { get; set; }
        public string FileName { get; set; }
        public MaterialAccess MaterialAccess { get; set; }
        public int? CourseId { get; set; }
        public int? GroupId { get; set; }
    }
}
