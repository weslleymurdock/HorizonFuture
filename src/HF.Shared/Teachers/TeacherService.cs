using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HF.Client;
using HF.Client.Models;
using HF.Shared.Teachers.Models;
using Microsoft.Kiota.Abstractions;

namespace HF.Shared.Teachers;

/// <summary>
/// Shared service centralizing REST consumption of Teachers endpoints
/// using the Kiota client in HF.Client, adapting responses for PageModels and UI.
/// </summary>
public class TeacherService : ITeacherService
{
    private readonly ApiClient _apiClient;

    public TeacherService(ApiClient apiClient)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }

    /// <inheritdoc />
    public async Task<TeacherResult<IReadOnlyList<TeacherItemDto>>> ListAsync(TeacherFilterDto? filter = null, CancellationToken ct = default)
    {
        try
        {
            var items = await _apiClient.Api.V1.Teachers.GetAsync(q =>
            {
                if (!string.IsNullOrWhiteSpace(filter?.SchoolUnitId))
                {
                    q.QueryParameters.SchoolUnitId = filter.SchoolUnitId;
                }
                if (filter?.Status.HasValue == true)
                {
                    q.QueryParameters.Status = (int)filter.Status.Value;
                }
                if (!string.IsNullOrWhiteSpace(filter?.Specialty))
                {
                    q.QueryParameters.Specialty = filter.Specialty;
                }
            }, cancellationToken: ct);

            var result = (items ?? new List<TeacherResponse>())
                .Select(MapTeacher)
                .ToList();

            return TeacherResult<IReadOnlyList<TeacherItemDto>>.Ok(result, "Professores listados com sucesso.");
        }
        catch (ApiException ex)
        {
            return HandleApiException<IReadOnlyList<TeacherItemDto>>(ex);
        }
        catch (Exception ex)
        {
            return TeacherResult<IReadOnlyList<TeacherItemDto>>.Fail(MapExceptionMessage(ex));
        }
    }

    /// <inheritdoc />
    public async Task<TeacherResult<TeacherItemDto>> GetByIdAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return TeacherResult<TeacherItemDto>.Fail("Identificador do professor é obrigatório.", statusCode: 400);
        }

        try
        {
            var response = await _apiClient.Api.V1.Teachers[id].GetAsync(cancellationToken: ct);
            if (response is not null)
            {
                return TeacherResult<TeacherItemDto>.Ok(MapTeacher(response), "Professor carregado com sucesso.");
            }

            return TeacherResult<TeacherItemDto>.Fail("Professor não encontrado.", statusCode: 404);
        }
        catch (ApiException ex)
        {
            return HandleApiException<TeacherItemDto>(ex);
        }
        catch (Exception ex)
        {
            return TeacherResult<TeacherItemDto>.Fail(MapExceptionMessage(ex));
        }
    }

    /// <inheritdoc />
    public async Task<TeacherResult<TeacherItemDto>> CreateAsync(CreateTeacherDto request, CancellationToken ct = default)
    {
        if (request is null)
        {
            return TeacherResult<TeacherItemDto>.Fail("Dados do professor são obrigatórios.", statusCode: 400);
        }

        try
        {
            var kiotaBody = new HF.Client.Models.CreateTeacherRequest
            {
                UserId = request.UserId,
                EmployeeNumber = request.EmployeeNumber,
                Specialty = request.Specialty,
                AcademicDegree = request.AcademicDegree,
                Bio = request.Bio,
                SchoolUnitId = request.SchoolUnitId,
                Status = (int)request.Status,
                HireDate = request.HireDate.HasValue
                    ? new Date(request.HireDate.Value.Year, request.HireDate.Value.Month, request.HireDate.Value.Day)
                    : null
            };

            var response = await _apiClient.Api.V1.Teachers.PostAsync(kiotaBody, cancellationToken: ct);
            if (response is not null)
            {
                return TeacherResult<TeacherItemDto>.Ok(MapTeacher(response), "Professor cadastrado com sucesso.");
            }

            return TeacherResult<TeacherItemDto>.Fail("Falha ao registrar novo professor no servidor.");
        }
        catch (ApiException ex)
        {
            return HandleApiException<TeacherItemDto>(ex);
        }
        catch (Exception ex)
        {
            return TeacherResult<TeacherItemDto>.Fail(MapExceptionMessage(ex));
        }
    }

    /// <inheritdoc />
    public async Task<TeacherResult<TeacherItemDto>> UpdateAsync(UpdateTeacherDto request, CancellationToken ct = default)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Id))
        {
            return TeacherResult<TeacherItemDto>.Fail("Identificador e dados do professor são obrigatórios.", statusCode: 400);
        }

        try
        {
            var kiotaBody = new HF.Client.Models.UpdateTeacherRequest
            {
                Id = request.Id,
                EmployeeNumber = request.EmployeeNumber,
                Specialty = request.Specialty,
                AcademicDegree = request.AcademicDegree,
                Bio = request.Bio,
                SchoolUnitId = request.SchoolUnitId,
                Status = (int)request.Status
            };

            var response = await _apiClient.Api.V1.Teachers[request.Id].PutAsync(kiotaBody, cancellationToken: ct);
            if (response is not null)
            {
                return TeacherResult<TeacherItemDto>.Ok(MapTeacher(response), "Dados do professor atualizados com sucesso.");
            }

            return TeacherResult<TeacherItemDto>.Fail("Falha ao atualizar dados do professor no servidor.");
        }
        catch (ApiException ex)
        {
            return HandleApiException<TeacherItemDto>(ex);
        }
        catch (Exception ex)
        {
            return TeacherResult<TeacherItemDto>.Fail(MapExceptionMessage(ex));
        }
    }

    /// <inheritdoc />
    public async Task<TeacherResult> DeleteAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return TeacherResult.Fail("Identificador do professor é obrigatório.", statusCode: 400);
        }

        try
        {
            await _apiClient.Api.V1.Teachers[id].DeleteAsync(cancellationToken: ct);
            return TeacherResult.Ok("Professor excluído com sucesso.");
        }
        catch (ApiException ex)
        {
            return HandleApiException(ex);
        }
        catch (Exception ex)
        {
            return TeacherResult.Fail(MapExceptionMessage(ex));
        }
    }

    private static TeacherItemDto MapTeacher(TeacherResponse response)
    {
        var status = response.Status.HasValue && Enum.IsDefined(typeof(TeacherStatus), response.Status.Value)
            ? (TeacherStatus)response.Status.Value
            : TeacherStatus.Active;

        DateOnly? hireDate = response.HireDate.HasValue
            ? new DateOnly(response.HireDate.Value.Year, response.HireDate.Value.Month, response.HireDate.Value.Day)
            : null;

        return new TeacherItemDto(
            Id: response.Id ?? string.Empty,
            UserId: response.UserId ?? string.Empty,
            EmployeeNumber: response.EmployeeNumber ?? string.Empty,
            Specialty: response.Specialty ?? string.Empty,
            AcademicDegree: response.AcademicDegree,
            Bio: response.Bio,
            SchoolUnitId: response.SchoolUnitId,
            HireDate: hireDate,
            Status: status,
            CreatedAt: response.CreatedAt?.UtcDateTime,
            UpdatedAt: response.UpdatedAt?.UtcDateTime,
            CreatedBy: response.CreatedBy,
            UpdatedBy: response.UpdatedBy);
    }

    private static TeacherResult<T> HandleApiException<T>(ApiException ex)
    {
        var message = MapApiExceptionMessage(ex);
        return TeacherResult<T>.Fail(message, statusCode: ex.ResponseStatusCode);
    }

    private static TeacherResult HandleApiException(ApiException ex)
    {
        var message = MapApiExceptionMessage(ex);
        return TeacherResult.Fail(message, statusCode: ex.ResponseStatusCode);
    }

    private static string MapApiExceptionMessage(ApiException ex)
    {
        if (ex is ProblemDetails pd && !string.IsNullOrWhiteSpace(pd.Detail))
        {
            return pd.Detail;
        }

        if (ex is ResponseBaseOfTeacherResponse rbt && !string.IsNullOrWhiteSpace(rbt.MessageEscaped))
        {
            return rbt.MessageEscaped;
        }

        if (ex is ResponseBaseOfboolean rbb && !string.IsNullOrWhiteSpace(rbb.MessageEscaped))
        {
            return rbb.MessageEscaped;
        }

        return ex.ResponseStatusCode switch
        {
            400 => !string.IsNullOrWhiteSpace(ex.Message) && !ex.Message.Contains("Exception") && !ex.Message.StartsWith("The HTTP")
                ? ex.Message
                : "Requisição inválida. Verifique os dados informados para o professor.",
            401 => "Sessão expirada ou não autenticada (401). Faça login novamente para continuar.",
            403 => "Acesso negado: seu usuário não possui a permissão necessária para executar esta ação (403).",
            404 => "Professor não encontrado (404).",
            409 => "Conflito: número de matrícula já existente, usuário já vinculado a outro perfil de professor ou vínculos ativos impedem a exclusão (409).",
            422 => "Dados inválidos para o professor. Verifique os campos informados (422).",
            >= 500 => "Ocorreu um erro interno no servidor (500). Tente novamente mais tarde.",
            _ => !string.IsNullOrWhiteSpace(ex.Message) && !ex.Message.Contains("Exception") && !ex.Message.StartsWith("The HTTP")
                ? ex.Message
                : "Falha na comunicação com o servidor."
        };
    }

    private static string MapExceptionMessage(Exception ex)
    {
        return ex switch
        {
            HttpRequestException => "Não foi possível conectar ao servidor. Verifique sua conexão de rede.",
            TaskCanceledException => "A operação foi cancelada ou o tempo limite da requisição expirou.",
            _ => $"Erro na requisição: {ex.Message}"
        };
    }
}

