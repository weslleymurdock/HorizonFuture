using System;
using System.Linq;
using System.Threading.Tasks;
using HF.Shared.Enrollments.Models;
using HF.Shared.PageModels.Enrollments;
using HF.UITests.Shared.Fakes;
using Xunit;

namespace HF.UITests.Shared;

public class SharedEnrollmentsPageModelsTests
{
    private readonly FakeEnrollmentService _fakeEnrollmentService = new();
    private readonly FakeStudentService _fakeStudentService = new();
    private readonly FakeSubjectService _fakeSubjectService = new();

    #region EnrollmentListPageModel Tests

    [Fact]
    public void EnrollmentListPageModel_InitialState_PropertiesAreDefault()
    {
        var model = new EnrollmentListPageModel(_fakeEnrollmentService);

        Assert.Equal("Matrículas", model.Title);
        Assert.False(model.IsBusy);
        Assert.False(model.IsForbidden);
        Assert.False(model.IsUnauthorized);
        Assert.False(model.IsDeleting);
        Assert.Null(model.ErrorMessage);
        Assert.Null(model.SuccessMessage);
        Assert.Empty(model.Enrollments);
        Assert.Empty(model.SearchText);
        Assert.Null(model.StatusFilter);
        Assert.Null(model.StudentIdFilter);
        Assert.Null(model.SubjectIdFilter);
        Assert.Null(model.AcademicYearFilter);
        Assert.Null(model.AcademicSemesterFilter);
    }

    [Fact]
    public async Task EnrollmentListPageModel_LoadEnrollmentsAsync_Success_PopulatesEnrollments()
    {
        var model = new EnrollmentListPageModel(_fakeEnrollmentService);

        var success = await model.LoadEnrollmentsAsync();

        Assert.True(success);
        Assert.False(model.IsBusy);
        Assert.Null(model.ErrorMessage);
        Assert.Equal(2, model.Enrollments.Count);
        Assert.Contains(model.Enrollments, e => e.Id == "enr-1");
        Assert.Contains(model.Enrollments, e => e.Id == "enr-2");
    }

    [Fact]
    public async Task EnrollmentListPageModel_LoadEnrollmentsAsync_EmptyList_SetsEmptyEnrollments()
    {
        _fakeEnrollmentService.MockEnrollments.Clear();
        var model = new EnrollmentListPageModel(_fakeEnrollmentService);

        var success = await model.LoadEnrollmentsAsync();

        Assert.True(success);
        Assert.Empty(model.Enrollments);
        Assert.Null(model.ErrorMessage);
    }

    [Fact]
    public async Task EnrollmentListPageModel_LoadEnrollmentsAsync_ApiFailure_SetsErrorMessage()
    {
        _fakeEnrollmentService.ShouldSucceed = false;
        _fakeEnrollmentService.FailureMessage = "Erro ao buscar matrículas do servidor";
        _fakeEnrollmentService.FailureStatusCode = 500;

        var model = new EnrollmentListPageModel(_fakeEnrollmentService);

        var success = await model.LoadEnrollmentsAsync();

        Assert.False(success);
        Assert.False(model.IsBusy);
        Assert.Equal("Erro ao buscar matrículas do servidor", model.ErrorMessage);
        Assert.Empty(model.Enrollments);
    }

    [Fact]
    public async Task EnrollmentListPageModel_LoadEnrollmentsAsync_Forbidden_SetsIsForbidden()
    {
        _fakeEnrollmentService.ShouldSucceed = false;
        _fakeEnrollmentService.FailureMessage = "Acesso negado";
        _fakeEnrollmentService.FailureStatusCode = 403;

        var model = new EnrollmentListPageModel(_fakeEnrollmentService);

        var success = await model.LoadEnrollmentsAsync();

        Assert.False(success);
        Assert.True(model.IsForbidden);
        Assert.Contains("Enrollments.Read", model.ErrorMessage);
    }

    [Fact]
    public async Task EnrollmentListPageModel_LoadEnrollmentsAsync_Unauthorized_SetsIsUnauthorized()
    {
        _fakeEnrollmentService.ShouldSucceed = false;
        _fakeEnrollmentService.FailureMessage = "Unauthorized";
        _fakeEnrollmentService.FailureStatusCode = 401;

        var model = new EnrollmentListPageModel(_fakeEnrollmentService);

        var success = await model.LoadEnrollmentsAsync();

        Assert.False(success);
        Assert.True(model.IsUnauthorized);
        Assert.Contains("Sessão expirada", model.ErrorMessage);
    }

    [Fact]
    public async Task EnrollmentListPageModel_Filters_PropagatedToService()
    {
        var model = new EnrollmentListPageModel(_fakeEnrollmentService)
        {
            StudentIdFilter = "student-1",
            SubjectIdFilter = "subject-1",
            AcademicYearFilter = 2026,
            AcademicSemesterFilter = 1,
            StatusFilter = EnrollmentStatus.Active
        };

        var success = await model.LoadEnrollmentsAsync();

        Assert.True(success);
        Assert.Single(model.Enrollments);
        Assert.Equal("enr-1", model.Enrollments[0].Id);
        Assert.NotNull(_fakeEnrollmentService.LastFilter);
        Assert.Equal("student-1", _fakeEnrollmentService.LastFilter.StudentId);
        Assert.Equal("subject-1", _fakeEnrollmentService.LastFilter.SubjectId);
        Assert.Equal(2026, _fakeEnrollmentService.LastFilter.AcademicYear);
        Assert.Equal(1, _fakeEnrollmentService.LastFilter.AcademicSemester);
        Assert.Equal(EnrollmentStatus.Active, _fakeEnrollmentService.LastFilter.Status);
    }

    [Fact]
    public async Task EnrollmentListPageModel_DeleteEnrollmentAsync_Success_RemovesFromCollection()
    {
        var model = new EnrollmentListPageModel(_fakeEnrollmentService);
        await model.LoadEnrollmentsAsync();
        Assert.Equal(2, model.Enrollments.Count);

        var success = await model.DeleteEnrollmentAsync("enr-1");

        Assert.True(success);
        Assert.False(model.IsDeleting);
        Assert.Single(model.Enrollments);
        Assert.DoesNotContain(model.Enrollments, e => e.Id == "enr-1");
        Assert.Contains("sucesso", model.SuccessMessage);
    }

    #endregion

    #region EnrollmentCreatePageModel Tests

    [Fact]
    public void EnrollmentCreatePageModel_InitialState_DefaultsAreValid()
    {
        var model = new EnrollmentCreatePageModel(_fakeEnrollmentService, _fakeStudentService, _fakeSubjectService);

        Assert.Equal("Nova Matrícula", model.Title);
        Assert.Empty(model.StudentId);
        Assert.Empty(model.SubjectId);
        Assert.Equal(DateTime.UtcNow.Year, model.AcademicYear);
        Assert.Equal(1, model.AcademicSemester);
        Assert.Equal(EnrollmentStatus.Active, model.Status);
        Assert.Null(model.FinalGrade);
        Assert.Empty(model.AvailableStudents);
        Assert.Empty(model.AvailableSubjects);
    }

    [Fact]
    public async Task EnrollmentCreatePageModel_LoadDependenciesAsync_PopulatesStudentsAndSubjects()
    {
        var model = new EnrollmentCreatePageModel(_fakeEnrollmentService, _fakeStudentService, _fakeSubjectService);

        await model.LoadDependenciesAsync();

        Assert.NotEmpty(model.AvailableStudents);
        Assert.NotEmpty(model.AvailableSubjects);
        Assert.Null(model.DependenciesErrorMessage);
    }

    [Fact]
    public async Task EnrollmentCreatePageModel_CreateEnrollmentAsync_MissingStudent_ReturnsFalse()
    {
        var model = new EnrollmentCreatePageModel(_fakeEnrollmentService, _fakeStudentService, _fakeSubjectService)
        {
            StudentId = "",
            SubjectId = "subj-1"
        };

        var success = await model.CreateEnrollmentAsync();

        Assert.False(success);
        Assert.Contains("aluno", model.ErrorMessage);
    }

    [Fact]
    public async Task EnrollmentCreatePageModel_CreateEnrollmentAsync_MissingSubject_ReturnsFalse()
    {
        var model = new EnrollmentCreatePageModel(_fakeEnrollmentService, _fakeStudentService, _fakeSubjectService)
        {
            StudentId = "stu-1",
            SubjectId = ""
        };

        var success = await model.CreateEnrollmentAsync();

        Assert.False(success);
        Assert.Contains("disciplina", model.ErrorMessage);
    }

    [Fact]
    public async Task EnrollmentCreatePageModel_CreateEnrollmentAsync_InvalidAcademicYear_ReturnsFalse()
    {
        var model = new EnrollmentCreatePageModel(_fakeEnrollmentService, _fakeStudentService, _fakeSubjectService)
        {
            StudentId = "stu-1",
            SubjectId = "sub-1",
            AcademicYear = 1990
        };

        var success = await model.CreateEnrollmentAsync();

        Assert.False(success);
        Assert.Contains("ano letivo", model.ErrorMessage);
    }

    [Fact]
    public async Task EnrollmentCreatePageModel_CreateEnrollmentAsync_InvalidFinalGrade_ReturnsFalse()
    {
        var model = new EnrollmentCreatePageModel(_fakeEnrollmentService, _fakeStudentService, _fakeSubjectService)
        {
            StudentId = "stu-1",
            SubjectId = "sub-1",
            FinalGrade = 11.5m
        };

        var success = await model.CreateEnrollmentAsync();

        Assert.False(success);
        Assert.Contains("nota final", model.ErrorMessage);
    }

    [Fact]
    public async Task EnrollmentCreatePageModel_CreateEnrollmentAsync_Success_ReturnsTrueAndSetsMessage()
    {
        var model = new EnrollmentCreatePageModel(_fakeEnrollmentService, _fakeStudentService, _fakeSubjectService)
        {
            StudentId = "student-1",
            SubjectId = "subject-1",
            AcademicYear = 2026,
            AcademicSemester = 1,
            EnrollmentDate = new DateTime(2026, 2, 10),
            Status = EnrollmentStatus.Active,
            FinalGrade = 9.0m
        };

        var success = await model.CreateEnrollmentAsync();

        Assert.True(success);
        Assert.False(model.IsBusy);
        Assert.Contains("sucesso", model.SuccessMessage);
        Assert.NotNull(_fakeEnrollmentService.LastCreatedRequest);
        Assert.Equal("student-1", _fakeEnrollmentService.LastCreatedRequest.StudentId);
        Assert.Equal("subject-1", _fakeEnrollmentService.LastCreatedRequest.SubjectId);
    }

    [Fact]
    public async Task EnrollmentCreatePageModel_CreateEnrollmentAsync_Forbidden_SetsIsForbidden()
    {
        _fakeEnrollmentService.ShouldSucceed = false;
        _fakeEnrollmentService.FailureStatusCode = 403;
        _fakeEnrollmentService.FailureMessage = "Acesso negado";

        var model = new EnrollmentCreatePageModel(_fakeEnrollmentService, _fakeStudentService, _fakeSubjectService)
        {
            StudentId = "student-1",
            SubjectId = "subject-1"
        };

        var success = await model.CreateEnrollmentAsync();

        Assert.False(success);
        Assert.True(model.IsForbidden);
        Assert.Contains("Enrollments.Create", model.ErrorMessage);
    }

    #endregion

    #region EnrollmentEditPageModel Tests

    [Fact]
    public async Task EnrollmentEditPageModel_LoadEnrollmentAsync_Success_PopulatesFields()
    {
        var model = new EnrollmentEditPageModel(_fakeEnrollmentService, _fakeStudentService, _fakeSubjectService);

        var success = await model.LoadEnrollmentAsync("enr-1");

        Assert.True(success);
        Assert.Equal("enr-1", model.Id);
        Assert.Equal("student-1", model.StudentId);
        Assert.Equal("subject-1", model.SubjectId);
        Assert.Equal(2026, model.AcademicYear);
        Assert.Equal(1, model.AcademicSemester);
        Assert.Equal(EnrollmentStatus.Active, model.Status);
        Assert.Equal(8.5m, model.FinalGrade);
    }

    [Fact]
    public async Task EnrollmentEditPageModel_LoadEnrollmentAsync_NotFound_ReturnsFalse()
    {
        var model = new EnrollmentEditPageModel(_fakeEnrollmentService, _fakeStudentService, _fakeSubjectService);

        var success = await model.LoadEnrollmentAsync("non-existent");

        Assert.False(success);
        Assert.Contains("não encontrada", model.ErrorMessage);
    }

    [Fact]
    public async Task EnrollmentEditPageModel_UpdateEnrollmentAsync_Success_ReturnsTrue()
    {
        var model = new EnrollmentEditPageModel(_fakeEnrollmentService, _fakeStudentService, _fakeSubjectService);
        await model.LoadEnrollmentAsync("enr-1");

        model.Status = EnrollmentStatus.Completed;
        model.FinalGrade = 10.0m;

        var success = await model.UpdateEnrollmentAsync();

        Assert.True(success);
        Assert.False(model.IsBusy);
        Assert.Contains("sucesso", model.SuccessMessage);
        Assert.NotNull(_fakeEnrollmentService.LastUpdatedRequest);
        Assert.Equal(EnrollmentStatus.Completed, _fakeEnrollmentService.LastUpdatedRequest.Status);
        Assert.Equal(10.0m, _fakeEnrollmentService.LastUpdatedRequest.FinalGrade);
    }

    [Fact]
    public async Task EnrollmentEditPageModel_UpdateEnrollmentAsync_Forbidden_SetsIsForbidden()
    {
        var model = new EnrollmentEditPageModel(_fakeEnrollmentService, _fakeStudentService, _fakeSubjectService);
        await model.LoadEnrollmentAsync("enr-1");

        _fakeEnrollmentService.ShouldSucceed = false;
        _fakeEnrollmentService.FailureStatusCode = 403;

        var success = await model.UpdateEnrollmentAsync();

        Assert.False(success);
        Assert.True(model.IsForbidden);
        Assert.Contains("Enrollments.Update", model.ErrorMessage);
    }

    #endregion

    #region EnrollmentDetailsPageModel Tests

    [Fact]
    public async Task EnrollmentDetailsPageModel_LoadDetailsAsync_Success_LoadsEnrollmentAndDependencies()
    {
        var model = new EnrollmentDetailsPageModel(_fakeEnrollmentService, _fakeStudentService, _fakeSubjectService);

        var success = await model.LoadDetailsAsync("enr-1");

        Assert.True(success);
        Assert.NotNull(model.Enrollment);
        Assert.Equal("enr-1", model.Enrollment.Id);
        Assert.Null(model.ErrorMessage);
    }

    [Fact]
    public async Task EnrollmentDetailsPageModel_LoadDetailsAsync_NotFound_ReturnsFalse()
    {
        var model = new EnrollmentDetailsPageModel(_fakeEnrollmentService, _fakeStudentService, _fakeSubjectService);

        var success = await model.LoadDetailsAsync("non-existent");

        Assert.False(success);
        Assert.Null(model.Enrollment);
        Assert.Contains("não encontrada", model.ErrorMessage);
    }

    [Fact]
    public async Task EnrollmentDetailsPageModel_LoadDetailsAsync_Forbidden_SetsIsForbidden()
    {
        _fakeEnrollmentService.ShouldSucceed = false;
        _fakeEnrollmentService.FailureStatusCode = 403;

        var model = new EnrollmentDetailsPageModel(_fakeEnrollmentService, _fakeStudentService, _fakeSubjectService);

        var success = await model.LoadDetailsAsync("enr-1");

        Assert.False(success);
        Assert.True(model.IsForbidden);
        Assert.Contains("Enrollments.Read", model.ErrorMessage);
    }

    #endregion
}

