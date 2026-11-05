using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using HF.Shared.Teachers;
using HF.Shared.Teachers.Models;

namespace HF.Shared.PageModels.Teachers;

/// <summary>
/// PageModel para a tela de cadastro de professor (/teachers/create).
/// </summary>
public partial class TeacherCreatePageModel : PageModelBase
{
    private readonly ITeacherService _teacherService;

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? SuccessMessage { get; set; }

    [ObservableProperty]
    public partial bool IsForbidden { get; set; }

    [ObservableProperty]
    public partial bool IsUnauthorized { get; set; }

    [ObservableProperty]
    public partial string UserId { get; set; } = string.Empty;

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
    public partial DateTime? HireDate { get; set; }

    [ObservableProperty]
    public partial TeacherStatus Status { get; set; } = TeacherStatus.Active;

    [ObservableProperty]
    public partial TeacherItemDto? CreatedTeacher { get; set; }

    public TeacherCreatePageModel(ITeacherService teacherService)
    {
        _teacherService = teacherService ?? throw new ArgumentNullException(nameof(teacherService));
        Title = "Cadastrar Professor";
    }

    /// <summary>
    /// Valida e submete a criação do professor na API.
    /// </summary>
    public async Task<bool> CreateTeacherAsync(CancellationToken ct = default)
    {
        ErrorMessage = null;
        SuccessMessage = null;
        IsForbidden = false;
        IsUnauthorized = false;
        CreatedTeacher = null;

        // Validações no cliente
        if (string.IsNullOrWhiteSpace(UserId))
        {
            ErrorMessage = "O identificador do usuário (UserId) é obrigatório.";
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
            DateOnly? hireDateOnly = HireDate.HasValue
                ? DateOnly.FromDateTime(HireDate.Value)
                : null;

            var request = new CreateTeacherDto(
                UserId: UserId.Trim(),
                EmployeeNumber: EmployeeNumber.Trim(),
                Specialty: Specialty.Trim(),
                AcademicDegree: string.IsNullOrWhiteSpace(AcademicDegree) ? null : AcademicDegree.Trim(),
                Bio: string.IsNullOrWhiteSpace(Bio) ? null : Bio.Trim(),
                SchoolUnitId: string.IsNullOrWhiteSpace(SchoolUnitId) ? null : SchoolUnitId.Trim(),
                HireDate: hireDateOnly,
                Status: Status);

            var result = await _teacherService.CreateAsync(request, ct);

            if (result.Succeeded && result.Data is not null)
            {
                CreatedTeacher = result.Data;
                SuccessMessage = "Professor cadastrado com sucesso!";
                return true;
            }

            ErrorMessage = result.Message;
            if (result.StatusCode == 403)
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

