using HF.Application.Auth;
using HF.Application.Common.Responses;
using HF.Application.TeacherAssignments.Requests;
using HF.Application.TeacherAssignments.Responses;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HF.API.v1.Controllers;

/// <summary>
/// Provides REST API endpoints for teacher assignment management.
/// </summary>
[Route("api/v1/teacherassignments")]
[ApiController]
[Tags("TeacherAssignments")]
public sealed class TeacherAssignmentsController(IMediator mediator) : ControllerBase
{
    /// <summary>Creates a new teacher assignment record.</summary>
    [HttpPost]
    [Authorize(Policy = AppPermissions.TeacherAssignmentsCreate)]
    [ProducesResponseType<TeacherAssignmentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ResponseBase<TeacherAssignmentResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTeacherAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(result);
    }

    /// <summary>Gets a teacher assignment record by unique identifier.</summary>
    [HttpGet("{id}")]
    [Authorize(Policy = AppPermissions.TeacherAssignmentsRead)]
    [ProducesResponseType<TeacherAssignmentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<TeacherAssignmentResponse>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTeacherAssignmentByIdRequest(id), cancellationToken);
        return result.Success
            ? Ok(result.Value)
            : NotFound(result);
    }

    /// <summary>Lists teacher assignment records matching optional filters.</summary>
    [HttpGet]
    [Authorize(Policy = AppPermissions.TeacherAssignmentsRead)]
    [ProducesResponseType<IReadOnlyList<TeacherAssignmentResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> List(
        [FromQuery] string? teacherId,
        [FromQuery] string? subjectId,
        [FromQuery] int? academicYear,
        [FromQuery] int? academicSemester,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ListTeacherAssignmentsRequest(teacherId, subjectId, academicYear, academicSemester),
            cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>Updates an existing teacher assignment record.</summary>
    [HttpPut("{id}")]
    [Authorize(Policy = AppPermissions.TeacherAssignmentsUpdate)]
    [ProducesResponseType<TeacherAssignmentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResponseBase<TeacherAssignmentResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<TeacherAssignmentResponse>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        [FromRoute] string id,
        [FromBody] UpdateTeacherAssignmentRequest request,
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

    /// <summary>Deletes a teacher assignment record by identifier.</summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = AppPermissions.TeacherAssignmentsDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteTeacherAssignmentRequest(id), cancellationToken);
        if (result.Success)
        {
            return NoContent();
        }

        return result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
            ? NotFound(result)
            : Conflict(result);
    }
}

