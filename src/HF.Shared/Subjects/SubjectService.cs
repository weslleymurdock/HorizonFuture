using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HF.Client;
using HF.Client.Models;
using HF.Shared.Subjects.Models;
using Microsoft.Kiota.Abstractions;

namespace HF.Shared.Subjects;

/// <summary>
/// Shared service centralizing REST consumption of Subjects endpoints
/// using the Kiota client in HF.Client, adapting responses for PageModels and UI.
/// </summary>
public class SubjectService : ISubjectService
{
    private readonly ApiClient _apiClient;

    public SubjectService(ApiClient apiClient)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }

    /// <inheritdoc />
    public async Task<SubjectResult<IReadOnlyList<SubjectItemDto>>> ListAsync(SubjectFilterDto? filter = null, CancellationToken ct = default)
    {
        try
        {
            var items = await _apiClient.Api.V1.Subjects.GetAsync(q =>
            {
                if (!string.IsNullOrWhiteSpace(filter?.CourseId))
                {
                    q.QueryParameters.CourseId = filter.CourseId;
                }
                if (!string.IsNullOrWhiteSpace(filter?.SchoolUnitId))
                {
                    q.QueryParameters.SchoolUnitId = filter.SchoolUnitId;
                }
                if (filter?.Status.HasValue == true)
                {
                    q.QueryParameters.Status = (int)filter.Status.Value;
                }
            }, cancellationToken: ct);

            var result = (items ?? new List<SubjectResponse>())
                .Select(MapSubject)
                .ToList();

            return SubjectResult<IReadOnlyList<SubjectItemDto>>.Ok(result, "Disciplinas listadas com sucesso.");
        }
        catch (ApiException ex)
        {
            return HandleApiException<IReadOnlyList<SubjectItemDto>>(ex);
        }
        catch (Exception ex)
        {
            return SubjectResult<IReadOnlyList<SubjectItemDto>>.Fail(MapExceptionMessage(ex));
        }
    }

    /// <inheritdoc />
    public async Task<SubjectResult<SubjectItemDto>> GetByIdAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return SubjectResult<SubjectItemDto>.Fail("Identificador da disciplina é obrigatório.", statusCode: 400);
        }

        try
        {
            var response = await _apiClient.Api.V1.Subjects[id].GetAsync(cancellationToken: ct);
            if (response is not null)
            {
                return SubjectResult<SubjectItemDto>.Ok(MapSubject(response), "Disciplina carregada com sucesso.");
            }

            return SubjectResult<SubjectItemDto>.Fail("Disciplina não encontrada.", statusCode: 404);
        }
        catch (ApiException ex)
        {
            return HandleApiException<SubjectItemDto>(ex);
        }
        catch (Exception ex)
        {
            return SubjectResult<SubjectItemDto>.Fail(MapExceptionMessage(ex));
        }
    }

    /// <inheritdoc />
    public async Task<SubjectResult<SubjectItemDto>> CreateAsync(CreateSubjectDto request, CancellationToken ct = default)
    {
        if (request is null)
        {
            return SubjectResult<SubjectItemDto>.Fail("Dados para criação da disciplina são obrigatórios.", statusCode: 400);
        }

        try
        {
            var kiotaRequest = new CreateSubjectRequest
            {
                Name = request.Name,
                Code = request.Code,
                WorkloadHours = request.WorkloadHours,
                Description = request.Description,
                CourseId = string.IsNullOrWhiteSpace(request.CourseId) ? null : request.CourseId,
                SchoolUnitId = string.IsNullOrWhiteSpace(request.SchoolUnitId) ? null : request.SchoolUnitId,
                Status = (int)request.Status
            };

            var response = await _apiClient.Api.V1.Subjects.PostAsync(kiotaRequest, cancellationToken: ct);
            if (response is not null)
            {
                return SubjectResult<SubjectItemDto>.Ok(MapSubject(response), "Disciplina criada com sucesso.");
            }

            return SubjectResult<SubjectItemDto>.Fail("Falha ao registrar nova disciplina.");
        }
        catch (ApiException ex)
        {
            return HandleApiException<SubjectItemDto>(ex);
        }
        catch (Exception ex)
        {
            return SubjectResult<SubjectItemDto>.Fail(MapExceptionMessage(ex));
        }
    }

    /// <inheritdoc />
    public async Task<SubjectResult<SubjectItemDto>> UpdateAsync(UpdateSubjectDto request, CancellationToken ct = default)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Id))
        {
            return SubjectResult<SubjectItemDto>.Fail("Identificador e dados da disciplina são obrigatórios.", statusCode: 400);
        }

        try
        {
            var kiotaRequest = new UpdateSubjectRequest
            {
                Id = request.Id,
                Name = request.Name,
                Code = request.Code,
                WorkloadHours = request.WorkloadHours,
                Description = request.Description,
                CourseId = string.IsNullOrWhiteSpace(request.CourseId) ? null : request.CourseId,
                SchoolUnitId = string.IsNullOrWhiteSpace(request.SchoolUnitId) ? null : request.SchoolUnitId,
                Status = (int)request.Status
            };

            var response = await _apiClient.Api.V1.Subjects[request.Id].PutAsync(kiotaRequest, cancellationToken: ct);
            if (response is not null)
            {
                return SubjectResult<SubjectItemDto>.Ok(MapSubject(response), "Disciplina atualizada com sucesso.");
            }

            return SubjectResult<SubjectItemDto>.Fail("Falha ao atualizar disciplina.");
        }
        catch (ApiException ex)
        {
            return HandleApiException<SubjectItemDto>(ex);
        }
        catch (Exception ex)
        {
            return SubjectResult<SubjectItemDto>.Fail(MapExceptionMessage(ex));
        }
    }

    /// <inheritdoc />
    public async Task<SubjectResult> DeleteAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return SubjectResult.Fail("Identificador da disciplina é obrigatório.", statusCode: 400);
        }

        try
        {
            await _apiClient.Api.V1.Subjects[id].DeleteAsync(cancellationToken: ct);
            return SubjectResult.Ok("Disciplina excluída com sucesso.");
        }
        catch (ApiException ex)
        {
            return HandleApiException(ex);
        }
        catch (Exception ex)
        {
            return SubjectResult.Fail(MapExceptionMessage(ex));
        }
    }

    private static SubjectItemDto MapSubject(SubjectResponse response)
    {
        var status = response.Status.HasValue && Enum.IsDefined(typeof(SubjectStatus), response.Status.Value)
            ? (SubjectStatus)response.Status.Value
            : SubjectStatus.Active;

        return new SubjectItemDto(
            Id: response.Id ?? string.Empty,
            Name: response.Name ?? string.Empty,
            Code: response.Code ?? string.Empty,
            WorkloadHours: response.WorkloadHours ?? 0,
            Description: response.Description,
            CourseId: response.CourseId,
            SchoolUnitId: response.SchoolUnitId,
            Status: status,
            CreatedAt: response.CreatedAt,
            UpdatedAt: response.UpdatedAt,
            CreatedBy: response.CreatedBy,
            UpdatedBy: response.UpdatedBy);
    }

    private static SubjectResult<T> HandleApiException<T>(ApiException ex)
    {
        var message = MapApiExceptionMessage(ex);
        return SubjectResult<T>.Fail(message, statusCode: ex.ResponseStatusCode);
    }

    private static SubjectResult HandleApiException(ApiException ex)
    {
        var message = MapApiExceptionMessage(ex);
        return SubjectResult.Fail(message, statusCode: ex.ResponseStatusCode);
    }

    private static string MapApiExceptionMessage(ApiException ex)
    {
        if (ex is ProblemDetails pd && !string.IsNullOrWhiteSpace(pd.Detail))
        {
            return pd.Detail;
        }

        if (ex is ResponseBaseOfSubjectResponse rbs && !string.IsNullOrWhiteSpace(rbs.MessageEscaped))
        {
            return rbs.MessageEscaped;
        }

        if (ex is ResponseBaseOfboolean rbb && !string.IsNullOrWhiteSpace(rbb.MessageEscaped))
        {
            return rbb.MessageEscaped;
        }

        return ex.ResponseStatusCode switch
        {
            400 => !string.IsNullOrWhiteSpace(ex.Message) && !ex.Message.Contains("Exception") && !ex.Message.StartsWith("The HTTP")
                ? ex.Message
                : "Requisição inválida. Verifique os dados informados para a disciplina.",
            401 => "Sessão expirada ou não autenticada (401). Faça login novamente para continuar.",
            403 => "Acesso negado: seu usuário não possui a permissão necessária para executar esta ação (403).",
            404 => "Disciplina não encontrada (404).",
            409 => "Conflito: o código da disciplina já está em uso ou existem turmas/matrículas vinculadas a ela (409).",
            422 => "Dados inválidos para a disciplina. Verifique os campos informados (422).",
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

