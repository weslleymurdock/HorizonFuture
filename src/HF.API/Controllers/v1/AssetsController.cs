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
/// Provides REST API endpoints for asset management.
/// </summary>
[Route("api/v1/assets")]
[ApiController]
[Tags("Assets")]
public sealed class AssetsController(IMediator mediator) : ControllerBase
{
    /// <summary>Creates a new asset record.</summary>
    [HttpPost]
    [Authorize(Policy = AppPermissions.AssetsCreate)]
    [ProducesResponseType<AssetResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ResponseBase<AssetResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAssetRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(result);
    }

    /// <summary>Gets an asset record by unique identifier.</summary>
    [HttpGet("{id}")]
    [Authorize(Policy = AppPermissions.AssetsRead)]
    [ProducesResponseType<AssetResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<AssetResponse>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAssetByIdRequest(id), cancellationToken);
        return result.Success
            ? Ok(result.Value)
            : NotFound(result);
    }

    /// <summary>Lists asset records matching optional filters.</summary>
    [HttpGet]
    [Authorize(Policy = AppPermissions.AssetsRead)]
    [ProducesResponseType<IReadOnlyList<AssetResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> List(
        [FromQuery] string? schoolUnitId,
        [FromQuery] AssetStatus? status,
        [FromQuery] AssetCategory? category,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListAssetsRequest(schoolUnitId, status, category), cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>Updates an existing asset record.</summary>
    [HttpPut("{id}")]
    [Authorize(Policy = AppPermissions.AssetsUpdate)]
    [ProducesResponseType<AssetResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResponseBase<AssetResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<AssetResponse>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        [FromRoute] string id,
        [FromBody] UpdateAssetRequest request,
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

    /// <summary>Deletes an asset record by identifier.</summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = AppPermissions.AssetsDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteAssetRequest(id), cancellationToken);
        if (result.Success)
        {
            return NoContent();
        }

        return result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
            ? NotFound(result)
            : Conflict(result);
    }
}

