using CommunityToolkit.Mvvm.ComponentModel;
using HF.Shared.Courses;
using HF.Shared.Courses.Models;

namespace HF.Shared.PageModels.Courses;

/// <summary>
/// PageModel para o formulário de cadastro de novo curso (/courses/create).
/// </summary>
public partial class CourseCreatePageModel : PageModelBase
{
    private readonly ICourseService _courseService;

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
    public partial bool IsCreated { get; set; }

    [ObservableProperty]
    public partial bool IsForbidden { get; set; }

    [ObservableProperty]
    public partial CourseItemDto? CreatedCourse { get; set; }

    public CourseCreatePageModel(ICourseService courseService)
    {
        _courseService = courseService;
        Title = "Cadastrar Curso";
    }

    /// <summary>
    /// Validações client-side em conformidade com o validador CreateCourseRequestValidator.
    /// </summary>
    public bool Validate()
    {
        ErrorMessage = null;

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
    /// Envia a criação do curso à API. Previne submissões concorrentes e preserva dados em caso de falha.
    /// </summary>
    public async Task<bool> CreateCourseAsync(CancellationToken ct = default)
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

            var request = new CreateCourseDto(
                Name: Name.Trim(),
                Code: Code.Trim().ToUpperInvariant(),
                WorkloadHours: WorkloadHours,
                Description: string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                SchoolUnitId: string.IsNullOrWhiteSpace(SchoolUnitId) ? null : SchoolUnitId.Trim(),
                TargetAudience: string.IsNullOrWhiteSpace(TargetAudience) ? null : TargetAudience.Trim(),
                Status: Status);

            var result = await _courseService.CreateAsync(request, ct);

            if (result.Succeeded && result.Data is not null)
            {
                IsCreated = true;
                CreatedCourse = result.Data;
                SuccessMessage = $"Curso '{result.Data.Name}' cadastrado com sucesso!";
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
    /// Reseta o formulário para inclusão de outro curso.
    /// </summary>
    public void ResetForm()
    {
        Name = string.Empty;
        Code = string.Empty;
        WorkloadHours = 40;
        Description = null;
        SchoolUnitId = null;
        TargetAudience = null;
        Status = CourseStatus.Active;
        ErrorMessage = null;
        SuccessMessage = null;
        IsCreated = false;
        IsForbidden = false;
        CreatedCourse = null;
    }
}

