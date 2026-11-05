using HF.Application.AssetsLiabilities.Requests;
using HF.Application.AssetsLiabilities.Responses;
using HF.Application.Auth;
using HF.Application.Common.Responses;
using HF.Domain.AssetsLiabilities;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HF.API.v1.Controllers;

/// <summary>
/// Provides REST API endpoints for liability management.
/// </summary>
[Route("api/v1/liabilities")]
[ApiController]
[Tags("Liabilities")]
public sealed class LiabilitiesController(IMediator mediator) : ControllerBase
{
    /// <summary>Creates a new liability record.</summary>
    [HttpPost]
    [Authorize(Policy = AppPermissions.LiabilitiesCreate)]
    [ProducesResponseType<LiabilityResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ResponseBase<LiabilityResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateLiabilityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(result);
    }

    /// <summary>Gets a liability record by unique identifier.</summary>
    [HttpGet("{id}")]
    [Authorize(Policy = AppPermissions.LiabilitiesRead)]
    [ProducesResponseType<LiabilityResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<LiabilityResponse>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetLiabilityByIdRequest(id), cancellationToken);
        return result.Success
            ? Ok(result.Value)
            : NotFound(result);
    }

    /// <summary>Lists liability records matching optional filters.</summary>
    [HttpGet]
    [Authorize(Policy = AppPermissions.LiabilitiesRead)]
    [ProducesResponseType<IReadOnlyList<LiabilityResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> List(
        [FromQuery] string? schoolUnitId,
        [FromQuery] LiabilityStatus? status,
        [FromQuery] LiabilityCategory? category,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListLiabilitiesRequest(schoolUnitId, status, category), cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>Updates an existing liability record.</summary>
    [HttpPut("{id}")]
    [Authorize(Policy = AppPermissions.LiabilitiesUpdate)]
    [ProducesResponseType<LiabilityResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResponseBase<LiabilityResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<LiabilityResponse>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        [FromRoute] string id,
        [FromBody] UpdateLiabilityRequest request,
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

    /// <summary>Deletes a liability record by identifier.</summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = AppPermissions.LiabilitiesDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteLiabilityRequest(id), cancellationToken);
        if (result.Success)
        {
            return NoContent();
        }

        return result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
            ? NotFound(result)
            : Conflict(result);
    }
}

