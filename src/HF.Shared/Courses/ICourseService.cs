using HF.Shared.Courses.Models;

namespace HF.Shared.Courses;

/// <summary>
/// Contrato do serviço cliente para todos os endpoints do CoursesController da API.
/// </summary>
public interface ICourseService
{
    /// <summary>
    /// Lista cursos com filtros opcionais (GET /api/v1/courses).
    /// </summary>
    Task<CourseResult<IReadOnlyList<CourseItemDto>>> ListAsync(CourseFilterDto? filter = null, CancellationToken ct = default);

    /// <summary>
    /// Obtém detalhes de um curso pelo identificador único (GET /api/v1/courses/{id}).
    /// </summary>
    Task<CourseResult<CourseItemDto>> GetByIdAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Cria um novo registro de curso (POST /api/v1/courses).
    /// </summary>
    Task<CourseResult<CourseItemDto>> CreateAsync(CreateCourseDto request, CancellationToken ct = default);

    /// <summary>
    /// Atualiza um curso existente (PUT /api/v1/courses/{id}).
    /// </summary>
    Task<CourseResult<CourseItemDto>> UpdateAsync(UpdateCourseDto request, CancellationToken ct = default);

    /// <summary>
    /// Exclui um curso pelo identificador único (DELETE /api/v1/courses/{id}).
    /// </summary>
    Task<CourseResult> DeleteAsync(string id, CancellationToken ct = default);
}

