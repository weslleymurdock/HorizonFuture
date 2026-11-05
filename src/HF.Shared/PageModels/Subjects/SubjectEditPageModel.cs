using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using HF.Shared.Subjects;
using HF.Shared.Subjects.Models;

namespace HF.Shared.PageModels.Subjects;

/// <summary>
/// PageModel for editing an existing subject (/subjects/{id}/edit).
/// </summary>
public partial class SubjectEditPageModel : PageModelBase
{
    private readonly ISubjectService _subjectService;

    [ObservableProperty]
    public partial string Id { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Code { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int WorkloadHours { get; set; } = 40;

    [ObservableProperty]
    public partial string? Description { get; set; }

    [ObservableProperty]
    public partial string? CourseId { get; set; }

    [ObservableProperty]
    public partial string? SchoolUnitId { get; set; }

    [ObservableProperty]
    public partial SubjectStatus Status { get; set; } = SubjectStatus.Active;

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? SuccessMessage { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial bool IsSaved { get; set; }

    [ObservableProperty]
    public partial bool IsNotFound { get; set; }

    [ObservableProperty]
    public partial bool IsForbidden { get; set; }

    [ObservableProperty]
    public partial bool IsUnauthorized { get; set; }

    public SubjectEditPageModel(ISubjectService subjectService)
    {
        _subjectService = subjectService;
        Title = "Editar Disciplina";
    }

    /// <summary>
    /// Loads existing subject data for editing.
    /// </summary>
    public async Task<bool> LoadSubjectAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            Id = string.Empty;
            IsNotFound = true;
            ErrorMessage = "Identificador da disciplina não informado.";
            return false;
        }

        Id = id;
        ErrorMessage = null;
        SuccessMessage = null;
        IsNotFound = false;
        IsForbidden = false;
        IsUnauthorized = false;
        IsSaved = false;

        try
        {
            IsLoading = true;
            IsBusy = true;

            var result = await _subjectService.GetByIdAsync(id, ct);

            if (result.Succeeded && result.Data is not null)
            {
                Name = result.Data.Name;
                Code = result.Data.Code;
                WorkloadHours = result.Data.WorkloadHours;
                Description = result.Data.Description;
                CourseId = result.Data.CourseId;
                SchoolUnitId = result.Data.SchoolUnitId;
                Status = result.Data.Status;
                Title = $"Editar: {result.Data.Name}";
                return true;
            }

            if (result.StatusCode == 404)
            {
                IsNotFound = true;
                ErrorMessage = "Disciplina não encontrada.";
            }
            else if (result.StatusCode == 403)
            {
                IsForbidden = true;
                ErrorMessage = "Acesso negado: permissão insuficiente para editar disciplinas.";
            }
            else if (result.StatusCode == 401)
            {
                IsUnauthorized = true;
                ErrorMessage = "Sessão expirada. Faça login novamente.";
            }
            else
            {
                ErrorMessage = result.Message;
            }

            return false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao carregar disciplina: {ex.Message}";
            return false;
        }
        finally
        {
            IsLoading = false;
            IsBusy = false;
        }
    }

    /// <summary>
    /// Client-side validation in compliance with UpdateSubjectRequestValidator.
    /// </summary>
    public bool Validate()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(Id))
        {
            ErrorMessage = "O identificador da disciplina é obrigatório.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "O nome da disciplina é obrigatório.";
            return false;
        }

        if (Name.Trim().Length > 150)
        {
            ErrorMessage = "O nome da disciplina não pode exceder 150 caracteres.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Code))
        {
            ErrorMessage = "O código da disciplina é obrigatório.";
            return false;
        }

        if (Code.Trim().Length > 30)
        {
            ErrorMessage = "O código da disciplina não pode exceder 30 caracteres.";
            return false;
        }

        if (WorkloadHours <= 0)
        {
            ErrorMessage = "A carga horária deve ser maior que zero.";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Saves updated subject data to the API. Preserves entered fields on failure.
    /// </summary>
    public async Task<bool> SaveSubjectAsync(CancellationToken ct = default)
    {
        if (IsBusy)
        {
            return false;
        }

        SuccessMessage = null;
        ErrorMessage = null;
        IsForbidden = false;

        if (!Validate())
        {
            return false;
        }

        try
        {
            IsBusy = true;

            var request = new UpdateSubjectDto(
                Id: Id,
                Name: Name.Trim(),
                Code: Code.Trim().ToUpperInvariant(),
                WorkloadHours: WorkloadHours,
                Description: string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                CourseId: string.IsNullOrWhiteSpace(CourseId) ? null : CourseId.Trim(),
                SchoolUnitId: string.IsNullOrWhiteSpace(SchoolUnitId) ? null : SchoolUnitId.Trim(),
                Status: Status);

            var result = await _subjectService.UpdateAsync(request, ct);

            if (result.Succeeded && result.Data is not null)
            {
                IsSaved = true;
                SuccessMessage = $"Disciplina '{result.Data.Name}' atualizada com sucesso!";
                return true;
            }

            ErrorMessage = !string.IsNullOrWhiteSpace(result.Message)
                ? result.Message
                : string.Join("; ", result.Errors);

            if (result.StatusCode == 404)
            {
                IsNotFound = true;
            }
            else if (result.StatusCode == 403)
            {
                IsForbidden = true;
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

