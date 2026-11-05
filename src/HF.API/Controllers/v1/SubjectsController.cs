using HF.Application.Auth;
using HF.Application.Common.Responses;
using HF.Application.Courses.Requests;
using HF.Application.Courses.Responses;
using HF.Domain.Courses;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HF.API.v1.Controllers;

/// <summary>
/// Provides REST API endpoints for subject management.
/// </summary>
[Route("api/v1/subjects")]
[ApiController]
[Tags("Subjects")]
public sealed class SubjectsController(IMediator mediator) : ControllerBase
{
    /// <summary>Creates a new subject record.</summary>
    [HttpPost]
    [Authorize(Policy = AppPermissions.SubjectsCreate)]
    [ProducesResponseType<SubjectResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ResponseBase<SubjectResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSubjectRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(result);
    }

    /// <summary>Gets a subject record by unique identifier.</summary>
    [HttpGet("{id}")]
    [Authorize(Policy = AppPermissions.SubjectsRead)]
    [ProducesResponseType<SubjectResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<SubjectResponse>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSubjectByIdRequest(id), cancellationToken);
        return result.Success
            ? Ok(result.Value)
            : NotFound(result);
    }

    /// <summary>Lists subject records matching optional filters.</summary>
    [HttpGet]
    [Authorize(Policy = AppPermissions.SubjectsRead)]
    [ProducesResponseType<IReadOnlyList<SubjectResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> List(
        [FromQuery] string? courseId,
        [FromQuery] string? schoolUnitId,
        [FromQuery] SubjectStatus? status,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListSubjectsRequest(courseId, schoolUnitId, status), cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>Updates an existing subject record.</summary>
    [HttpPut("{id}")]
    [Authorize(Policy = AppPermissions.SubjectsUpdate)]
    [ProducesResponseType<SubjectResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResponseBase<SubjectResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<SubjectResponse>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        [FromRoute] string id,
        [FromBody] UpdateSubjectRequest request,
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

    /// <summary>Deletes a subject record by identifier.</summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = AppPermissions.SubjectsDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteSubjectRequest(id), cancellationToken);
        if (result.Success)
        {
            return NoContent();
        }

        return result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
            ? NotFound(result)
            : Conflict(result);
    }
}

