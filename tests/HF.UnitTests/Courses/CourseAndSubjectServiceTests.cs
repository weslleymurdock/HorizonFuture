using HF.Application.Courses.Requests;
using HF.Application.Courses.Services;
using HF.Application.Tests.Common;
using HF.Domain.Courses;
using HF.Domain.Enrollments;
using HF.Domain.TeacherAssignments;
using Xunit;

namespace HF.Application.Tests.Courses;

public class CourseAndSubjectServiceTests
{
    private readonly InMemoryRepository<Course> _courseRepo = new();
    private readonly InMemoryRepository<Subject> _subjectRepo = new();
    private readonly InMemoryRepository<TeacherAssignment> _assignmentRepo = new();
    private readonly InMemoryRepository<Enrollment> _enrollmentRepo = new();
    private readonly CourseService _courseService;
    private readonly SubjectService _subjectService;

    public CourseAndSubjectServiceTests()
    {
        _courseService = new CourseService(_courseRepo);
        _subjectService = new SubjectService(_subjectRepo, _courseRepo, _assignmentRepo, _enrollmentRepo);
    }

    [Fact]
    public async Task Course_CreateAndGet_ShouldSucceed()
    {
        var request = new CreateCourseRequest(
            Name: "Robotics 101",
            Code: "ROB101",
            WorkloadHours: 60);

        var response = await _courseService.CreateAsync(request, CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal("ROB101", response.Code);

        var fetched = await _courseService.GetByIdAsync(response.Id, CancellationToken.None);
        Assert.NotNull(fetched);
        Assert.Equal("Robotics 101", fetched.Name);
    }

    [Fact]
    public async Task Subject_DeleteAsync_WhenHasAssignments_ShouldFail()
    {
        var subject = new Subject("subj-1")
        {
            Name = "Mathematics",
            Code = "MATH1"
        };
        await _subjectRepo.AddAsync(subject, CancellationToken.None);

        var assignment = new TeacherAssignment("assign-1")
        {
            TeacherId = "teach-1",
            SubjectId = "subj-1",
            AcademicYear = 2026,
            AcademicSemester = 1
        };
        await _assignmentRepo.AddAsync(assignment, CancellationToken.None);

        var (succeeded, error) = await _subjectService.DeleteAsync("subj-1", CancellationToken.None);

        Assert.False(succeeded);
        Assert.Contains("teacher assignment history", error);
        Assert.Single(_subjectRepo.Items);
    }
}

