using HF.Application.ClassSessions.Services;
using HF.Application.Enrollments.Services;
using HF.Application.TeacherAssignments.Services;
using HF.Application.Tests.Common;
using HF.Domain.Attendances;
using HF.Domain.ClassSessions;
using HF.Domain.Courses;
using HF.Domain.Enrollments;
using HF.Domain.TeacherAssignments;
using HF.Domain.Teachers;
using Xunit;

namespace HF.Application.Tests.Academic;

public class AcademicRulesTests
{
    private readonly InMemoryRepository<TeacherAssignment> _assignmentRepo = new();
    private readonly InMemoryRepository<Teacher> _teacherRepo = new();
    private readonly InMemoryRepository<Subject> _subjectRepo = new();
    private readonly InMemoryRepository<ClassSession> _sessionRepo = new();
    private readonly InMemoryRepository<Attendance> _attendanceRepo = new();
    private readonly InMemoryRepository<Enrollment> _enrollmentRepo = new();

    [Fact]
    public async Task TeacherAssignment_DeleteAsync_WhenHasClassSessions_ShouldFail()
    {
        var assignmentService = new TeacherAssignmentService(_assignmentRepo, _teacherRepo, _subjectRepo, _sessionRepo);

        var assignment = new TeacherAssignment("ta-1")
        {
            TeacherId = "t-1",
            SubjectId = "s-1",
            AcademicYear = 2026,
            AcademicSemester = 1
        };
        await _assignmentRepo.AddAsync(assignment, CancellationToken.None);

        var session = new ClassSession("cs-1")
        {
            TeacherAssignmentId = "ta-1",
            Date = new DateOnly(2026, 3, 1)
        };
        await _sessionRepo.AddAsync(session, CancellationToken.None);

        var (succeeded, error) = await assignmentService.DeleteAsync("ta-1", CancellationToken.None);

        Assert.False(succeeded);
        Assert.Contains("class sessions", error);
        Assert.Single(_assignmentRepo.Items);
    }

    [Fact]
    public async Task Enrollment_DeleteAsync_WhenHasAttendances_ShouldFail()
    {
        var studentRepo = new InMemoryRepository<HF.Domain.Students.Student>();
        var enrollmentService = new EnrollmentService(_enrollmentRepo, studentRepo, _subjectRepo, _attendanceRepo);

        var enrollment = new Enrollment("e-1")
        {
            StudentId = "stu-1",
            SubjectId = "sub-1",
            AcademicYear = 2026,
            AcademicSemester = 1
        };
        await _enrollmentRepo.AddAsync(enrollment, CancellationToken.None);

        var attendance = new Attendance("att-1")
        {
            ClassSessionId = "cs-1",
            EnrollmentId = "e-1"
        };
        await _attendanceRepo.AddAsync(attendance, CancellationToken.None);

        var (succeeded, error) = await enrollmentService.DeleteAsync("e-1", CancellationToken.None);

        Assert.False(succeeded);
        Assert.Contains("attendance records", error);
        Assert.Single(_enrollmentRepo.Items);
    }

    [Fact]
    public async Task ClassSession_DeleteAsync_WhenHasAttendances_ShouldFail()
    {
        var sessionService = new ClassSessionService(_sessionRepo, _assignmentRepo, _attendanceRepo);

        var session = new ClassSession("cs-1")
        {
            TeacherAssignmentId = "ta-1",
            Date = new DateOnly(2026, 3, 1)
        };
        await _sessionRepo.AddAsync(session, CancellationToken.None);

        var attendance = new Attendance("att-1")
        {
            ClassSessionId = "cs-1",
            EnrollmentId = "e-1"
        };
        await _attendanceRepo.AddAsync(attendance, CancellationToken.None);

        var (succeeded, error) = await sessionService.DeleteAsync("cs-1", CancellationToken.None);

        Assert.False(succeeded);
        Assert.Contains("attendance records", error);
        Assert.Single(_sessionRepo.Items);
    }
}

