using System;
using System.Linq;
using System.Threading.Tasks;
using HF.Shared.PageModels.Teachers;
using HF.Shared.Teachers.Models;
using HF.UITests.Shared.Fakes;
using Xunit;

namespace HF.UITests.Shared;

public class SharedTeachersPageModelsTests
{
    private readonly FakeTeacherService _fakeService;

    public SharedTeachersPageModelsTests()
    {
        _fakeService = new FakeTeacherService();
    }

    #region 1. TeacherListPageModel Tests

    [Fact]
    public void TeacherListPageModel_InitialState_IsClean()
    {
        var model = new TeacherListPageModel(_fakeService);

        Assert.Equal("Professores", model.Title);
        Assert.False(model.IsBusy);
        Assert.False(model.IsForbidden);
        Assert.False(model.IsUnauthorized);
        Assert.Null(model.ErrorMessage);
        Assert.Null(model.SuccessMessage);
        Assert.Empty(model.Teachers);
        Assert.Empty(model.SearchText);
        Assert.Null(model.StatusFilter);
        Assert.Null(model.SchoolUnitIdFilter);
        Assert.Null(model.SpecialtyFilter);
    }

    [Fact]
    public async Task TeacherListPageModel_LoadTeachersAsync_PopulatesCollection()
    {
        var model = new TeacherListPageModel(_fakeService);

        var success = await model.LoadTeachersAsync();

        Assert.True(success);
        Assert.Equal(3, model.Teachers.Count);
        Assert.False(model.IsBusy);
        Assert.Null(model.ErrorMessage);
    }

    [Fact]
    public async Task TeacherListPageModel_LoadTeachersAsync_WithFilters_AppliesCorrectCriteria()
    {
        var model = new TeacherListPageModel(_fakeService)
        {
            StatusFilter = TeacherStatus.OnLeave,
            SchoolUnitIdFilter = "unit-2",
            SpecialtyFilter = "Ciências"
        };

        var success = await model.LoadTeachersAsync();

        Assert.True(success);
        Assert.Single(model.Teachers);
        Assert.Equal("TCH-003", model.Teachers[0].EmployeeNumber);
    }

    [Fact]
    public async Task TeacherListPageModel_FilteredTeachers_PerformsClientSideSearch()
    {
        var model = new TeacherListPageModel(_fakeService);
        await model.LoadTeachersAsync();

        // Search by employee number
        model.SearchText = "TCH-002";
        Assert.Single(model.FilteredTeachers);
        Assert.Equal("TCH-002", model.FilteredTeachers.First().EmployeeNumber);

        // Search by specialty
        model.SearchText = "Matemática";
        Assert.Single(model.FilteredTeachers);

        // Search case-insensitive
        model.SearchText = "português";
        Assert.Single(model.FilteredTeachers);

        // Empty search returns all
        model.SearchText = "  ";
        Assert.Equal(3, model.FilteredTeachers.Count());
    }

    [Fact]
    public async Task TeacherListPageModel_LoadTeachersAsync_401Unauthorized_SetsIsUnauthorized()
    {
        _fakeService.ShouldFail = true;
        _fakeService.FailureStatusCode = 401;
        _fakeService.FailureMessage = "Sessão expirada.";

        var model = new TeacherListPageModel(_fakeService);
        var success = await model.LoadTeachersAsync();

        Assert.False(success);
        Assert.True(model.IsUnauthorized);
        Assert.False(model.IsForbidden);
        Assert.Equal("Sessão expirada.", model.ErrorMessage);
    }

    [Fact]
    public async Task TeacherListPageModel_LoadTeachersAsync_403Forbidden_SetsIsForbidden()
    {
        _fakeService.ShouldFail = true;
        _fakeService.FailureStatusCode = 403;
        _fakeService.FailureMessage = "Acesso negado.";

        var model = new TeacherListPageModel(_fakeService);
        var success = await model.LoadTeachersAsync();

        Assert.False(success);
        Assert.True(model.IsForbidden);
        Assert.False(model.IsUnauthorized);
        Assert.Equal("Acesso negado.", model.ErrorMessage);
    }

    [Fact]
    public async Task TeacherListPageModel_DeleteTeacherAsync_RemovesItemFromCollection()
    {
        var model = new TeacherListPageModel(_fakeService);
        await model.LoadTeachersAsync();

        var success = await model.DeleteTeacherAsync("teacher-1");

        Assert.True(success);
        Assert.Equal(2, model.Teachers.Count);
        Assert.DoesNotContain(model.Teachers, t => t.Id == "teacher-1");
        Assert.Equal("Professor excluído com sucesso.", model.SuccessMessage);
    }

    [Fact]
    public async Task TeacherListPageModel_ResetFiltersAsync_ClearsFiltersAndReloads()
    {
        var model = new TeacherListPageModel(_fakeService)
        {
            SearchText = "Matemática",
            StatusFilter = TeacherStatus.Active,
            SchoolUnitIdFilter = "unit-1",
            SpecialtyFilter = "Matemática"
        };

        await model.ResetFiltersAsync();

        Assert.Empty(model.SearchText);
        Assert.Null(model.StatusFilter);
        Assert.Null(model.SchoolUnitIdFilter);
        Assert.Null(model.SpecialtyFilter);
        Assert.Equal(3, model.Teachers.Count);
    }

    #endregion

    #region 2. TeacherCreatePageModel Tests

    [Fact]
    public void TeacherCreatePageModel_InitialState_IsClean()
    {
        var model = new TeacherCreatePageModel(_fakeService);

        Assert.Equal("Cadastrar Professor", model.Title);
        Assert.Empty(model.UserId);
        Assert.Empty(model.EmployeeNumber);
        Assert.Empty(model.Specialty);
        Assert.Null(model.AcademicDegree);
        Assert.Null(model.Bio);
        Assert.Null(model.SchoolUnitId);
        Assert.Null(model.HireDate);
        Assert.Equal(TeacherStatus.Active, model.Status);
        Assert.Null(model.CreatedTeacher);
    }

    [Fact]
    public async Task TeacherCreatePageModel_Validation_FailsWhenUserIdIsEmpty()
    {
        var model = new TeacherCreatePageModel(_fakeService)
        {
            UserId = "",
            EmployeeNumber = "TCH-999",
            Specialty = "Física"
        };

        var success = await model.CreateTeacherAsync();

        Assert.False(success);
        Assert.Contains("UserId", model.ErrorMessage);
    }

    [Fact]
    public async Task TeacherCreatePageModel_Validation_FailsWhenEmployeeNumberIsEmpty()
    {
        var model = new TeacherCreatePageModel(_fakeService)
        {
            UserId = "user-123",
            EmployeeNumber = "   ",
            Specialty = "Física"
        };

        var success = await model.CreateTeacherAsync();

        Assert.False(success);
        Assert.Contains("matrícula", model.ErrorMessage);
    }

    [Fact]
    public async Task TeacherCreatePageModel_Validation_FailsWhenEmployeeNumberExceeds50Chars()
    {
        var model = new TeacherCreatePageModel(_fakeService)
        {
            UserId = "user-123",
            EmployeeNumber = new string('A', 51),
            Specialty = "Física"
        };

        var success = await model.CreateTeacherAsync();

        Assert.False(success);
        Assert.Contains("50 caracteres", model.ErrorMessage);
    }

    [Fact]
    public async Task TeacherCreatePageModel_Validation_FailsWhenSpecialtyIsEmpty()
    {
        var model = new TeacherCreatePageModel(_fakeService)
        {
            UserId = "user-123",
            EmployeeNumber = "TCH-999",
            Specialty = " "
        };

        var success = await model.CreateTeacherAsync();

        Assert.False(success);
        Assert.Contains("especialidade", model.ErrorMessage);
    }

    [Fact]
    public async Task TeacherCreatePageModel_CreateTeacherAsync_Success_CreatesTeacher()
    {
        var model = new TeacherCreatePageModel(_fakeService)
        {
            UserId = "user-200",
            EmployeeNumber = "TCH-100",
            Specialty = "História Geral",
            AcademicDegree = "Licenciatura em História",
            Bio = "Professor experiente de história",
            SchoolUnitId = "unit-1",
            HireDate = new DateTime(2024, 3, 1),
            Status = TeacherStatus.Active
        };

        var success = await model.CreateTeacherAsync();

        Assert.True(success);
        Assert.NotNull(model.CreatedTeacher);
        Assert.Equal("TCH-100", model.CreatedTeacher.EmployeeNumber);
        Assert.Equal("História Geral", model.CreatedTeacher.Specialty);
        Assert.NotNull(model.SuccessMessage);
    }

    [Fact]
    public async Task TeacherCreatePageModel_CreateTeacherAsync_ConflictError_HandledProperly()
    {
        var model = new TeacherCreatePageModel(_fakeService)
        {
            UserId = "user-201",
            EmployeeNumber = "TCH-001", // Duplicate employee number seeded in fake
            Specialty = "Artes"
        };

        var success = await model.CreateTeacherAsync();

        Assert.False(success);
        Assert.Contains("já está em uso", model.ErrorMessage);
    }

    #endregion

    #region 3. TeacherDetailsPageModel Tests

    [Fact]
    public void TeacherDetailsPageModel_InitialState_IsClean()
    {
        var model = new TeacherDetailsPageModel(_fakeService);

        Assert.Equal("Detalhes do Professor", model.Title);
        Assert.Null(model.Teacher);
        Assert.False(model.IsNotFound);
        Assert.False(model.IsBusy);
    }

    [Fact]
    public async Task TeacherDetailsPageModel_LoadTeacherAsync_Success_LoadsData()
    {
        var model = new TeacherDetailsPageModel(_fakeService);

        var success = await model.LoadTeacherAsync("teacher-1");

        Assert.True(success);
        Assert.NotNull(model.Teacher);
        Assert.Equal("TCH-001", model.Teacher.EmployeeNumber);
        Assert.Equal("Matemática", model.Teacher.Specialty);
        Assert.Contains("TCH-001", model.Title);
    }

    [Fact]
    public async Task TeacherDetailsPageModel_LoadTeacherAsync_NonExistentId_SetsIsNotFound()
    {
        var model = new TeacherDetailsPageModel(_fakeService);

        var success = await model.LoadTeacherAsync("non-existent-id");

        Assert.False(success);
        Assert.True(model.IsNotFound);
        Assert.Null(model.Teacher);
    }

    [Fact]
    public async Task TeacherDetailsPageModel_DeleteTeacherAsync_Success_DeletesCurrentTeacher()
    {
        var model = new TeacherDetailsPageModel(_fakeService);
        await model.LoadTeacherAsync("teacher-2");

        var success = await model.DeleteTeacherAsync();

        Assert.True(success);
        Assert.Equal("teacher-2", _fakeService.LastDeletedId);
        Assert.NotNull(model.SuccessMessage);
    }

    #endregion

    #region 4. TeacherEditPageModel Tests

    [Fact]
    public void TeacherEditPageModel_InitialState_IsClean()
    {
        var model = new TeacherEditPageModel(_fakeService);

        Assert.Equal("Editar Professor", model.Title);
        Assert.Empty(model.Id);
        Assert.Empty(model.EmployeeNumber);
        Assert.Empty(model.Specialty);
    }

    [Fact]
    public async Task TeacherEditPageModel_LoadTeacherAsync_Success_PopulatesFields()
    {
        var model = new TeacherEditPageModel(_fakeService);

        var success = await model.LoadTeacherAsync("teacher-1");

        Assert.True(success);
        Assert.Equal("teacher-1", model.Id);
        Assert.Equal("TCH-001", model.EmployeeNumber);
        Assert.Equal("Matemática", model.Specialty);
        Assert.Equal("Licenciatura em Matemática", model.AcademicDegree);
        Assert.Equal("unit-1", model.SchoolUnitId);
        Assert.Equal(TeacherStatus.Active, model.Status);
    }

    [Fact]
    public async Task TeacherEditPageModel_SaveTeacherAsync_Validation_FailsWhenSpecialtyIsEmpty()
    {
        var model = new TeacherEditPageModel(_fakeService);
        await model.LoadTeacherAsync("teacher-1");

        model.Specialty = "";
        var success = await model.SaveTeacherAsync();

        Assert.False(success);
        Assert.Contains("especialidade", model.ErrorMessage);
    }

    [Fact]
    public async Task TeacherEditPageModel_SaveTeacherAsync_Success_UpdatesTeacher()
    {
        var model = new TeacherEditPageModel(_fakeService);
        await model.LoadTeacherAsync("teacher-1");

        model.Specialty = "Matemática Avançada";
        model.Status = TeacherStatus.OnLeave;
        model.AcademicDegree = "Doutorado em Educação Matemática";

        var success = await model.SaveTeacherAsync();

        Assert.True(success);
        Assert.NotNull(model.SuccessMessage);
        Assert.Equal("Matemática Avançada", model.Teacher?.Specialty);
        Assert.Equal(TeacherStatus.OnLeave, model.Teacher?.Status);
    }

    [Fact]
    public async Task TeacherEditPageModel_SaveTeacherAsync_Conflict_HandledProperly()
    {
        var model = new TeacherEditPageModel(_fakeService);
        await model.LoadTeacherAsync("teacher-1");

        // Try to rename employee number to another teacher's employee number
        model.EmployeeNumber = "TCH-002";

        var success = await model.SaveTeacherAsync();

        Assert.False(success);
        Assert.Contains("já está em uso", model.ErrorMessage);
    }

    #endregion
}

