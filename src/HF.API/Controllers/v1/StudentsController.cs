using HF.Application.Auth;
using HF.Application.Common.Responses;
using HF.Application.Students.Requests;
using HF.Application.Students.Responses;
using HF.Domain.Students;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HF.API.v1.Controllers;

/// <summary>
/// Provides REST API endpoints for student management.
/// </summary>
[Route("api/v1/students")]
[ApiController]
[Tags("Students")]
public sealed class StudentsController(IMediator mediator) : ControllerBase
{
    /// <summary>Creates a new student record.</summary>
    [HttpPost]
    [Authorize(Policy = AppPermissions.StudentsCreate)]
    [ProducesResponseType<StudentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ResponseBase<StudentResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateStudentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(result);
    }

    /// <summary>Gets a student record by unique identifier.</summary>
    [HttpGet("{id}")]
    [Authorize(Policy = AppPermissions.StudentsRead)]
    [ProducesResponseType<StudentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<StudentResponse>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetStudentByIdRequest(id), cancellationToken);
        return result.Success
            ? Ok(result.Value)
            : NotFound(result);
    }

    /// <summary>Lists student records matching optional filters.</summary>
    [HttpGet]
    [Authorize(Policy = AppPermissions.StudentsRead)]
    [ProducesResponseType<IReadOnlyList<StudentResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> List(
        [FromQuery] string? schoolUnitId,
        [FromQuery] StudentStatus? status,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListStudentsRequest(schoolUnitId, status), cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>Updates an existing student record.</summary>
    [HttpPut("{id}")]
    [Authorize(Policy = AppPermissions.StudentsUpdate)]
    [ProducesResponseType<StudentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResponseBase<StudentResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<StudentResponse>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        [FromRoute] string id,
        [FromBody] UpdateStudentRequest request,
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

    /// <summary>Deletes a student record by identifier.</summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = AppPermissions.StudentsDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteStudentRequest(id), cancellationToken);
        if (result.Success)
        {
            return NoContent();
        }

        return result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
            ? NotFound(result)
            : Conflict(result);
    }
}

