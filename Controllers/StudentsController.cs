using APBD_TASK8.Data;
using APBD_TASK8.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APBD_TASK8.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController(UniversityTasksDbContext db) : ControllerBase
{
    // GET /api/students/{idStudent}/dashboard
    [HttpGet("{idStudent:int}/dashboard")]
    public async Task<IActionResult> GetDashboard(int idStudent)
    {
        var student = await db.Students
            .AsNoTracking()
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
            .Include(s => s.Submissions)
                .ThenInclude(sub => sub.Assignment)
            .FirstOrDefaultAsync(s => s.StudentId == idStudent);

        if (student is null)
            return NotFound($"Student {idStudent} not found.");

        var dashboard = new StudentDashboardDto(
            student.StudentId,
            student.IndexNumber,
            student.FullName,
            student.IsActive,
            student.Enrollments.Select(e => new EnrollmentSummaryDto(
                e.EnrollmentId,
                e.CourseId,
                e.Course.Code,
                e.Course.Name,
                e.Status)),
            student.Submissions.Select(s => new SubmissionSummaryDto(
                s.SubmissionId,
                s.AssignmentId,
                s.Assignment.Title,
                s.RepositoryUrl,
                s.SubmittedAt,
                s.Status,
                s.Score)));

        return Ok(dashboard);
    }
}
