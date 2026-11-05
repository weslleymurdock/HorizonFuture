using System;
using System.Linq;
using System.Threading.Tasks;
using HF.Shared.PageModels.Students;
using HF.Shared.Students.Models;
using HF.UITests.Shared.Fakes;
using Xunit;

namespace HF.UITests.Shared;

public class SharedStudentsPageModelsTests
{
    private readonly FakeStudentService _fakeStudentService = new();

    #region StudentListPageModel Tests

    [Fact]
    public void StudentListPageModel_InitialState_PropertiesAreDefault()
    {
        var model = new StudentListPageModel(_fakeStudentService);

        Assert.Equal("Alunos", model.Title);
        Assert.False(model.IsBusy);
        Assert.False(model.IsForbidden);
        Assert.False(model.IsUnauthorized);
        Assert.False(model.IsDeleting);
        Assert.Null(model.ErrorMessage);
        Assert.Null(model.SuccessMessage);
        Assert.Empty(model.Students);
        Assert.Empty(model.SearchText);
        Assert.Null(model.StatusFilter);
        Assert.Null(model.SchoolUnitIdFilter);
    }

    [Fact]
    public async Task StudentListPageModel_LoadStudentsAsync_Success_PopulatesStudents()
    {
        var model = new StudentListPageModel(_fakeStudentService);

        var success = await model.LoadStudentsAsync();

        Assert.True(success);
        Assert.False(model.IsBusy);
        Assert.Null(model.ErrorMessage);
        Assert.Equal(2, model.Students.Count);
        Assert.Contains(model.Students, s => s.RegistrationNumber == "MAT-2026-001");
        Assert.Contains(model.Students, s => s.RegistrationNumber == "MAT-2026-002");
    }

    [Fact]
    public async Task StudentListPageModel_LoadStudentsAsync_EmptyList_SetsEmptyStudents()
    {
        _fakeStudentService.MockStudents.Clear();
        var model = new StudentListPageModel(_fakeStudentService);

        var success = await model.LoadStudentsAsync();

        Assert.True(success);
        Assert.Empty(model.Students);
        Assert.Null(model.ErrorMessage);
    }

    [Fact]
    public async Task StudentListPageModel_LoadStudentsAsync_ApiFailure_SetsErrorMessage()
    {
        _fakeStudentService.ShouldSucceed = false;
        _fakeStudentService.FailureMessage = "Erro ao buscar alunos do servidor";
        _fakeStudentService.FailureStatusCode = 500;

        var model = new StudentListPageModel(_fakeStudentService);

        var success = await model.LoadStudentsAsync();

        Assert.False(success);
        Assert.False(model.IsBusy);
        Assert.Equal("Erro ao buscar alunos do servidor", model.ErrorMessage);
        Assert.Empty(model.Students);
    }

    [Fact]
    public async Task StudentListPageModel_LoadStudentsAsync_Forbidden_SetsIsForbidden()
    {
        _fakeStudentService.ShouldSucceed = false;
        _fakeStudentService.FailureMessage = "Acesso negado: permissão Students.Read ausente";
        _fakeStudentService.FailureStatusCode = 403;

        var model = new StudentListPageModel(_fakeStudentService);

        var success = await model.LoadStudentsAsync();

        Assert.False(success);
        Assert.True(model.IsForbidden);
        Assert.False(model.IsUnauthorized);
        Assert.NotNull(model.ErrorMessage);
    }

    [Fact]
    public async Task StudentListPageModel_LoadStudentsAsync_Unauthorized_SetsIsUnauthorized()
    {
        _fakeStudentService.ShouldSucceed = false;
        _fakeStudentService.FailureMessage = "Sessão expirada";
        _fakeStudentService.FailureStatusCode = 401;

        var model = new StudentListPageModel(_fakeStudentService);

        var success = await model.LoadStudentsAsync();

        Assert.False(success);
        Assert.False(model.IsForbidden);
        Assert.True(model.IsUnauthorized);
        Assert.NotNull(model.ErrorMessage);
    }

    [Fact]
    public async Task StudentListPageModel_FilteredStudents_FiltersByRegistrationNumberAndGuardian()
    {
        var model = new StudentListPageModel(_fakeStudentService);
        await model.LoadStudentsAsync();

        model.SearchText = "MAT-2026-001";
        var filtered = model.FilteredStudents.ToList();
        Assert.Single(filtered);
        Assert.Equal("MAT-2026-001", filtered[0].RegistrationNumber);

        model.SearchText = "João";
        var filteredGuardian = model.FilteredStudents.ToList();
        Assert.Single(filteredGuardian);
        Assert.Equal("MAT-2026-002", filteredGuardian[0].RegistrationNumber);

        model.SearchText = string.Empty;
        Assert.Equal(2, model.FilteredStudents.Count());
    }

    [Fact]
    public async Task StudentListPageModel_ResetFiltersAsync_ClearsFiltersAndReloads()
    {
        var model = new StudentListPageModel(_fakeStudentService)
        {
            SearchText = "filtrado",
            StatusFilter = StudentStatus.Suspended,
            SchoolUnitIdFilter = "unit-xyz"
        };

        await model.ResetFiltersAsync();

        Assert.Empty(model.SearchText);
        Assert.Null(model.StatusFilter);
        Assert.Null(model.SchoolUnitIdFilter);
        Assert.Equal(2, model.Students.Count);
    }

    [Fact]
    public async Task StudentListPageModel_DeleteStudentAsync_Success_RemovesFromCollection()
    {
        var model = new StudentListPageModel(_fakeStudentService);
        await model.LoadStudentsAsync();

        var success = await model.DeleteStudentAsync("student-1");

        Assert.True(success);
        Assert.Single(model.Students);
        Assert.DoesNotContain(model.Students, s => s.Id == "student-1");
        Assert.Equal("Aluno excluído com sucesso.", model.SuccessMessage);
    }

    [Fact]
    public async Task StudentListPageModel_DeleteStudentAsync_Conflict_SetsConflictMessage()
    {
        _fakeStudentService.ShouldSucceed = false;
        _fakeStudentService.FailureStatusCode = 409;
        _fakeStudentService.FailureMessage = "Aluno possui matrículas ativas";

        var model = new StudentListPageModel(_fakeStudentService);
        await model.LoadStudentsAsync();

        var success = await model.DeleteStudentAsync("student-1");

        Assert.False(success);
        Assert.Contains("matrículas vinculadas", model.ErrorMessage);
    }

    [Fact]
    public async Task StudentListPageModel_DeleteStudentAsync_EmptyId_ReturnsFalse()
    {
        var model = new StudentListPageModel(_fakeStudentService);

        var success = await model.DeleteStudentAsync(string.Empty);

        Assert.False(success);
        Assert.NotNull(model.ErrorMessage);
    }

    #endregion

    #region StudentCreatePageModel Tests

    [Theory]
    [InlineData("", "MAT-001", "2012-01-01", "UserId")]
    [InlineData("user-1", "", "2012-01-01", "matrícula")]
    [InlineData("user-1", "MAT-001", "2099-01-01", "passada")]
    public void StudentCreatePageModel_Validate_InvalidFields_SetsErrorMessage(string userId, string regNum, string dobString, string expectedSubstring)
    {
        var model = new StudentCreatePageModel(_fakeStudentService)
        {
            UserId = userId,
            RegistrationNumber = regNum,
            DateOfBirth = DateTime.Parse(dobString)
        };

        var isValid = model.Validate();

        Assert.False(isValid);
        Assert.NotNull(model.ErrorMessage);
        Assert.Contains(expectedSubstring, model.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void StudentCreatePageModel_Validate_ExceedingLength_FailsValidation()
    {
        var model = new StudentCreatePageModel(_fakeStudentService)
        {
            UserId = "user-1",
            RegistrationNumber = new string('A', 51),
            DateOfBirth = DateTime.Today.AddYears(-12)
        };

        var isValid = model.Validate();

        Assert.False(isValid);
        Assert.Contains("50 caracteres", model.ErrorMessage);
    }

    [Fact]
    public async Task StudentCreatePageModel_CreateStudentAsync_ValidInput_CreatesSuccessfully()
    {
        var model = new StudentCreatePageModel(_fakeStudentService)
        {
            UserId = "user-synth-3",
            RegistrationNumber = "MAT-2026-003",
            DateOfBirth = new DateTime(2012, 4, 10),
            GuardianName = "Ana Pereira",
            GuardianRelationship = "Mãe",
            GuardianPhone = "(11) 97777-8888",
            SchoolUnitId = "unit-central",
            Status = StudentStatus.Active
        };

        var success = await model.CreateStudentAsync();

        Assert.True(success);
        Assert.True(model.IsCreated);
        Assert.NotNull(model.CreatedStudent);
        Assert.Equal("MAT-2026-003", model.CreatedStudent.RegistrationNumber);
        Assert.Equal("Aluno cadastrado com sucesso.", model.SuccessMessage);
    }

    [Fact]
    public async Task StudentCreatePageModel_CreateStudentAsync_ApiFailure_PreservesInputs()
    {
        _fakeStudentService.ShouldSucceed = false;
        _fakeStudentService.FailureMessage = "Erro no servidor ao criar aluno";

        var model = new StudentCreatePageModel(_fakeStudentService)
        {
            UserId = "user-1",
            RegistrationNumber = "MAT-PRESERVED",
            DateOfBirth = DateTime.Today.AddYears(-10)
        };

        var success = await model.CreateStudentAsync();

        Assert.False(success);
        Assert.False(model.IsCreated);
        Assert.Equal("MAT-PRESERVED", model.RegistrationNumber);
        Assert.Equal("Erro no servidor ao criar aluno", model.ErrorMessage);
    }

    [Fact]
    public async Task StudentCreatePageModel_CreateStudentAsync_Conflict_SetsErrorMessage()
    {
        _fakeStudentService.ShouldSucceed = false;
        _fakeStudentService.FailureStatusCode = 409;

        var model = new StudentCreatePageModel(_fakeStudentService)
        {
            UserId = "user-1",
            RegistrationNumber = "MAT-EXISTING",
            DateOfBirth = DateTime.Today.AddYears(-10)
        };

        var success = await model.CreateStudentAsync();

        Assert.False(success);
        Assert.Contains("matrícula já está em uso", model.ErrorMessage);
    }

    [Fact]
    public async Task StudentCreatePageModel_CreateStudentAsync_WhileBusy_PreventsDoubleSubmission()
    {
        var model = new StudentCreatePageModel(_fakeStudentService)
        {
            UserId = "user-1",
            RegistrationNumber = "MAT-001",
            DateOfBirth = DateTime.Today.AddYears(-10),
            IsBusy = true
        };

        var success = await model.CreateStudentAsync();

        Assert.False(success);
        Assert.False(model.IsCreated);
    }

    [Fact]
    public void StudentCreatePageModel_ResetForm_ClearsFields()
    {
        var model = new StudentCreatePageModel(_fakeStudentService)
        {
            UserId = "user-1",
            RegistrationNumber = "MAT-001",
            GuardianName = "Nome",
            ErrorMessage = "Erro",
            IsCreated = true
        };

        model.ResetForm();

        Assert.Empty(model.UserId);
        Assert.Empty(model.RegistrationNumber);
        Assert.Null(model.GuardianName);
        Assert.Null(model.ErrorMessage);
        Assert.False(model.IsCreated);
    }

    #endregion

    #region StudentDetailsPageModel Tests

    [Fact]
    public async Task StudentDetailsPageModel_LoadStudentAsync_Success_PopulatesStudent()
    {
        var model = new StudentDetailsPageModel(_fakeStudentService);

        var success = await model.LoadStudentAsync("student-1");

        Assert.True(success);
        Assert.NotNull(model.Student);
        Assert.Equal("MAT-2026-001", model.Student.RegistrationNumber);
        Assert.Equal("Aluno: MAT-2026-001", model.Title);
        Assert.False(model.IsNotFound);
    }

    [Fact]
    public async Task StudentDetailsPageModel_LoadStudentAsync_NotFound_SetsIsNotFound()
    {
        var model = new StudentDetailsPageModel(_fakeStudentService);

        var success = await model.LoadStudentAsync("non-existent");

        Assert.False(success);
        Assert.Null(model.Student);
        Assert.True(model.IsNotFound);
        Assert.Equal("Aluno não encontrado.", model.ErrorMessage);
    }

    [Fact]
    public async Task StudentDetailsPageModel_LoadStudentAsync_Forbidden_SetsIsForbidden()
    {
        _fakeStudentService.ShouldSucceed = false;
        _fakeStudentService.FailureStatusCode = 403;

        var model = new StudentDetailsPageModel(_fakeStudentService);

        var success = await model.LoadStudentAsync("student-1");

        Assert.False(success);
        Assert.True(model.IsForbidden);
    }

    [Fact]
    public async Task StudentDetailsPageModel_LoadStudentAsync_EmptyId_SetsNotFound()
    {
        var model = new StudentDetailsPageModel(_fakeStudentService);

        var success = await model.LoadStudentAsync(string.Empty);

        Assert.False(success);
        Assert.True(model.IsNotFound);
    }

    [Fact]
    public async Task StudentDetailsPageModel_DeleteStudentAsync_Success_SetsIsDeleted()
    {
        var model = new StudentDetailsPageModel(_fakeStudentService);
        await model.LoadStudentAsync("student-1");

        var success = await model.DeleteStudentAsync();

        Assert.True(success);
        Assert.True(model.IsDeleted);
        Assert.Equal("Aluno excluído com sucesso.", model.SuccessMessage);
    }

    [Fact]
    public async Task StudentDetailsPageModel_DeleteStudentAsync_Conflict_SetsErrorMessage()
    {
        var model = new StudentDetailsPageModel(_fakeStudentService);
        await model.LoadStudentAsync("student-1");

        _fakeStudentService.ShouldSucceed = false;
        _fakeStudentService.FailureStatusCode = 409;

        var success = await model.DeleteStudentAsync();

        Assert.False(success);
        Assert.False(model.IsDeleted);
        Assert.Contains("matrículas vinculadas", model.ErrorMessage);
    }

    #endregion

    #region StudentEditPageModel Tests

    [Fact]
    public async Task StudentEditPageModel_LoadStudentAsync_Success_PopulatesFields()
    {
        var model = new StudentEditPageModel(_fakeStudentService);

        var success = await model.LoadStudentAsync("student-1");

        Assert.True(success);
        Assert.Equal("student-1", model.StudentId);
        Assert.Equal("MAT-2026-001", model.RegistrationNumber);
        Assert.Equal("Maria da Silva", model.GuardianName);
        Assert.Equal(StudentStatus.Active, model.Status);
        Assert.False(model.IsNotFound);
    }

    [Fact]
    public async Task StudentEditPageModel_LoadStudentAsync_NotFound_SetsIsNotFound()
    {
        var model = new StudentEditPageModel(_fakeStudentService);

        var success = await model.LoadStudentAsync("unknown");

        Assert.False(success);
        Assert.True(model.IsNotFound);
        Assert.Equal("Aluno não encontrado.", model.ErrorMessage);
    }

    [Theory]
    [InlineData("", "MAT-001", "2012-01-01", "obrigatório")]
    [InlineData("id-1", "", "2012-01-01", "matrícula")]
    [InlineData("id-1", "MAT-001", "2099-01-01", "passada")]
    public void StudentEditPageModel_Validate_InvalidFields_SetsErrorMessage(string id, string regNum, string dobString, string expectedSub)
    {
        var model = new StudentEditPageModel(_fakeStudentService)
        {
            StudentId = id,
            RegistrationNumber = regNum,
            DateOfBirth = DateTime.Parse(dobString)
        };

        var isValid = model.Validate();

        Assert.False(isValid);
        Assert.Contains(expectedSub, model.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task StudentEditPageModel_SaveStudentAsync_Success_SetsIsSaved()
    {
        var model = new StudentEditPageModel(_fakeStudentService);
        await model.LoadStudentAsync("student-1");

        model.RegistrationNumber = "MAT-2026-UPDATED";
        var success = await model.SaveStudentAsync();

        Assert.True(success);
        Assert.True(model.IsSaved);
        Assert.Equal("Aluno atualizado com sucesso.", model.SuccessMessage);

        var updated = _fakeStudentService.MockStudents.First(s => s.Id == "student-1");
        Assert.Equal("MAT-2026-UPDATED", updated.RegistrationNumber);
    }

    [Fact]
    public async Task StudentEditPageModel_SaveStudentAsync_Conflict_SetsErrorMessage()
    {
        var model = new StudentEditPageModel(_fakeStudentService);
        await model.LoadStudentAsync("student-1");

        _fakeStudentService.ShouldSucceed = false;
        _fakeStudentService.FailureStatusCode = 409;

        var success = await model.SaveStudentAsync();

        Assert.False(success);
        Assert.Contains("matrícula já está em uso", model.ErrorMessage);
    }

    [Fact]
    public async Task StudentEditPageModel_SaveStudentAsync_NotFound_SetsIsNotFound()
    {
        var model = new StudentEditPageModel(_fakeStudentService);
        await model.LoadStudentAsync("student-1");

        _fakeStudentService.ShouldSucceed = false;
        _fakeStudentService.FailureStatusCode = 404;

        var success = await model.SaveStudentAsync();

        Assert.False(success);
        Assert.True(model.IsNotFound);
    }

    [Fact]
    public async Task StudentEditPageModel_SaveStudentAsync_WhileBusy_PreventsDoubleSubmission()
    {
        var model = new StudentEditPageModel(_fakeStudentService);
        await model.LoadStudentAsync("student-1");

        model.IsBusy = true;
        var success = await model.SaveStudentAsync();

        Assert.False(success);
        Assert.False(model.IsSaved);
    }

    #endregion
}

