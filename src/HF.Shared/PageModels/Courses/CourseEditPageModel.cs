using CommunityToolkit.Mvvm.ComponentModel;
using HF.Shared.Courses;
using HF.Shared.Courses.Models;

namespace HF.Shared.PageModels.Courses;

/// <summary>
/// PageModel para edição de um curso existente (/courses/{id}/edit).
/// </summary>
public partial class CourseEditPageModel : PageModelBase
{
    private readonly ICourseService _courseService;

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
    public partial string? SchoolUnitId { get; set; }

    [ObservableProperty]
    public partial string? TargetAudience { get; set; }

    [ObservableProperty]
    public partial CourseStatus Status { get; set; } = CourseStatus.Active;

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

    public CourseEditPageModel(ICourseService courseService)
    {
        _courseService = courseService;
        Title = "Editar Curso";
    }

    /// <summary>
    /// Carrega os dados existentes do curso para edição.
    /// </summary>
    public async Task<bool> LoadCourseAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            Id = string.Empty;
            IsNotFound = true;
            ErrorMessage = "Identificador do curso não informado.";
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

            var result = await _courseService.GetByIdAsync(id, ct);

            if (result.Succeeded && result.Data is not null)
            {
                Name = result.Data.Name;
                Code = result.Data.Code;
                WorkloadHours = result.Data.WorkloadHours;
                Description = result.Data.Description;
                SchoolUnitId = result.Data.SchoolUnitId;
                TargetAudience = result.Data.TargetAudience;
                Status = result.Data.Status;
                Title = $"Editar: {result.Data.Name}";
                return true;
            }

            if (result.StatusCode == 404)
            {
                IsNotFound = true;
                ErrorMessage = "Curso não encontrado.";
            }
            else if (result.StatusCode == 403)
            {
                IsForbidden = true;
                ErrorMessage = "Acesso negado: permissão insuficiente para editar cursos.";
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
            ErrorMessage = $"Erro ao carregar curso: {ex.Message}";
            return false;
        }
        finally
        {
            IsLoading = false;
            IsBusy = false;
        }
    }

    /// <summary>
    /// Validação client-side coerente com UpdateCourseRequestValidator.
    /// </summary>
    public bool Validate()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(Id))
        {
            ErrorMessage = "O identificador do curso é obrigatório.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "O nome do curso é obrigatório.";
            return false;
        }

        if (Name.Trim().Length > 150)
        {
            ErrorMessage = "O nome do curso não pode exceder 150 caracteres.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Code))
        {
            ErrorMessage = "O código do curso é obrigatório.";
            return false;
        }

        if (Code.Trim().Length > 30)
        {
            ErrorMessage = "O código do curso não pode exceder 30 caracteres.";
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
    /// Salva as alterações do curso via API. Preserva os campos preenchidos caso ocorra falha.
    /// </summary>
    public async Task<bool> SaveCourseAsync(CancellationToken ct = default)
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

            var request = new UpdateCourseDto(
                Id: Id,
                Name: Name.Trim(),
                Code: Code.Trim().ToUpperInvariant(),
                WorkloadHours: WorkloadHours,
                Description: string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                SchoolUnitId: string.IsNullOrWhiteSpace(SchoolUnitId) ? null : SchoolUnitId.Trim(),
                TargetAudience: string.IsNullOrWhiteSpace(TargetAudience) ? null : TargetAudience.Trim(),
                Status: Status);

            var result = await _courseService.UpdateAsync(request, ct);

            if (result.Succeeded && result.Data is not null)
            {
                IsSaved = true;
                SuccessMessage = $"Curso '{result.Data.Name}' atualizado com sucesso!";
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

