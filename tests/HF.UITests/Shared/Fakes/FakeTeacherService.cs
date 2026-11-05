using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HF.Shared.Teachers;
using HF.Shared.Teachers.Models;

namespace HF.UITests.Shared.Fakes;

/// <summary>
/// Deterministic in-memory fake for ITeacherService used in PageModel and UI workflow unit tests.
/// </summary>
public class FakeTeacherService : ITeacherService
{
    public List<TeacherItemDto> Teachers { get; } = new();

    public bool ShouldFail { get; set; }
    public string FailureMessage { get; set; } = "Erro simulado no serviço de professores.";
    public int? FailureStatusCode { get; set; }

    public int ListCalls { get; private set; }
    public int GetByIdCalls { get; private set; }
    public int CreateCalls { get; private set; }
    public int UpdateCalls { get; private set; }
    public int DeleteCalls { get; private set; }

    public TeacherFilterDto? LastFilter { get; private set; }
    public CreateTeacherDto? LastCreateRequest { get; private set; }
    public UpdateTeacherDto? LastUpdateRequest { get; private set; }
    public string? LastDeletedId { get; private set; }

    public FakeTeacherService()
    {
        SeedDefaultTeachers();
    }

    public void SeedDefaultTeachers()
    {
        Teachers.Clear();
        Teachers.Add(new TeacherItemDto(
            Id: "teacher-1",
            UserId: "user-101",
            EmployeeNumber: "TCH-001",
            Specialty: "Matemática",
            AcademicDegree: "Licenciatura em Matemática",
            Bio: "Professor de matemática do ensino fundamental",
            SchoolUnitId: "unit-1",
            HireDate: new DateOnly(2022, 2, 1),
            Status: TeacherStatus.Active,
            CreatedAt: DateTime.UtcNow.AddMonths(-12),
            UpdatedAt: null,
            CreatedBy: "admin@horizon.local",
            UpdatedBy: null));

        Teachers.Add(new TeacherItemDto(
            Id: "teacher-2",
            UserId: "user-102",
            EmployeeNumber: "TCH-002",
            Specialty: "Português e Literatura",
            AcademicDegree: "Mestrado em Letras",
            Bio: "Docente de língua portuguesa e redação",
            SchoolUnitId: "unit-1",
            HireDate: new DateOnly(2023, 1, 15),
            Status: TeacherStatus.Active,
            CreatedAt: DateTime.UtcNow.AddMonths(-6),
            UpdatedAt: null,
            CreatedBy: "admin@horizon.local",
            UpdatedBy: null));

        Teachers.Add(new TeacherItemDto(
            Id: "teacher-3",
            UserId: "user-103",
            EmployeeNumber: "TCH-003",
            Specialty: "Ciências Naturais",
            AcademicDegree: "Bacharelado e Licenciatura em Biologia",
            Bio: "Professor em licença para capacitação",
            SchoolUnitId: "unit-2",
            HireDate: new DateOnly(2021, 8, 10),
            Status: TeacherStatus.OnLeave,
            CreatedAt: DateTime.UtcNow.AddMonths(-18),
            UpdatedAt: null,
            CreatedBy: "admin@horizon.local",
            UpdatedBy: null));
    }

    public Task<TeacherResult<IReadOnlyList<TeacherItemDto>>> ListAsync(TeacherFilterDto? filter = null, CancellationToken ct = default)
    {
        ListCalls++;
        LastFilter = filter;

        if (ShouldFail)
        {
            return Task.FromResult(TeacherResult<IReadOnlyList<TeacherItemDto>>.Fail(FailureMessage, statusCode: FailureStatusCode));
        }

        IEnumerable<TeacherItemDto> query = Teachers;

        if (!string.IsNullOrWhiteSpace(filter?.SchoolUnitId))
        {
            query = query.Where(t => string.Equals(t.SchoolUnitId, filter.SchoolUnitId, StringComparison.OrdinalIgnoreCase));
        }

        if (filter?.Status.HasValue == true)
        {
            query = query.Where(t => t.Status == filter.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter?.Specialty))
        {
            query = query.Where(t => t.Specialty.Contains(filter.Specialty, StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult(TeacherResult<IReadOnlyList<TeacherItemDto>>.Ok(query.ToList()));
    }

    public Task<TeacherResult<TeacherItemDto>> GetByIdAsync(string id, CancellationToken ct = default)
    {
        GetByIdCalls++;

        if (ShouldFail)
        {
            return Task.FromResult(TeacherResult<TeacherItemDto>.Fail(FailureMessage, statusCode: FailureStatusCode));
        }

        var item = Teachers.FirstOrDefault(t => t.Id == id);
        if (item is null)
        {
            return Task.FromResult(TeacherResult<TeacherItemDto>.Fail("Professor não encontrado.", statusCode: 404));
        }

        return Task.FromResult(TeacherResult<TeacherItemDto>.Ok(item));
    }

    public Task<TeacherResult<TeacherItemDto>> CreateAsync(CreateTeacherDto request, CancellationToken ct = default)
    {
        CreateCalls++;
        LastCreateRequest = request;

        if (ShouldFail)
        {
            return Task.FromResult(TeacherResult<TeacherItemDto>.Fail(FailureMessage, statusCode: FailureStatusCode));
        }

        if (Teachers.Any(t => t.EmployeeNumber.Equals(request.EmployeeNumber, StringComparison.OrdinalIgnoreCase)))
        {
            return Task.FromResult(TeacherResult<TeacherItemDto>.Fail("O número de matrícula já está em uso.", statusCode: 409));
        }

        var newTeacher = new TeacherItemDto(
            Id: $"teacher-{Guid.NewGuid():N}",
            UserId: request.UserId,
            EmployeeNumber: request.EmployeeNumber,
            Specialty: request.Specialty,
            AcademicDegree: request.AcademicDegree,
            Bio: request.Bio,
            SchoolUnitId: request.SchoolUnitId,
            HireDate: request.HireDate,
            Status: request.Status,
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: null,
            CreatedBy: "test@horizon.local",
            UpdatedBy: null);

        Teachers.Add(newTeacher);
        return Task.FromResult(TeacherResult<TeacherItemDto>.Ok(newTeacher, "Professor cadastrado com sucesso."));
    }

    public Task<TeacherResult<TeacherItemDto>> UpdateAsync(UpdateTeacherDto request, CancellationToken ct = default)
    {
        UpdateCalls++;
        LastUpdateRequest = request;

        if (ShouldFail)
        {
            return Task.FromResult(TeacherResult<TeacherItemDto>.Fail(FailureMessage, statusCode: FailureStatusCode));
        }

        var index = Teachers.FindIndex(t => t.Id == request.Id);
        if (index < 0)
        {
            return Task.FromResult(TeacherResult<TeacherItemDto>.Fail("Professor não encontrado.", statusCode: 404));
        }

        if (Teachers.Any(t => t.Id != request.Id && t.EmployeeNumber.Equals(request.EmployeeNumber, StringComparison.OrdinalIgnoreCase)))
        {
            return Task.FromResult(TeacherResult<TeacherItemDto>.Fail("O número de matrícula já está em uso por outro docente.", statusCode: 409));
        }

        var existing = Teachers[index];
        var updated = existing with
        {
            EmployeeNumber = request.EmployeeNumber,
            Specialty = request.Specialty,
            AcademicDegree = request.AcademicDegree,
            Bio = request.Bio,
            SchoolUnitId = request.SchoolUnitId,
            Status = request.Status,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = "test@horizon.local"
        };

        Teachers[index] = updated;
        return Task.FromResult(TeacherResult<TeacherItemDto>.Ok(updated, "Professor atualizado com sucesso."));
    }

    public Task<TeacherResult> DeleteAsync(string id, CancellationToken ct = default)
    {
        DeleteCalls++;
        LastDeletedId = id;

        if (ShouldFail)
        {
            return Task.FromResult(TeacherResult.Fail(FailureMessage, statusCode: FailureStatusCode));
        }

        var existing = Teachers.FirstOrDefault(t => t.Id == id);
        if (existing is null)
        {
            return Task.FromResult(TeacherResult.Fail("Professor não encontrado.", statusCode: 404));
        }

        Teachers.Remove(existing);
        return Task.FromResult(TeacherResult.Ok("Professor excluído com sucesso."));
    }
}
