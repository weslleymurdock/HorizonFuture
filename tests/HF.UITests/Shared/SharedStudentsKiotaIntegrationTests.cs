using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HF.Client;
using HF.Shared.Auth;
using HF.Shared.Students;
using HF.Shared.Students.Models;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Xunit;

namespace HF.UITests.Shared;

public class SharedStudentsKiotaIntegrationTests
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

    private static (StudentService service, TestHandler handler) CreateTestStudentService()
    {
        var testHandler = new TestHandler();
        var tokenStore = new InMemoryTokenStore();
        var authProvider = new KiotaAuthenticationProvider(tokenStore);
        var adapter = new HttpClientRequestAdapter(authProvider, httpClient: new HttpClient(testHandler))
        {
            BaseUrl = "http://localhost:8080"
        };
        var apiClient = new ApiClient(adapter);
        var studentService = new StudentService(apiClient);
        return (studentService, testHandler);
    }

    #region GET /api/v1/students (List)

    [Fact]
    public async Task ListAsync_Success_ReturnsMappedStudentsList()
    {
        var (service, handler) = CreateTestStudentService();

        handler.Handler = req =>
        {
            Assert.Equal(HttpMethod.Get, req.Method);
            Assert.Contains("/api/v1/students", req.RequestUri!.ToString());
            Assert.Contains("schoolUnitId=unit-1", req.RequestUri.Query);
            Assert.Contains("status=1", req.RequestUri.Query);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    [
                        {
                            "id": "stud-001",
                            "userId": "user-synth-1",
                            "registrationNumber": "MAT-2026-001",
                            "dateOfBirth": "2010-05-14",
                            "guardianName": "Maria da Silva",
                            "guardianRelationship": "Mãe",
                            "guardianPhone": "(11) 98765-4321",
                            "emergencyContactName": "Carlos da Silva",
                            "emergencyContactPhone": "(11) 91234-5678",
                            "schoolUnitId": "unit-1",
                            "status": 1,
                            "specialNeeds": "Nenhuma",
                            "medicalNotes": "Alergia a amendoim",
                            "enrollmentDate": "2026-02-01",
                            "createdAt": "2026-01-01T10:00:00Z",
                            "createdBy": "admin@horizon.org"
                        }
                    ]
                    """,
                    Encoding.UTF8,
                    "application/json")
            };
        };

        var filter = new StudentFilterDto("unit-1", StudentStatus.Active);
        var result = await service.ListAsync(filter);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);

        var item = result.Data[0];
        Assert.Equal("stud-001", item.Id);
        Assert.Equal("MAT-2026-001", item.RegistrationNumber);
        Assert.Equal(new DateOnly(2010, 5, 14), item.DateOfBirth);
        Assert.Equal("Maria da Silva", item.GuardianName);
        Assert.Equal(StudentStatus.Active, item.Status);
    }

    [Fact]
    public async Task ListAsync_401Unauthorized_MapsToAuthErrorMessage()
    {
        var (service, handler) = CreateTestStudentService();
        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("{\"title\":\"Unauthorized\"}", Encoding.UTF8, "application/json")
        };

        var result = await service.ListAsync();

        Assert.False(result.Succeeded);
        Assert.Equal(401, result.StatusCode);
        Assert.Contains("401", result.Message);
    }

    [Fact]
    public async Task ListAsync_403Forbidden_MapsToForbiddenErrorMessage()
    {
        var (service, handler) = CreateTestStudentService();
        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent("{\"title\":\"Forbidden\"}", Encoding.UTF8, "application/json")
        };

        var result = await service.ListAsync();

        Assert.False(result.Succeeded);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("403", result.Message);
    }

    #endregion

    #region GET /api/v1/students/{id}

    [Fact]
    public async Task GetByIdAsync_Success_ReturnsMappedStudent()
    {
        var (service, handler) = CreateTestStudentService();

        handler.Handler = req =>
        {
            Assert.Equal(HttpMethod.Get, req.Method);
            Assert.EndsWith("/api/v1/students/stud-123", req.RequestUri!.AbsolutePath);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {
                        "id": "stud-123",
                        "userId": "user-synth-2",
                        "registrationNumber": "MAT-2026-123",
                        "dateOfBirth": "2012-08-10",
                        "guardianName": "José Pereira",
                        "guardianRelationship": "Pai",
                        "guardianPhone": "(11) 95555-4444",
                        "schoolUnitId": "unit-central",
                        "status": 1,
                        "specialNeeds": null,
                        "medicalNotes": null,
                        "enrollmentDate": "2026-02-15",
                        "createdAt": "2026-02-15T08:00:00Z",
                        "createdBy": "admin@horizon.org"
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            };
        };

        var result = await service.GetByIdAsync("stud-123");

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("stud-123", result.Data.Id);
        Assert.Equal("MAT-2026-123", result.Data.RegistrationNumber);
        Assert.Equal(new DateOnly(2012, 8, 10), result.Data.DateOfBirth);
    }

    [Fact]
    public async Task GetByIdAsync_404NotFound_ReturnsNotFoundResult()
    {
        var (service, handler) = CreateTestStudentService();
        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(
                "{\"message\":\"Aluno não encontrado.\"}",
                Encoding.UTF8,
                "application/json")
        };

        var result = await service.GetByIdAsync("missing-id");

        Assert.False(result.Succeeded);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task GetByIdAsync_EmptyId_ReturnsBadRequestWithoutHttpCall()
    {
        var (service, handler) = CreateTestStudentService();
        bool called = false;
        handler.Handler = _ => { called = true; return new HttpResponseMessage(HttpStatusCode.OK); };

        var result = await service.GetByIdAsync(string.Empty);

        Assert.False(result.Succeeded);
        Assert.Equal(400, result.StatusCode);
        Assert.False(called);
    }

    #endregion

    #region POST /api/v1/students (Create)

    [Fact]
    public async Task CreateAsync_Success_SendsCorrectPayloadAndReturnsStudent()
    {
        var (service, handler) = CreateTestStudentService();

        handler.Handler = req =>
        {
            Assert.Equal(HttpMethod.Post, req.Method);
            Assert.EndsWith("/api/v1/students", req.RequestUri!.AbsolutePath);

            var body = req.Content!.ReadAsStringAsync().Result;
            Assert.Contains("user-synth-3", body);
            Assert.Contains("MAT-2026-789", body);

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(
                    """
                    {
                        "id": "stud-new-999",
                        "userId": "user-synth-3",
                        "registrationNumber": "MAT-2026-789",
                        "dateOfBirth": "2013-03-20",
                        "guardianName": "Ana Oliveira",
                        "schoolUnitId": "unit-1",
                        "status": 1,
                        "enrollmentDate": "2026-03-01",
                        "createdAt": "2026-03-01T12:00:00Z",
                        "createdBy": "user-synth-3"
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            };
        };

        var dto = new CreateStudentDto(
            UserId: "user-synth-3",
            RegistrationNumber: "MAT-2026-789",
            DateOfBirth: new DateOnly(2013, 3, 20),
            GuardianName: "Ana Oliveira",
            SchoolUnitId: "unit-1",
            Status: StudentStatus.Active,
            EnrollmentDate: new DateOnly(2026, 3, 1));

        var result = await service.CreateAsync(dto);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("stud-new-999", result.Data.Id);
        Assert.Equal("MAT-2026-789", result.Data.RegistrationNumber);
    }

    [Fact]
    public async Task CreateAsync_400BadRequest_ReturnsErrorMessage()
    {
        var (service, handler) = CreateTestStudentService();
        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(
                "{\"message\":\"O campo UserId é obrigatório.\"}",
                Encoding.UTF8,
                "application/json")
        };

        var dto = new CreateStudentDto(
            UserId: "",
            RegistrationNumber: "MAT-001",
            DateOfBirth: new DateOnly(2012, 1, 1));

        var result = await service.CreateAsync(dto);

        Assert.False(result.Succeeded);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_422Unprocessable_ExtractsProblemDetail()
    {
        var (service, handler) = CreateTestStudentService();
        handler.Handler = _ => new HttpResponseMessage((HttpStatusCode)422)
        {
            Content = new StringContent(
                """
                {
                    "title": "Unprocessable Entity",
                    "status": 422,
                    "detail": "A data de nascimento deve ser uma data passada."
                }
                """,
                Encoding.UTF8,
                "application/problem+json")
        };

        var dto = new CreateStudentDto(
            UserId: "user-1",
            RegistrationNumber: "MAT-001",
            DateOfBirth: new DateOnly(2099, 1, 1));

        var result = await service.CreateAsync(dto);

        Assert.False(result.Succeeded);
        Assert.Equal(422, result.StatusCode);
        Assert.Contains("passada", result.Message);
    }

    #endregion

    #region PUT /api/v1/students/{id} (Update)

    [Fact]
    public async Task UpdateAsync_Success_SendsPutAndReturnsUpdatedStudent()
    {
        var (service, handler) = CreateTestStudentService();

        handler.Handler = req =>
        {
            Assert.Equal(HttpMethod.Put, req.Method);
            Assert.EndsWith("/api/v1/students/stud-100", req.RequestUri!.AbsolutePath);

            var body = req.Content!.ReadAsStringAsync().Result;
            Assert.Contains("MAT-UPDATED", body);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {
                        "id": "stud-100",
                        "userId": "user-synth-1",
                        "registrationNumber": "MAT-UPDATED",
                        "dateOfBirth": "2011-06-15",
                        "status": 1,
                        "enrollmentDate": "2026-02-01"
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            };
        };

        var dto = new UpdateStudentDto(
            Id: "stud-100",
            RegistrationNumber: "MAT-UPDATED",
            DateOfBirth: new DateOnly(2011, 6, 15),
            Status: StudentStatus.Active);

        var result = await service.UpdateAsync(dto);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("MAT-UPDATED", result.Data.RegistrationNumber);
    }

    [Fact]
    public async Task UpdateAsync_404NotFound_ReturnsNotFoundResult()
    {
        var (service, handler) = CreateTestStudentService();
        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("{\"message\":\"Student not found.\"}", Encoding.UTF8, "application/json")
        };

        var dto = new UpdateStudentDto(
            Id: "missing-id",
            RegistrationNumber: "MAT-001",
            DateOfBirth: new DateOnly(2011, 6, 15));

        var result = await service.UpdateAsync(dto);

        Assert.False(result.Succeeded);
        Assert.Equal(404, result.StatusCode);
    }

    #endregion

    #region DELETE /api/v1/students/{id}

    [Fact]
    public async Task DeleteAsync_Success_SendsDeleteAndReturnsOk()
    {
        var (service, handler) = CreateTestStudentService();

        handler.Handler = req =>
        {
            Assert.Equal(HttpMethod.Delete, req.Method);
            Assert.EndsWith("/api/v1/students/stud-del-1", req.RequestUri!.AbsolutePath);
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        };

        var result = await service.DeleteAsync("stud-del-1");

        Assert.True(result.Succeeded);
        Assert.Equal("Aluno excluído com sucesso.", result.Message);
    }

    [Fact]
    public async Task DeleteAsync_404NotFound_ReturnsNotFoundResult()
    {
        var (service, handler) = CreateTestStudentService();
        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("{\"message\":\"Student not found.\"}", Encoding.UTF8, "application/json")
        };

        var result = await service.DeleteAsync("missing-id");

        Assert.False(result.Succeeded);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task DeleteAsync_409Conflict_ReturnsConflictMessage()
    {
        var (service, handler) = CreateTestStudentService();
        handler.Handler = _ => new HttpResponseMessage(HttpStatusCode.Conflict)
        {
            Content = new StringContent(
                "{\"message\":\"Cannot delete student with existing academic enrollment history.\"}",
                Encoding.UTF8,
                "application/json")
        };

        var result = await service.DeleteAsync("stud-with-enrollments");

        Assert.False(result.Succeeded);
        Assert.Equal(409, result.StatusCode);
        Assert.Contains("Cannot delete student", result.Message);
    }

    #endregion
}
