namespace HF.Shared.Courses.Models;

/// <summary>
/// Status operacional de um curso.
/// </summary>
public enum CourseStatus
{
    Active = 1,
    Draft = 2,
    Archived = 3,
    Suspended = 4
}

/// <summary>
/// DTO representando um curso retornado pela API para exibição na UI.
/// </summary>
public sealed record CourseItemDto(
    string Id,
    string Name,
    string Code,
    int WorkloadHours,
    string? Description,
    string? SchoolUnitId,
    string? TargetAudience,
    CourseStatus Status,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? CreatedBy,
    string? UpdatedBy);

/// <summary>
/// DTO para criação de um novo curso.
/// </summary>
public sealed record CreateCourseDto(
    string Name,
    string Code,
    int WorkloadHours,
    string? Description = null,
    string? SchoolUnitId = null,
    string? TargetAudience = null,
    CourseStatus Status = CourseStatus.Active);

/// <summary>
/// DTO para atualização de um curso existente.
/// </summary>
public sealed record UpdateCourseDto(
    string Id,
    string Name,
    string Code,
    int WorkloadHours,
    string? Description = null,
    string? SchoolUnitId = null,
    string? TargetAudience = null,
    CourseStatus Status = CourseStatus.Active);

/// <summary>
/// Filtros opcionais para listagem de cursos.
/// </summary>
public sealed record CourseFilterDto(
    string? SchoolUnitId = null,
    CourseStatus? Status = null);

