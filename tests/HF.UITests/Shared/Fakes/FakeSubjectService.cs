using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HF.Shared.Subjects;
using HF.Shared.Subjects.Models;

namespace HF.UITests.Shared.Fakes;

/// <summary>
/// Fake implementation of ISubjectService for PageModel unit tests.
/// </summary>
public class FakeSubjectService : ISubjectService
{
    public bool ShouldSucceed { get; set; } = true;
    public string FailureMessage { get; set; } = "Simulated failure in subject operation";
    public int? FailureStatusCode { get; set; } = 400;

    public List<SubjectItemDto> MockSubjects { get; set; } = new()
    {
        new(
            Id: "subject-1",
            Name: "Matemática Financeira",
            Code: "MAT-101",
            WorkloadHours: 40,
            Description: "Conceitos de juros, inflação e orçamento pessoal",
            SchoolUnitId: "unit-central",
            CourseId: "course-1",
            Status: SubjectStatus.Active,
            CreatedAt: DateTimeOffset.UtcNow.AddMonths(-2),
            UpdatedAt: null,
            CreatedBy: "admin@horizon.org",
            UpdatedBy: null),
        new(
            Id: "subject-2",
            Name: "Programação em C#",
            Code: "CS-201",
            WorkloadHours: 60,
            Description: "Fundamentos de lógica e orientação a objetos",
            SchoolUnitId: "unit-norte",
            CourseId: "course-2",
            Status: SubjectStatus.Inactive,
            CreatedAt: DateTimeOffset.UtcNow.AddMonths(-1),
            UpdatedAt: DateTimeOffset.UtcNow.AddDays(-3),
            CreatedBy: "admin@horizon.org",
            UpdatedBy: "coord@horizon.org")
    };

    public SubjectFilterDto? LastFilter { get; private set; }
    public string? LastGetById { get; private set; }
    public CreateSubjectDto? LastCreatedRequest { get; private set; }
    public UpdateSubjectDto? LastUpdatedRequest { get; private set; }
    public string? LastDeletedId { get; private set; }

    public Task<SubjectResult<IReadOnlyList<SubjectItemDto>>> ListAsync(SubjectFilterDto? filter = null, CancellationToken ct = default)
    {
        LastFilter = filter;

        if (!ShouldSucceed)
        {
            return Task.FromResult(SubjectResult<IReadOnlyList<SubjectItemDto>>.Fail(FailureMessage, statusCode: FailureStatusCode));
        }

        IEnumerable<SubjectItemDto> query = MockSubjects;

        if (!string.IsNullOrWhiteSpace(filter?.CourseId))
        {
            query = query.Where(s => string.Equals(s.CourseId, filter.CourseId, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(filter?.SchoolUnitId))
        {
            query = query.Where(s => string.Equals(s.SchoolUnitId, filter.SchoolUnitId, StringComparison.OrdinalIgnoreCase));
        }

        if (filter?.Status.HasValue == true)
        {
            query = query.Where(s => s.Status == filter.Status.Value);
        }

        return Task.FromResult(SubjectResult<IReadOnlyList<SubjectItemDto>>.Ok(query.ToList(), "Subjects listed successfully."));
    }

    public Task<SubjectResult<SubjectItemDto>> GetByIdAsync(string id, CancellationToken ct = default)
    {
        LastGetById = id;

        if (!ShouldSucceed)
        {
            return Task.FromResult(SubjectResult<SubjectItemDto>.Fail(FailureMessage, statusCode: FailureStatusCode));
        }

        var subject = MockSubjects.FirstOrDefault(s => s.Id == id);
        if (subject is not null)
        {
            return Task.FromResult(SubjectResult<SubjectItemDto>.Ok(subject, "Subject found."));
        }

        return Task.FromResult(SubjectResult<SubjectItemDto>.Fail("Subject not found.", statusCode: 404));
    }

    public Task<SubjectResult<SubjectItemDto>> CreateAsync(CreateSubjectDto request, CancellationToken ct = default)
    {
        LastCreatedRequest = request;

        if (!ShouldSucceed)
        {
            return Task.FromResult(SubjectResult<SubjectItemDto>.Fail(FailureMessage, statusCode: FailureStatusCode));
        }

        var created = new SubjectItemDto(
            Id: $"subject-{Guid.NewGuid():N}",
            Name: request.Name,
            Code: request.Code,
            WorkloadHours: request.WorkloadHours,
            Description: request.Description,
            SchoolUnitId: request.SchoolUnitId,
            CourseId: request.CourseId,
            Status: request.Status,
            CreatedAt: DateTimeOffset.UtcNow,
            UpdatedAt: null,
            CreatedBy: "test@horizon.org",
            UpdatedBy: null);

        MockSubjects.Add(created);
        return Task.FromResult(SubjectResult<SubjectItemDto>.Ok(created, "Subject created successfully."));
    }

    public Task<SubjectResult<SubjectItemDto>> UpdateAsync(UpdateSubjectDto request, CancellationToken ct = default)
    {
        LastUpdatedRequest = request;

        if (!ShouldSucceed)
        {
            return Task.FromResult(SubjectResult<SubjectItemDto>.Fail(FailureMessage, statusCode: FailureStatusCode));
        }

        var index = MockSubjects.FindIndex(s => s.Id == request.Id);
        if (index >= 0)
        {
            var updated = new SubjectItemDto(
                Id: request.Id,
                Name: request.Name,
                Code: request.Code,
                WorkloadHours: request.WorkloadHours,
                Description: request.Description,
                SchoolUnitId: request.SchoolUnitId,
                CourseId: request.CourseId,
                Status: request.Status,
                CreatedAt: MockSubjects[index].CreatedAt,
                UpdatedAt: DateTimeOffset.UtcNow,
                CreatedBy: MockSubjects[index].CreatedBy,
                UpdatedBy: "test@horizon.org");

            MockSubjects[index] = updated;
            return Task.FromResult(SubjectResult<SubjectItemDto>.Ok(updated, "Subject updated successfully."));
        }

        return Task.FromResult(SubjectResult<SubjectItemDto>.Fail("Subject not found.", statusCode: 404));
    }

    public Task<SubjectResult> DeleteAsync(string id, CancellationToken ct = default)
    {
        LastDeletedId = id;

        if (!ShouldSucceed)
        {
            return Task.FromResult(SubjectResult.Fail(FailureMessage, statusCode: FailureStatusCode));
        }

        var removed = MockSubjects.RemoveAll(s => s.Id == id);
        if (removed > 0)
        {
            return Task.FromResult(SubjectResult.Ok("Subject deleted successfully."));
        }

        return Task.FromResult(SubjectResult.Fail("Subject not found.", statusCode: 404));
    }
}
