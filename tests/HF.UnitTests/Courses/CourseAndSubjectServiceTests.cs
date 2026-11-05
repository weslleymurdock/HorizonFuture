using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
    public async Task Subject_CreateAsync_ValidInput_ShouldSucceed()
    {
        var request = new CreateSubjectRequest(
            Name: "Calculus I",
            Code: "calc-101",
            WorkloadHours: 80,
            Description: "Introduction to differential calculus",
            SchoolUnitId: "unit-1",
            Status: SubjectStatus.Active);

        var response = await _subjectService.CreateAsync(request, CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal("Calculus I", response.Name);
        Assert.Equal("CALC-101", response.Code);
        Assert.Equal(80, response.WorkloadHours);
        Assert.Equal("unit-1", response.SchoolUnitId);
        Assert.Equal(SubjectStatus.Active, response.Status);
    }

    [Fact]
    public async Task Subject_CreateAsync_WithNonExistentCourse_ShouldThrowKeyNotFoundException()
    {
        var request = new CreateSubjectRequest(
            Name: "Calculus I",
            Code: "CALC101",
            WorkloadHours: 80,
            CourseId: "non-existent-course-id");

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _subjectService.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task Subject_GetByIdAsync_ExistingAndNonExisting_ShouldReturnExpected()
    {
        var subject = new Subject("subj-10")
        {
            Name = "Physics",
            Code = "PHYS101",
            WorkloadHours = 60
        };
        await _subjectRepo.AddAsync(subject, CancellationToken.None);

        var existing = await _subjectService.GetByIdAsync("subj-10", CancellationToken.None);
        Assert.NotNull(existing);
        Assert.Equal("Physics", existing.Name);

        var nonExisting = await _subjectService.GetByIdAsync("non-existent", CancellationToken.None);
        Assert.Null(nonExisting);
    }

    [Fact]
    public async Task Subject_ListAsync_FiltersByCourseAndUnitAndStatus()
    {
        await _subjectRepo.AddAsync(new Subject("subj-1") { Name = "Math", Code = "M1", CourseId = "c1", SchoolUnitId = "u1", Status = SubjectStatus.Active }, CancellationToken.None);
        await _subjectRepo.AddAsync(new Subject("subj-2") { Name = "Science", Code = "S1", CourseId = "c1", SchoolUnitId = "u2", Status = SubjectStatus.Active }, CancellationToken.None);
        await _subjectRepo.AddAsync(new Subject("subj-3") { Name = "History", Code = "H1", CourseId = "c2", SchoolUnitId = "u1", Status = SubjectStatus.Inactive }, CancellationToken.None);

        var listCourse1 = await _subjectService.ListAsync(new ListSubjectsRequest(CourseId: "c1"), CancellationToken.None);
        Assert.Equal(2, listCourse1.Count);

        var listUnit2 = await _subjectService.ListAsync(new ListSubjectsRequest(SchoolUnitId: "u2"), CancellationToken.None);
        Assert.Single(listUnit2);
        Assert.Equal("Science", listUnit2[0].Name);

        var listInactive = await _subjectService.ListAsync(new ListSubjectsRequest(Status: SubjectStatus.Inactive), CancellationToken.None);
        Assert.Single(listInactive);
        Assert.Equal("History", listInactive[0].Name);
    }

    [Fact]
    public async Task Subject_UpdateAsync_ValidInput_ShouldUpdateAndReturn()
    {
        var subject = new Subject("subj-20")
        {
            Name = "Original Name",
            Code = "ORIG",
            WorkloadHours = 40,
            Status = SubjectStatus.Active
        };
        await _subjectRepo.AddAsync(subject, CancellationToken.None);

        var updateRequest = new UpdateSubjectRequest(
            Id: "subj-20",
            Name: "Updated Name",
            Code: "updt",
            WorkloadHours: 50,
            Description: "Updated description",
            Status: SubjectStatus.Inactive);

        var updated = await _subjectService.UpdateAsync(updateRequest, CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated.Name);
        Assert.Equal("UPDT", updated.Code);
        Assert.Equal(50, updated.WorkloadHours);
        Assert.Equal(SubjectStatus.Inactive, updated.Status);
    }

    [Fact]
    public async Task Subject_UpdateAsync_NonExisting_ShouldReturnNull()
    {
        var updateRequest = new UpdateSubjectRequest(
            Id: "not-found",
            Name: "Name",
            Code: "CODE",
            WorkloadHours: 40);

        var result = await _subjectService.UpdateAsync(updateRequest, CancellationToken.None);
        Assert.Null(result);
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

    [Fact]
    public async Task Subject_DeleteAsync_WhenHasEnrollments_ShouldFail()
    {
        var subject = new Subject("subj-30")
        {
            Name = "Chemistry",
            Code = "CHEM1"
        };
        await _subjectRepo.AddAsync(subject, CancellationToken.None);

        var enrollment = new Enrollment("enr-1")
        {
            StudentId = "stud-1",
            SubjectId = "subj-30",
            AcademicYear = 2026,
            AcademicSemester = 1
        };
        await _enrollmentRepo.AddAsync(enrollment, CancellationToken.None);

        var (succeeded, error) = await _subjectService.DeleteAsync("subj-30", CancellationToken.None);

        Assert.False(succeeded);
        Assert.Contains("student enrollment history", error);
        Assert.Single(_subjectRepo.Items);
    }

    [Fact]
    public async Task Subject_DeleteAsync_WhenNotFound_ShouldReturnFalse()
    {
        var (succeeded, error) = await _subjectService.DeleteAsync("missing-id", CancellationToken.None);

        Assert.False(succeeded);
        Assert.Equal("Subject not found.", error);
    }

    [Fact]
    public async Task Subject_DeleteAsync_WhenNoAssignmentsOrEnrollments_ShouldSucceed()
    {
        var subject = new Subject("subj-40")
        {
            Name = "Art",
            Code = "ART1"
        };
        await _subjectRepo.AddAsync(subject, CancellationToken.None);

        var (succeeded, error) = await _subjectService.DeleteAsync("subj-40", CancellationToken.None);

        Assert.True(succeeded);
        Assert.Null(error);
        Assert.Empty(_subjectRepo.Items);
    }

    [Fact]
    public async Task Subject_CodeExistsAsync_ValidatesUniqueness()
    {
        var subject = new Subject("subj-50")
        {
            Name = "Biology",
            Code = "BIO1"
        };
        await _subjectRepo.AddAsync(subject, CancellationToken.None);

        bool exists1 = await _subjectService.CodeExistsAsync("BIO1", null, CancellationToken.None);
        Assert.True(exists1);

        bool existsExcluded = await _subjectService.CodeExistsAsync("BIO1", "subj-50", CancellationToken.None);
        Assert.False(existsExcluded);

        bool existsOther = await _subjectService.CodeExistsAsync("GEO1", null, CancellationToken.None);
        Assert.False(existsOther);
    }
}
