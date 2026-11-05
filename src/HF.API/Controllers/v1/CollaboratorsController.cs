using HF.Application.Auth;
using HF.Application.Collaborators.Requests;
using HF.Application.Collaborators.Responses;
using HF.Application.Common.Responses;
using HF.Domain.Collaborators;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HF.API.v1.Controllers;

/// <summary>
/// Provides REST API endpoints for collaborator management.
/// </summary>
[Route("api/v1/collaborators")]
[ApiController]
[Tags("Collaborators")]
public sealed class CollaboratorsController(IMediator mediator) : ControllerBase
{
    /// <summary>Creates a new collaborator record.</summary>
    [HttpPost]
    [Authorize(Policy = AppPermissions.CollaboratorsCreate)]
    [ProducesResponseType<CollaboratorResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ResponseBase<CollaboratorResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCollaboratorRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(result);
    }

    /// <summary>Gets a collaborator record by unique identifier.</summary>
    [HttpGet("{id}")]
    [Authorize(Policy = AppPermissions.CollaboratorsRead)]
    [ProducesResponseType<CollaboratorResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<CollaboratorResponse>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCollaboratorByIdRequest(id), cancellationToken);
        return result.Success
            ? Ok(result.Value)
            : NotFound(result);
    }

    /// <summary>Lists collaborator records matching optional filters.</summary>
    [HttpGet]
    [Authorize(Policy = AppPermissions.CollaboratorsRead)]
    [ProducesResponseType<IReadOnlyList<CollaboratorResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> List(
        [FromQuery] string? schoolUnitId,
        [FromQuery] CollaboratorStatus? status,
        [FromQuery] string? department,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListCollaboratorsRequest(schoolUnitId, status, department), cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>Updates an existing collaborator record.</summary>
    [HttpPut("{id}")]
    [Authorize(Policy = AppPermissions.CollaboratorsUpdate)]
    [ProducesResponseType<CollaboratorResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResponseBase<CollaboratorResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<CollaboratorResponse>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        [FromRoute] string id,
        [FromBody] UpdateCollaboratorRequest request,
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

    /// <summary>Deletes a collaborator record by identifier.</summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = AppPermissions.CollaboratorsDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteCollaboratorRequest(id), cancellationToken);
        if (result.Success)
        {
            return NoContent();
        }

        return result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
            ? NotFound(result)
            : Conflict(result);
    }
}

