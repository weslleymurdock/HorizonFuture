using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HF.Shared.Students;
using HF.Shared.Students.Models;

namespace HF.UITests.Shared.Fakes;

/// <summary>
/// Fake implementation of IStudentService for PageModel unit tests.
/// Uses synthetic data only with no real PII or medical data.
/// </summary>
public class FakeStudentService : IStudentService
{
    public bool ShouldSucceed { get; set; } = true;
    public string FailureMessage { get; set; } = "Simulated failure in student operation";
    public int? FailureStatusCode { get; set; } = 400;

    public List<StudentItemDto> MockStudents { get; set; } = new()
    {
        new(
            Id: "student-1",
            UserId: "user-synth-1",
            RegistrationNumber: "MAT-2026-001",
            DateOfBirth: new DateOnly(2010, 5, 14),
            GuardianName: "Maria da Silva",
            GuardianRelationship: "Mãe",
            GuardianPhone: "(11) 98765-4321",
            EmergencyContactName: "Carlos da Silva",
            EmergencyContactPhone: "(11) 91234-5678",
            SchoolUnitId: "unit-central",
            Status: StudentStatus.Active,
            SpecialNeeds: "Nenhuma",
            MedicalNotes: "Alergia leve a pólen",
            EnrollmentDate: new DateOnly(2026, 2, 1),
            CreatedAt: DateTimeOffset.UtcNow.AddMonths(-2),
            UpdatedAt: null,
            CreatedBy: "admin@horizon.org",
            UpdatedBy: null),
        new(
            Id: "student-2",
            UserId: "user-synth-2",
            RegistrationNumber: "MAT-2026-002",
            DateOfBirth: new DateOnly(2011, 8, 20),
            GuardianName: "João Santos",
            GuardianRelationship: "Pai",
            GuardianPhone: "(11) 99887-6655",
            EmergencyContactName: null,
            EmergencyContactPhone: null,
            SchoolUnitId: "unit-norte",
            Status: StudentStatus.Inactive,
            SpecialNeeds: "Baixa visão - necessita fonte ampliada",
            MedicalNotes: null,
            EnrollmentDate: new DateOnly(2026, 2, 5),
            CreatedAt: DateTimeOffset.UtcNow.AddMonths(-1),
            UpdatedAt: DateTimeOffset.UtcNow.AddDays(-3),
            CreatedBy: "admin@horizon.org",
            UpdatedBy: "coord@horizon.org")
    };

    public StudentFilterDto? LastFilter { get; private set; }
    public string? LastGetById { get; private set; }
    public CreateStudentDto? LastCreatedRequest { get; private set; }
    public UpdateStudentDto? LastUpdatedRequest { get; private set; }
    public string? LastDeletedId { get; private set; }

    public Task<StudentResult<IReadOnlyList<StudentItemDto>>> ListAsync(StudentFilterDto? filter = null, CancellationToken ct = default)
    {
        LastFilter = filter;

        if (!ShouldSucceed)
        {
            return Task.FromResult(StudentResult<IReadOnlyList<StudentItemDto>>.Fail(FailureMessage, statusCode: FailureStatusCode));
        }

        IEnumerable<StudentItemDto> query = MockStudents;

        if (!string.IsNullOrWhiteSpace(filter?.SchoolUnitId))
        {
            query = query.Where(s => string.Equals(s.SchoolUnitId, filter.SchoolUnitId, StringComparison.OrdinalIgnoreCase));
        }

        if (filter?.Status.HasValue == true)
        {
            query = query.Where(s => s.Status == filter.Status.Value);
        }

        return Task.FromResult(StudentResult<IReadOnlyList<StudentItemDto>>.Ok(query.ToList()));
    }

    public Task<StudentResult<StudentItemDto>> GetByIdAsync(string id, CancellationToken ct = default)
    {
        LastGetById = id;

        if (!ShouldSucceed)
        {
            return Task.FromResult(StudentResult<StudentItemDto>.Fail(FailureMessage, statusCode: FailureStatusCode));
        }

        var student = MockStudents.FirstOrDefault(s => s.Id == id);
        if (student is not null)
        {
            return Task.FromResult(StudentResult<StudentItemDto>.Ok(student));
        }

        return Task.FromResult(StudentResult<StudentItemDto>.Fail("Student not found", statusCode: 404));
    }

    public Task<StudentResult<StudentItemDto>> CreateAsync(CreateStudentDto request, CancellationToken ct = default)
    {
        LastCreatedRequest = request;

        if (!ShouldSucceed)
        {
            return Task.FromResult(StudentResult<StudentItemDto>.Fail(FailureMessage, statusCode: FailureStatusCode));
        }

        var newStudent = new StudentItemDto(
            Id: $"student-{Guid.NewGuid():N}"[..12],
            UserId: request.UserId,
            RegistrationNumber: request.RegistrationNumber,
            DateOfBirth: request.DateOfBirth,
            GuardianName: request.GuardianName,
            GuardianRelationship: request.GuardianRelationship,
            GuardianPhone: request.GuardianPhone,
            EmergencyContactName: request.EmergencyContactName,
            EmergencyContactPhone: request.EmergencyContactPhone,
            SchoolUnitId: request.SchoolUnitId,
            Status: request.Status,
            SpecialNeeds: request.SpecialNeeds,
            MedicalNotes: request.MedicalNotes,
            EnrollmentDate: request.EnrollmentDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            CreatedAt: DateTimeOffset.UtcNow,
            UpdatedAt: null,
            CreatedBy: "test@horizon.org",
            UpdatedBy: null);

        MockStudents.Add(newStudent);
        return Task.FromResult(StudentResult<StudentItemDto>.Ok(newStudent));
    }

    public Task<StudentResult<StudentItemDto>> UpdateAsync(UpdateStudentDto request, CancellationToken ct = default)
    {
        LastUpdatedRequest = request;

        if (!ShouldSucceed)
        {
            return Task.FromResult(StudentResult<StudentItemDto>.Fail(FailureMessage, statusCode: FailureStatusCode));
        }

        var index = MockStudents.FindIndex(s => s.Id == request.Id);
        if (index < 0)
        {
            return Task.FromResult(StudentResult<StudentItemDto>.Fail("Student not found for update", statusCode: 404));
        }

        var existing = MockStudents[index];
        var updated = new StudentItemDto(
            Id: existing.Id,
            UserId: existing.UserId,
            RegistrationNumber: request.RegistrationNumber,
            DateOfBirth: request.DateOfBirth,
            GuardianName: request.GuardianName,
            GuardianRelationship: request.GuardianRelationship,
            GuardianPhone: request.GuardianPhone,
            EmergencyContactName: request.EmergencyContactName,
            EmergencyContactPhone: request.EmergencyContactPhone,
            SchoolUnitId: request.SchoolUnitId,
            Status: request.Status,
            SpecialNeeds: request.SpecialNeeds,
            MedicalNotes: request.MedicalNotes,
            EnrollmentDate: existing.EnrollmentDate,
            CreatedAt: existing.CreatedAt,
            UpdatedAt: DateTimeOffset.UtcNow,
            CreatedBy: existing.CreatedBy,
            UpdatedBy: "test@horizon.org");

        MockStudents[index] = updated;
        return Task.FromResult(StudentResult<StudentItemDto>.Ok(updated));
    }

    public Task<StudentResult> DeleteAsync(string id, CancellationToken ct = default)
    {
        LastDeletedId = id;

        if (!ShouldSucceed)
        {
            return Task.FromResult(StudentResult.Fail(FailureMessage, statusCode: FailureStatusCode));
        }

        var removed = MockStudents.RemoveAll(s => s.Id == id);
        if (removed > 0)
        {
            return Task.FromResult(StudentResult.Ok("Student deleted successfully"));
        }

        return Task.FromResult(StudentResult.Fail("Student not found for deletion", statusCode: 404));
    }
}

