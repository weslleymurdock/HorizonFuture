using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
    public async Task GetByIdAsync_WhenFound_ReturnsStudent()
    {
        var student = new Student("student-1")
        {
            UserId = "user-1",
            RegistrationNumber = "STU-001",
            DateOfBirth = new DateOnly(2010, 5, 12)
        };
        await _studentRepo.AddAsync(student, CancellationToken.None);

        var result = await _sut.GetByIdAsync("student-1", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("STU-001", result.RegistrationNumber);

        var missing = await _sut.GetByIdAsync("missing-id", CancellationToken.None);
        Assert.Null(missing);
    }

    [Fact]
    public async Task ListAsync_FiltersBySchoolUnitAndStatus()
    {
        await _studentRepo.AddAsync(new Student("s1") { UserId = "u1", RegistrationNumber = "R1", SchoolUnitId = "unit-1", Status = StudentStatus.Active }, CancellationToken.None);
        await _studentRepo.AddAsync(new Student("s2") { UserId = "u2", RegistrationNumber = "R2", SchoolUnitId = "unit-1", Status = StudentStatus.Inactive }, CancellationToken.None);
        await _studentRepo.AddAsync(new Student("s3") { UserId = "u3", RegistrationNumber = "R3", SchoolUnitId = "unit-2", Status = StudentStatus.Active }, CancellationToken.None);

        var unit1Students = await _sut.ListAsync(new ListStudentsRequest(SchoolUnitId: "unit-1"), CancellationToken.None);
        Assert.Equal(2, unit1Students.Count);

        var inactiveStudents = await _sut.ListAsync(new ListStudentsRequest(Status: StudentStatus.Inactive), CancellationToken.None);
        Assert.Single(inactiveStudents);
        Assert.Equal("R2", inactiveStudents[0].RegistrationNumber);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesStudentFields()
    {
        var student = new Student("student-1")
        {
            UserId = "user-1",
            RegistrationNumber = "ORIG-REG",
            DateOfBirth = new DateOnly(2010, 1, 1),
            Status = StudentStatus.Active
        };
        await _studentRepo.AddAsync(student, CancellationToken.None);

        var updateRequest = new UpdateStudentRequest(
            Id: "student-1",
            RegistrationNumber: "NEW-REG",
            DateOfBirth: new DateOnly(2010, 2, 2),
            GuardianName: "Updated Guardian",
            Status: StudentStatus.Graduated);

        var updated = await _sut.UpdateAsync(updateRequest, CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal("NEW-REG", updated.RegistrationNumber);
        Assert.Equal("Updated Guardian", updated.GuardianName);
        Assert.Equal(StudentStatus.Graduated, updated.Status);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ReturnsNull()
    {
        var updateRequest = new UpdateStudentRequest(
            Id: "not-found",
            RegistrationNumber: "NEW-REG",
            DateOfBirth: new DateOnly(2010, 2, 2));

        var result = await _sut.UpdateAsync(updateRequest, CancellationToken.None);
        Assert.Null(result);
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

    [Fact]
    public async Task RegistrationNumberExistsAsync_ValidatesUniqueness()
    {
        var student = new Student("student-1")
        {
            UserId = "user-1",
            RegistrationNumber = "EXISTING-123"
        };
        await _studentRepo.AddAsync(student, CancellationToken.None);

        bool exists = await _sut.RegistrationNumberExistsAsync("EXISTING-123", null, CancellationToken.None);
        Assert.True(exists);

        bool existsExcluded = await _sut.RegistrationNumberExistsAsync("EXISTING-123", "student-1", CancellationToken.None);
        Assert.False(existsExcluded);

        bool existsOther = await _sut.RegistrationNumberExistsAsync("OTHER", null, CancellationToken.None);
        Assert.False(existsOther);
    }

    [Fact]
    public async Task UserAvailableAsync_ChecksExistenceAndUniqueness()
    {
        _identityService.AddUser("user-available");
        _identityService.AddUser("user-taken");

        await _studentRepo.AddAsync(new Student("s-taken") { UserId = "user-taken", RegistrationNumber = "REG-T" }, CancellationToken.None);

        bool available = await _sut.UserAvailableAsync("user-available", null, CancellationToken.None);
        Assert.True(available);

        bool taken = await _sut.UserAvailableAsync("user-taken", null, CancellationToken.None);
        Assert.False(taken);

        bool nonExistentUser = await _sut.UserAvailableAsync("user-non-existent", null, CancellationToken.None);
        Assert.False(nonExistentUser);
    }
}
