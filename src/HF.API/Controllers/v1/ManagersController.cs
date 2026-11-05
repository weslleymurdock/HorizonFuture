using HF.Application.Auth;
using HF.Application.Common.Responses;
using HF.Application.Managers.Requests;
using HF.Application.Managers.Responses;
using HF.Domain.Managers;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HF.API.v1.Controllers;

/// <summary>
/// Provides REST API endpoints for manager management.
/// </summary>
[Route("api/v1/managers")]
[ApiController]
[Tags("Managers")]
public sealed class ManagersController(IMediator mediator) : ControllerBase
{
    /// <summary>Creates a new manager record.</summary>
    [HttpPost]
    [Authorize(Policy = AppPermissions.ManagersCreate)]
    [ProducesResponseType<ManagerResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ResponseBase<ManagerResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateManagerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(result);
    }

    /// <summary>Gets a manager record by unique identifier.</summary>
    [HttpGet("{id}")]
    [Authorize(Policy = AppPermissions.ManagersRead)]
    [ProducesResponseType<ManagerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<ManagerResponse>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetManagerByIdRequest(id), cancellationToken);
        return result.Success
            ? Ok(result.Value)
            : NotFound(result);
    }

    /// <summary>Lists manager records matching optional filters.</summary>
    [HttpGet]
    [Authorize(Policy = AppPermissions.ManagersRead)]
    [ProducesResponseType<IReadOnlyList<ManagerResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> List(
        [FromQuery] string? schoolUnitId,
        [FromQuery] ManagerStatus? status,
        [FromQuery] string? department,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListManagersRequest(schoolUnitId, status, department), cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>Updates an existing manager record.</summary>
    [HttpPut("{id}")]
    [Authorize(Policy = AppPermissions.ManagersUpdate)]
    [ProducesResponseType<ManagerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResponseBase<ManagerResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<ManagerResponse>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        [FromRoute] string id,
        [FromBody] UpdateManagerRequest request,
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

    /// <summary>Deletes a manager record by identifier.</summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = AppPermissions.ManagersDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteManagerRequest(id), cancellationToken);
        if (result.Success)
        {
            return NoContent();
        }

        return result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
            ? NotFound(result)
            : Conflict(result);
    }
}

