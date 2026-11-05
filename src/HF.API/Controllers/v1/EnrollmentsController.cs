using HF.Application.Auth;
using HF.Application.Common.Responses;
using HF.Application.Enrollments.Requests;
using HF.Application.Enrollments.Responses;
using HF.Domain.Enrollments;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HF.API.v1.Controllers;

/// <summary>
/// Provides REST API endpoints for enrollment management.
/// </summary>
[Route("api/v1/enrollments")]
[ApiController]
[Tags("Enrollments")]
public sealed class EnrollmentsController(IMediator mediator) : ControllerBase
{
    /// <summary>Creates a new enrollment record.</summary>
    [HttpPost]
    [Authorize(Policy = AppPermissions.EnrollmentsCreate)]
    [ProducesResponseType<EnrollmentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ResponseBase<EnrollmentResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateEnrollmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(result);
    }

    /// <summary>Gets an enrollment record by unique identifier.</summary>
    [HttpGet("{id}")]
    [Authorize(Policy = AppPermissions.EnrollmentsRead)]
    [ProducesResponseType<EnrollmentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<EnrollmentResponse>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetEnrollmentByIdRequest(id), cancellationToken);
        return result.Success
            ? Ok(result.Value)
            : NotFound(result);
    }

    /// <summary>Lists enrollment records matching optional filters.</summary>
    [HttpGet]
    [Authorize(Policy = AppPermissions.EnrollmentsRead)]
    [ProducesResponseType<IReadOnlyList<EnrollmentResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> List(
        [FromQuery] string? studentId,
        [FromQuery] string? subjectId,
        [FromQuery] int? academicYear,
        [FromQuery] int? academicSemester,
        [FromQuery] EnrollmentStatus? status,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ListEnrollmentsRequest(studentId, subjectId, academicYear, academicSemester, status),
            cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>Updates an existing enrollment record.</summary>
    [HttpPut("{id}")]
    [Authorize(Policy = AppPermissions.EnrollmentsUpdate)]
    [ProducesResponseType<EnrollmentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResponseBase<EnrollmentResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<EnrollmentResponse>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        [FromRoute] string id,
        [FromBody] UpdateEnrollmentRequest request,
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

    /// <summary>Deletes an enrollment record by identifier.</summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = AppPermissions.EnrollmentsDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteEnrollmentRequest(id), cancellationToken);
        if (result.Success)
        {
            return NoContent();
        }

        return result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
            ? NotFound(result)
            : Conflict(result);
    }
}

