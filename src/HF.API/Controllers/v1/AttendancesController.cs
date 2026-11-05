using HF.Application.Attendances.Requests;
using HF.Application.Attendances.Responses;
using HF.Application.Auth;
using HF.Application.Common.Responses;
using HF.Domain.Attendances;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HF.API.v1.Controllers;

/// <summary>
/// Provides REST API endpoints for attendance management.
/// </summary>
[Route("api/v1/attendances")]
[ApiController]
[Tags("Attendances")]
public sealed class AttendancesController(IMediator mediator) : ControllerBase
{
    /// <summary>Creates a new attendance record.</summary>
    [HttpPost]
    [Authorize(Policy = AppPermissions.AttendancesCreate)]
    [ProducesResponseType<AttendanceResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ResponseBase<AttendanceResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAttendanceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(result);
    }

    /// <summary>Gets an attendance record by unique identifier.</summary>
    [HttpGet("{id}")]
    [Authorize(Policy = AppPermissions.AttendancesRead)]
    [ProducesResponseType<AttendanceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<AttendanceResponse>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAttendanceByIdRequest(id), cancellationToken);
        return result.Success
            ? Ok(result.Value)
            : NotFound(result);
    }

    /// <summary>Lists attendance records matching optional filters.</summary>
    [HttpGet]
    [Authorize(Policy = AppPermissions.AttendancesRead)]
    [ProducesResponseType<IReadOnlyList<AttendanceResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> List(
        [FromQuery] string? classSessionId,
        [FromQuery] string? enrollmentId,
        [FromQuery] AttendanceStatus? status,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ListAttendancesRequest(classSessionId, enrollmentId, status),
            cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>Updates an existing attendance record.</summary>
    [HttpPut("{id}")]
    [Authorize(Policy = AppPermissions.AttendancesUpdate)]
    [ProducesResponseType<AttendanceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResponseBase<AttendanceResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<AttendanceResponse>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        [FromRoute] string id,
        [FromBody] UpdateAttendanceRequest request,
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

    /// <summary>Deletes an attendance record by identifier.</summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = AppPermissions.AttendancesDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteAttendanceRequest(id), cancellationToken);
        if (result.Success)
        {
            return NoContent();
        }

        return result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
            ? NotFound(result)
            : Conflict(result);
    }
}

