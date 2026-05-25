namespace APBD_TASK8.DTOs;

// ── Response DTOs ──────────────────────────────────────────────

public record CourseDto(
    int CourseId,
    string Code,
    string Name,
    int Credits,
    bool IsActive,
    int AssignmentCount);

public record AssignmentDto(
    int AssignmentId,
    string Title,
    DateTime DueDate,
    int MaxPoints,
    bool IsPublished,
    int SubmissionCount);

public record EnrollmentSummaryDto(
    int EnrollmentId,
    int CourseId,
    string CourseCode,
    string CourseName,
    string Status);

public record SubmissionSummaryDto(
    int SubmissionId,
    int AssignmentId,
    string AssignmentTitle,
    string RepositoryUrl,
    DateTime SubmittedAt,
    string Status,
    int? Score);

public record StudentDashboardDto(
    int StudentId,
    string IndexNumber,
    string FullName,
    bool IsActive,
    IEnumerable<EnrollmentSummaryDto> Enrollments,
    IEnumerable<SubmissionSummaryDto> Submissions);

public record StudentBriefDto(int StudentId, string IndexNumber, string FullName);
public record AssignmentBriefDto(int AssignmentId, string Title, int CourseId);

public record SubmissionDto(
    int SubmissionId,
    StudentBriefDto Student,
    AssignmentBriefDto Assignment,
    string RepositoryUrl,
    DateTime SubmittedAt,
    string Status,
    int? Score,
    string? Feedback);

// ── Request DTOs ───────────────────────────────────────────────

public record CreateSubmissionDto(
    int AssignmentId,
    int StudentId,
    string RepositoryUrl);

public record GradeSubmissionDto(
    int Score,
    string? Feedback);
