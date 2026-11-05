using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HF.Client;
using HF.Shared.Auth;
using HF.Shared.Subjects;
using HF.Shared.Subjects.Models;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Xunit;

namespace HF.UITests.Shared;

public class SharedSubjectsKiotaIntegrationTests
{
    private class TestHandler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, HttpResponseMessage> Handler { get; set; } =
            _ => new HttpResponseMessage(HttpStatusCode.OK);

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Handler(request));
        }
    }

    private static (SubjectService service, TestHandler handler) CreateTestSubjectService()
    {
        var testHandler = new TestHandler();
        var tokenStore = new InMemoryTokenStore();
        var authProvider = new KiotaAuthenticationProvider(tokenStore);
        var adapter = new HttpClientRequestAdapter(authProvider, httpClient: new HttpClient(testHandler))
        {
            BaseUrl = "http://localhost:8080"
        };
        var apiClient = new ApiClient(adapter);
        var subjectService = new SubjectService(apiClient);
        return (subjectService, testHandler);
    }

    #region GET /api/v1/subjects (List)

    [Fact]
    public async Task ListAsync_Success_ReturnsMappedSubjectsList()
    {
        var (service, handler) = CreateTestSubjectService();

        handler.Handler = req =>
        {
            Assert.Equal(HttpMethod.Get, req.Method);
            Assert.Contains("/api/v1/subjects", req.RequestUri!.ToString());
            Assert.Contains("courseId=c-001", req.RequestUri.Query);
            Assert.Contains("schoolUnitId=unit-1", req.RequestUri.Query);
            Assert.Contains("status=1", req.RequestUri.Query);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    [
                        {
                            "id": "s-001",
                            "name": "Matemática Financeira",
                            "code": "MAT-FIN-01",
                            "workloadHours": 40,
                            "description": "Juros simples e compostos",
                            "schoolUnitId": "unit-1",
                            "courseId": "c-001",
                            "status": 1,
                            "createdAt": "2026-01-01T10:00:00Z",
                            "createdBy": "admin@horizon.org"
                        }
                    ]
                    """,
                    Encoding.UTF8,
                    "application/json")
            };
        };

        var filter = new SubjectFilterDto("c-001", "unit-1", SubjectStatus.Active);
        var result = await service.ListAsync(filter);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);

        var item = result.Data[0];
        Assert.Equal("s-001", item.Id);
        Assert.Equal("Matemática Financeira", item.Name);
        Assert.Equal("MAT-FIN-01", item.Code);
        Assert.Equal(40, item.WorkloadHours);
        Assert.Equal("c-001", item.CourseId);
        Assert.Equal(SubjectStatus.Active, item.Status);
        Assert.Equal("unit-1", item.SchoolUnitId);
    }

    [Fact]
    public async Task ListAsync_401Unauthorized_MapsToAuthErrorMessage()
    {
        var (service, handler) = CreateTestSubjectService();

        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent(
                """
                {
                    "title": "Unauthorized",
                    "status": 401,
                    "detail": "Token expired or invalid"
                }
                """,
                Encoding.UTF8,
                "application/problem+json")
        };

        var result = await service.ListAsync();

        Assert.False(result.Succeeded);
        Assert.Equal(401, result.StatusCode);
        Assert.Contains("Token expired or invalid", result.Message);
    }

    [Fact]
    public async Task ListAsync_403Forbidden_MapsToForbiddenErrorMessage()
    {
        var (service, handler) = CreateTestSubjectService();

        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(
                """
                {
                    "title": "Forbidden",
                    "status": 403,
                    "detail": "Missing Subjects.Read permission"
                }
                """,
                Encoding.UTF8,
                "application/problem+json")
        };

        var result = await service.ListAsync();

        Assert.False(result.Succeeded);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("Missing Subjects.Read permission", result.Message);
    }

    #endregion

    #region GET /api/v1/subjects/{id} (GetById)

    [Fact]
    public async Task GetByIdAsync_Success_ReturnsMappedSubject()
    {
        var (service, handler) = CreateTestSubjectService();

        handler.Handler = req =>
        {
            Assert.Equal(HttpMethod.Get, req.Method);
            Assert.Contains("/api/v1/subjects/s-123", req.RequestUri!.ToString());

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {
                        "id": "s-123",
                        "name": "Introdução a Redes",
                        "code": "RED-01",
                        "workloadHours": 60,
                        "description": "Topologias e protocolos TCP/IP",
                        "schoolUnitId": "unit-2",
                        "courseId": "c-002",
                        "status": 2,
                        "createdAt": "2026-02-01T10:00:00Z",
                        "createdBy": "prof@horizon.org"
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            };
        };

        var result = await service.GetByIdAsync("s-123");

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("s-123", result.Data.Id);
        Assert.Equal("Introdução a Redes", result.Data.Name);
        Assert.Equal("RED-01", result.Data.Code);
        Assert.Equal(SubjectStatus.Inactive, result.Data.Status);
    }

    [Fact]
    public async Task GetByIdAsync_404NotFound_ReturnsNotFoundResult()
    {
        var (service, handler) = CreateTestSubjectService();

        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(
                """
                {
                    "success": false,
                    "message": "Subject not found",
                    "value": null
                }
                """,
                Encoding.UTF8,
                "application/json")
        };

        var result = await service.GetByIdAsync("s-inexistent");

        Assert.False(result.Succeeded);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Subject not found", result.Message);
    }

    [Fact]
    public async Task GetByIdAsync_EmptyId_ReturnsBadRequestWithoutHttpCall()
    {
        var (service, _) = CreateTestSubjectService();

        var result = await service.GetByIdAsync("");

        Assert.False(result.Succeeded);
        Assert.Equal(400, result.StatusCode);
    }

    #endregion

    #region POST /api/v1/subjects (Create)

    [Fact]
    public async Task CreateAsync_Success_SendsCorrectPayloadAndReturnsSubject()
    {
        var (service, handler) = CreateTestSubjectService();

        handler.Handler = req =>
        {
            Assert.Equal(HttpMethod.Post, req.Method);
            Assert.Contains("/api/v1/subjects", req.RequestUri!.ToString());

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(
                    """
                    {
                        "id": "s-999",
                        "name": "Banco de Dados Relacional",
                        "code": "BD-01",
                        "workloadHours": 50,
                        "description": "SQL e modelagem ER",
                        "schoolUnitId": "unit-1",
                        "courseId": "c-001",
                        "status": 1,
                        "createdAt": "2026-03-01T12:00:00Z",
                        "createdBy": "admin@horizon.org"
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            };
        };

        var request = new CreateSubjectDto(
            Name: "Banco de Dados Relacional",
            Code: "BD-01",
            WorkloadHours: 50,
            Description: "SQL e modelagem ER",
            SchoolUnitId: "unit-1",
            CourseId: "c-001",
            Status: SubjectStatus.Active);

        var result = await service.CreateAsync(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("s-999", result.Data.Id);
        Assert.Equal("Banco de Dados Relacional", result.Data.Name);
        Assert.Equal("BD-01", result.Data.Code);
    }

    [Fact]
    public async Task CreateAsync_400BadRequest_ReturnsErrorMessage()
    {
        var (service, handler) = CreateTestSubjectService();

        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(
                """
                {
                    "success": false,
                    "message": "Subject code is already in use.",
                    "value": null
                }
                """,
                Encoding.UTF8,
                "application/json")
        };

        var request = new CreateSubjectDto("Disciplina", "DUP-01", 40);
        var result = await service.CreateAsync(request);

        Assert.False(result.Succeeded);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("Subject code is already in use.", result.Message);
    }

    [Fact]
    public async Task CreateAsync_422Unprocessable_ExtractsProblemDetail()
    {
        var (service, handler) = CreateTestSubjectService();

        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.UnprocessableEntity)
        {
            Content = new StringContent(
                """
                {
                    "title": "Unprocessable Entity",
                    "status": 422,
                    "detail": "Workload hours must be greater than zero."
                }
                """,
                Encoding.UTF8,
                "application/problem+json")
        };

        var request = new CreateSubjectDto("Disciplina", "COD-01", 0);
        var result = await service.CreateAsync(request);

        Assert.False(result.Succeeded);
        Assert.Equal(422, result.StatusCode);
        Assert.Contains("Workload hours must be greater than zero.", result.Message);
    }

    #endregion

    #region PUT /api/v1/subjects/{id} (Update)

    [Fact]
    public async Task UpdateAsync_Success_SendsPutAndReturnsUpdatedSubject()
    {
        var (service, handler) = CreateTestSubjectService();

        handler.Handler = req =>
        {
            Assert.Equal(HttpMethod.Put, req.Method);
            Assert.Contains("/api/v1/subjects/s-123", req.RequestUri!.ToString());

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {
                        "id": "s-123",
                        "name": "Banco de Dados Avançado",
                        "code": "BD-02",
                        "workloadHours": 80,
                        "description": "Tuning e indexação",
                        "schoolUnitId": "unit-1",
                        "courseId": "c-001",
                        "status": 1,
                        "createdAt": "2026-03-01T12:00:00Z",
                        "updatedAt": "2026-03-02T15:00:00Z",
                        "createdBy": "admin@horizon.org",
                        "updatedBy": "coord@horizon.org"
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            };
        };

        var request = new UpdateSubjectDto("s-123", "Banco de Dados Avançado", "BD-02", 80);
        var result = await service.UpdateAsync(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("Banco de Dados Avançado", result.Data.Name);
        Assert.Equal(80, result.Data.WorkloadHours);
    }

    [Fact]
    public async Task UpdateAsync_404NotFound_ReturnsNotFoundResult()
    {
        var (service, handler) = CreateTestSubjectService();

        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(
                """
                {
                    "success": false,
                    "message": "Subject not found",
                    "value": null
                }
                """,
                Encoding.UTF8,
                "application/json")
        };

        var request = new UpdateSubjectDto("s-999", "Nome", "COD", 40);
        var result = await service.UpdateAsync(request);

        Assert.False(result.Succeeded);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Subject not found", result.Message);
    }

    #endregion

    #region DELETE /api/v1/subjects/{id} (Delete)

    [Fact]
    public async Task DeleteAsync_Success_SendsDeleteAndReturnsOk()
    {
        var (service, handler) = CreateTestSubjectService();

        handler.Handler = req =>
        {
            Assert.Equal(HttpMethod.Delete, req.Method);
            Assert.Contains("/api/v1/subjects/s-123", req.RequestUri!.ToString());

            return new HttpResponseMessage(HttpStatusCode.NoContent);
        };

        var result = await service.DeleteAsync("s-123");

        Assert.True(result.Succeeded);
        Assert.Contains("excluída com sucesso", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeleteAsync_404NotFound_ReturnsNotFoundResult()
    {
        var (service, handler) = CreateTestSubjectService();

        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(
                """
                {
                    "success": false,
                    "message": "Subject not found",
                    "value": false
                }
                """,
                Encoding.UTF8,
                "application/json")
        };

        var result = await service.DeleteAsync("s-inexistent");

        Assert.False(result.Succeeded);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Subject not found", result.Message);
    }

    [Fact]
    public async Task DeleteAsync_409Conflict_ReturnsConflictMessage()
    {
        var (service, handler) = CreateTestSubjectService();

        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.Conflict)
        {
            Content = new StringContent(
                """
                {
                    "success": false,
                    "message": "Cannot delete subject associated with active classes.",
                    "value": false
                }
                """,
                Encoding.UTF8,
                "application/json")
        };

        var result = await service.DeleteAsync("s-in-use");

        Assert.False(result.Succeeded);
        Assert.Equal(409, result.StatusCode);
        Assert.Contains("Cannot delete subject associated with active classes.", result.Message);
    }

    #endregion
}
