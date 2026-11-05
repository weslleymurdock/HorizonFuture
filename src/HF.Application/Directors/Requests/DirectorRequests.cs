using HF.Application.Common.Responses;
using HF.Application.Directors.Responses;
using HF.Domain.Directors;
using Mediator;

namespace HF.Application.Directors.Requests;

/// <summary>Command to create a new director.</summary>
public sealed record CreateDirectorRequest(
    string UserId,
    string Directorate,
    string PositionTitle,
    string? SchoolUnitId = null,
    DateOnly? MandateStart = null,
    DateOnly? MandateEnd = null,
    DirectorStatus Status = DirectorStatus.Active)
    : IRequest<ResponseBase<DirectorResponse>>;

/// <summary>Query to get a director by identifier.</summary>
public sealed record GetDirectorByIdRequest(string Id)
    : IRequest<ResponseBase<DirectorResponse>>;

/// <summary>Query to list directors matching optional filters.</summary>
public sealed record ListDirectorsRequest(
    string? SchoolUnitId = null,
    DirectorStatus? Status = null,
    string? Directorate = null)
    : IRequest<ResponseBase<IReadOnlyList<DirectorResponse>>>;

/// <summary>Command to update an existing director.</summary>
public sealed record UpdateDirectorRequest(
    string Id,
    string Directorate,
    string PositionTitle,
    string? SchoolUnitId = null,
    DateOnly? MandateStart = null,
    DateOnly? MandateEnd = null,
    DirectorStatus Status = DirectorStatus.Active)
    : IRequest<ResponseBase<DirectorResponse>>;

/// <summary>Command to delete a director by identifier.</summary>
public sealed record DeleteDirectorRequest(string Id)
    : IRequest<ResponseBase<bool>>;

