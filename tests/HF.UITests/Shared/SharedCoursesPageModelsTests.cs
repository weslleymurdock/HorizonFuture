using System.Linq;
using System.Threading.Tasks;
using HF.Shared.Courses.Models;
using HF.Shared.PageModels.Courses;
using HF.UITests.Shared.Fakes;
using Xunit;

namespace HF.UITests.Shared;

public class SharedCoursesPageModelsTests
{
    private readonly FakeCourseService _fakeCourseService = new();

    #region CourseListPageModel Tests

    [Fact]
    public void CourseListPageModel_InitialState_PropertiesAreDefault()
    {
        var model = new CourseListPageModel(_fakeCourseService);

        Assert.Equal("Cursos", model.Title);
        Assert.False(model.IsBusy);
        Assert.False(model.IsForbidden);
        Assert.False(model.IsUnauthorized);
        Assert.False(model.IsDeleting);
        Assert.Null(model.ErrorMessage);
        Assert.Null(model.SuccessMessage);
        Assert.Empty(model.Courses);
        Assert.Empty(model.SearchText);
        Assert.Null(model.StatusFilter);
        Assert.Null(model.SchoolUnitIdFilter);
    }

    [Fact]
    public async Task CourseListPageModel_LoadCoursesAsync_Success_PopulatesCourses()
    {
        var model = new CourseListPageModel(_fakeCourseService);

        var success = await model.LoadCoursesAsync();

        Assert.True(success);
        Assert.False(model.IsBusy);
        Assert.Null(model.ErrorMessage);
        Assert.Equal(2, model.Courses.Count);
        Assert.Contains(model.Courses, c => c.Code == "ALF-101");
        Assert.Contains(model.Courses, c => c.Code == "ROB-201");
    }

    [Fact]
    public async Task CourseListPageModel_LoadCoursesAsync_EmptyList_SetsEmptyCourses()
    {
        _fakeCourseService.MockCourses.Clear();
        var model = new CourseListPageModel(_fakeCourseService);

        var success = await model.LoadCoursesAsync();

        Assert.True(success);
        Assert.Empty(model.Courses);
        Assert.Null(model.ErrorMessage);
    }

    [Fact]
    public async Task CourseListPageModel_LoadCoursesAsync_ApiFailure_SetsErrorMessage()
    {
        _fakeCourseService.ShouldSucceed = false;
        _fakeCourseService.FailureMessage = "Erro ao buscar cursos do servidor";
        _fakeCourseService.FailureStatusCode = 500;

        var model = new CourseListPageModel(_fakeCourseService);

        var success = await model.LoadCoursesAsync();

        Assert.False(success);
        Assert.False(model.IsBusy);
        Assert.Equal("Erro ao buscar cursos do servidor", model.ErrorMessage);
        Assert.Empty(model.Courses);
    }

    [Fact]
    public async Task CourseListPageModel_LoadCoursesAsync_Forbidden_SetsIsForbidden()
    {
        _fakeCourseService.ShouldSucceed = false;
        _fakeCourseService.FailureMessage = "Acesso negado: permissão Courses.Read ausente";
        _fakeCourseService.FailureStatusCode = 403;

        var model = new CourseListPageModel(_fakeCourseService);

        var success = await model.LoadCoursesAsync();

        Assert.False(success);
        Assert.True(model.IsForbidden);
        Assert.False(model.IsUnauthorized);
        Assert.NotNull(model.ErrorMessage);
    }

    [Fact]
    public async Task CourseListPageModel_LoadCoursesAsync_Unauthorized_SetsIsUnauthorized()
    {
        _fakeCourseService.ShouldSucceed = false;
        _fakeCourseService.FailureMessage = "Sessão expirada";
        _fakeCourseService.FailureStatusCode = 401;

        var model = new CourseListPageModel(_fakeCourseService);

        var success = await model.LoadCoursesAsync();

        Assert.False(success);
        Assert.True(model.IsUnauthorized);
        Assert.False(model.IsForbidden);
    }

    [Fact]
    public async Task CourseListPageModel_FilteredCourses_FiltersByNameAndCode()
    {
        var model = new CourseListPageModel(_fakeCourseService);
        await model.LoadCoursesAsync();

        // Filter by partial name
        model.SearchText = "Robótica";
        var filteredByName = model.FilteredCourses.ToList();
        Assert.Single(filteredByName);
        Assert.Equal("ROB-201", filteredByName[0].Code);

        // Filter by partial code
        model.SearchText = "ALF";
        var filteredByCode = model.FilteredCourses.ToList();
        Assert.Single(filteredByCode);
        Assert.Equal("Alfabetização Digital", filteredByCode[0].Name);

        // Filter with empty text returns all
        model.SearchText = string.Empty;
        Assert.Equal(2, model.FilteredCourses.Count());
    }

    [Fact]
    public async Task CourseListPageModel_ResetFiltersAsync_ClearsFiltersAndReloads()
    {
        var model = new CourseListPageModel(_fakeCourseService)
        {
            SearchText = "xyz",
            StatusFilter = CourseStatus.Archived,
            SchoolUnitIdFilter = "unit-123"
        };

        await model.ResetFiltersAsync();

        Assert.Equal(string.Empty, model.SearchText);
        Assert.Null(model.StatusFilter);
        Assert.Null(model.SchoolUnitIdFilter);
        Assert.Equal(2, model.Courses.Count);
    }

    [Fact]
    public async Task CourseListPageModel_DeleteCourseAsync_Success_RemovesFromCollection()
    {
        var model = new CourseListPageModel(_fakeCourseService);
        await model.LoadCoursesAsync();
        Assert.Equal(2, model.Courses.Count);

        var success = await model.DeleteCourseAsync("course-1");

        Assert.True(success);
        Assert.False(model.IsDeleting);
        Assert.Null(model.DeletingCourseId);
        Assert.Single(model.Courses);
        Assert.DoesNotContain(model.Courses, c => c.Id == "course-1");
        Assert.NotNull(model.SuccessMessage);
    }

    [Fact]
    public async Task CourseListPageModel_DeleteCourseAsync_Failure_SetsErrorMessage()
    {
        _fakeCourseService.ShouldSucceed = false;
        _fakeCourseService.FailureMessage = "Conflito: o curso possui turmas vinculadas";
        _fakeCourseService.FailureStatusCode = 409;

        var model = new CourseListPageModel(_fakeCourseService);
        await model.LoadCoursesAsync();

        var success = await model.DeleteCourseAsync("course-1");

        Assert.False(success);
        Assert.False(model.IsDeleting);
        Assert.Equal("Conflito: o curso possui turmas vinculadas", model.ErrorMessage);
    }

    [Fact]
    public async Task CourseListPageModel_DeleteCourseAsync_EmptyId_ReturnsFalse()
    {
        var model = new CourseListPageModel(_fakeCourseService);

        var success = await model.DeleteCourseAsync(string.Empty);

        Assert.False(success);
        Assert.False(model.IsDeleting);
    }

    #endregion

    #region CourseCreatePageModel Tests

    [Theory]
    [InlineData("", "MAT-101", 40, "nome do curso é obrigatório")]
    [InlineData("   ", "MAT-101", 40, "nome do curso é obrigatório")]
    [InlineData("Curso Teste", "", 40, "código do curso é obrigatório")]
    [InlineData("Curso Teste", "   ", 40, "código do curso é obrigatório")]
    [InlineData("Curso Teste", "MAT-101", 0, "carga horária deve ser maior que zero")]
    [InlineData("Curso Teste", "MAT-101", -5, "carga horária deve ser maior que zero")]
    public void CourseCreatePageModel_Validate_InvalidFields_SetsErrorMessage(string name, string code, int hours, string expectedSubstring)
    {
        var model = new CourseCreatePageModel(_fakeCourseService)
        {
            Name = name,
            Code = code,
            WorkloadHours = hours
        };

        var isValid = model.Validate();

        Assert.False(isValid);
        Assert.NotNull(model.ErrorMessage);
        Assert.Contains(expectedSubstring, model.ErrorMessage);
    }

    [Fact]
    public void CourseCreatePageModel_Validate_ExceedingLength_FailsValidation()
    {
        var model = new CourseCreatePageModel(_fakeCourseService)
        {
            Name = new string('A', 151),
            Code = "MAT-101",
            WorkloadHours = 40
        };

        Assert.False(model.Validate());
        Assert.Contains("150 caracteres", model.ErrorMessage);

        model.Name = "Curso Válido";
        model.Code = new string('B', 31);
        Assert.False(model.Validate());
        Assert.Contains("30 caracteres", model.ErrorMessage);
    }

    [Fact]
    public async Task CourseCreatePageModel_CreateCourseAsync_ValidInput_CreatesSuccessfully()
    {
        var model = new CourseCreatePageModel(_fakeCourseService)
        {
            Name = "Desenvolvimento Web",
            Code = "WEB-202",
            WorkloadHours = 120,
            Description = "HTML, CSS e JavaScript",
            TargetAudience = "Estudantes do ensino médio",
            SchoolUnitId = "unit-sul",
            Status = CourseStatus.Active
        };

        var result = await model.CreateCourseAsync();

        Assert.True(result);
        Assert.True(model.IsCreated);
        Assert.NotNull(model.CreatedCourse);
        Assert.Equal("Desenvolvimento Web", model.CreatedCourse.Name);
        Assert.Equal("WEB-202", model.CreatedCourse.Code);
        Assert.Equal(120, model.CreatedCourse.WorkloadHours);
        Assert.NotNull(model.SuccessMessage);
        Assert.Null(model.ErrorMessage);
    }

    [Fact]
    public async Task CourseCreatePageModel_CreateCourseAsync_ApiFailure_PreservesInputs()
    {
        _fakeCourseService.ShouldSucceed = false;
        _fakeCourseService.FailureMessage = "Código de curso já cadastrado";
        _fakeCourseService.FailureStatusCode = 409;

        var model = new CourseCreatePageModel(_fakeCourseService)
        {
            Name = "Curso Conflitante",
            Code = "DUP-001",
            WorkloadHours = 60,
            Description = "Descrição detalhada preservada",
            TargetAudience = "Público alvo preservado",
            SchoolUnitId = "unit-1"
        };

        var result = await model.CreateCourseAsync();

        Assert.False(result);
        Assert.False(model.IsCreated);
        Assert.Equal("Código de curso já cadastrado", model.ErrorMessage);

        // Verify inputs are preserved for user correction
        Assert.Equal("Curso Conflitante", model.Name);
        Assert.Equal("DUP-001", model.Code);
        Assert.Equal(60, model.WorkloadHours);
        Assert.Equal("Descrição detalhada preservada", model.Description);
        Assert.Equal("Público alvo preservado", model.TargetAudience);
        Assert.Equal("unit-1", model.SchoolUnitId);
    }

    [Fact]
    public async Task CourseCreatePageModel_CreateCourseAsync_Forbidden_SetsIsForbidden()
    {
        _fakeCourseService.ShouldSucceed = false;
        _fakeCourseService.FailureMessage = "Acesso negado: permissão Courses.Create necessária";
        _fakeCourseService.FailureStatusCode = 403;

        var model = new CourseCreatePageModel(_fakeCourseService)
        {
            Name = "Curso Qualquer",
            Code = "ANY-01",
            WorkloadHours = 40
        };

        var result = await model.CreateCourseAsync();

        Assert.False(result);
        Assert.True(model.IsForbidden);
    }

    [Fact]
    public async Task CourseCreatePageModel_CreateCourseAsync_WhileBusy_PreventsDoubleSubmission()
    {
        var model = new CourseCreatePageModel(_fakeCourseService)
        {
            Name = "Curso Teste",
            Code = "TST-01",
            WorkloadHours = 40,
            IsBusy = true
        };

        var result = await model.CreateCourseAsync();

        Assert.False(result);
        Assert.Null(_fakeCourseService.LastCreatedRequest);
    }

    [Fact]
    public void CourseCreatePageModel_ResetForm_ClearsFields()
    {
        var model = new CourseCreatePageModel(_fakeCourseService)
        {
            Name = "Curso",
            Code = "CUR-01",
            WorkloadHours = 80,
            Description = "Desc",
            SchoolUnitId = "unit",
            TargetAudience = "Adultos",
            ErrorMessage = "Erro",
            SuccessMessage = "Sucesso",
            IsCreated = true,
            IsForbidden = true
        };

        model.ResetForm();

        Assert.Equal(string.Empty, model.Name);
        Assert.Equal(string.Empty, model.Code);
        Assert.Equal(40, model.WorkloadHours);
        Assert.Null(model.Description);
        Assert.Null(model.SchoolUnitId);
        Assert.Null(model.TargetAudience);
        Assert.Equal(CourseStatus.Active, model.Status);
        Assert.Null(model.ErrorMessage);
        Assert.Null(model.SuccessMessage);
        Assert.False(model.IsCreated);
        Assert.False(model.IsForbidden);
        Assert.Null(model.CreatedCourse);
    }

    #endregion

    #region CourseDetailsPageModel Tests

    [Fact]
    public async Task CourseDetailsPageModel_LoadCourseAsync_Success_PopulatesCourse()
    {
        var model = new CourseDetailsPageModel(_fakeCourseService);

        var result = await model.LoadCourseAsync("course-1");

        Assert.True(result);
        Assert.NotNull(model.Course);
        Assert.Equal("Alfabetização Digital", model.Course.Name);
        Assert.Equal("ALF-101", model.Course.Code);
        Assert.Equal(60, model.Course.WorkloadHours);
        Assert.Equal(CourseStatus.Active, model.Course.Status);
        Assert.Null(model.ErrorMessage);
        Assert.False(model.IsNotFound);
        Assert.False(model.IsForbidden);
    }

    [Fact]
    public async Task CourseDetailsPageModel_LoadCourseAsync_NotFound_SetsIsNotFound()
    {
        var model = new CourseDetailsPageModel(_fakeCourseService);

        var result = await model.LoadCourseAsync("non-existent-id");

        Assert.False(result);
        Assert.Null(model.Course);
        Assert.True(model.IsNotFound);
        Assert.Equal("Curso não encontrado.", model.ErrorMessage);
    }

    [Fact]
    public async Task CourseDetailsPageModel_LoadCourseAsync_Forbidden_SetsIsForbidden()
    {
        _fakeCourseService.ShouldSucceed = false;
        _fakeCourseService.FailureStatusCode = 403;

        var model = new CourseDetailsPageModel(_fakeCourseService);

        var result = await model.LoadCourseAsync("course-1");

        Assert.False(result);
        Assert.Null(model.Course);
        Assert.True(model.IsForbidden);
        Assert.Contains("Acesso negado", model.ErrorMessage);
    }

    [Fact]
    public async Task CourseDetailsPageModel_LoadCourseAsync_EmptyId_SetsNotFound()
    {
        var model = new CourseDetailsPageModel(_fakeCourseService);

        var result = await model.LoadCourseAsync("");

        Assert.False(result);
        Assert.True(model.IsNotFound);
    }

    [Fact]
    public async Task CourseDetailsPageModel_DeleteCourseAsync_Success_SetsIsDeleted()
    {
        var model = new CourseDetailsPageModel(_fakeCourseService);
        await model.LoadCourseAsync("course-1");

        var result = await model.DeleteCourseAsync();

        Assert.True(result);
        Assert.True(model.IsDeleted);
        Assert.NotNull(model.SuccessMessage);
    }

    [Fact]
    public async Task CourseDetailsPageModel_DeleteCourseAsync_Failure_SetsErrorMessage()
    {
        var model = new CourseDetailsPageModel(_fakeCourseService);
        await model.LoadCourseAsync("course-1");

        _fakeCourseService.ShouldSucceed = false;
        _fakeCourseService.FailureMessage = "Não é possível excluir curso ativo com alunos";
        _fakeCourseService.FailureStatusCode = 409;

        var result = await model.DeleteCourseAsync();

        Assert.False(result);
        Assert.False(model.IsDeleted);
        Assert.Equal("Não é possível excluir curso ativo com alunos", model.ErrorMessage);
    }

    #endregion

    #region CourseEditPageModel Tests

    [Fact]
    public async Task CourseEditPageModel_LoadCourseAsync_Success_PopulatesFields()
    {
        var model = new CourseEditPageModel(_fakeCourseService);

        var result = await model.LoadCourseAsync("course-1");

        Assert.True(result);
        Assert.Equal("course-1", model.Id);
        Assert.Equal("Alfabetização Digital", model.Name);
        Assert.Equal("ALF-101", model.Code);
        Assert.Equal(60, model.WorkloadHours);
        Assert.Equal("Introdução ao uso de computadores e internet", model.Description);
        Assert.Equal("unit-central", model.SchoolUnitId);
        Assert.Equal("Jovens e Adultos", model.TargetAudience);
        Assert.Equal(CourseStatus.Active, model.Status);
        Assert.False(model.IsNotFound);
        Assert.False(model.IsForbidden);
    }

    [Fact]
    public async Task CourseEditPageModel_LoadCourseAsync_NotFound_SetsIsNotFound()
    {
        var model = new CourseEditPageModel(_fakeCourseService);

        var result = await model.LoadCourseAsync("id-inexistente");

        Assert.False(result);
        Assert.True(model.IsNotFound);
        Assert.Equal("Curso não encontrado.", model.ErrorMessage);
    }

    [Fact]
    public async Task CourseEditPageModel_LoadCourseAsync_Forbidden_SetsIsForbidden()
    {
        _fakeCourseService.ShouldSucceed = false;
        _fakeCourseService.FailureStatusCode = 403;

        var model = new CourseEditPageModel(_fakeCourseService);

        var result = await model.LoadCourseAsync("course-1");

        Assert.False(result);
        Assert.True(model.IsForbidden);
        Assert.Contains("Acesso negado", model.ErrorMessage);
    }

    [Theory]
    [InlineData("", "Nome", "COD-1", 40, "identificador do curso é obrigatório")]
    [InlineData("id-1", "", "COD-1", 40, "nome do curso é obrigatório")]
    [InlineData("id-1", "Nome", "", 40, "código do curso é obrigatório")]
    [InlineData("id-1", "Nome", "COD-1", 0, "carga horária deve ser maior que zero")]
    [InlineData("id-1", "Nome", "COD-1", -10, "carga horária deve ser maior que zero")]
    public void CourseEditPageModel_Validate_InvalidFields_SetsErrorMessage(string id, string name, string code, int hours, string expectedSub)
    {
        var model = new CourseEditPageModel(_fakeCourseService)
        {
            Id = id,
            Name = name,
            Code = code,
            WorkloadHours = hours
        };

        var isValid = model.Validate();

        Assert.False(isValid);
        Assert.Contains(expectedSub, model.ErrorMessage);
    }

    [Fact]
    public async Task CourseEditPageModel_SaveCourseAsync_Success_SetsIsSaved()
    {
        var model = new CourseEditPageModel(_fakeCourseService);
        await model.LoadCourseAsync("course-1");

        model.Name = "Alfabetização Digital Avançada";
        model.WorkloadHours = 90;

        var result = await model.SaveCourseAsync();

        Assert.True(result);
        Assert.True(model.IsSaved);
        Assert.NotNull(model.SuccessMessage);
        Assert.Null(model.ErrorMessage);

        // Verify updated in fake store
        var inStore = _fakeCourseService.MockCourses.First(c => c.Id == "course-1");
        Assert.Equal("Alfabetização Digital Avançada", inStore.Name);
        Assert.Equal(90, inStore.WorkloadHours);
    }

    [Fact]
    public async Task CourseEditPageModel_SaveCourseAsync_ApiFailure_PreservesInputs()
    {
        var model = new CourseEditPageModel(_fakeCourseService);
        await model.LoadCourseAsync("course-1");

        model.Name = "Nome Modificado";
        model.Code = "COD-NOVO";

        _fakeCourseService.ShouldSucceed = false;
        _fakeCourseService.FailureMessage = "Código já em uso por outro curso";
        _fakeCourseService.FailureStatusCode = 409;

        var result = await model.SaveCourseAsync();

        Assert.False(result);
        Assert.False(model.IsSaved);
        Assert.Equal("Código já em uso por outro curso", model.ErrorMessage);

        // Preserves user changes on screen
        Assert.Equal("Nome Modificado", model.Name);
        Assert.Equal("COD-NOVO", model.Code);
    }

    [Fact]
    public async Task CourseEditPageModel_SaveCourseAsync_NotFound_SetsIsNotFound()
    {
        var model = new CourseEditPageModel(_fakeCourseService);
        await model.LoadCourseAsync("course-1");

        _fakeCourseService.ShouldSucceed = false;
        _fakeCourseService.FailureMessage = "Curso não encontrado";
        _fakeCourseService.FailureStatusCode = 404;

        var result = await model.SaveCourseAsync();

        Assert.False(result);
        Assert.True(model.IsNotFound);
    }

    [Fact]
    public async Task CourseEditPageModel_SaveCourseAsync_WhileBusy_PreventsDoubleSubmission()
    {
        var model = new CourseEditPageModel(_fakeCourseService)
        {
            Id = "course-1",
            Name = "Nome",
            Code = "COD-1",
            WorkloadHours = 40,
            IsBusy = true
        };

        var result = await model.SaveCourseAsync();

        Assert.False(result);
        Assert.Null(_fakeCourseService.LastUpdatedRequest);
    }

    #endregion
}

