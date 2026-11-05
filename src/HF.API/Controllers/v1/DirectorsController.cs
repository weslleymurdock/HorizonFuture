using HF.Application.Auth;
using HF.Application.Common.Responses;
using HF.Application.Directors.Requests;
using HF.Application.Directors.Responses;
using HF.Domain.Directors;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HF.API.v1.Controllers;

/// <summary>
/// Provides REST API endpoints for director management.
/// </summary>
[Route("api/v1/directors")]
[ApiController]
[Tags("Directors")]
public sealed class DirectorsController(IMediator mediator) : ControllerBase
{
    /// <summary>Creates a new director record.</summary>
    [HttpPost]
    [Authorize(Policy = AppPermissions.DirectorsCreate)]
    [ProducesResponseType<DirectorResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ResponseBase<DirectorResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateDirectorRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(result);
    }

    /// <summary>Gets a director record by unique identifier.</summary>
    [HttpGet("{id}")]
    [Authorize(Policy = AppPermissions.DirectorsRead)]
    [ProducesResponseType<DirectorResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<DirectorResponse>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDirectorByIdRequest(id), cancellationToken);
        return result.Success
            ? Ok(result.Value)
            : NotFound(result);
    }

    /// <summary>Lists director records matching optional filters.</summary>
    [HttpGet]
    [Authorize(Policy = AppPermissions.DirectorsRead)]
    [ProducesResponseType<IReadOnlyList<DirectorResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> List(
        [FromQuery] string? schoolUnitId,
        [FromQuery] DirectorStatus? status,
        [FromQuery] string? directorate,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListDirectorsRequest(schoolUnitId, status, directorate), cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>Updates an existing director record.</summary>
    [HttpPut("{id}")]
    [Authorize(Policy = AppPermissions.DirectorsUpdate)]
    [ProducesResponseType<DirectorResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResponseBase<DirectorResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<DirectorResponse>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        [FromRoute] string id,
        [FromBody] UpdateDirectorRequest request,
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

    /// <summary>Deletes a director record by identifier.</summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = AppPermissions.DirectorsDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteDirectorRequest(id), cancellationToken);
        if (result.Success)
        {
            return NoContent();
        }

        return result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
            ? NotFound(result)
            : Conflict(result);
    }
}

