using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using HF.Shared.Subjects;
using HF.Shared.Subjects.Models;

namespace HF.Shared.PageModels.Subjects;

/// <summary>
/// PageModel for the new subject creation form (/subjects/create).
/// </summary>
public partial class SubjectCreatePageModel : PageModelBase
{
    private readonly ISubjectService _subjectService;

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
    public partial bool IsCreated { get; set; }

    [ObservableProperty]
    public partial bool IsForbidden { get; set; }

    [ObservableProperty]
    public partial SubjectItemDto? CreatedSubject { get; set; }

    public SubjectCreatePageModel(ISubjectService subjectService)
    {
        _subjectService = subjectService;
        Title = "Cadastrar Disciplina";
    }

    /// <summary>
    /// Client-side validation in compliance with CreateSubjectRequestValidator.
    /// </summary>
    public bool Validate()
    {
        ErrorMessage = null;

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
    /// Sends subject creation to the API. Prevents concurrent submissions and preserves data on failure.
    /// </summary>
    public async Task<bool> CreateSubjectAsync(CancellationToken ct = default)
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

            var request = new CreateSubjectDto(
                Name: Name.Trim(),
                Code: Code.Trim().ToUpperInvariant(),
                WorkloadHours: WorkloadHours,
                Description: string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                CourseId: string.IsNullOrWhiteSpace(CourseId) ? null : CourseId.Trim(),
                SchoolUnitId: string.IsNullOrWhiteSpace(SchoolUnitId) ? null : SchoolUnitId.Trim(),
                Status: Status);

            var result = await _subjectService.CreateAsync(request, ct);

            if (result.Succeeded && result.Data is not null)
            {
                IsCreated = true;
                CreatedSubject = result.Data;
                SuccessMessage = $"Disciplina '{result.Data.Name}' cadastrada com sucesso!";
                return true;
            }

            ErrorMessage = !string.IsNullOrWhiteSpace(result.Message)
                ? result.Message
                : string.Join("; ", result.Errors);

            if (result.StatusCode == 403)
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

    /// <summary>
    /// Resets the form for adding another subject.
    /// </summary>
    public void ResetForm()
    {
        Name = string.Empty;
        Code = string.Empty;
        WorkloadHours = 40;
        Description = null;
        CourseId = null;
        SchoolUnitId = null;
        Status = SubjectStatus.Active;
        ErrorMessage = null;
        SuccessMessage = null;
        IsCreated = false;
        IsForbidden = false;
        CreatedSubject = null;
    }
}

