using APBD_TASK8.Data;
using APBD_TASK8.DTOs;
using APBD_TASK8.Models;
using Microsoft.EntityFrameworkCore;

namespace APBD_TASK8.Services;

public class SubmissionService(UniversityTasksDbContext db)
{
    public async Task<(SubmissionDto? dto, int statusCode, string? error)> CreateAsync(
        CreateSubmissionDto request)
    {
        if (string.IsNullOrWhiteSpace(request.RepositoryUrl) ||
            !request.RepositoryUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return (null, 400, "RepositoryUrl must be non-empty and start with https://");

        var student = await db.Students.AsNoTracking()
            .FirstOrDefaultAsync(s => s.StudentId == request.StudentId);

        if (student is null)
            return (null, 404, "Student not found.");

        if (!student.IsActive)
            return (null, 400, "Student is not active.");

        var assignment = await db.Assignments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.AssignmentId == request.AssignmentId);

        if (assignment is null)
            return (null, 404, "Assignment not found.");

        if (!assignment.IsPublished)
            return (null, 400, "Assignment is not published.");

        var enrolled = await db.Enrollments.AsNoTracking()
            .AnyAsync(e => e.StudentId == request.StudentId
                        && e.CourseId == assignment.CourseId
                        && (e.Status == "Active" || e.Status == "Completed"));

        if (!enrolled)
            return (null, 400, "Student is not enrolled (Active/Completed) in the course that owns this assignment.");

        var duplicate = await db.Submissions.AsNoTracking()
            .AnyAsync(s => s.AssignmentId == request.AssignmentId
                        && s.StudentId == request.StudentId);

        if (duplicate)
            return (null, 409, "Student has already submitted this assignment.");

        var now = DateTime.UtcNow;
        var submission = new Submission
        {
            AssignmentId = request.AssignmentId,
            StudentId = request.StudentId,
            RepositoryUrl = request.RepositoryUrl,
            SubmittedAt = now,
            Status = assignment.IsOverdue(now) ? "Late" : "Submitted"
        };

        db.Submissions.Add(submission);
        await db.SaveChangesAsync();

        var saved = await db.Submissions
            .AsNoTracking()
            .Include(s => s.Student)
            .Include(s => s.Assignment)
            .FirstAsync(s => s.SubmissionId == submission.SubmissionId);

        return (ToDto(saved), 201, null);
    }

    public async Task<(SubmissionDto? dto, int statusCode, string? error)> GradeAsync(
        int id, GradeSubmissionDto request)
    {
        var submission = await db.Submissions
            .Include(s => s.Student)
            .Include(s => s.Assignment)
            .FirstOrDefaultAsync(s => s.SubmissionId == id);

        if (submission is null)
            return (null, 404, "Submission not found.");

        if (request.Score < 0)
            return (null, 400, "Score cannot be negative.");

        if (request.Score > submission.Assignment.MaxPoints)
            return (null, 400,
                $"Score ({request.Score}) exceeds MaxPoints ({submission.Assignment.MaxPoints}) for this assignment.");

        submission.Score = request.Score;
        submission.Feedback = request.Feedback;
        submission.Status = "Graded";

        await db.SaveChangesAsync();

        return (ToDto(submission), 200, null);
    }

    public async Task<(int statusCode, string? error)> DeleteAsync(int id)
    {
        var submission = await db.Submissions
            .FirstOrDefaultAsync(s => s.SubmissionId == id);

        if (submission is null)
            return (404, "Submission not found.");

        if (submission.Status == "Graded")
            return (400, "Cannot delete a graded submission.");

        db.Submissions.Remove(submission);
        await db.SaveChangesAsync();

        return (204, null);
    }

    private static SubmissionDto ToDto(Submission s) => new(
        s.SubmissionId,
        new StudentBriefDto(s.Student.StudentId, s.Student.IndexNumber, s.Student.FullName),
        new AssignmentBriefDto(s.Assignment.AssignmentId, s.Assignment.Title, s.Assignment.CourseId),
        s.RepositoryUrl,
        s.SubmittedAt,
        s.Status,
        s.Score,
        s.Feedback);
}