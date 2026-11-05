using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using HF.Shared.Students;
using HF.Shared.Students.Models;

namespace HF.Shared.PageModels.Students;

/// <summary>
/// PageModel for the student editing form (/students/{id}/edit).
/// </summary>
public partial class StudentEditPageModel : PageModelBase
{
    private readonly IStudentService _studentService;

    [ObservableProperty]
    public partial string StudentId { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string RegistrationNumber { get; set; } = string.Empty;

    [ObservableProperty]
    public partial DateTime? DateOfBirth { get; set; }

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
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? SuccessMessage { get; set; }

    [ObservableProperty]
    public partial bool IsNotFound { get; set; }

    [ObservableProperty]
    public partial bool IsForbidden { get; set; }

    [ObservableProperty]
    public partial bool IsSaved { get; set; }

    public StudentEditPageModel(IStudentService studentService)
    {
        _studentService = studentService;
        Title = "Editar Aluno";
    }

    /// <summary>
    /// Loads student data into edit fields by unique identifier.
    /// </summary>
    public async Task<bool> LoadStudentAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            StudentId = string.Empty;
            IsNotFound = true;
            ErrorMessage = "Identificador do aluno não informado.";
            return false;
        }

        StudentId = id;
        ErrorMessage = null;
        SuccessMessage = null;
        IsNotFound = false;
        IsForbidden = false;
        IsSaved = false;

        try
        {
            IsBusy = true;
            var result = await _studentService.GetByIdAsync(id, ct);

            if (result.Succeeded && result.Data is not null)
            {
                var s = result.Data;
                RegistrationNumber = s.RegistrationNumber;
                DateOfBirth = s.DateOfBirth.ToDateTime(TimeOnly.MinValue);
                GuardianName = s.GuardianName;
                GuardianRelationship = s.GuardianRelationship;
                GuardianPhone = s.GuardianPhone;
                EmergencyContactName = s.EmergencyContactName;
                EmergencyContactPhone = s.EmergencyContactPhone;
                SchoolUnitId = s.SchoolUnitId;
                Status = s.Status;
                SpecialNeeds = s.SpecialNeeds;
                MedicalNotes = s.MedicalNotes;
                Title = $"Editar Aluno: {s.RegistrationNumber}";
                return true;
            }

            if (result.StatusCode == 404)
            {
                IsNotFound = true;
                ErrorMessage = "Aluno não encontrado.";
            }
            else if (result.StatusCode == 403)
            {
                IsForbidden = true;
                ErrorMessage = "Acesso negado: permissão insuficiente para editar este aluno.";
            }
            else
            {
                ErrorMessage = !string.IsNullOrWhiteSpace(result.Message)
                    ? result.Message
                    : "Falha ao carregar dados do aluno.";
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
    /// Validates inputs against UpdateStudentRequestValidator rules.
    /// </summary>
    public bool Validate()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(StudentId))
        {
            ErrorMessage = "Identificador do aluno é obrigatório.";
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
    /// Sends student updates to the API.
    /// </summary>
    public async Task<bool> SaveStudentAsync(CancellationToken ct = default)
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
        IsSaved = false;

        try
        {
            IsBusy = true;

            var dob = DateOnly.FromDateTime(DateOfBirth!.Value);

            var request = new UpdateStudentDto(
                Id: StudentId.Trim(),
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
                MedicalNotes: string.IsNullOrWhiteSpace(MedicalNotes) ? null : MedicalNotes.Trim());

            var result = await _studentService.UpdateAsync(request, ct);

            if (result.Succeeded && result.Data is not null)
            {
                IsSaved = true;
                SuccessMessage = "Aluno atualizado com sucesso.";
                return true;
            }

            if (result.StatusCode == 404)
            {
                IsNotFound = true;
                ErrorMessage = "Aluno não encontrado no servidor.";
            }
            else if (result.StatusCode == 403)
            {
                IsForbidden = true;
                ErrorMessage = "Acesso negado: permissão Students.Update ausente.";
            }
            else if (result.StatusCode == 409)
            {
                ErrorMessage = "Conflito: o número de matrícula já está em uso por outro aluno.";
            }
            else
            {
                ErrorMessage = !string.IsNullOrWhiteSpace(result.Message)
                    ? result.Message
                    : "Falha ao salvar alterações do aluno.";
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

