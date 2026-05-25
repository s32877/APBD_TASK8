using APBD_TASK8.Data;
using APBD_TASK8.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APBD_TASK8.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController(UniversityTasksDbContext db) : ControllerBase
{
    // GET /api/courses?activeOnly=true
    [HttpGet]
    public async Task<IActionResult> GetCourses([FromQuery] bool activeOnly = false)
    {
        var query = db.Courses.AsNoTracking();

        if (activeOnly)
            query = query.Where(c => c.IsActive);

        var courses = await query
            .Select(c => new CourseDto(
                c.CourseId,
                c.Code,
                c.Name,
                c.Credits,
                c.Assignments.Count))
            .ToListAsync();

        return Ok(courses);
    }

    // GET /api/courses/{idCourse}/assignments?publishedOnly=true
    [HttpGet("{idCourse:int}/assignments")]
    public async Task<IActionResult> GetAssignments(
        int idCourse, [FromQuery] bool publishedOnly = false)
    {
        var courseExists = await db.Courses.AsNoTracking()
            .AnyAsync(c => c.CourseId == idCourse);

        if (!courseExists)
            return NotFound($"Course {idCourse} not found.");

        var query = db.Assignments.AsNoTracking()
            .Where(a => a.CourseId == idCourse);

        if (publishedOnly)
            query = query.Where(a => a.IsPublished);

        var assignments = await query
            .Select(a => new AssignmentDto(
                a.AssignmentId,
                a.Title,
                a.DueDate,
                a.MaxPoints,
                a.IsPublished,
                a.Submissions.Count))
            .ToListAsync();

        return Ok(assignments);
    }
}