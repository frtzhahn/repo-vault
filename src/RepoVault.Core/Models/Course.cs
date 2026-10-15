namespace RepoVault.Core.Models;

public sealed class Course
{
    public int CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Semester { get; set; } = string.Empty;
    public int CreatedByFacultyId { get; set; }
    public DateTime CreatedAt { get; set; }
}
