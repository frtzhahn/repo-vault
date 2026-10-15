using MySqlConnector;
using RepoVault.Core.Data;
using RepoVault.Core.Models;
using RepoVault.Core.Security;

namespace RepoVault.Core.Services;

public sealed class CourseService
{
    private readonly DbConnectionFactory _db;

    public CourseService(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<(bool Ok, string Message, Course? Course)> CreateCourseAsync(
        User actor, string courseCode, string courseTitle, string department,
        string semester, CancellationToken ct = default)
    {
        if (!RbacGuard.CanManageCourses(actor))
        {
            return (false, "only faculty can manage courses.", null);
        }

        courseCode = courseCode.Trim();
        courseTitle = courseTitle.Trim();
        department = department.Trim();
        semester = semester.Trim();

        if (courseCode.Length == 0 || courseTitle.Length == 0 ||
            department.Length == 0 || semester.Length == 0)
        {
            return (false, "course code, title, department, and semester are required.", null);
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Courses (CourseCode, CourseTitle, Department, Semester, " +
                          "CreatedByFacultyId, CreatedAt) VALUES (@c, @t, @d, @s, @f, @now);";
        cmd.Parameters.AddWithValue("@c", courseCode);
        cmd.Parameters.AddWithValue("@t", courseTitle);
        cmd.Parameters.AddWithValue("@d", department);
        cmd.Parameters.AddWithValue("@s", semester);
        cmd.Parameters.AddWithValue("@f", actor.UserId);
        cmd.Parameters.AddWithValue("@now", DateTime.UtcNow);

        try
        {
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            return (false, "a course with this code and semester already exists.", null);
        }

        var created = new Course
        {
            CourseId = (int)cmd.LastInsertedId,
            CourseCode = courseCode,
            CourseTitle = courseTitle,
            Department = department,
            Semester = semester,
            CreatedByFacultyId = actor.UserId,
            CreatedAt = DateTime.UtcNow,
        };
        return (true, "ok", created);
    }

    public async Task<Course?> GetByIdAsync(int courseId, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT CourseId, CourseCode, CourseTitle, Department, Semester, " +
                          "CreatedByFacultyId, CreatedAt FROM Courses WHERE CourseId = @id LIMIT 1;";
        cmd.Parameters.AddWithValue("@id", courseId);

        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        if (!await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            return null;
        }

        return MapCourse(reader);
    }

    public async Task<List<Course>> ListCoursesAsync(
        string? semesterFilter = null, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();

        string sql = "SELECT CourseId, CourseCode, CourseTitle, Department, Semester, " +
                     "CreatedByFacultyId, CreatedAt FROM Courses";
        if (!string.IsNullOrWhiteSpace(semesterFilter))
        {
            sql += " WHERE Semester = @s";
            cmd.Parameters.AddWithValue("@s", semesterFilter.Trim());
        }

        cmd.CommandText = sql + ";";

        var courses = new List<Course>();
        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        while (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            courses.Add(MapCourse(reader));
        }

        return courses;
    }

    public async Task<(bool Ok, string Message)> UpdateCourseAsync(
        User actor, int courseId, string courseCode, string courseTitle,
        string department, string semester, CancellationToken ct = default)
    {
        if (!RbacGuard.CanManageCourses(actor))
        {
            return (false, "only faculty can manage courses.");
        }

        courseCode = courseCode.Trim();
        courseTitle = courseTitle.Trim();
        department = department.Trim();
        semester = semester.Trim();

        if (courseCode.Length == 0 || courseTitle.Length == 0 ||
            department.Length == 0 || semester.Length == 0)
        {
            return (false, "course code, title, department, and semester are required.");
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Courses SET CourseCode = @c, CourseTitle = @t, Department = @d, " +
                          "Semester = @s WHERE CourseId = @id;";
        cmd.Parameters.AddWithValue("@c", courseCode);
        cmd.Parameters.AddWithValue("@t", courseTitle);
        cmd.Parameters.AddWithValue("@d", department);
        cmd.Parameters.AddWithValue("@s", semester);
        cmd.Parameters.AddWithValue("@id", courseId);

        try
        {
            int rows = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            if (rows == 0)
            {
                return (false, "course not found.");
            }
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            return (false, "a course with this code and semester already exists.");
        }

        return (true, "ok");
    }

    public async Task<(bool Ok, string Message)> DeleteCourseAsync(
        User actor, int courseId, CancellationToken ct = default)
    {
        if (!RbacGuard.CanManageCourses(actor))
        {
            return (false, "only faculty can manage courses.");
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);

        // courses cascade to repositories, so refuse while any repo still references it
        await using (var count = conn.CreateCommand())
        {
            count.CommandText = "SELECT COUNT(*) FROM Repositories WHERE CourseId = @id;";
            count.Parameters.AddWithValue("@id", courseId);
            object? total = await count.ExecuteScalarAsync(ct).ConfigureAwait(false);
            if (Convert.ToInt32(total) > 0)
            {
                return (false, "cannot delete a course that still has repositories.");
            }
        }

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Courses WHERE CourseId = @id;";
        cmd.Parameters.AddWithValue("@id", courseId);

        int rows = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        if (rows == 0)
        {
            return (false, "course not found.");
        }

        return (true, "ok");
    }

    internal static Course MapCourse(MySqlDataReader reader)
    {
        return new Course
        {
            CourseId = Convert.ToInt32(reader["CourseId"]),
            CourseCode = reader["CourseCode"]?.ToString() ?? string.Empty,
            CourseTitle = reader["CourseTitle"]?.ToString() ?? string.Empty,
            Department = reader["Department"]?.ToString() ?? string.Empty,
            Semester = reader["Semester"]?.ToString() ?? string.Empty,
            CreatedByFacultyId = Convert.ToInt32(reader["CreatedByFacultyId"]),
            CreatedAt = (DateTime)reader["CreatedAt"],
        };
    }
}
