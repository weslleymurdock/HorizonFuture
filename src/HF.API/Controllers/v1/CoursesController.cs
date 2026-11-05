using HF.Application.Auth;
using HF.Application.Common.Responses;
using HF.Application.Courses.Requests;
using HF.Application.Courses.Responses;
using HF.Domain.Courses;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HF.API.v1.Controllers;

/// <summary>
/// Provides REST API endpoints for course management.
/// </summary>
[Route("api/v1/courses")]
[ApiController]
[Tags("Courses")]
public sealed class CoursesController(IMediator mediator) : ControllerBase
{
    /// <summary>Creates a new course record.</summary>
    [HttpPost]
    [Authorize(Policy = AppPermissions.CoursesCreate)]
    [ProducesResponseType<CourseResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ResponseBase<CourseResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCourseRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(result);
    }

    /// <summary>Gets a course record by unique identifier.</summary>
    [HttpGet("{id}")]
    [Authorize(Policy = AppPermissions.CoursesRead)]
    [ProducesResponseType<CourseResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<CourseResponse>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCourseByIdRequest(id), cancellationToken);
        return result.Success
            ? Ok(result.Value)
            : NotFound(result);
    }

    /// <summary>Lists course records matching optional filters.</summary>
    [HttpGet]
    [Authorize(Policy = AppPermissions.CoursesRead)]
    [ProducesResponseType<IReadOnlyList<CourseResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> List(
        [FromQuery] string? schoolUnitId,
        [FromQuery] CourseStatus? status,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListCoursesRequest(schoolUnitId, status), cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>Updates an existing course record.</summary>
    [HttpPut("{id}")]
    [Authorize(Policy = AppPermissions.CoursesUpdate)]
    [ProducesResponseType<CourseResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResponseBase<CourseResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<CourseResponse>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        [FromRoute] string id,
        [FromBody] UpdateCourseRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request with { Id = id }, cancellationToken);
        if (!result.Success && result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(result);
        }

        return result.Success
            ? Ok(result.Value)
            : BadRequest(result);
    }

    /// <summary>Deletes a course record by identifier.</summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = AppPermissions.CoursesDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteCourseRequest(id), cancellationToken);
        if (result.Success)
        {
            return NoContent();
        }

        return result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
            ? NotFound(result)
            : Conflict(result);
    }
}

