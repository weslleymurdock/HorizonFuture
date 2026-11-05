using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HF.Client;
using HF.Client.Models;
using HF.Shared.Students.Models;
using Microsoft.Kiota.Abstractions;

namespace HF.Shared.Students;

/// <summary>
/// Shared service centralizing REST consumption of Students endpoints
/// using the Kiota client in HF.Client, adapting responses for PageModels and UI.
/// </summary>
public class StudentService : IStudentService
{
    private readonly ApiClient _apiClient;

    public StudentService(ApiClient apiClient)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }

    /// <inheritdoc />
    public async Task<StudentResult<IReadOnlyList<StudentItemDto>>> ListAsync(StudentFilterDto? filter = null, CancellationToken ct = default)
    {
        try
        {
            var items = await _apiClient.Api.V1.Students.GetAsync(q =>
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

            var result = (items ?? new List<StudentResponse>())
                .Select(MapStudent)
                .ToList();

            return StudentResult<IReadOnlyList<StudentItemDto>>.Ok(result, "Alunos listados com sucesso.");
        }
        catch (ApiException ex)
        {
            return HandleApiException<IReadOnlyList<StudentItemDto>>(ex);
        }
        catch (Exception ex)
        {
            return StudentResult<IReadOnlyList<StudentItemDto>>.Fail(MapExceptionMessage(ex));
        }
    }

    /// <inheritdoc />
    public async Task<StudentResult<StudentItemDto>> GetByIdAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return StudentResult<StudentItemDto>.Fail("Identificador do aluno é obrigatório.", statusCode: 400);
        }

        try
        {
            var response = await _apiClient.Api.V1.Students[id].GetAsync(cancellationToken: ct);
            if (response is not null)
            {
                return StudentResult<StudentItemDto>.Ok(MapStudent(response), "Aluno carregado com sucesso.");
            }

            return StudentResult<StudentItemDto>.Fail("Aluno não encontrado.", statusCode: 404);
        }
        catch (ApiException ex)
        {
            return HandleApiException<StudentItemDto>(ex);
        }
        catch (Exception ex)
        {
            return StudentResult<StudentItemDto>.Fail(MapExceptionMessage(ex));
        }
    }

    /// <inheritdoc />
    public async Task<StudentResult<StudentItemDto>> CreateAsync(CreateStudentDto request, CancellationToken ct = default)
    {
        if (request is null)
        {
            return StudentResult<StudentItemDto>.Fail("Dados para cadastro do aluno são obrigatórios.", statusCode: 400);
        }

        try
        {
            var kiotaRequest = new CreateStudentRequest
            {
                UserId = request.UserId,
                RegistrationNumber = request.RegistrationNumber,
                DateOfBirth = new Date(request.DateOfBirth.Year, request.DateOfBirth.Month, request.DateOfBirth.Day),
                GuardianName = request.GuardianName,
                GuardianRelationship = request.GuardianRelationship,
                GuardianPhone = request.GuardianPhone,
                EmergencyContactName = request.EmergencyContactName,
                EmergencyContactPhone = request.EmergencyContactPhone,
                SchoolUnitId = string.IsNullOrWhiteSpace(request.SchoolUnitId) ? null : request.SchoolUnitId,
                Status = (int)request.Status,
                SpecialNeeds = request.SpecialNeeds,
                MedicalNotes = request.MedicalNotes,
                EnrollmentDate = request.EnrollmentDate.HasValue
                    ? new Date(request.EnrollmentDate.Value.Year, request.EnrollmentDate.Value.Month, request.EnrollmentDate.Value.Day)
                    : null
            };

            var response = await _apiClient.Api.V1.Students.PostAsync(kiotaRequest, cancellationToken: ct);
            if (response is not null)
            {
                return StudentResult<StudentItemDto>.Ok(MapStudent(response), "Aluno cadastrado com sucesso.");
            }

            return StudentResult<StudentItemDto>.Fail("Falha ao registrar novo aluno.");
        }
        catch (ApiException ex)
        {
            return HandleApiException<StudentItemDto>(ex);
        }
        catch (Exception ex)
        {
            return StudentResult<StudentItemDto>.Fail(MapExceptionMessage(ex));
        }
    }

    /// <inheritdoc />
    public async Task<StudentResult<StudentItemDto>> UpdateAsync(UpdateStudentDto request, CancellationToken ct = default)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Id))
        {
            return StudentResult<StudentItemDto>.Fail("Identificador e dados do aluno são obrigatórios.", statusCode: 400);
        }

        try
        {
            var kiotaRequest = new UpdateStudentRequest
            {
                Id = request.Id,
                RegistrationNumber = request.RegistrationNumber,
                DateOfBirth = new Date(request.DateOfBirth.Year, request.DateOfBirth.Month, request.DateOfBirth.Day),
                GuardianName = request.GuardianName,
                GuardianRelationship = request.GuardianRelationship,
                GuardianPhone = request.GuardianPhone,
                EmergencyContactName = request.EmergencyContactName,
                EmergencyContactPhone = request.EmergencyContactPhone,
                SchoolUnitId = string.IsNullOrWhiteSpace(request.SchoolUnitId) ? null : request.SchoolUnitId,
                Status = (int)request.Status,
                SpecialNeeds = request.SpecialNeeds,
                MedicalNotes = request.MedicalNotes
            };

            var response = await _apiClient.Api.V1.Students[request.Id].PutAsync(kiotaRequest, cancellationToken: ct);
            if (response is not null)
            {
                return StudentResult<StudentItemDto>.Ok(MapStudent(response), "Aluno atualizado com sucesso.");
            }

            return StudentResult<StudentItemDto>.Fail("Falha ao atualizar aluno.");
        }
        catch (ApiException ex)
        {
            return HandleApiException<StudentItemDto>(ex);
        }
        catch (Exception ex)
        {
            return StudentResult<StudentItemDto>.Fail(MapExceptionMessage(ex));
        }
    }

    /// <inheritdoc />
    public async Task<StudentResult> DeleteAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return StudentResult.Fail("Identificador do aluno é obrigatório.", statusCode: 400);
        }

        try
        {
            await _apiClient.Api.V1.Students[id].DeleteAsync(cancellationToken: ct);
            return StudentResult.Ok("Aluno excluído com sucesso.");
        }
        catch (ApiException ex)
        {
            return HandleApiException(ex);
        }
        catch (Exception ex)
        {
            return StudentResult.Fail(MapExceptionMessage(ex));
        }
    }

    private static StudentItemDto MapStudent(StudentResponse response)
    {
        var status = response.Status.HasValue && Enum.IsDefined(typeof(StudentStatus), response.Status.Value)
            ? (StudentStatus)response.Status.Value
            : StudentStatus.Active;

        var dob = response.DateOfBirth.HasValue
            ? new DateOnly(response.DateOfBirth.Value.Year, response.DateOfBirth.Value.Month, response.DateOfBirth.Value.Day)
            : default;

        var enrollmentDate = response.EnrollmentDate.HasValue
            ? new DateOnly(response.EnrollmentDate.Value.Year, response.EnrollmentDate.Value.Month, response.EnrollmentDate.Value.Day)
            : DateOnly.FromDateTime(DateTime.UtcNow);

        return new StudentItemDto(
            Id: response.Id ?? string.Empty,
            UserId: response.UserId ?? string.Empty,
            RegistrationNumber: response.RegistrationNumber ?? string.Empty,
            DateOfBirth: dob,
            GuardianName: response.GuardianName,
            GuardianRelationship: response.GuardianRelationship,
            GuardianPhone: response.GuardianPhone,
            EmergencyContactName: response.EmergencyContactName,
            EmergencyContactPhone: response.EmergencyContactPhone,
            SchoolUnitId: response.SchoolUnitId,
            Status: status,
            SpecialNeeds: response.SpecialNeeds,
            MedicalNotes: response.MedicalNotes,
            EnrollmentDate: enrollmentDate,
            CreatedAt: response.CreatedAt,
            UpdatedAt: response.UpdatedAt,
            CreatedBy: response.CreatedBy,
            UpdatedBy: response.UpdatedBy);
    }

    private static StudentResult<T> HandleApiException<T>(ApiException ex)
    {
        var message = MapApiExceptionMessage(ex);
        return StudentResult<T>.Fail(message, statusCode: ex.ResponseStatusCode);
    }

    private static StudentResult HandleApiException(ApiException ex)
    {
        var message = MapApiExceptionMessage(ex);
        return StudentResult.Fail(message, statusCode: ex.ResponseStatusCode);
    }

    private static string MapApiExceptionMessage(ApiException ex)
    {
        if (ex is ProblemDetails pd && !string.IsNullOrWhiteSpace(pd.Detail))
        {
            return pd.Detail;
        }

        if (ex is ResponseBaseOfStudentResponse rbs && !string.IsNullOrWhiteSpace(rbs.MessageEscaped))
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
                : "Requisição inválida. Verifique os dados informados para o aluno.",
            401 => "Sessão expirada ou não autenticada (401). Faça login novamente para continuar.",
            403 => "Acesso negado: seu usuário não possui a permissão necessária para executar esta ação (403).",
            404 => "Aluno não encontrado (404).",
            409 => "Conflito: o número de matrícula já está em uso, usuário já vinculado ou existem matrículas vinculadas ao aluno (409).",
            422 => "Dados inválidos para o aluno. Verifique os campos informados (422).",
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
