using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HF.Client;
using HF.Client.Models;
using HF.Shared.Enrollments.Models;
using Microsoft.Kiota.Abstractions;

namespace HF.Shared.Enrollments;

/// <summary>
/// Shared service centralizing REST consumption of Enrollments endpoints
/// using the Kiota client in HF.Client, adapting responses for PageModels and UI.
/// </summary>
public class EnrollmentService : IEnrollmentService
{
    private readonly ApiClient _apiClient;

    public EnrollmentService(ApiClient apiClient)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }

    /// <inheritdoc />
    public async Task<EnrollmentResult<IReadOnlyList<EnrollmentItemDto>>> ListAsync(EnrollmentFilterDto? filter = null, CancellationToken ct = default)
    {
        try
        {
            var items = await _apiClient.Api.V1.Enrollments.GetAsync(q =>
            {
                if (!string.IsNullOrWhiteSpace(filter?.StudentId))
                {
                    q.QueryParameters.StudentId = filter.StudentId;
                }
                if (!string.IsNullOrWhiteSpace(filter?.SubjectId))
                {
                    q.QueryParameters.SubjectId = filter.SubjectId;
                }
                if (filter?.AcademicYear.HasValue == true)
                {
                    q.QueryParameters.AcademicYear = filter.AcademicYear.Value;
                }
                if (filter?.AcademicSemester.HasValue == true)
                {
                    q.QueryParameters.AcademicSemester = filter.AcademicSemester.Value;
                }
                if (filter?.Status.HasValue == true)
                {
                    q.QueryParameters.Status = (int)filter.Status.Value;
                }
            }, cancellationToken: ct);

            var result = (items ?? new List<EnrollmentResponse>())
                .Select(MapEnrollment)
                .ToList();

            return EnrollmentResult<IReadOnlyList<EnrollmentItemDto>>.Ok(result, "Matrículas listadas com sucesso.");
        }
        catch (ApiException ex)
        {
            return HandleApiException<IReadOnlyList<EnrollmentItemDto>>(ex);
        }
        catch (Exception ex)
        {
            return EnrollmentResult<IReadOnlyList<EnrollmentItemDto>>.Fail(MapExceptionMessage(ex));
        }
    }

    /// <inheritdoc />
    public async Task<EnrollmentResult<EnrollmentItemDto>> GetByIdAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return EnrollmentResult<EnrollmentItemDto>.Fail("Identificador da matrícula é obrigatório.", statusCode: 400);
        }

        try
        {
            var response = await _apiClient.Api.V1.Enrollments[id].GetAsync(cancellationToken: ct);
            if (response is not null)
            {
                return EnrollmentResult<EnrollmentItemDto>.Ok(MapEnrollment(response), "Matrícula carregada com sucesso.");
            }

            return EnrollmentResult<EnrollmentItemDto>.Fail("Matrícula não encontrada.", statusCode: 404);
        }
        catch (ApiException ex)
        {
            return HandleApiException<EnrollmentItemDto>(ex);
        }
        catch (Exception ex)
        {
            return EnrollmentResult<EnrollmentItemDto>.Fail(MapExceptionMessage(ex));
        }
    }

    /// <inheritdoc />
    public async Task<EnrollmentResult<EnrollmentItemDto>> CreateAsync(CreateEnrollmentDto request, CancellationToken ct = default)
    {
        if (request is null)
        {
            return EnrollmentResult<EnrollmentItemDto>.Fail("Dados para cadastro da matrícula são obrigatórios.", statusCode: 400);
        }

        try
        {
            var kiotaRequest = new CreateEnrollmentRequest
            {
                StudentId = request.StudentId,
                SubjectId = request.SubjectId,
                AcademicYear = request.AcademicYear,
                AcademicSemester = request.AcademicSemester,
                EnrollmentDate = request.EnrollmentDate.HasValue
                    ? new Date(request.EnrollmentDate.Value.Year, request.EnrollmentDate.Value.Month, request.EnrollmentDate.Value.Day)
                    : null,
                Status = (int)request.Status,
                FinalGrade = request.FinalGrade.HasValue ? (double)request.FinalGrade.Value : null
            };

            var response = await _apiClient.Api.V1.Enrollments.PostAsync(kiotaRequest, cancellationToken: ct);
            if (response is not null)
            {
                return EnrollmentResult<EnrollmentItemDto>.Ok(MapEnrollment(response), "Matrícula realizada com sucesso.");
            }

            return EnrollmentResult<EnrollmentItemDto>.Fail("Falha ao registrar matrícula.");
        }
        catch (ApiException ex)
        {
            return HandleApiException<EnrollmentItemDto>(ex);
        }
        catch (Exception ex)
        {
            return EnrollmentResult<EnrollmentItemDto>.Fail(MapExceptionMessage(ex));
        }
    }

    /// <inheritdoc />
    public async Task<EnrollmentResult<EnrollmentItemDto>> UpdateAsync(UpdateEnrollmentDto request, CancellationToken ct = default)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Id))
        {
            return EnrollmentResult<EnrollmentItemDto>.Fail("Identificador e dados da matrícula são obrigatórios.", statusCode: 400);
        }

        try
        {
            var kiotaRequest = new UpdateEnrollmentRequest
            {
                Id = request.Id,
                StudentId = request.StudentId,
                SubjectId = request.SubjectId,
                AcademicYear = request.AcademicYear,
                AcademicSemester = request.AcademicSemester,
                EnrollmentDate = new Date(request.EnrollmentDate.Year, request.EnrollmentDate.Month, request.EnrollmentDate.Day),
                Status = (int)request.Status,
                FinalGrade = request.FinalGrade.HasValue ? (double)request.FinalGrade.Value : null
            };

            var response = await _apiClient.Api.V1.Enrollments[request.Id].PutAsync(kiotaRequest, cancellationToken: ct);
            if (response is not null)
            {
                return EnrollmentResult<EnrollmentItemDto>.Ok(MapEnrollment(response), "Matrícula atualizada com sucesso.");
            }

            return EnrollmentResult<EnrollmentItemDto>.Fail("Falha ao atualizar matrícula.");
        }
        catch (ApiException ex)
        {
            return HandleApiException<EnrollmentItemDto>(ex);
        }
        catch (Exception ex)
        {
            return EnrollmentResult<EnrollmentItemDto>.Fail(MapExceptionMessage(ex));
        }
    }

    /// <inheritdoc />
    public async Task<EnrollmentResult> DeleteAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return EnrollmentResult.Fail("Identificador da matrícula é obrigatório.", statusCode: 400);
        }

        try
        {
            await _apiClient.Api.V1.Enrollments[id].DeleteAsync(cancellationToken: ct);
            return EnrollmentResult.Ok("Matrícula excluída com sucesso.");
        }
        catch (ApiException ex)
        {
            return HandleApiException(ex);
        }
        catch (Exception ex)
        {
            return EnrollmentResult.Fail(MapExceptionMessage(ex));
        }
    }

    private static EnrollmentItemDto MapEnrollment(EnrollmentResponse response)
    {
        var status = response.Status.HasValue && Enum.IsDefined(typeof(EnrollmentStatus), response.Status.Value)
            ? (EnrollmentStatus)response.Status.Value
            : EnrollmentStatus.Active;

        var enrollmentDate = response.EnrollmentDate.HasValue
            ? new DateOnly(response.EnrollmentDate.Value.Year, response.EnrollmentDate.Value.Month, response.EnrollmentDate.Value.Day)
            : DateOnly.FromDateTime(DateTime.UtcNow);

        return new EnrollmentItemDto(
            Id: response.Id ?? string.Empty,
            StudentId: response.StudentId ?? string.Empty,
            SubjectId: response.SubjectId ?? string.Empty,
            AcademicYear: response.AcademicYear ?? DateTime.UtcNow.Year,
            AcademicSemester: response.AcademicSemester ?? 1,
            EnrollmentDate: enrollmentDate,
            Status: status,
            FinalGrade: response.FinalGrade.HasValue ? (decimal)response.FinalGrade.Value : null,
            CreatedAt: response.CreatedAt,
            UpdatedAt: response.UpdatedAt,
            CreatedBy: response.CreatedBy,
            UpdatedBy: response.UpdatedBy);
    }

    private static EnrollmentResult<T> HandleApiException<T>(ApiException ex)
    {
        var message = MapApiExceptionMessage(ex);
        return EnrollmentResult<T>.Fail(message, statusCode: ex.ResponseStatusCode);
    }

    private static EnrollmentResult HandleApiException(ApiException ex)
    {
        var message = MapApiExceptionMessage(ex);
        return EnrollmentResult.Fail(message, statusCode: ex.ResponseStatusCode);
    }

    private static string MapApiExceptionMessage(ApiException ex)
    {
        return ex.ResponseStatusCode switch
        {
            400 => !string.IsNullOrWhiteSpace(ex.Message) && !ex.Message.Contains("Exception") && !ex.Message.StartsWith("The HTTP")
                ? ex.Message
                : "Requisição inválida. Verifique os dados da matrícula.",
            401 => "Sessão expirada ou não autenticada (401). Faça login novamente para continuar.",
            403 => "Acesso negado: seu usuário não possui permissão para executar esta ação em matrículas (403).",
            404 => "Matrícula não encontrada (404).",
            409 => "Conflito: a operação não pôde ser concluída devido a um conflito de dados na matrícula (409).",
            422 => "Entidade não processável: os dados informados não satisfazem as regras de negócio de matrícula (422).",
            429 => "Muitas requisições. Aguarde alguns instantes antes de tentar novamente (429).",
            >= 500 => "Ocorreu um erro interno no servidor (500). Tente novamente mais tarde.",
            _ => !string.IsNullOrWhiteSpace(ex.Message) && !ex.Message.Contains("Exception") && !ex.Message.StartsWith("The HTTP")
                ? ex.Message
                : "Falha na comunicação com o servidor de matrículas."
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

