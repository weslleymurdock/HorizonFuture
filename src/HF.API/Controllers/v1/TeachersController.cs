using HF.Application.Auth;
using HF.Application.Common.Responses;
using HF.Application.Teachers.Requests;
using HF.Application.Teachers.Responses;
using HF.Domain.Teachers;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HF.API.v1.Controllers;

/// <summary>
/// Provides REST API endpoints for teacher management.
/// </summary>
[Route("api/v1/teachers")]
[ApiController]
[Tags("Teachers")]
public sealed class TeachersController(IMediator mediator) : ControllerBase
{
    /// <summary>Creates a new teacher record.</summary>
    [HttpPost]
    [Authorize(Policy = AppPermissions.TeachersCreate)]
    [ProducesResponseType<TeacherResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ResponseBase<TeacherResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTeacherRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(result);
    }

    /// <summary>Gets a teacher record by unique identifier.</summary>
    [HttpGet("{id}")]
    [Authorize(Policy = AppPermissions.TeachersRead)]
    [ProducesResponseType<TeacherResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<TeacherResponse>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTeacherByIdRequest(id), cancellationToken);
        return result.Success
            ? Ok(result.Value)
            : NotFound(result);
    }

    /// <summary>Lists teacher records matching optional filters.</summary>
    [HttpGet]
    [Authorize(Policy = AppPermissions.TeachersRead)]
    [ProducesResponseType<IReadOnlyList<TeacherResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> List(
        [FromQuery] string? schoolUnitId,
        [FromQuery] TeacherStatus? status,
        [FromQuery] string? specialty,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListTeachersRequest(schoolUnitId, status, specialty), cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>Updates an existing teacher record.</summary>
    [HttpPut("{id}")]
    [Authorize(Policy = AppPermissions.TeachersUpdate)]
    [ProducesResponseType<TeacherResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResponseBase<TeacherResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<TeacherResponse>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        [FromRoute] string id,
        [FromBody] UpdateTeacherRequest request,
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

    /// <summary>Deletes a teacher record by identifier.</summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = AppPermissions.TeachersDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteTeacherRequest(id), cancellationToken);
        if (result.Success)
        {
            return NoContent();
        }

        return result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
            ? NotFound(result)
            : Conflict(result);
    }
}

