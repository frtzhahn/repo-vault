using RepoVault.Core.Models;

namespace RepoVault.Desktop.Views.Student;

internal sealed class StudentRepoRow
{
    public StudentRepoRow(RepositoryRecord repo, string courseCode)
    {
        Repo = repo;
        CourseCode = courseCode;
    }

    public RepositoryRecord Repo { get; }
    public string CourseCode { get; }

    public string Repository => Repo.RepoSlug;
    public string Title => Repo.Title;
    public string Course => CourseCode;
    public string Language => Repo.PrimaryLanguage ?? "-";
    public int SourceLines => Repo.SourceLines;
    public string Deadline => UiTheme.FormatUtcShort(Repo.DeadlineUtc);
    public string Status => UiTheme.RepoStatus(Repo);
}
