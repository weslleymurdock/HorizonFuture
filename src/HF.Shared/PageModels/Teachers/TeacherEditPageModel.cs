using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using HF.Shared.Teachers;
using HF.Shared.Teachers.Models;

namespace HF.Shared.PageModels.Teachers;

/// <summary>
/// PageModel para a tela de edição de professor (/teachers/{id}/edit).
/// </summary>
public partial class TeacherEditPageModel : PageModelBase
{
    private readonly ITeacherService _teacherService;

    [ObservableProperty]
    public partial string Id { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EmployeeNumber { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Specialty { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? AcademicDegree { get; set; }

    [ObservableProperty]
    public partial string? Bio { get; set; }

    [ObservableProperty]
    public partial string? SchoolUnitId { get; set; }

    [ObservableProperty]
    public partial TeacherStatus Status { get; set; } = TeacherStatus.Active;

    [ObservableProperty]
    public partial TeacherItemDto? Teacher { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? SuccessMessage { get; set; }

    [ObservableProperty]
    public partial bool IsForbidden { get; set; }

    [ObservableProperty]
    public partial bool IsUnauthorized { get; set; }

    [ObservableProperty]
    public partial bool IsNotFound { get; set; }

    public TeacherEditPageModel(ITeacherService teacherService)
    {
        _teacherService = teacherService ?? throw new ArgumentNullException(nameof(teacherService));
        Title = "Editar Professor";
    }

    /// <summary>
    /// Carrega os dados do professor existente para edição.
    /// </summary>
    public async Task<bool> LoadTeacherAsync(string id, CancellationToken ct = default)
    {
        ErrorMessage = null;
        SuccessMessage = null;
        IsForbidden = false;
        IsUnauthorized = false;
        IsNotFound = false;

        if (string.IsNullOrWhiteSpace(id))
        {
            ErrorMessage = "Identificador do professor é obrigatório.";
            IsNotFound = true;
            return false;
        }

        try
        {
            IsBusy = true;
            var result = await _teacherService.GetByIdAsync(id, ct);

            if (result.Succeeded && result.Data is not null)
            {
                Teacher = result.Data;
                Id = Teacher.Id;
                EmployeeNumber = Teacher.EmployeeNumber;
                Specialty = Teacher.Specialty;
                AcademicDegree = Teacher.AcademicDegree;
                Bio = Teacher.Bio;
                SchoolUnitId = Teacher.SchoolUnitId;
                Status = Teacher.Status;
                Title = $"Editar Professor - {Teacher.EmployeeNumber}";
                return true;
            }

            ErrorMessage = result.Message;
            if (result.StatusCode == 404)
            {
                IsNotFound = true;
            }
            else if (result.StatusCode == 403)
            {
                IsForbidden = true;
            }
            else if (result.StatusCode == 401)
            {
                IsUnauthorized = true;
            }

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
    /// Valida e salva as alterações do professor na API.
    /// </summary>
    public async Task<bool> SaveTeacherAsync(CancellationToken ct = default)
    {
        ErrorMessage = null;
        SuccessMessage = null;
        IsForbidden = false;
        IsUnauthorized = false;

        if (string.IsNullOrWhiteSpace(Id))
        {
            ErrorMessage = "Identificador do professor não encontrado.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(EmployeeNumber))
        {
            ErrorMessage = "O número de matrícula é obrigatório.";
            return false;
        }

        if (EmployeeNumber.Trim().Length > 50)
        {
            ErrorMessage = "O número de matrícula não pode exceder 50 caracteres.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Specialty))
        {
            ErrorMessage = "A especialidade pedagógica é obrigatória.";
            return false;
        }

        if (Specialty.Trim().Length > 100)
        {
            ErrorMessage = "A especialidade pedagógica não pode exceder 100 caracteres.";
            return false;
        }

        try
        {
            IsBusy = true;
            var request = new UpdateTeacherDto(
                Id: Id,
                EmployeeNumber: EmployeeNumber.Trim(),
                Specialty: Specialty.Trim(),
                AcademicDegree: string.IsNullOrWhiteSpace(AcademicDegree) ? null : AcademicDegree.Trim(),
                Bio: string.IsNullOrWhiteSpace(Bio) ? null : Bio.Trim(),
                SchoolUnitId: string.IsNullOrWhiteSpace(SchoolUnitId) ? null : SchoolUnitId.Trim(),
                Status: Status);

            var result = await _teacherService.UpdateAsync(request, ct);

            if (result.Succeeded && result.Data is not null)
            {
                Teacher = result.Data;
                SuccessMessage = "Dados do professor atualizados com sucesso!";
                return true;
            }

            ErrorMessage = result.Message;
            if (result.StatusCode == 404)
            {
                IsNotFound = true;
            }
            else if (result.StatusCode == 403)
            {
                IsForbidden = true;
            }
            else if (result.StatusCode == 401)
            {
                IsUnauthorized = true;
            }

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

