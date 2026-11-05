using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using HF.Shared.Students;
using HF.Shared.Students.Models;

namespace HF.Shared.PageModels.Students;

/// <summary>
/// PageModel for the student creation form (/students/create).
/// </summary>
public partial class StudentCreatePageModel : PageModelBase
{
    private readonly IStudentService _studentService;

    [ObservableProperty]
    public partial string UserId { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string RegistrationNumber { get; set; } = string.Empty;

    [ObservableProperty]
    public partial DateTime? DateOfBirth { get; set; } = DateTime.Today.AddYears(-10);

    [ObservableProperty]
    public partial string? GuardianName { get; set; }

    [ObservableProperty]
    public partial string? GuardianRelationship { get; set; }

    [ObservableProperty]
    public partial string? GuardianPhone { get; set; }

    [ObservableProperty]
    public partial string? EmergencyContactName { get; set; }

    [ObservableProperty]
    public partial string? EmergencyContactPhone { get; set; }

    [ObservableProperty]
    public partial string? SchoolUnitId { get; set; }

    [ObservableProperty]
    public partial StudentStatus Status { get; set; } = StudentStatus.Active;

    [ObservableProperty]
    public partial string? SpecialNeeds { get; set; }

    [ObservableProperty]
    public partial string? MedicalNotes { get; set; }

    [ObservableProperty]
    public partial DateTime? EnrollmentDate { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? SuccessMessage { get; set; }

    [ObservableProperty]
    public partial bool IsCreated { get; set; }

    [ObservableProperty]
    public partial bool IsForbidden { get; set; }

    [ObservableProperty]
    public partial StudentItemDto? CreatedStudent { get; set; }

    public StudentCreatePageModel(IStudentService studentService)
    {
        _studentService = studentService;
        Title = "Cadastrar Aluno";
    }

    /// <summary>
    /// Client-side validation in compliance with CreateStudentRequestValidator.
    /// </summary>
    public bool Validate()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(UserId))
        {
            ErrorMessage = "O identificador do usuário (UserId) é obrigatório.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(RegistrationNumber))
        {
            ErrorMessage = "O número de matrícula é obrigatório.";
            return false;
        }

        if (RegistrationNumber.Trim().Length > 50)
        {
            ErrorMessage = "O número de matrícula não pode exceder 50 caracteres.";
            return false;
        }

        if (!DateOfBirth.HasValue)
        {
            ErrorMessage = "A data de nascimento é obrigatória.";
            return false;
        }

        var dob = DateOnly.FromDateTime(DateOfBirth.Value);
        if (dob >= DateOnly.FromDateTime(DateTime.UtcNow))
        {
            ErrorMessage = "A data de nascimento deve ser uma data passada.";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Sends student creation to the API. Prevents concurrent submissions and preserves data on failure.
    /// </summary>
    public async Task<bool> CreateStudentAsync(CancellationToken ct = default)
    {
        if (IsBusy)
        {
            return false;
        }

        if (!Validate())
        {
            return false;
        }

        ErrorMessage = null;
        SuccessMessage = null;
        IsForbidden = false;
        IsCreated = false;
        CreatedStudent = null;

        try
        {
            IsBusy = true;

            var dob = DateOnly.FromDateTime(DateOfBirth!.Value);
            var enrollment = EnrollmentDate.HasValue
                ? DateOnly.FromDateTime(EnrollmentDate.Value)
                : (DateOnly?)null;

            var request = new CreateStudentDto(
                UserId: UserId.Trim(),
                RegistrationNumber: RegistrationNumber.Trim(),
                DateOfBirth: dob,
                GuardianName: string.IsNullOrWhiteSpace(GuardianName) ? null : GuardianName.Trim(),
                GuardianRelationship: string.IsNullOrWhiteSpace(GuardianRelationship) ? null : GuardianRelationship.Trim(),
                GuardianPhone: string.IsNullOrWhiteSpace(GuardianPhone) ? null : GuardianPhone.Trim(),
                EmergencyContactName: string.IsNullOrWhiteSpace(EmergencyContactName) ? null : EmergencyContactName.Trim(),
                EmergencyContactPhone: string.IsNullOrWhiteSpace(EmergencyContactPhone) ? null : EmergencyContactPhone.Trim(),
                SchoolUnitId: string.IsNullOrWhiteSpace(SchoolUnitId) ? null : SchoolUnitId.Trim(),
                Status: Status,
                SpecialNeeds: string.IsNullOrWhiteSpace(SpecialNeeds) ? null : SpecialNeeds.Trim(),
                MedicalNotes: string.IsNullOrWhiteSpace(MedicalNotes) ? null : MedicalNotes.Trim(),
                EnrollmentDate: enrollment);

            var result = await _studentService.CreateAsync(request, ct);

            if (result.Succeeded && result.Data is not null)
            {
                IsCreated = true;
                CreatedStudent = result.Data;
                SuccessMessage = "Aluno cadastrado com sucesso.";
                return true;
            }

            if (result.StatusCode == 403)
            {
                IsForbidden = true;
                ErrorMessage = "Acesso negado: permissão Students.Create ausente.";
            }
            else if (result.StatusCode == 409)
            {
                ErrorMessage = "Conflito: a matrícula já está em uso ou o usuário já está vinculado a outro aluno.";
            }
            else
            {
                ErrorMessage = !string.IsNullOrWhiteSpace(result.Message)
                    ? result.Message
                    : "Falha ao registrar novo aluno no servidor.";
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
    /// Resets the input form back to clean default values.
    /// </summary>
    public void ResetForm()
    {
        UserId = string.Empty;
        RegistrationNumber = string.Empty;
        DateOfBirth = DateTime.Today.AddYears(-10);
        GuardianName = null;
        GuardianRelationship = null;
        GuardianPhone = null;
        EmergencyContactName = null;
        EmergencyContactPhone = null;
        SchoolUnitId = null;
        Status = StudentStatus.Active;
        SpecialNeeds = null;
        MedicalNotes = null;
        EnrollmentDate = DateTime.Today;
        ErrorMessage = null;
        SuccessMessage = null;
        IsCreated = false;
        IsForbidden = false;
        CreatedStudent = null;
    }
}

