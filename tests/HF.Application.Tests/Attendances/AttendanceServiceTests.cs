using HF.Application.Attendances.Requests;
using HF.Application.Attendances.Services;
using HF.Application.Tests.Common;
using HF.Domain.Attendances;
using HF.Domain.ClassSessions;
using HF.Domain.Enrollments;
using HF.Domain.TeacherAssignments;
using Xunit;

namespace HF.Application.Tests.Attendances;

public class AttendanceServiceTests
{
    private readonly InMemoryRepository<Attendance> _attendanceRepo = new();
    private readonly InMemoryRepository<ClassSession> _sessionRepo = new();
    private readonly InMemoryRepository<Enrollment> _enrollmentRepo = new();
    private readonly InMemoryRepository<TeacherAssignment> _assignmentRepo = new();
    private readonly AttendanceService _sut;

    public AttendanceServiceTests()
    {
        _sut = new AttendanceService(_attendanceRepo, _sessionRepo, _enrollmentRepo, _assignmentRepo);
    }

    [Fact]
    public async Task CreateAsync_WhenSubjectAndAcademicPeriodMatch_ShouldSucceed()
    {
        // Arrange
        var assignment = new TeacherAssignment("assignment-1")
        {
            TeacherId = "teacher-1",
            SubjectId = "math-101",
            AcademicYear = 2026,
            AcademicSemester = 1
        };
        await _assignmentRepo.AddAsync(assignment, CancellationToken.None);

        var session = new ClassSession("session-1")
        {
            TeacherAssignmentId = "assignment-1",
            Date = new DateOnly(2026, 3, 10)
        };
        await _sessionRepo.AddAsync(session, CancellationToken.None);

        var enrollment = new Enrollment("enrollment-1")
        {
            StudentId = "student-1",
            SubjectId = "math-101",
            AcademicYear = 2026,
            AcademicSemester = 1
        };
        await _enrollmentRepo.AddAsync(enrollment, CancellationToken.None);

        var request = new CreateAttendanceRequest(
            ClassSessionId: "session-1",
            EnrollmentId: "enrollment-1",
            Status: AttendanceStatus.Present);

        // Act
        var result = await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("session-1", result.ClassSessionId);
        Assert.Equal("enrollment-1", result.EnrollmentId);
        Assert.Single(_attendanceRepo.Items);
    }

    [Fact]
    public async Task CreateAsync_WhenSubjectMismatch_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var assignment = new TeacherAssignment("assignment-1")
        {
            TeacherId = "teacher-1",
            SubjectId = "math-101",
            AcademicYear = 2026,
            AcademicSemester = 1
        };
        await _assignmentRepo.AddAsync(assignment, CancellationToken.None);

        var session = new ClassSession("session-1")
        {
            TeacherAssignmentId = "assignment-1",
            Date = new DateOnly(2026, 3, 10)
        };
        await _sessionRepo.AddAsync(session, CancellationToken.None);

        var enrollment = new Enrollment("enrollment-1")
        {
            StudentId = "student-1",
            SubjectId = "portuguese-101", // Different subject!
            AcademicYear = 2026,
            AcademicSemester = 1
        };
        await _enrollmentRepo.AddAsync(enrollment, CancellationToken.None);

        var request = new CreateAttendanceRequest(
            ClassSessionId: "session-1",
            EnrollmentId: "enrollment-1",
            Status: AttendanceStatus.Present);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(request, CancellationToken.None));
        Assert.Contains("does not match class session subject", ex.Message);
        Assert.Empty(_attendanceRepo.Items);
    }

    [Fact]
    public async Task CreateAsync_WhenAcademicPeriodMismatch_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var assignment = new TeacherAssignment("assignment-1")
        {
            TeacherId = "teacher-1",
            SubjectId = "math-101",
            AcademicYear = 2026,
            AcademicSemester = 1
        };
        await _assignmentRepo.AddAsync(assignment, CancellationToken.None);

        var session = new ClassSession("session-1")
        {
            TeacherAssignmentId = "assignment-1",
            Date = new DateOnly(2026, 3, 10)
        };
        await _sessionRepo.AddAsync(session, CancellationToken.None);

        var enrollment = new Enrollment("enrollment-1")
        {
            StudentId = "student-1",
            SubjectId = "math-101",
            AcademicYear = 2025, // Different year!
            AcademicSemester = 1
        };
        await _enrollmentRepo.AddAsync(enrollment, CancellationToken.None);

        var request = new CreateAttendanceRequest(
            ClassSessionId: "session-1",
            EnrollmentId: "enrollment-1",
            Status: AttendanceStatus.Present);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(request, CancellationToken.None));
        Assert.Contains("does not match class session academic period", ex.Message);
        Assert.Empty(_attendanceRepo.Items);
    }
}

