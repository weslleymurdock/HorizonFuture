using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using HF.Shared.Enrollments;
using HF.Shared.Enrollments.Models;
using HF.Shared.Students;
using HF.Shared.Students.Models;
using HF.Shared.Subjects;
using HF.Shared.Subjects.Models;

namespace HF.Shared.PageModels.Enrollments;

/// <summary>
/// PageModel for creating a new enrollment (/enrollments/create).
/// </summary>
public partial class EnrollmentCreatePageModel : PageModelBase
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly IStudentService _studentService;
    private readonly ISubjectService _subjectService;

    [ObservableProperty]
    public partial string StudentId { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SubjectId { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int AcademicYear { get; set; } = DateTime.UtcNow.Year;

    [ObservableProperty]
    public partial int AcademicSemester { get; set; } = 1;

    [ObservableProperty]
    public partial DateTime? EnrollmentDate { get; set; } = DateTime.UtcNow.Date;

    [ObservableProperty]
    public partial EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;

    [ObservableProperty]
    public partial decimal? FinalGrade { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? SuccessMessage { get; set; }

    [ObservableProperty]
    public partial bool IsForbidden { get; set; }

    [ObservableProperty]
    public partial bool IsUnauthorized { get; set; }

    [ObservableProperty]
    public partial bool IsLoadingDependencies { get; set; }

    [ObservableProperty]
    public partial string? DependenciesErrorMessage { get; set; }

    public ObservableCollection<StudentItemDto> AvailableStudents { get; } = new();
    public ObservableCollection<SubjectItemDto> AvailableSubjects { get; } = new();

    public EnrollmentCreatePageModel(
        IEnrollmentService enrollmentService,
        IStudentService studentService,
        ISubjectService subjectService)
    {
        _enrollmentService = enrollmentService ?? throw new ArgumentNullException(nameof(enrollmentService));
        _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
        _subjectService = subjectService ?? throw new ArgumentNullException(nameof(subjectService));
        Title = "Nova Matrícula";
    }

    /// <summary>
    /// Loads active students and subjects for select dropdowns.
    /// </summary>
    public async Task LoadDependenciesAsync(CancellationToken ct = default)
    {
        try
        {
            IsLoadingDependencies = true;
            DependenciesErrorMessage = null;

            AvailableStudents.Clear();
            AvailableSubjects.Clear();

            var studentsTask = _studentService.ListAsync(ct: ct);
            var subjectsTask = _subjectService.ListAsync(ct: ct);

            await Task.WhenAll(studentsTask, subjectsTask);

            var studentsResult = await studentsTask;
            if (studentsResult.Succeeded && studentsResult.Data is not null)
            {
                foreach (var student in studentsResult.Data)
                {
                    AvailableStudents.Add(student);
                }
            }
            else if (studentsResult.StatusCode == 403)
            {
                DependenciesErrorMessage = "Permissão insuficiente para listar estudantes.";
            }

            var subjectsResult = await subjectsTask;
            if (subjectsResult.Succeeded && subjectsResult.Data is not null)
            {
                foreach (var subject in subjectsResult.Data)
                {
                    AvailableSubjects.Add(subject);
                }
            }
            else if (subjectsResult.StatusCode == 403)
            {
                DependenciesErrorMessage = (DependenciesErrorMessage != null ? DependenciesErrorMessage + " " : "") +
                                           "Permissão insuficiente para listar disciplinas.";
            }
        }
        catch (Exception ex)
        {
            DependenciesErrorMessage = $"Falha ao carregar opções: {ex.Message}";
        }
        finally
        {
            IsLoadingDependencies = false;
        }
    }

    /// <summary>
    /// Submits the new enrollment to the API.
    /// </summary>
    public async Task<bool> CreateEnrollmentAsync(CancellationToken ct = default)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (string.IsNullOrWhiteSpace(StudentId))
        {
            ErrorMessage = "Selecione um aluno para a matrícula.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(SubjectId))
        {
            ErrorMessage = "Selecione uma disciplina para a matrícula.";
            return false;
        }

        if (AcademicYear < 2000 || AcademicYear > 2100)
        {
            ErrorMessage = "O ano letivo informado é inválido.";
            return false;
        }

        if (AcademicSemester < 1 || AcademicSemester > 4)
        {
            ErrorMessage = "O semestre letivo deve estar entre 1 e 4.";
            return false;
        }

        if (FinalGrade.HasValue && (FinalGrade.Value < 0 || FinalGrade.Value > 10))
        {
            ErrorMessage = "A nota final deve estar entre 0 e 10.";
            return false;
        }

        try
        {
            IsBusy = true;
            var enrollmentDateOnly = EnrollmentDate.HasValue
                ? DateOnly.FromDateTime(EnrollmentDate.Value)
                : (DateOnly?)null;

            var dto = new CreateEnrollmentDto(
                StudentId: StudentId.Trim(),
                SubjectId: SubjectId.Trim(),
                AcademicYear: AcademicYear,
                AcademicSemester: AcademicSemester,
                EnrollmentDate: enrollmentDateOnly,
                Status: Status,
                FinalGrade: FinalGrade);

            var result = await _enrollmentService.CreateAsync(dto, ct);

            if (result.Succeeded && result.Data is not null)
            {
                SuccessMessage = "Matrícula realizada com sucesso!";
                return true;
            }

            if (result.StatusCode == 401)
            {
                IsUnauthorized = true;
                ErrorMessage = "Sessão expirada. Faça login novamente.";
                return false;
            }

            if (result.StatusCode == 403)
            {
                IsForbidden = true;
                ErrorMessage = "Acesso negado: seu perfil não possui a permissão necessária para criar matrículas (Enrollments.Create).";
                return false;
            }

            ErrorMessage = !string.IsNullOrWhiteSpace(result.Message)
                ? result.Message
                : "Falha ao registrar matrícula.";
            return false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro inesperado: {ex.Message}";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }
}

