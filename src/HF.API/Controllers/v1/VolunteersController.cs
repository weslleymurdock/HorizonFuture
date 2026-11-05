using HF.Application.Auth;
using HF.Application.Common.Responses;
using HF.Application.Volunteers.Requests;
using HF.Application.Volunteers.Responses;
using HF.Domain.Volunteers;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HF.API.v1.Controllers;

/// <summary>
/// Provides REST API endpoints for volunteer management.
/// </summary>
[Route("api/v1/volunteers")]
[ApiController]
[Tags("Volunteers")]
public sealed class VolunteersController(IMediator mediator) : ControllerBase
{
    /// <summary>Creates a new volunteer record.</summary>
    [HttpPost]
    [Authorize(Policy = AppPermissions.VolunteersCreate)]
    [ProducesResponseType<VolunteerResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ResponseBase<VolunteerResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateVolunteerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(result);
    }

    /// <summary>Gets a volunteer record by unique identifier.</summary>
    [HttpGet("{id}")]
    [Authorize(Policy = AppPermissions.VolunteersRead)]
    [ProducesResponseType<VolunteerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<VolunteerResponse>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetVolunteerByIdRequest(id), cancellationToken);
        return result.Success
            ? Ok(result.Value)
            : NotFound(result);
    }

    /// <summary>Lists volunteer records matching optional filters.</summary>
    [HttpGet]
    [Authorize(Policy = AppPermissions.VolunteersRead)]
    [ProducesResponseType<IReadOnlyList<VolunteerResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> List(
        [FromQuery] string? schoolUnitId,
        [FromQuery] VolunteerStatus? status,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListVolunteersRequest(schoolUnitId, status), cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>Updates an existing volunteer record.</summary>
    [HttpPut("{id}")]
    [Authorize(Policy = AppPermissions.VolunteersUpdate)]
    [ProducesResponseType<VolunteerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResponseBase<VolunteerResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<VolunteerResponse>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        [FromRoute] string id,
        [FromBody] UpdateVolunteerRequest request,
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

    /// <summary>Deletes a volunteer record by identifier.</summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = AppPermissions.VolunteersDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteVolunteerRequest(id), cancellationToken);
        if (result.Success)
        {
            return NoContent();
        }

        return result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
            ? NotFound(result)
            : Conflict(result);
    }
}

