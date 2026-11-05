using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HF.Client;
using HF.Client.Models;
using HF.Shared.Courses.Models;
using Microsoft.Kiota.Abstractions;

namespace HF.Shared.Courses;

/// <summary>
/// Serviço compartilhado que centraliza o consumo REST dos endpoints de Courses
/// utilizando o cliente Kiota de HF.Client, adaptando respostas para os PageModels e UI.
/// </summary>
public class CourseService : ICourseService
{
    private readonly ApiClient _apiClient;

    public CourseService(ApiClient apiClient)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }

    /// <inheritdoc />
    public async Task<CourseResult<IReadOnlyList<CourseItemDto>>> ListAsync(CourseFilterDto? filter = null, CancellationToken ct = default)
    {
        try
        {
            var items = await _apiClient.Api.V1.Courses.GetAsync(q =>
            {
                if (!string.IsNullOrWhiteSpace(filter?.SchoolUnitId))
                {
                    q.QueryParameters.SchoolUnitId = filter.SchoolUnitId;
                }
                if (filter?.Status.HasValue == true)
                {
                    q.QueryParameters.Status = (int)filter.Status.Value;
                }
            }, cancellationToken: ct);

            var result = (items ?? new List<CourseResponse>())
                .Select(MapCourse)
                .ToList();

            return CourseResult<IReadOnlyList<CourseItemDto>>.Ok(result, "Cursos listados com sucesso.");
        }
        catch (ApiException ex)
        {
            return HandleApiException<IReadOnlyList<CourseItemDto>>(ex);
        }
        catch (Exception ex)
        {
            return CourseResult<IReadOnlyList<CourseItemDto>>.Fail(MapExceptionMessage(ex));
        }
    }

    /// <inheritdoc />
    public async Task<CourseResult<CourseItemDto>> GetByIdAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return CourseResult<CourseItemDto>.Fail("Identificador do curso é obrigatório.", statusCode: 400);
        }

        try
        {
            var response = await _apiClient.Api.V1.Courses[id].GetAsync(cancellationToken: ct);
            if (response is not null)
            {
                return CourseResult<CourseItemDto>.Ok(MapCourse(response), "Curso carregado com sucesso.");
            }

            return CourseResult<CourseItemDto>.Fail("Curso não encontrado.", statusCode: 404);
        }
        catch (ApiException ex)
        {
            return HandleApiException<CourseItemDto>(ex);
        }
        catch (Exception ex)
        {
            return CourseResult<CourseItemDto>.Fail(MapExceptionMessage(ex));
        }
    }

    /// <inheritdoc />
    public async Task<CourseResult<CourseItemDto>> CreateAsync(CreateCourseDto request, CancellationToken ct = default)
    {
        if (request is null)
        {
            return CourseResult<CourseItemDto>.Fail("Dados para criação do curso são obrigatórios.", statusCode: 400);
        }

        try
        {
            var kiotaRequest = new CreateCourseRequest
            {
                Name = request.Name,
                Code = request.Code,
                WorkloadHours = request.WorkloadHours,
                Description = request.Description,
                SchoolUnitId = string.IsNullOrWhiteSpace(request.SchoolUnitId) ? null : request.SchoolUnitId,
                TargetAudience = request.TargetAudience,
                Status = (int)request.Status
            };

            var response = await _apiClient.Api.V1.Courses.PostAsync(kiotaRequest, cancellationToken: ct);
            if (response is not null)
            {
                return CourseResult<CourseItemDto>.Ok(MapCourse(response), "Curso criado com sucesso.");
            }

            return CourseResult<CourseItemDto>.Fail("Falha ao registrar novo curso.");
        }
        catch (ApiException ex)
        {
            return HandleApiException<CourseItemDto>(ex);
        }
        catch (Exception ex)
        {
            return CourseResult<CourseItemDto>.Fail(MapExceptionMessage(ex));
        }
    }

    /// <inheritdoc />
    public async Task<CourseResult<CourseItemDto>> UpdateAsync(UpdateCourseDto request, CancellationToken ct = default)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Id))
        {
            return CourseResult<CourseItemDto>.Fail("Identificador e dados do curso são obrigatórios.", statusCode: 400);
        }

        try
        {
            var kiotaRequest = new UpdateCourseRequest
            {
                Id = request.Id,
                Name = request.Name,
                Code = request.Code,
                WorkloadHours = request.WorkloadHours,
                Description = request.Description,
                SchoolUnitId = string.IsNullOrWhiteSpace(request.SchoolUnitId) ? null : request.SchoolUnitId,
                TargetAudience = request.TargetAudience,
                Status = (int)request.Status
            };

            var response = await _apiClient.Api.V1.Courses[request.Id].PutAsync(kiotaRequest, cancellationToken: ct);
            if (response is not null)
            {
                return CourseResult<CourseItemDto>.Ok(MapCourse(response), "Curso atualizado com sucesso.");
            }

            return CourseResult<CourseItemDto>.Fail("Falha ao atualizar curso.");
        }
        catch (ApiException ex)
        {
            return HandleApiException<CourseItemDto>(ex);
        }
        catch (Exception ex)
        {
            return CourseResult<CourseItemDto>.Fail(MapExceptionMessage(ex));
        }
    }

    /// <inheritdoc />
    public async Task<CourseResult> DeleteAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return CourseResult.Fail("Identificador do curso é obrigatório.", statusCode: 400);
        }

        try
        {
            await _apiClient.Api.V1.Courses[id].DeleteAsync(cancellationToken: ct);
            return CourseResult.Ok("Curso excluído com sucesso.");
        }
        catch (ApiException ex)
        {
            return HandleApiException(ex);
        }
        catch (Exception ex)
        {
            return CourseResult.Fail(MapExceptionMessage(ex));
        }
    }

    private static CourseItemDto MapCourse(CourseResponse response)
    {
        var status = response.Status.HasValue && Enum.IsDefined(typeof(CourseStatus), response.Status.Value)
            ? (CourseStatus)response.Status.Value
            : CourseStatus.Active;

        return new CourseItemDto(
            Id: response.Id ?? string.Empty,
            Name: response.Name ?? string.Empty,
            Code: response.Code ?? string.Empty,
            WorkloadHours: response.WorkloadHours ?? 0,
            Description: response.Description,
            SchoolUnitId: response.SchoolUnitId,
            TargetAudience: response.TargetAudience,
            Status: status,
            CreatedAt: response.CreatedAt,
            UpdatedAt: response.UpdatedAt,
            CreatedBy: response.CreatedBy,
            UpdatedBy: response.UpdatedBy);
    }

    private static CourseResult<T> HandleApiException<T>(ApiException ex)
    {
        var message = MapApiExceptionMessage(ex);
        return CourseResult<T>.Fail(message, statusCode: ex.ResponseStatusCode);
    }

    private static CourseResult HandleApiException(ApiException ex)
    {
        var message = MapApiExceptionMessage(ex);
        return CourseResult.Fail(message, statusCode: ex.ResponseStatusCode);
    }

    private static string MapApiExceptionMessage(ApiException ex)
    {
        if (ex is ProblemDetails pd && !string.IsNullOrWhiteSpace(pd.Detail))
        {
            return pd.Detail;
        }

        if (ex is ResponseBaseOfCourseResponse rbc && !string.IsNullOrWhiteSpace(rbc.MessageEscaped))
        {
            return rbc.MessageEscaped;
        }

        if (ex is ResponseBaseOfboolean rbb && !string.IsNullOrWhiteSpace(rbb.MessageEscaped))
        {
            return rbb.MessageEscaped;
        }

        return ex.ResponseStatusCode switch
        {
            400 => !string.IsNullOrWhiteSpace(ex.Message) && !ex.Message.Contains("Exception") && !ex.Message.StartsWith("The HTTP")
                ? ex.Message
                : "Requisição inválida. Verifique os dados informados para o curso.",
            401 => "Sessão expirada ou não autenticada (401). Faça login novamente para continuar.",
            403 => "Acesso negado: seu usuário não possui a permissão necessária para executar esta ação (403).",
            404 => "Curso não encontrado (404).",
            409 => "Conflito: o código do curso já está em uso ou existem registros vinculados a este curso (409).",
            422 => "Dados inválidos para o curso. Verifique os campos informados (422).",
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

