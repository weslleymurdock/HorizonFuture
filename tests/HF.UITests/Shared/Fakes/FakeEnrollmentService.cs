using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HF.Shared.Enrollments;
using HF.Shared.Enrollments.Models;

namespace HF.UITests.Shared.Fakes;

/// <summary>
/// Fake implementation of IEnrollmentService for Enrollment PageModel unit tests.
/// </summary>
public class FakeEnrollmentService : IEnrollmentService
{
    public bool ShouldSucceed { get; set; } = true;
    public string FailureMessage { get; set; } = "Falha simulada na operação de matrículas";
    public int? FailureStatusCode { get; set; } = 400;

    public List<EnrollmentItemDto> MockEnrollments { get; set; } = new()
    {
        new(
            Id: "enr-1",
            StudentId: "student-1",
            SubjectId: "subject-1",
            AcademicYear: 2026,
            AcademicSemester: 1,
            EnrollmentDate: new DateOnly(2026, 2, 10),
            Status: EnrollmentStatus.Active,
            FinalGrade: 8.5m,
            CreatedAt: DateTimeOffset.UtcNow.AddMonths(-1),
            UpdatedAt: null,
            CreatedBy: "admin@horizon.org",
            UpdatedBy: null),
        new(
            Id: "enr-2",
            StudentId: "student-2",
            SubjectId: "subject-2",
            AcademicYear: 2026,
            AcademicSemester: 2,
            EnrollmentDate: new DateOnly(2026, 8, 1),
            Status: EnrollmentStatus.Locked,
            FinalGrade: null,
            CreatedAt: DateTimeOffset.UtcNow.AddMonths(-2),
            UpdatedAt: null,
            CreatedBy: "admin@horizon.org",
            UpdatedBy: null)
    };

    public EnrollmentFilterDto? LastFilter { get; private set; }
    public string? LastGetById { get; private set; }
    public CreateEnrollmentDto? LastCreatedRequest { get; private set; }
    public UpdateEnrollmentDto? LastUpdatedRequest { get; private set; }
    public string? LastDeletedId { get; private set; }

    public Task<EnrollmentResult<IReadOnlyList<EnrollmentItemDto>>> ListAsync(EnrollmentFilterDto? filter = null, CancellationToken ct = default)
    {
        LastFilter = filter;

        if (!ShouldSucceed)
        {
            return Task.FromResult(EnrollmentResult<IReadOnlyList<EnrollmentItemDto>>.Fail(FailureMessage, statusCode: FailureStatusCode));
        }

        IEnumerable<EnrollmentItemDto> query = MockEnrollments;

        if (filter != null)
        {
            if (!string.IsNullOrWhiteSpace(filter.StudentId))
            {
                query = query.Where(e => e.StudentId == filter.StudentId);
            }
            if (!string.IsNullOrWhiteSpace(filter.SubjectId))
            {
                query = query.Where(e => e.SubjectId == filter.SubjectId);
            }
            if (filter.AcademicYear.HasValue)
            {
                query = query.Where(e => e.AcademicYear == filter.AcademicYear.Value);
            }
            if (filter.AcademicSemester.HasValue)
            {
                query = query.Where(e => e.AcademicSemester == filter.AcademicSemester.Value);
            }
            if (filter.Status.HasValue)
            {
                query = query.Where(e => e.Status == filter.Status.Value);
            }
        }

        IReadOnlyList<EnrollmentItemDto> result = query.ToList();
        return Task.FromResult(EnrollmentResult<IReadOnlyList<EnrollmentItemDto>>.Ok(result));
    }

    public Task<EnrollmentResult<EnrollmentItemDto>> GetByIdAsync(string id, CancellationToken ct = default)
    {
        LastGetById = id;

        if (!ShouldSucceed)
        {
            return Task.FromResult(EnrollmentResult<EnrollmentItemDto>.Fail(FailureMessage, statusCode: FailureStatusCode));
        }

        var item = MockEnrollments.FirstOrDefault(e => e.Id == id);
        if (item == null)
        {
            return Task.FromResult(EnrollmentResult<EnrollmentItemDto>.Fail("Matrícula não encontrada", statusCode: 404));
        }

        return Task.FromResult(EnrollmentResult<EnrollmentItemDto>.Ok(item));
    }

    public Task<EnrollmentResult<EnrollmentItemDto>> CreateAsync(CreateEnrollmentDto dto, CancellationToken ct = default)
    {
        LastCreatedRequest = dto;

        if (!ShouldSucceed)
        {
            return Task.FromResult(EnrollmentResult<EnrollmentItemDto>.Fail(FailureMessage, statusCode: FailureStatusCode));
        }

        var created = new EnrollmentItemDto(
            Id: $"enr-{Guid.NewGuid():N}"[..8],
            StudentId: dto.StudentId,
            SubjectId: dto.SubjectId,
            AcademicYear: dto.AcademicYear,
            AcademicSemester: dto.AcademicSemester,
            EnrollmentDate: dto.EnrollmentDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            Status: dto.Status,
            FinalGrade: dto.FinalGrade,
            CreatedAt: DateTimeOffset.UtcNow,
            UpdatedAt: null,
            CreatedBy: "tester@horizon.org",
            UpdatedBy: null);

        MockEnrollments.Add(created);
        return Task.FromResult(EnrollmentResult<EnrollmentItemDto>.Ok(created, "Matrícula criada com sucesso"));
    }

    public Task<EnrollmentResult<EnrollmentItemDto>> UpdateAsync(UpdateEnrollmentDto dto, CancellationToken ct = default)
    {
        LastUpdatedRequest = dto;

        if (!ShouldSucceed)
        {
            return Task.FromResult(EnrollmentResult<EnrollmentItemDto>.Fail(FailureMessage, statusCode: FailureStatusCode));
        }

        var existingIndex = MockEnrollments.FindIndex(e => e.Id == dto.Id);
        if (existingIndex < 0)
        {
            return Task.FromResult(EnrollmentResult<EnrollmentItemDto>.Fail("Matrícula não encontrada", statusCode: 404));
        }

        var updated = new EnrollmentItemDto(
            Id: dto.Id,
            StudentId: dto.StudentId,
            SubjectId: dto.SubjectId,
            AcademicYear: dto.AcademicYear,
            AcademicSemester: dto.AcademicSemester,
            EnrollmentDate: dto.EnrollmentDate,
            Status: dto.Status,
            FinalGrade: dto.FinalGrade,
            CreatedAt: MockEnrollments[existingIndex].CreatedAt,
            UpdatedAt: DateTimeOffset.UtcNow,
            CreatedBy: MockEnrollments[existingIndex].CreatedBy,
            UpdatedBy: "updater@horizon.org");

        MockEnrollments[existingIndex] = updated;
        return Task.FromResult(EnrollmentResult<EnrollmentItemDto>.Ok(updated, "Matrícula atualizada com sucesso"));
    }

    public Task<EnrollmentResult> DeleteAsync(string id, CancellationToken ct = default)
    {
        LastDeletedId = id;

        if (!ShouldSucceed)
        {
            return Task.FromResult(EnrollmentResult.Fail(FailureMessage, statusCode: FailureStatusCode));
        }

        var item = MockEnrollments.FirstOrDefault(e => e.Id == id);
        if (item == null)
        {
            return Task.FromResult(EnrollmentResult.Fail("Matrícula não encontrada", statusCode: 404));
        }

        MockEnrollments.Remove(item);
        return Task.FromResult(EnrollmentResult.Ok("Matrícula excluída com sucesso"));
    }
}
