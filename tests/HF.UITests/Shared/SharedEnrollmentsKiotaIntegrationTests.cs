using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HF.Client;
using HF.Shared.Auth;
using HF.Shared.Enrollments;
using HF.Shared.Enrollments.Models;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Xunit;

namespace HF.UITests.Shared;

public class SharedEnrollmentsKiotaIntegrationTests
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

    private static (EnrollmentService service, TestHandler handler) CreateTestEnrollmentService()
    {
        var testHandler = new TestHandler();
        var tokenStore = new InMemoryTokenStore();
        var authProvider = new KiotaAuthenticationProvider(tokenStore);
        var adapter = new HttpClientRequestAdapter(authProvider, httpClient: new HttpClient(testHandler))
        {
            BaseUrl = "http://localhost:8080"
        };
        var apiClient = new ApiClient(adapter);
        var enrollmentService = new EnrollmentService(apiClient);
        return (enrollmentService, testHandler);
    }

    #region GET /api/v1/enrollments (List)

    [Fact]
    public async Task ListAsync_Success_ReturnsMappedEnrollmentsList()
    {
        var (service, handler) = CreateTestEnrollmentService();

        handler.Handler = req =>
        {
            Assert.Equal(HttpMethod.Get, req.Method);
            Assert.Contains("/api/v1/enrollments", req.RequestUri!.ToString());
            Assert.Contains("studentId=stu-1", req.RequestUri.Query);
            Assert.Contains("subjectId=sub-1", req.RequestUri.Query);
            Assert.Contains("academicYear=2026", req.RequestUri.Query);
            Assert.Contains("academicSemester=1", req.RequestUri.Query);
            Assert.Contains("status=1", req.RequestUri.Query);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    [
                        {
                            "id": "enr-001",
                            "studentId": "stu-1",
                            "subjectId": "sub-1",
                            "academicYear": 2026,
                            "academicSemester": 1,
                            "enrollmentDate": "2026-02-15",
                            "status": 1,
                            "finalGrade": 9.5,
                            "createdAt": "2026-02-15T08:00:00Z",
                            "createdBy": "coordinator@horizon.org"
                        }
                    ]
                    """,
                    Encoding.UTF8,
                    "application/json")
            };
        };

        var filter = new EnrollmentFilterDto("stu-1", "sub-1", 2026, 1, EnrollmentStatus.Active);
        var result = await service.ListAsync(filter);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);

        var item = result.Data[0];
        Assert.Equal("enr-001", item.Id);
        Assert.Equal("stu-1", item.StudentId);
        Assert.Equal("sub-1", item.SubjectId);
        Assert.Equal(2026, item.AcademicYear);
        Assert.Equal(1, item.AcademicSemester);
        Assert.Equal(new DateOnly(2026, 2, 15), item.EnrollmentDate);
        Assert.Equal(EnrollmentStatus.Active, item.Status);
        Assert.Equal(9.5m, item.FinalGrade);
    }

    [Fact]
    public async Task ListAsync_401Unauthorized_MapsToAuthErrorMessage()
    {
        var (service, handler) = CreateTestEnrollmentService();

        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.Unauthorized);

        var result = await service.ListAsync();

        Assert.False(result.Succeeded);
        Assert.Equal(401, result.StatusCode);
        Assert.Contains("Sessão expirada", result.Message);
    }

    [Fact]
    public async Task ListAsync_403Forbidden_MapsToForbiddenMessage()
    {
        var (service, handler) = CreateTestEnrollmentService();

        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.Forbidden);

        var result = await service.ListAsync();

        Assert.False(result.Succeeded);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("Acesso negado", result.Message);
    }

    #endregion

    #region GET /api/v1/enrollments/{id} (GetById)

    [Fact]
    public async Task GetByIdAsync_Success_ReturnsEnrollmentDetails()
    {
        var (service, handler) = CreateTestEnrollmentService();

        handler.Handler = req =>
        {
            Assert.Equal(HttpMethod.Get, req.Method);
            Assert.EndsWith("/api/v1/enrollments/enr-123", req.RequestUri!.AbsolutePath);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {
                        "id": "enr-123",
                        "studentId": "stu-1",
                        "subjectId": "sub-2",
                        "academicYear": 2026,
                        "academicSemester": 2,
                        "enrollmentDate": "2026-08-01",
                        "status": 3,
                        "finalGrade": 8.0,
                        "createdAt": "2026-08-01T10:00:00Z"
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            };
        };

        var result = await service.GetByIdAsync("enr-123");

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("enr-123", result.Data.Id);
        Assert.Equal(EnrollmentStatus.Completed, result.Data.Status);
        Assert.Equal(8.0m, result.Data.FinalGrade);
    }

    [Fact]
    public async Task GetByIdAsync_404NotFound_ReturnsNotFoundMessage()
    {
        var (service, handler) = CreateTestEnrollmentService();

        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.NotFound);

        var result = await service.GetByIdAsync("nonexistent");

        Assert.False(result.Succeeded);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("não encontrada", result.Message);
    }

    #endregion

    #region POST /api/v1/enrollments (Create)

    [Fact]
    public async Task CreateAsync_Success_ReturnsCreatedEnrollment()
    {
        var (service, handler) = CreateTestEnrollmentService();

        handler.Handler = req =>
        {
            Assert.Equal(HttpMethod.Post, req.Method);
            Assert.EndsWith("/api/v1/enrollments", req.RequestUri!.AbsolutePath);

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(
                    """
                    {
                        "id": "enr-new-999",
                        "studentId": "stu-1",
                        "subjectId": "sub-1",
                        "academicYear": 2026,
                        "academicSemester": 1,
                        "enrollmentDate": "2026-02-15",
                        "status": 1,
                        "finalGrade": null,
                        "createdAt": "2026-02-15T09:00:00Z"
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            };
        };

        var dto = new CreateEnrollmentDto(
            StudentId: "stu-1",
            SubjectId: "sub-1",
            AcademicYear: 2026,
            AcademicSemester: 1,
            EnrollmentDate: new DateOnly(2026, 2, 15),
            Status: EnrollmentStatus.Active,
            FinalGrade: null);

        var result = await service.CreateAsync(dto);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("enr-new-999", result.Data.Id);
        Assert.Equal("stu-1", result.Data.StudentId);
    }

    [Fact]
    public async Task CreateAsync_400BadRequest_ReturnsErrorMessage()
    {
        var (service, handler) = CreateTestEnrollmentService();

        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("{\"message\":\"O estudante já está matriculado nesta disciplina no período especificado.\"}", Encoding.UTF8, "application/json")
        };

        var dto = new CreateEnrollmentDto("stu-1", "sub-1", 2026, 1);
        var result = await service.CreateAsync(dto);

        Assert.False(result.Succeeded);
        Assert.Equal(400, result.StatusCode);
    }

    #endregion

    #region PUT /api/v1/enrollments/{id} (Update)

    [Fact]
    public async Task UpdateAsync_Success_ReturnsUpdatedEnrollment()
    {
        var (service, handler) = CreateTestEnrollmentService();

        handler.Handler = req =>
        {
            Assert.Equal(HttpMethod.Put, req.Method);
            Assert.EndsWith("/api/v1/enrollments/enr-001", req.RequestUri!.AbsolutePath);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {
                        "id": "enr-001",
                        "studentId": "stu-1",
                        "subjectId": "sub-1",
                        "academicYear": 2026,
                        "academicSemester": 1,
                        "enrollmentDate": "2026-02-15",
                        "status": 3,
                        "finalGrade": 10.0,
                        "updatedAt": "2026-11-05T12:00:00Z"
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            };
        };

        var dto = new UpdateEnrollmentDto(
            Id: "enr-001",
            StudentId: "stu-1",
            SubjectId: "sub-1",
            AcademicYear: 2026,
            AcademicSemester: 1,
            EnrollmentDate: new DateOnly(2026, 2, 15),
            Status: EnrollmentStatus.Completed,
            FinalGrade: 10.0m);

        var result = await service.UpdateAsync(dto);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(EnrollmentStatus.Completed, result.Data.Status);
        Assert.Equal(10.0m, result.Data.FinalGrade);
    }

    #endregion

    #region DELETE /api/v1/enrollments/{id} (Delete)

    [Fact]
    public async Task DeleteAsync_Success_ReturnsSucceededResult()
    {
        var (service, handler) = CreateTestEnrollmentService();

        handler.Handler = req =>
        {
            Assert.Equal(HttpMethod.Delete, req.Method);
            Assert.EndsWith("/api/v1/enrollments/enr-delete-1", req.RequestUri!.AbsolutePath);

            return new HttpResponseMessage(HttpStatusCode.NoContent);
        };

        var result = await service.DeleteAsync("enr-delete-1");

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task DeleteAsync_404NotFound_ReturnsFailureResult()
    {
        var (service, handler) = CreateTestEnrollmentService();

        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.NotFound);

        var result = await service.DeleteAsync("nonexistent");

        Assert.False(result.Succeeded);
        Assert.Equal(404, result.StatusCode);
    }

    #endregion
}

