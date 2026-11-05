using HF.Application.Auth;
using HF.Application.ClassSessions.Requests;
using HF.Application.ClassSessions.Responses;
using HF.Application.Common.Responses;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HF.API.v1.Controllers;

/// <summary>
/// Provides REST API endpoints for class session management.
/// </summary>
[Route("api/v1/classsessions")]
[ApiController]
[Tags("ClassSessions")]
public sealed class ClassSessionsController(IMediator mediator) : ControllerBase
{
    /// <summary>Creates a new class session record.</summary>
    [HttpPost]
    [Authorize(Policy = AppPermissions.ClassSessionsCreate)]
    [ProducesResponseType<ClassSessionResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ResponseBase<ClassSessionResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateClassSessionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(result);
    }

    /// <summary>Gets a class session record by unique identifier.</summary>
    [HttpGet("{id}")]
    [Authorize(Policy = AppPermissions.ClassSessionsRead)]
    [ProducesResponseType<ClassSessionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<ClassSessionResponse>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetClassSessionByIdRequest(id), cancellationToken);
        return result.Success
            ? Ok(result.Value)
            : NotFound(result);
    }

    /// <summary>Lists class session records matching optional filters.</summary>
    [HttpGet]
    [Authorize(Policy = AppPermissions.ClassSessionsRead)]
    [ProducesResponseType<IReadOnlyList<ClassSessionResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> List(
        [FromQuery] string? teacherAssignmentId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ListClassSessionsRequest(teacherAssignmentId, fromDate, toDate),
            cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>Updates an existing class session record.</summary>
    [HttpPut("{id}")]
    [Authorize(Policy = AppPermissions.ClassSessionsUpdate)]
    [ProducesResponseType<ClassSessionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResponseBase<ClassSessionResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<ClassSessionResponse>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        [FromRoute] string id,
        [FromBody] UpdateClassSessionRequest request,
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

    /// <summary>Deletes a class session record by identifier.</summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = AppPermissions.ClassSessionsDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteClassSessionRequest(id), cancellationToken);
        if (result.Success)
        {
            return NoContent();
        }

        return result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
            ? NotFound(result)
            : Conflict(result);
    }
}

