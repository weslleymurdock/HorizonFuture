using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HF.Client;
using HF.Shared.Auth;
using HF.Shared.Teachers;
using HF.Shared.Teachers.Models;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Xunit;

namespace HF.UITests.Shared;

public class SharedTeachersKiotaIntegrationTests
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

    private static (TeacherService service, TestHandler handler, InMemoryTokenStore tokenStore) CreateTestTeacherService()
    {
        var testHandler = new TestHandler();
        var tokenStore = new InMemoryTokenStore();
        var authProvider = new KiotaAuthenticationProvider(tokenStore);
        var adapter = new HttpClientRequestAdapter(authProvider, httpClient: new HttpClient(testHandler))
        {
            BaseUrl = "http://localhost:8080"
        };
        var apiClient = new ApiClient(adapter);
        var teacherService = new TeacherService(apiClient);
        return (teacherService, testHandler, tokenStore);
    }

    #region GET /api/v1/teachers (List)

    [Fact]
    public async Task ListAsync_Success_ReturnsMappedTeachersList()
    {
        var (service, handler, _) = CreateTestTeacherService();

        handler.Handler = req =>
        {
            Assert.Equal(HttpMethod.Get, req.Method);
            Assert.Contains("/api/v1/teachers", req.RequestUri!.ToString());
            Assert.Contains("schoolUnitId=unit-1", req.RequestUri.Query);
            Assert.Contains("status=1", req.RequestUri.Query);
            Assert.Contains("specialty=Matematica", req.RequestUri.Query);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    [
                        {
                            "id": "tch-001",
                            "userId": "user-synth-1",
                            "employeeNumber": "EMP-101",
                            "specialty": "Matematica",
                            "academicDegree": "Licenciatura",
                            "bio": "Professor de matematica",
                            "schoolUnitId": "unit-1",
                            "hireDate": "2023-02-01",
                            "status": 1,
                            "createdAt": "2023-02-01T10:00:00Z",
                            "createdBy": "admin@horizon.local"
                        }
                    ]
                    """,
                    Encoding.UTF8,
                    "application/json")
            };
        };

        var filter = new TeacherFilterDto(SchoolUnitId: "unit-1", Status: TeacherStatus.Active, Specialty: "Matematica");
        var result = await service.ListAsync(filter);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);

        var item = result.Data[0];
        Assert.Equal("tch-001", item.Id);
        Assert.Equal("EMP-101", item.EmployeeNumber);
        Assert.Equal("Matematica", item.Specialty);
        Assert.Equal(TeacherStatus.Active, item.Status);
        Assert.Equal(new DateOnly(2023, 2, 1), item.HireDate);
    }

    [Fact]
    public async Task ListAsync_InjectsBearerAuthorizationHeader()
    {
        var (service, handler, tokenStore) = CreateTestTeacherService();
        await tokenStore.SaveTokensAsync("test-bearer-token-123", "refresh-token-456");

        handler.Handler = req =>
        {
            Assert.Equal("Bearer test-bearer-token-123", req.Headers.Authorization?.ToString());
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]", Encoding.UTF8, "application/json")
            };
        };

        var result = await service.ListAsync();
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task ListAsync_401Unauthorized_ReturnsFailedResult()
    {
        var (service, handler, _) = CreateTestTeacherService();

        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent(
                """
                {
                    "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
                    "title": "Unauthorized",
                    "status": 401,
                    "detail": "Token expirado ou inválido."
                }
                """,
                Encoding.UTF8,
                "application/problem+json")
        };

        var result = await service.ListAsync();

        Assert.False(result.Succeeded);
        Assert.Equal(401, result.StatusCode);
        Assert.Contains("Token expirado", result.Message);
    }

    [Fact]
    public async Task ListAsync_403Forbidden_ReturnsFailedResult()
    {
        var (service, handler, _) = CreateTestTeacherService();

        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(
                """
                {
                    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                    "title": "Forbidden",
                    "status": 403,
                    "detail": "Acesso negado: requer permissão Teachers.Read."
                }
                """,
                Encoding.UTF8,
                "application/problem+json")
        };

        var result = await service.ListAsync();

        Assert.False(result.Succeeded);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("Acesso negado", result.Message);
    }

    #endregion

    #region GET /api/v1/teachers/{id} (GetById)

    [Fact]
    public async Task GetByIdAsync_Success_ReturnsMappedTeacher()
    {
        var (service, handler, _) = CreateTestTeacherService();

        handler.Handler = req =>
        {
            Assert.Equal(HttpMethod.Get, req.Method);
            Assert.Equal("/api/v1/teachers/tch-123", req.RequestUri!.AbsolutePath);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {
                        "id": "tch-123",
                        "userId": "usr-555",
                        "employeeNumber": "EMP-555",
                        "specialty": "Física Geral",
                        "academicDegree": "Mestrado",
                        "bio": "Bio docente",
                        "schoolUnitId": "unit-central",
                        "hireDate": "2021-08-15",
                        "status": 2,
                        "createdAt": "2021-08-15T12:00:00Z",
                        "createdBy": "sysadmin"
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            };
        };

        var result = await service.GetByIdAsync("tch-123");

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("tch-123", result.Data.Id);
        Assert.Equal("EMP-555", result.Data.EmployeeNumber);
        Assert.Equal(TeacherStatus.OnLeave, result.Data.Status);
    }

    [Fact]
    public async Task GetByIdAsync_404NotFound_ReturnsFailedResult()
    {
        var (service, handler, _) = CreateTestTeacherService();

        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(
                """
                {
                    "success": false,
                    "message": "Teacher not found."
                }
                """,
                Encoding.UTF8,
                "application/json")
        };

        var result = await service.GetByIdAsync("tch-unknown");

        Assert.False(result.Succeeded);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("not found", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region POST /api/v1/teachers (Create)

    [Fact]
    public async Task CreateAsync_Success_ReturnsCreatedTeacher()
    {
        var (service, handler, _) = CreateTestTeacherService();

        handler.Handler = req =>
        {
            Assert.Equal(HttpMethod.Post, req.Method);
            Assert.Equal("/api/v1/teachers", req.RequestUri!.AbsolutePath);

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(
                    """
                    {
                        "id": "tch-new-1",
                        "userId": "usr-1",
                        "employeeNumber": "EMP-900",
                        "specialty": "Química",
                        "academicDegree": "Licenciatura",
                        "status": 1,
                        "createdAt": "2024-01-01T00:00:00Z",
                        "createdBy": "admin@horizon.local"
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            };
        };

        var request = new CreateTeacherDto("usr-1", "EMP-900", "Química");
        var result = await service.CreateAsync(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("tch-new-1", result.Data.Id);
        Assert.Equal("Química", result.Data.Specialty);
    }

    [Fact]
    public async Task CreateAsync_400BadRequest_ReturnsFailedResult()
    {
        var (service, handler, _) = CreateTestTeacherService();

        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(
                """
                {
                    "success": false,
                    "message": "The employee number is already in use."
                }
                """,
                Encoding.UTF8,
                "application/json")
        };

        var request = new CreateTeacherDto("usr-1", "EMP-EXISTING", "Química");
        var result = await service.CreateAsync(request);

        Assert.False(result.Succeeded);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("already in use", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_422UnprocessableEntity_ReturnsFailedResult()
    {
        var (service, handler, _) = CreateTestTeacherService();

        handler.Handler = _ => new HttpResponseMessage((HttpStatusCode)422)
        {
            Content = new StringContent(
                """
                {
                    "type": "https://tools.ietf.org/html/rfc4918#section-11.2",
                    "title": "Unprocessable Entity",
                    "status": 422,
                    "detail": "Employee number cannot exceed 50 characters."
                }
                """,
                Encoding.UTF8,
                "application/problem+json")
        };

        var request = new CreateTeacherDto("usr-1", new string('X', 55), "Química");
        var result = await service.CreateAsync(request);

        Assert.False(result.Succeeded);
        Assert.Equal(422, result.StatusCode);
        Assert.Contains("cannot exceed 50 characters", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region PUT /api/v1/teachers/{id} (Update)

    [Fact]
    public async Task UpdateAsync_Success_ReturnsUpdatedTeacher()
    {
        var (service, handler, _) = CreateTestTeacherService();

        handler.Handler = req =>
        {
            Assert.Equal(HttpMethod.Put, req.Method);
            Assert.Equal("/api/v1/teachers/tch-99", req.RequestUri!.AbsolutePath);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {
                        "id": "tch-99",
                        "userId": "usr-1",
                        "employeeNumber": "EMP-99-UPDATED",
                        "specialty": "História Contemporânea",
                        "status": 1
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            };
        };

        var request = new UpdateTeacherDto("tch-99", "EMP-99-UPDATED", "História Contemporânea");
        var result = await service.UpdateAsync(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("EMP-99-UPDATED", result.Data.EmployeeNumber);
    }

    [Fact]
    public async Task UpdateAsync_404NotFound_ReturnsFailedResult()
    {
        var (service, handler, _) = CreateTestTeacherService();

        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(
                """
                {
                    "success": false,
                    "message": "Teacher not found."
                }
                """,
                Encoding.UTF8,
                "application/json")
        };

        var request = new UpdateTeacherDto("tch-missing", "EMP-000", "Specialty");
        var result = await service.UpdateAsync(request);

        Assert.False(result.Succeeded);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("not found", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region DELETE /api/v1/teachers/{id} (Delete)

    [Fact]
    public async Task DeleteAsync_Success_ReturnsSuccessResult()
    {
        var (service, handler, _) = CreateTestTeacherService();

        handler.Handler = req =>
        {
            Assert.Equal(HttpMethod.Delete, req.Method);
            Assert.Equal("/api/v1/teachers/tch-to-del", req.RequestUri!.AbsolutePath);
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        };

        var result = await service.DeleteAsync("tch-to-del");

        Assert.True(result.Succeeded);
        Assert.Equal("Professor excluído com sucesso.", result.Message);
    }

    [Fact]
    public async Task DeleteAsync_409Conflict_ReturnsConflictMessage()
    {
        var (service, handler, _) = CreateTestTeacherService();

        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.Conflict)
        {
            Content = new StringContent(
                """
                {
                    "success": false,
                    "message": "Cannot delete teacher with active assignments or attendances."
                }
                """,
                Encoding.UTF8,
                "application/json")
        };

        var result = await service.DeleteAsync("tch-with-assignments");

        Assert.False(result.Succeeded);
        Assert.Equal(409, result.StatusCode);
        Assert.Contains("active assignments", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}

