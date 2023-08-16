namespace Core.Models;

public class CourseBackground : BaseEntity
{
    public string Name { get; set; }
    public string Url { get; set; }
    public string FileExtension { get; set; }
    public int CourseId { get; set; }
    public virtual Course Course { get; set; }
}