using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HF.Shared.Courses;
using HF.Shared.Courses.Models;

namespace HF.UITests.Shared.Fakes;

/// <summary>
/// Implementação fake de ICourseService para testes de unidade dos PageModels de Cursos.
/// </summary>
public class FakeCourseService : ICourseService
{
    public bool ShouldSucceed { get; set; } = true;
    public string FailureMessage { get; set; } = "Falha simulada na operação de cursos";
    public int? FailureStatusCode { get; set; } = 400;

    public List<CourseItemDto> MockCourses { get; set; } = new()
    {
        new(
            Id: "course-1",
            Name: "Alfabetização Digital",
            Code: "ALF-101",
            WorkloadHours: 60,
            Description: "Introdução ao uso de computadores e internet",
            SchoolUnitId: "unit-central",
            TargetAudience: "Jovens e Adultos",
            Status: CourseStatus.Active,
            CreatedAt: DateTimeOffset.UtcNow.AddMonths(-2),
            UpdatedAt: null,
            CreatedBy: "admin@horizon.org",
            UpdatedBy: null),
        new(
            Id: "course-2",
            Name: "Robótica Educacional",
            Code: "ROB-201",
            WorkloadHours: 80,
            Description: "Lógica de programação e montagem de circuitos",
            SchoolUnitId: "unit-norte",
            TargetAudience: "Crianças de 10 a 14 anos",
            Status: CourseStatus.Draft,
            CreatedAt: DateTimeOffset.UtcNow.AddMonths(-1),
            UpdatedAt: DateTimeOffset.UtcNow.AddDays(-5),
            CreatedBy: "admin@horizon.org",
            UpdatedBy: "coord@horizon.org")
    };

    public CourseFilterDto? LastFilter { get; private set; }
    public string? LastGetById { get; private set; }
    public CreateCourseDto? LastCreatedRequest { get; private set; }
    public UpdateCourseDto? LastUpdatedRequest { get; private set; }
    public string? LastDeletedId { get; private set; }

    public Task<CourseResult<IReadOnlyList<CourseItemDto>>> ListAsync(CourseFilterDto? filter = null, CancellationToken ct = default)
    {
        LastFilter = filter;

        if (!ShouldSucceed)
        {
            return Task.FromResult(CourseResult<IReadOnlyList<CourseItemDto>>.Fail(FailureMessage, statusCode: FailureStatusCode));
        }

        IEnumerable<CourseItemDto> query = MockCourses;

        if (!string.IsNullOrWhiteSpace(filter?.SchoolUnitId))
        {
            query = query.Where(c => string.Equals(c.SchoolUnitId, filter.SchoolUnitId, StringComparison.OrdinalIgnoreCase));
        }

        if (filter?.Status.HasValue == true)
        {
            query = query.Where(c => c.Status == filter.Status.Value);
        }

        return Task.FromResult(CourseResult<IReadOnlyList<CourseItemDto>>.Ok(query.ToList(), "Cursos listados com sucesso."));
    }

    public Task<CourseResult<CourseItemDto>> GetByIdAsync(string id, CancellationToken ct = default)
    {
        LastGetById = id;

        if (!ShouldSucceed)
        {
            return Task.FromResult(CourseResult<CourseItemDto>.Fail(FailureMessage, statusCode: FailureStatusCode));
        }

        var course = MockCourses.FirstOrDefault(c => c.Id == id);
        if (course is not null)
        {
            return Task.FromResult(CourseResult<CourseItemDto>.Ok(course, "Curso encontrado."));
        }

        return Task.FromResult(CourseResult<CourseItemDto>.Fail("Curso não encontrado.", statusCode: 404));
    }

    public Task<CourseResult<CourseItemDto>> CreateAsync(CreateCourseDto request, CancellationToken ct = default)
    {
        LastCreatedRequest = request;

        if (!ShouldSucceed)
        {
            return Task.FromResult(CourseResult<CourseItemDto>.Fail(FailureMessage, statusCode: FailureStatusCode));
        }

        var created = new CourseItemDto(
            Id: $"course-{Guid.NewGuid():N}",
            Name: request.Name,
            Code: request.Code,
            WorkloadHours: request.WorkloadHours,
            Description: request.Description,
            SchoolUnitId: request.SchoolUnitId,
            TargetAudience: request.TargetAudience,
            Status: request.Status,
            CreatedAt: DateTimeOffset.UtcNow,
            UpdatedAt: null,
            CreatedBy: "test@horizon.org",
            UpdatedBy: null);

        MockCourses.Add(created);
        return Task.FromResult(CourseResult<CourseItemDto>.Ok(created, "Curso criado com sucesso."));
    }

    public Task<CourseResult<CourseItemDto>> UpdateAsync(UpdateCourseDto request, CancellationToken ct = default)
    {
        LastUpdatedRequest = request;

        if (!ShouldSucceed)
        {
            return Task.FromResult(CourseResult<CourseItemDto>.Fail(FailureMessage, statusCode: FailureStatusCode));
        }

        var index = MockCourses.FindIndex(c => c.Id == request.Id);
        if (index >= 0)
        {
            var updated = new CourseItemDto(
                Id: request.Id,
                Name: request.Name,
                Code: request.Code,
                WorkloadHours: request.WorkloadHours,
                Description: request.Description,
                SchoolUnitId: request.SchoolUnitId,
                TargetAudience: request.TargetAudience,
                Status: request.Status,
                CreatedAt: MockCourses[index].CreatedAt,
                UpdatedAt: DateTimeOffset.UtcNow,
                CreatedBy: MockCourses[index].CreatedBy,
                UpdatedBy: "test@horizon.org");

            MockCourses[index] = updated;
            return Task.FromResult(CourseResult<CourseItemDto>.Ok(updated, "Curso atualizado com sucesso."));
        }

        return Task.FromResult(CourseResult<CourseItemDto>.Fail("Curso não encontrado.", statusCode: 404));
    }

    public Task<CourseResult> DeleteAsync(string id, CancellationToken ct = default)
    {
        LastDeletedId = id;

        if (!ShouldSucceed)
        {
            return Task.FromResult(CourseResult.Fail(FailureMessage, statusCode: FailureStatusCode));
        }

        var removed = MockCourses.RemoveAll(c => c.Id == id);
        if (removed > 0)
        {
            return Task.FromResult(CourseResult.Ok("Curso excluído com sucesso."));
        }

        return Task.FromResult(CourseResult.Fail("Curso não encontrado.", statusCode: 404));
    }
}

