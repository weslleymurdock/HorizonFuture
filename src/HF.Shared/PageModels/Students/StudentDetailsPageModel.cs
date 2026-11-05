using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using HF.Shared.Students;
using HF.Shared.Students.Models;

namespace HF.Shared.PageModels.Students;

/// <summary>
/// PageModel for the student details view (/students/{id}).
/// </summary>
public partial class StudentDetailsPageModel : PageModelBase
{
    private readonly IStudentService _studentService;

    [ObservableProperty]
    public partial string StudentId { get; set; } = string.Empty;

    [ObservableProperty]
    public partial StudentItemDto? Student { get; set; }

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

    public StudentDetailsPageModel(IStudentService studentService)
    {
        _studentService = studentService;
        Title = "Detalhes do Aluno";
    }

    /// <summary>
    /// Loads complete student information by its unique identifier.
    /// </summary>
    public async Task<bool> LoadStudentAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            StudentId = string.Empty;
            Student = null;
            IsNotFound = true;
            ErrorMessage = "Identificador do aluno não informado.";
            return false;
        }

        StudentId = id;
        ErrorMessage = null;
        SuccessMessage = null;
        IsNotFound = false;
        IsForbidden = false;
        IsUnauthorized = false;
        IsDeleted = false;

        try
        {
            IsBusy = true;
            var result = await _studentService.GetByIdAsync(id, ct);

            if (result.Succeeded && result.Data is not null)
            {
                Student = result.Data;
                Title = $"Aluno: {result.Data.RegistrationNumber}";
                return true;
            }

            Student = null;
            if (result.StatusCode == 404)
            {
                IsNotFound = true;
                ErrorMessage = "Aluno não encontrado.";
            }
            else if (result.StatusCode == 403)
            {
                IsForbidden = true;
                ErrorMessage = "Acesso negado: permissão insuficiente para visualizar os detalhes deste aluno.";
            }
            else if (result.StatusCode == 401)
            {
                IsUnauthorized = true;
                ErrorMessage = "Sessão expirada. Faça login novamente.";
            }
            else
            {
                ErrorMessage = !string.IsNullOrWhiteSpace(result.Message)
                    ? result.Message
                    : "Falha ao carregar detalhes do aluno.";
            }

            return false;
        }
        catch (Exception ex)
        {
            Student = null;
            ErrorMessage = $"Erro inesperado: {ex.Message}";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Deletes the currently viewed student.
    /// </summary>
    public async Task<bool> DeleteStudentAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(StudentId))
        {
            ErrorMessage = "Identificador do aluno é inválido.";
            return false;
        }

        if (IsDeleting)
        {
            return false;
        }

        ErrorMessage = null;
        SuccessMessage = null;
        IsDeleting = true;

        try
        {
            var result = await _studentService.DeleteAsync(StudentId, ct);

            if (result.Succeeded)
            {
                IsDeleted = true;
                SuccessMessage = "Aluno excluído com sucesso.";
                return true;
            }

            if (result.StatusCode == 409)
            {
                ErrorMessage = "Conflito: o aluno possui matrículas vinculadas ou histórico escolar e não pode ser removido.";
            }
            else if (result.StatusCode == 404)
            {
                ErrorMessage = "Aluno não encontrado ou já foi excluído.";
            }
            else if (result.StatusCode == 403)
            {
                ErrorMessage = "Acesso negado: permissão Students.Delete ausente.";
            }
            else
            {
                ErrorMessage = !string.IsNullOrWhiteSpace(result.Message)
                    ? result.Message
                    : "Falha ao excluir aluno.";
            }

            return false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao tentar excluir aluno: {ex.Message}";
            return false;
        }
        finally
        {
            IsDeleting = false;
        }
    }
}

