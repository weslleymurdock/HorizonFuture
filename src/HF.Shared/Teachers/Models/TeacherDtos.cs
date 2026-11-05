using System;

namespace HF.Shared.Teachers.Models;

/// <summary>
/// Status operacional de um professor.
/// </summary>
public enum TeacherStatus
{
    Active = 1,
    OnLeave = 2,
    Inactive = 3
}

/// <summary>
/// DTO representando um registro de professor retornado pela API para exibição na UI.
/// </summary>
public sealed record TeacherItemDto(
    string Id,
    string UserId,
    string EmployeeNumber,
    string Specialty,
    string? AcademicDegree,
    string? Bio,
    string? SchoolUnitId,
    DateOnly? HireDate,
    TeacherStatus Status,
    DateTime? CreatedAt,
    DateTime? UpdatedAt,
    string? CreatedBy,
    string? UpdatedBy);

/// <summary>
/// DTO para criação de um novo registro de professor.
/// </summary>
public sealed record CreateTeacherDto(
    string UserId,
    string EmployeeNumber,
    string Specialty,
    string? AcademicDegree = null,
    string? Bio = null,
    string? SchoolUnitId = null,
    DateOnly? HireDate = null,
    TeacherStatus Status = TeacherStatus.Active);

/// <summary>
/// DTO para atualização de um registro existente de professor.
/// </summary>
public sealed record UpdateTeacherDto(
    string Id,
    string EmployeeNumber,
    string Specialty,
    string? AcademicDegree = null,
    string? Bio = null,
    string? SchoolUnitId = null,
    TeacherStatus Status = TeacherStatus.Active);

/// <summary>
/// DTO de filtros para listagem de professores.
/// </summary>
public sealed record TeacherFilterDto(
    string? SchoolUnitId = null,
    TeacherStatus? Status = null,
    string? Specialty = null);

