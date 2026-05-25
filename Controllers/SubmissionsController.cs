using APBD_TASK8.DTOs;
using APBD_TASK8.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_TASK8.Controllers;

[ApiController]
[Route("api/submissions")]
public class SubmissionsController(SubmissionService svc) : ControllerBase
{
    // POST /api/submissions
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSubmissionDto dto)
    {
        var (result, status, error) = await svc.CreateAsync(dto);

        return status switch
        {
            201 => CreatedAtAction(nameof(Create), new { id = result!.SubmissionId }, result),
            400 => BadRequest(error),
            404 => NotFound(error),
            409 => Conflict(error),
            _   => StatusCode(status, error)
        };
    }

    // PUT /api/submissions/{idSubmission}/grade
    [HttpPut("{idSubmission:int}/grade")]
    public async Task<IActionResult> Grade(int idSubmission, [FromBody] GradeSubmissionDto dto)
    {
        var (result, status, error) = await svc.GradeAsync(idSubmission, dto);

        return status switch
        {
            200 => Ok(result),
            400 => BadRequest(error),
            404 => NotFound(error),
            _   => StatusCode(status, error)
        };
    }

    // DELETE /api/submissions/{idSubmission}
    [HttpDelete("{idSubmission:int}")]
    public async Task<IActionResult> Delete(int idSubmission)
    {
        var (status, error) = await svc.DeleteAsync(idSubmission);

        return status switch
        {
            204 => NoContent(),
            400 => BadRequest(error),
            404 => NotFound(error),
            _   => StatusCode(status, error)
        };
    }
}
