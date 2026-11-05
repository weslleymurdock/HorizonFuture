using HF.Application.Students.Requests;
using HF.Application.Students.Services;
using HF.Application.Tests.Common;
using HF.Domain.Enrollments;
using HF.Domain.Students;
using Xunit;

namespace HF.Application.Tests.Students;

public class StudentServiceTests
{
    private readonly InMemoryRepository<Student> _studentRepo = new();
    private readonly InMemoryRepository<Enrollment> _enrollmentRepo = new();
    private readonly FakeIdentityService _identityService = new();
    private readonly StudentService _sut;

    public StudentServiceTests()
    {
        _sut = new StudentService(_studentRepo, _enrollmentRepo, _identityService);
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistAndReturnStudent()
    {
        // Arrange
        _identityService.AddUser("user-1");
        var request = new CreateStudentRequest(
            UserId: "user-1",
            RegistrationNumber: "STU-001",
            DateOfBirth: new DateOnly(2010, 5, 12),
            GuardianName: "Jane Doe");

        // Act
        var result = await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("STU-001", result.RegistrationNumber);
        Assert.Equal("Jane Doe", result.GuardianName);
        Assert.Single(_studentRepo.Items);
    }

    [Fact]
    public async Task DeleteAsync_WhenStudentHasEnrollments_ShouldFailAndPreserveHistory()
    {
        // Arrange
        var student = new Student("student-1")
        {
            UserId = "user-1",
            RegistrationNumber = "STU-001"
        };
        await _studentRepo.AddAsync(student, CancellationToken.None);

        var enrollment = new Enrollment("enrollment-1")
        {
            StudentId = "student-1",
            SubjectId = "subject-1",
            AcademicYear = 2026,
            AcademicSemester = 1
        };
        await _enrollmentRepo.AddAsync(enrollment, CancellationToken.None);

        // Act
        var (succeeded, error) = await _sut.DeleteAsync("student-1", CancellationToken.None);

        // Assert
        Assert.False(succeeded);
        Assert.Contains("academic enrollment history", error);
        Assert.Single(_studentRepo.Items); // Not deleted!
    }

    [Fact]
    public async Task DeleteAsync_WhenStudentHasNoEnrollments_ShouldSucceed()
    {
        // Arrange
        var student = new Student("student-1")
        {
            UserId = "user-1",
            RegistrationNumber = "STU-001"
        };
        await _studentRepo.AddAsync(student, CancellationToken.None);

        // Act
        var (succeeded, error) = await _sut.DeleteAsync("student-1", CancellationToken.None);

        // Assert
        Assert.True(succeeded);
        Assert.Null(error);
        Assert.Empty(_studentRepo.Items);
    }
}

