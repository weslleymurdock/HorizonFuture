using HF.Application.Common.Abstractions.Repositories;
using HF.Application.Common.Responses;
using HF.Application.Managers.Responses;
using HF.Domain.Managers;
using Mediator;

namespace HF.Application.Managers.Requests;

/// <summary>Command to create a new manager.</summary>
public sealed record CreateManagerRequest(
    string UserId,
    string EmployeeNumber,
    string Department,
    string RoleTitle,
    string? SchoolUnitId = null,
    DateOnly? StartDate = null,
    ManagerStatus Status = ManagerStatus.Active,
    string? PersonId = null)
    : IRequest<ResponseBase<ManagerResponse>>, ITransactionalRequest;

/// <summary>Query to get a manager by identifier.</summary>
public sealed record GetManagerByIdRequest(string Id)
    : IRequest<ResponseBase<ManagerResponse>>;

/// <summary>Query to list managers matching optional filters.</summary>
public sealed record ListManagersRequest(
    string? SchoolUnitId = null,
    ManagerStatus? Status = null,
    string? Department = null)
    : IRequest<ResponseBase<IReadOnlyList<ManagerResponse>>>;

/// <summary>Command to update an existing manager.</summary>
public sealed record UpdateManagerRequest(
    string Id,
    string EmployeeNumber,
    string Department,
    string RoleTitle,
    string? SchoolUnitId = null,
    ManagerStatus Status = ManagerStatus.Active,
    string? PersonId = null)
    : IRequest<ResponseBase<ManagerResponse>>, ITransactionalRequest;

/// <summary>Command to delete a manager by identifier.</summary>
public sealed record DeleteManagerRequest(string Id)
    : IRequest<ResponseBase<bool>>, ITransactionalRequest;
