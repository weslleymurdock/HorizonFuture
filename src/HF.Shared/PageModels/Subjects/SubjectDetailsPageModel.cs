using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using HF.Shared.Subjects;
using HF.Shared.Subjects.Models;

namespace HF.Shared.PageModels.Subjects;

/// <summary>
/// PageModel for the subject details view (/subjects/{id}).
/// </summary>
public partial class SubjectDetailsPageModel : PageModelBase
{
    private readonly ISubjectService _subjectService;

    [ObservableProperty]
    public partial string SubjectId { get; set; } = string.Empty;

    [ObservableProperty]
    public partial SubjectItemDto? Subject { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? SuccessMessage { get; set; }

    [ObservableProperty]
    public partial bool IsNotFound { get; set; }

    [ObservableProperty]
    public partial bool IsForbidden { get; set; }

    [ObservableProperty]
    public partial bool IsUnauthorized { get; set; }

    [ObservableProperty]
    public partial bool IsDeleting { get; set; }

    [ObservableProperty]
    public partial bool IsDeleted { get; set; }

    public SubjectDetailsPageModel(ISubjectService subjectService)
    {
        _subjectService = subjectService;
        Title = "Detalhes da Disciplina";
    }

    /// <summary>
    /// Loads complete subject information by its unique identifier.
    /// </summary>
    public async Task<bool> LoadSubjectAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            SubjectId = string.Empty;
            Subject = null;
            IsNotFound = true;
            ErrorMessage = "Identificador da disciplina não informado.";
            return false;
        }

        SubjectId = id;
        ErrorMessage = null;
        SuccessMessage = null;
        IsNotFound = false;
        IsForbidden = false;
        IsUnauthorized = false;
        IsDeleted = false;

        try
        {
            IsBusy = true;
            var result = await _subjectService.GetByIdAsync(id, ct);

            if (result.Succeeded && result.Data is not null)
            {
                Subject = result.Data;
                Title = $"Disciplina: {result.Data.Name}";
                return true;
            }

            Subject = null;
            if (result.StatusCode == 404)
            {
                IsNotFound = true;
                ErrorMessage = "Disciplina não encontrada.";
            }
            else if (result.StatusCode == 403)
            {
                IsForbidden = true;
                ErrorMessage = "Acesso negado: permissão insuficiente para visualizar os detalhes desta disciplina.";
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
            Subject = null;
            ErrorMessage = $"Erro ao carregar detalhes: {ex.Message}";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Deletes the currently displayed subject after confirmation in the UI.
    /// </summary>
    public async Task<bool> DeleteSubjectAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(SubjectId) || IsDeleting)
        {
            return false;
        }

        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            IsDeleting = true;
            var result = await _subjectService.DeleteAsync(SubjectId, ct);

            if (result.Succeeded)
            {
                IsDeleted = true;
                SuccessMessage = "Disciplina excluída com sucesso!";
                return true;
            }

            ErrorMessage = result.Message;
            return false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao excluir disciplina: {ex.Message}";
            return false;
        }
        finally
        {
            IsDeleting = false;
        }
    }
}

