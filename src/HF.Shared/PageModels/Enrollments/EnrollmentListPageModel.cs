using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using HF.Shared.Enrollments;
using HF.Shared.Enrollments.Models;

namespace HF.Shared.PageModels.Enrollments;

/// <summary>
/// PageModel for the enrollments list screen (/enrollments).
/// </summary>
public partial class EnrollmentListPageModel : PageModelBase
{
    private readonly IEnrollmentService _enrollmentService;

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? SuccessMessage { get; set; }

    [ObservableProperty]
    public partial bool IsForbidden { get; set; }

    [ObservableProperty]
    public partial bool IsUnauthorized { get; set; }

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? StudentIdFilter { get; set; }

    [ObservableProperty]
    public partial string? SubjectIdFilter { get; set; }

    [ObservableProperty]
    public partial int? AcademicYearFilter { get; set; }

    [ObservableProperty]
    public partial int? AcademicSemesterFilter { get; set; }

    [ObservableProperty]
    public partial EnrollmentStatus? StatusFilter { get; set; }

    [ObservableProperty]
    public partial bool IsDeleting { get; set; }

    [ObservableProperty]
    public partial string? DeletingEnrollmentId { get; set; }

    public ObservableCollection<EnrollmentItemDto> Enrollments { get; } = new();

    public EnrollmentListPageModel(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService ?? throw new ArgumentNullException(nameof(enrollmentService));
        Title = "Matrículas";
    }

    /// <summary>
    /// Returns enrollments filtered by client-side search text (student ID, subject ID, or enrollment ID).
    /// </summary>
    public IEnumerable<EnrollmentItemDto> FilteredEnrollments
    {
        get
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                return Enrollments;
            }

            var query = SearchText.Trim();
            return Enrollments.Where(e =>
                e.Id.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                e.StudentId.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                e.SubjectId.Contains(query, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Loads enrollments from the REST API applying server-side filters.
    /// </summary>
    public async Task<bool> LoadEnrollmentsAsync(CancellationToken ct = default)
    {
        try
        {
            IsBusy = true;
            ErrorMessage = null;
            IsForbidden = false;
            IsUnauthorized = false;

            var filter = new EnrollmentFilterDto(
                StudentId: string.IsNullOrWhiteSpace(StudentIdFilter) ? null : StudentIdFilter.Trim(),
                SubjectId: string.IsNullOrWhiteSpace(SubjectIdFilter) ? null : SubjectIdFilter.Trim(),
                AcademicYear: AcademicYearFilter,
                AcademicSemester: AcademicSemesterFilter,
                Status: StatusFilter);

            var result = await _enrollmentService.ListAsync(filter, ct);

            if (result.Succeeded && result.Data is not null)
            {
                Enrollments.Clear();
                foreach (var item in result.Data)
                {
                    Enrollments.Add(item);
                }
                return true;
            }

            if (result.StatusCode == 401)
            {
                IsUnauthorized = true;
                ErrorMessage = "Sessão expirada ou não autenticada.";
                return false;
            }

            if (result.StatusCode == 403)
            {
                IsForbidden = true;
                ErrorMessage = "Acesso negado: seu perfil não possui a permissão necessária para visualizar a lista de matrículas (Enrollments.Read).";
                return false;
            }

            ErrorMessage = !string.IsNullOrWhiteSpace(result.Message)
                ? result.Message
                : "Falha ao carregar lista de matrículas.";
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

    /// <summary>
    /// Deletes an enrollment record by unique identifier after confirmation.
    /// </summary>
    public async Task<bool> DeleteEnrollmentAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            ErrorMessage = "Identificador da matrícula é obrigatório.";
            return false;
        }

        try
        {
            IsDeleting = true;
            DeletingEnrollmentId = id;
            ErrorMessage = null;
            SuccessMessage = null;

            var result = await _enrollmentService.DeleteAsync(id, ct);
            if (result.Succeeded)
            {
                var existing = Enrollments.FirstOrDefault(e => e.Id == id);
                if (existing is not null)
                {
                    Enrollments.Remove(existing);
                }
                SuccessMessage = "Matrícula excluída com sucesso!";
                return true;
            }

            if (result.StatusCode == 403)
            {
                ErrorMessage = "Acesso negado: seu perfil não possui a permissão necessária para excluir matrículas (Enrollments.Delete).";
                return false;
            }

            if (result.StatusCode == 409)
            {
                ErrorMessage = "Conflito: a matrícula possui vínculos ativos e não pode ser excluída.";
                return false;
            }

            ErrorMessage = !string.IsNullOrWhiteSpace(result.Message)
                ? result.Message
                : "Falha ao excluir matrícula.";
            return false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro inesperado: {ex.Message}";
            return false;
        }
        finally
        {
            IsDeleting = false;
            DeletingEnrollmentId = null;
        }
    }
}

