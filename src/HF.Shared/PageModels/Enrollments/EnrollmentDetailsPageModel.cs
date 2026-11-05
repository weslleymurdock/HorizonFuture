using System;
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
/// PageModel for the enrollment details screen (/enrollments/{Id}).
/// </summary>
public partial class EnrollmentDetailsPageModel : PageModelBase
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly IStudentService _studentService;
    private readonly ISubjectService _subjectService;

    [ObservableProperty]
    public partial EnrollmentItemDto? Enrollment { get; set; }

    [ObservableProperty]
    public partial StudentItemDto? Student { get; set; }

    [ObservableProperty]
    public partial SubjectItemDto? Subject { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool IsForbidden { get; set; }

    [ObservableProperty]
    public partial bool IsUnauthorized { get; set; }

    public EnrollmentDetailsPageModel(
        IEnrollmentService enrollmentService,
        IStudentService studentService,
        ISubjectService subjectService)
    {
        _enrollmentService = enrollmentService ?? throw new ArgumentNullException(nameof(enrollmentService));
        _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
        _subjectService = subjectService ?? throw new ArgumentNullException(nameof(subjectService));
        Title = "Detalhes da Matrícula";
    }

    /// <summary>
    /// Loads enrollment by unique identifier, plus associated student and subject if accessible.
    /// </summary>
    public async Task<bool> LoadDetailsAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            ErrorMessage = "Identificador da matrícula é obrigatório.";
            return false;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = null;
            IsForbidden = false;
            IsUnauthorized = false;
            Enrollment = null;
            Student = null;
            Subject = null;

            var result = await _enrollmentService.GetByIdAsync(id, ct);
            if (result.Succeeded && result.Data is not null)
            {
                Enrollment = result.Data;

                // Load related student and subject metadata in parallel
                try
                {
                    var studentTask = _studentService.GetByIdAsync(Enrollment.StudentId, ct);
                    var subjectTask = _subjectService.GetByIdAsync(Enrollment.SubjectId, ct);

                    await Task.WhenAll(studentTask, subjectTask);

                    var sResult = await studentTask;
                    if (sResult.Succeeded)
                    {
                        Student = sResult.Data;
                    }

                    var subjResult = await subjectTask;
                    if (subjResult.Succeeded)
                    {
                        Subject = subjResult.Data;
                    }
                }
                catch
                {
                    // Non-critical if user lacks read permissions for student/subject details
                }

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
                ErrorMessage = "Acesso negado: seu perfil não possui a permissão necessária para visualizar matrículas (Enrollments.Read).";
                return false;
            }

            if (result.StatusCode == 404)
            {
                ErrorMessage = "Matrícula não encontrada.";
                return false;
            }

            ErrorMessage = !string.IsNullOrWhiteSpace(result.Message)
                ? result.Message
                : "Falha ao carregar detalhes da matrícula.";
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

