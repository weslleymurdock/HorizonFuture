using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HF.Shared.Teachers.Models;

namespace HF.Shared.Teachers;

/// <summary>
/// Contrato do serviço cliente para todos os endpoints do TeachersController da API.
/// </summary>
public interface ITeacherService
{
    /// <summary>
    /// Lista professores com filtros opcionais (GET /api/v1/teachers).
    /// </summary>
    Task<TeacherResult<IReadOnlyList<TeacherItemDto>>> ListAsync(TeacherFilterDto? filter = null, CancellationToken ct = default);

    /// <summary>
    /// Obtém detalhes de um professor pelo identificador único (GET /api/v1/teachers/{id}).
    /// </summary>
    Task<TeacherResult<TeacherItemDto>> GetByIdAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Cria um novo registro de professor (POST /api/v1/teachers).
    /// </summary>
    Task<TeacherResult<TeacherItemDto>> CreateAsync(CreateTeacherDto request, CancellationToken ct = default);

    /// <summary>
    /// Atualiza um registro existente de professor (PUT /api/v1/teachers/{id}).
    /// </summary>
    Task<TeacherResult<TeacherItemDto>> UpdateAsync(UpdateTeacherDto request, CancellationToken ct = default);

    /// <summary>
    /// Exclui um registro de professor pelo identificador único (DELETE /api/v1/teachers/{id}).
    /// </summary>
    Task<TeacherResult> DeleteAsync(string id, CancellationToken ct = default);
}

