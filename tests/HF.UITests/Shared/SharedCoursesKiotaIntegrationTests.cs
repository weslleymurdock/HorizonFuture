using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HF.Client;
using HF.Shared.Auth;
using HF.Shared.Courses;
using HF.Shared.Courses.Models;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Xunit;

namespace HF.UITests.Shared;

public class SharedCoursesKiotaIntegrationTests
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

    private static (CourseService service, TestHandler handler) CreateTestCourseService()
    {
        var testHandler = new TestHandler();
        var tokenStore = new InMemoryTokenStore();
        var authProvider = new KiotaAuthenticationProvider(tokenStore);
        var adapter = new HttpClientRequestAdapter(authProvider, httpClient: new HttpClient(testHandler))
        {
            BaseUrl = "http://localhost:8080"
        };
        var apiClient = new ApiClient(adapter);
        var courseService = new CourseService(apiClient);
        return (courseService, testHandler);
    }

    #region GET /api/v1/courses (List)

    [Fact]
    public async Task ListAsync_Success_ReturnsMappedCoursesList()
    {
        var (service, handler) = CreateTestCourseService();

        handler.Handler = req =>
        {
            Assert.Equal(HttpMethod.Get, req.Method);
            Assert.Contains("/api/v1/courses", req.RequestUri!.ToString());
            Assert.Contains("schoolUnitId=unit-1", req.RequestUri.Query);
            Assert.Contains("status=1", req.RequestUri.Query);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    [
                        {
                            "id": "c-001",
                            "name": "Matemática Básica",
                            "code": "MAT-01",
                            "workloadHours": 40,
                            "description": "Aritmética e álgebra",
                            "schoolUnitId": "unit-1",
                            "targetAudience": "Ensino Fundamental",
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

        var filter = new CourseFilterDto("unit-1", CourseStatus.Active);
        var result = await service.ListAsync(filter);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);

        var item = result.Data[0];
        Assert.Equal("c-001", item.Id);
        Assert.Equal("Matemática Básica", item.Name);
        Assert.Equal("MAT-01", item.Code);
        Assert.Equal(40, item.WorkloadHours);
        Assert.Equal(CourseStatus.Active, item.Status);
        Assert.Equal("unit-1", item.SchoolUnitId);
    }

    [Fact]
    public async Task ListAsync_401Unauthorized_MapsToAuthErrorMessage()
    {
        var (service, handler) = CreateTestCourseService();

        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent(
                """
                {
                    "title": "Unauthorized",
                    "status": 401,
                    "detail": "Token expirado ou inválido"
                }
                """,
                Encoding.UTF8,
                "application/problem+json")
        };

        var result = await service.ListAsync();

        Assert.False(result.Succeeded);
        Assert.Equal(401, result.StatusCode);
        Assert.Contains("Token expirado ou inválido", result.Message);
    }

    [Fact]
    public async Task ListAsync_403Forbidden_MapsToForbiddenErrorMessage()
    {
        var (service, handler) = CreateTestCourseService();

        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(
                """
                {
                    "title": "Forbidden",
                    "status": 403,
                    "detail": "Permissão Courses.Read ausente"
                }
                """,
                Encoding.UTF8,
                "application/problem+json")
        };

        var result = await service.ListAsync();

        Assert.False(result.Succeeded);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("Permissão Courses.Read ausente", result.Message);
    }

    #endregion

    #region GET /api/v1/courses/{id} (GetById)

    [Fact]
    public async Task GetByIdAsync_Success_ReturnsMappedCourse()
    {
        var (service, handler) = CreateTestCourseService();

        handler.Handler = req =>
        {
            Assert.Equal(HttpMethod.Get, req.Method);
            Assert.Contains("/api/v1/courses/c-123", req.RequestUri!.ToString());

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {
                        "id": "c-123",
                        "name": "Ciências da Natureza",
                        "code": "CIE-01",
                        "workloadHours": 60,
                        "description": "Física e biologia básica",
                        "schoolUnitId": "unit-2",
                        "targetAudience": "Geral",
                        "status": 2,
                        "createdAt": "2026-02-01T10:00:00Z",
                        "createdBy": "prof@horizon.org"
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            };
        };

        var result = await service.GetByIdAsync("c-123");

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("c-123", result.Data.Id);
        Assert.Equal("Ciências da Natureza", result.Data.Name);
        Assert.Equal("CIE-01", result.Data.Code);
        Assert.Equal(CourseStatus.Draft, result.Data.Status);
    }

    [Fact]
    public async Task GetByIdAsync_404NotFound_ReturnsNotFoundResult()
    {
        var (service, handler) = CreateTestCourseService();

        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(
                """
                {
                    "success": false,
                    "message": "Course not found",
                    "value": null
                }
                """,
                Encoding.UTF8,
                "application/json")
        };

        var result = await service.GetByIdAsync("c-inexistente");

        Assert.False(result.Succeeded);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Course not found", result.Message);
    }

    [Fact]
    public async Task GetByIdAsync_EmptyId_ReturnsBadRequestWithoutHttpCall()
    {
        var (service, _) = CreateTestCourseService();

        var result = await service.GetByIdAsync("");

        Assert.False(result.Succeeded);
        Assert.Equal(400, result.StatusCode);
    }

    #endregion

    #region POST /api/v1/courses (Create)

    [Fact]
    public async Task CreateAsync_Success_SendsCorrectPayloadAndReturnsCourse()
    {
        var (service, handler) = CreateTestCourseService();

        handler.Handler = req =>
        {
            Assert.Equal(HttpMethod.Post, req.Method);
            Assert.Contains("/api/v1/courses", req.RequestUri!.ToString());

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(
                    """
                    {
                        "id": "c-999",
                        "name": "Inglês Instrumental",
                        "code": "ING-01",
                        "workloadHours": 50,
                        "description": "Leitura técnica",
                        "schoolUnitId": "unit-1",
                        "targetAudience": "Jovens",
                        "status": 1,
                        "createdAt": "2026-03-01T12:00:00Z",
                        "createdBy": "admin@horizon.org"
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            };
        };

        var request = new CreateCourseDto(
            Name: "Inglês Instrumental",
            Code: "ING-01",
            WorkloadHours: 50,
            Description: "Leitura técnica",
            SchoolUnitId: "unit-1",
            TargetAudience: "Jovens",
            Status: CourseStatus.Active);

        var result = await service.CreateAsync(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("c-999", result.Data.Id);
        Assert.Equal("Inglês Instrumental", result.Data.Name);
        Assert.Equal("ING-01", result.Data.Code);
    }

    [Fact]
    public async Task CreateAsync_400BadRequest_ReturnsErrorMessage()
    {
        var (service, handler) = CreateTestCourseService();

        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(
                """
                {
                    "success": false,
                    "message": "Course code is already in use.",
                    "value": null
                }
                """,
                Encoding.UTF8,
                "application/json")
        };

        var request = new CreateCourseDto("Curso", "DUP-01", 40);
        var result = await service.CreateAsync(request);

        Assert.False(result.Succeeded);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("Course code is already in use.", result.Message);
    }

    [Fact]
    public async Task CreateAsync_422Unprocessable_ExtractsProblemDetail()
    {
        var (service, handler) = CreateTestCourseService();

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

        var request = new CreateCourseDto("Curso", "COD-01", 0);
        var result = await service.CreateAsync(request);

        Assert.False(result.Succeeded);
        Assert.Equal(422, result.StatusCode);
        Assert.Contains("Workload hours must be greater than zero.", result.Message);
    }

    #endregion

    #region PUT /api/v1/courses/{id} (Update)

    [Fact]
    public async Task UpdateAsync_Success_SendsPutAndReturnsUpdatedCourse()
    {
        var (service, handler) = CreateTestCourseService();

        handler.Handler = req =>
        {
            Assert.Equal(HttpMethod.Put, req.Method);
            Assert.Contains("/api/v1/courses/c-123", req.RequestUri!.ToString());

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {
                        "id": "c-123",
                        "name": "Inglês Avançado",
                        "code": "ING-02",
                        "workloadHours": 80,
                        "description": "Conversação e escrita",
                        "schoolUnitId": "unit-1",
                        "targetAudience": "Adultos",
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

        var request = new UpdateCourseDto("c-123", "Inglês Avançado", "ING-02", 80);
        var result = await service.UpdateAsync(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("Inglês Avançado", result.Data.Name);
        Assert.Equal(80, result.Data.WorkloadHours);
    }

    [Fact]
    public async Task UpdateAsync_404NotFound_ReturnsNotFoundResult()
    {
        var (service, handler) = CreateTestCourseService();

        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(
                """
                {
                    "success": false,
                    "message": "Course not found",
                    "value": null
                }
                """,
                Encoding.UTF8,
                "application/json")
        };

        var request = new UpdateCourseDto("c-999", "Nome", "COD", 40);
        var result = await service.UpdateAsync(request);

        Assert.False(result.Succeeded);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Course not found", result.Message);
    }

    #endregion

    #region DELETE /api/v1/courses/{id} (Delete)

    [Fact]
    public async Task DeleteAsync_Success_SendsDeleteAndReturnsOk()
    {
        var (service, handler) = CreateTestCourseService();

        handler.Handler = req =>
        {
            Assert.Equal(HttpMethod.Delete, req.Method);
            Assert.Contains("/api/v1/courses/c-123", req.RequestUri!.ToString());

            return new HttpResponseMessage(HttpStatusCode.NoContent);
        };

        var result = await service.DeleteAsync("c-123");

        Assert.True(result.Succeeded);
        Assert.Contains("excluído com sucesso", result.Message);
    }

    [Fact]
    public async Task DeleteAsync_404NotFound_ReturnsNotFoundResult()
    {
        var (service, handler) = CreateTestCourseService();

        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(
                """
                {
                    "success": false,
                    "message": "Course not found",
                    "value": false
                }
                """,
                Encoding.UTF8,
                "application/json")
        };

        var result = await service.DeleteAsync("c-inexistente");

        Assert.False(result.Succeeded);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Course not found", result.Message);
    }

    [Fact]
    public async Task DeleteAsync_409Conflict_ReturnsConflictMessage()
    {
        var (service, handler) = CreateTestCourseService();

        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.Conflict)
        {
            Content = new StringContent(
                """
                {
                    "success": false,
                    "message": "Cannot delete course with active enrollments.",
                    "value": false
                }
                """,
                Encoding.UTF8,
                "application/json")
        };

        var result = await service.DeleteAsync("c-em-uso");

        Assert.False(result.Succeeded);
        Assert.Equal(409, result.StatusCode);
        Assert.Contains("Cannot delete course with active enrollments.", result.Message);
    }

    #endregion
}

