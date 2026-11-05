using System.Linq;
using System.Threading.Tasks;
using HF.Shared.Subjects.Models;
using HF.Shared.PageModels.Subjects;
using HF.UITests.Shared.Fakes;
using Xunit;

namespace HF.UITests.Shared;

public class SharedSubjectsPageModelsTests
{
    private readonly FakeSubjectService _fakeSubjectService = new();

    #region SubjectListPageModel Tests

    [Fact]
    public void SubjectListPageModel_InitialState_PropertiesAreDefault()
    {
        var model = new SubjectListPageModel(_fakeSubjectService);

        Assert.Equal("Disciplinas", model.Title);
        Assert.False(model.IsBusy);
        Assert.False(model.IsForbidden);
        Assert.False(model.IsUnauthorized);
        Assert.False(model.IsDeleting);
        Assert.Null(model.ErrorMessage);
        Assert.Null(model.SuccessMessage);
        Assert.Empty(model.Subjects);
        Assert.Empty(model.SearchText);
        Assert.Null(model.StatusFilter);
        Assert.Null(model.SchoolUnitIdFilter);
        Assert.Null(model.CourseIdFilter);
    }

    [Fact]
    public async Task SubjectListPageModel_LoadSubjectsAsync_Success_PopulatesSubjects()
    {
        var model = new SubjectListPageModel(_fakeSubjectService);

        var success = await model.LoadSubjectsAsync();

        Assert.True(success);
        Assert.False(model.IsBusy);
        Assert.Null(model.ErrorMessage);
        Assert.Equal(2, model.Subjects.Count);
        Assert.Contains(model.Subjects, s => s.Code == "MAT-101");
        Assert.Contains(model.Subjects, s => s.Code == "CS-201");
    }

    [Fact]
    public async Task SubjectListPageModel_LoadSubjectsAsync_EmptyList_SetsEmptySubjects()
    {
        _fakeSubjectService.MockSubjects.Clear();
        var model = new SubjectListPageModel(_fakeSubjectService);

        var success = await model.LoadSubjectsAsync();

        Assert.True(success);
        Assert.Empty(model.Subjects);
        Assert.Null(model.ErrorMessage);
    }

    [Fact]
    public async Task SubjectListPageModel_LoadSubjectsAsync_ApiFailure_SetsErrorMessage()
    {
        _fakeSubjectService.ShouldSucceed = false;
        _fakeSubjectService.FailureMessage = "Erro ao buscar disciplinas do servidor";
        _fakeSubjectService.FailureStatusCode = 500;

        var model = new SubjectListPageModel(_fakeSubjectService);

        var success = await model.LoadSubjectsAsync();

        Assert.False(success);
        Assert.False(model.IsBusy);
        Assert.Equal("Erro ao buscar disciplinas do servidor", model.ErrorMessage);
        Assert.Empty(model.Subjects);
    }

    [Fact]
    public async Task SubjectListPageModel_LoadSubjectsAsync_Forbidden_SetsIsForbidden()
    {
        _fakeSubjectService.ShouldSucceed = false;
        _fakeSubjectService.FailureMessage = "Acesso negado: permissão Subjects.Read ausente";
        _fakeSubjectService.FailureStatusCode = 403;

        var model = new SubjectListPageModel(_fakeSubjectService);

        var success = await model.LoadSubjectsAsync();

        Assert.False(success);
        Assert.True(model.IsForbidden);
        Assert.False(model.IsUnauthorized);
        Assert.NotNull(model.ErrorMessage);
    }

    [Fact]
    public async Task SubjectListPageModel_LoadSubjectsAsync_Unauthorized_SetsIsUnauthorized()
    {
        _fakeSubjectService.ShouldSucceed = false;
        _fakeSubjectService.FailureMessage = "Sessão expirada";
        _fakeSubjectService.FailureStatusCode = 401;

        var model = new SubjectListPageModel(_fakeSubjectService);

        var success = await model.LoadSubjectsAsync();

        Assert.False(success);
        Assert.True(model.IsUnauthorized);
        Assert.False(model.IsForbidden);
    }

    [Fact]
    public async Task SubjectListPageModel_FilteredSubjects_FiltersByNameAndCode()
    {
        var model = new SubjectListPageModel(_fakeSubjectService);
        await model.LoadSubjectsAsync();

        // Filter by partial name
        model.SearchText = "Programação";
        var filteredByName = model.FilteredSubjects.ToList();
        Assert.Single(filteredByName);
        Assert.Equal("CS-201", filteredByName[0].Code);

        // Filter by partial code
        model.SearchText = "MAT";
        var filteredByCode = model.FilteredSubjects.ToList();
        Assert.Single(filteredByCode);
        Assert.Equal("Matemática Financeira", filteredByCode[0].Name);

        // Filter with empty text returns all
        model.SearchText = string.Empty;
        Assert.Equal(2, model.FilteredSubjects.Count());
    }

    [Fact]
    public async Task SubjectListPageModel_ResetFiltersAsync_ClearsFiltersAndReloads()
    {
        var model = new SubjectListPageModel(_fakeSubjectService)
        {
            SearchText = "xyz",
            StatusFilter = SubjectStatus.Inactive,
            SchoolUnitIdFilter = "unit-123",
            CourseIdFilter = "course-123"
        };

        await model.ResetFiltersAsync();

        Assert.Equal(string.Empty, model.SearchText);
        Assert.Null(model.StatusFilter);
        Assert.Null(model.SchoolUnitIdFilter);
        Assert.Null(model.CourseIdFilter);
        Assert.Equal(2, model.Subjects.Count);
    }

    [Fact]
    public async Task SubjectListPageModel_DeleteSubjectAsync_Success_RemovesFromCollection()
    {
        var model = new SubjectListPageModel(_fakeSubjectService);
        await model.LoadSubjectsAsync();
        Assert.Equal(2, model.Subjects.Count);

        var success = await model.DeleteSubjectAsync("subject-1");

        Assert.True(success);
        Assert.False(model.IsDeleting);
        Assert.Null(model.DeletingSubjectId);
        Assert.Single(model.Subjects);
        Assert.DoesNotContain(model.Subjects, s => s.Id == "subject-1");
        Assert.NotNull(model.SuccessMessage);
    }

    [Fact]
    public async Task SubjectListPageModel_DeleteSubjectAsync_Failure_SetsErrorMessage()
    {
        _fakeSubjectService.ShouldSucceed = false;
        _fakeSubjectService.FailureMessage = "Conflito: a disciplina possui turmas ativas vinculadas";
        _fakeSubjectService.FailureStatusCode = 409;

        var model = new SubjectListPageModel(_fakeSubjectService);
        await model.LoadSubjectsAsync();

        var success = await model.DeleteSubjectAsync("subject-1");

        Assert.False(success);
        Assert.False(model.IsDeleting);
        Assert.Equal("Conflito: a disciplina possui turmas ativas vinculadas", model.ErrorMessage);
    }

    [Fact]
    public async Task SubjectListPageModel_DeleteSubjectAsync_EmptyId_ReturnsFalse()
    {
        var model = new SubjectListPageModel(_fakeSubjectService);

        var success = await model.DeleteSubjectAsync(string.Empty);

        Assert.False(success);
        Assert.False(model.IsDeleting);
    }

    #endregion

    #region SubjectCreatePageModel Tests

    [Theory]
    [InlineData("", "MAT-101", 40, "nome da disciplina é obrigatório")]
    [InlineData("   ", "MAT-101", 40, "nome da disciplina é obrigatório")]
    [InlineData("Disciplina Teste", "", 40, "código da disciplina é obrigatório")]
    [InlineData("Disciplina Teste", "   ", 40, "código da disciplina é obrigatório")]
    [InlineData("Disciplina Teste", "MAT-101", 0, "carga horária deve ser maior que zero")]
    [InlineData("Disciplina Teste", "MAT-101", -5, "carga horária deve ser maior que zero")]
    public void SubjectCreatePageModel_Validate_InvalidFields_SetsErrorMessage(string name, string code, int hours, string expectedSubstring)
    {
        var model = new SubjectCreatePageModel(_fakeSubjectService)
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
    public void SubjectCreatePageModel_Validate_ExceedingLength_FailsValidation()
    {
        var model = new SubjectCreatePageModel(_fakeSubjectService)
        {
            Name = new string('A', 151),
            Code = "MAT-101",
            WorkloadHours = 40
        };

        Assert.False(model.Validate());
        Assert.Contains("150 caracteres", model.ErrorMessage);

        model.Name = "Disciplina Válida";
        model.Code = new string('B', 31);
        Assert.False(model.Validate());
        Assert.Contains("30 caracteres", model.ErrorMessage);
    }

    [Fact]
    public async Task SubjectCreatePageModel_CreateSubjectAsync_ValidInput_CreatesSuccessfully()
    {
        var model = new SubjectCreatePageModel(_fakeSubjectService)
        {
            Name = "Arquitetura de Computadores",
            Code = "ARQ-101",
            WorkloadHours = 60,
            Description = "CPU, memória e barramentos",
            SchoolUnitId = "unit-sul",
            CourseId = "course-1",
            Status = SubjectStatus.Active
        };

        var result = await model.CreateSubjectAsync();

        Assert.True(result);
        Assert.True(model.IsCreated);
        Assert.NotNull(model.CreatedSubject);
        Assert.Equal("Arquitetura de Computadores", model.CreatedSubject.Name);
        Assert.Equal("ARQ-101", model.CreatedSubject.Code);
        Assert.Equal(60, model.CreatedSubject.WorkloadHours);
        Assert.NotNull(model.SuccessMessage);
        Assert.Null(model.ErrorMessage);
    }

    [Fact]
    public async Task SubjectCreatePageModel_CreateSubjectAsync_ApiFailure_PreservesInputs()
    {
        _fakeSubjectService.ShouldSucceed = false;
        _fakeSubjectService.FailureMessage = "Código de disciplina já cadastrado";
        _fakeSubjectService.FailureStatusCode = 409;

        var model = new SubjectCreatePageModel(_fakeSubjectService)
        {
            Name = "Disciplina Conflitante",
            Code = "DUP-001",
            WorkloadHours = 60,
            Description = "Descrição preservada",
            SchoolUnitId = "unit-1",
            CourseId = "course-1"
        };

        var result = await model.CreateSubjectAsync();

        Assert.False(result);
        Assert.False(model.IsCreated);
        Assert.Equal("Código de disciplina já cadastrado", model.ErrorMessage);

        // Verify inputs are preserved
        Assert.Equal("Disciplina Conflitante", model.Name);
        Assert.Equal("DUP-001", model.Code);
        Assert.Equal(60, model.WorkloadHours);
        Assert.Equal("Descrição preservada", model.Description);
        Assert.Equal("unit-1", model.SchoolUnitId);
        Assert.Equal("course-1", model.CourseId);
    }

    [Fact]
    public async Task SubjectCreatePageModel_CreateSubjectAsync_Forbidden_SetsIsForbidden()
    {
        _fakeSubjectService.ShouldSucceed = false;
        _fakeSubjectService.FailureMessage = "Acesso negado: permissão Subjects.Create necessária";
        _fakeSubjectService.FailureStatusCode = 403;

        var model = new SubjectCreatePageModel(_fakeSubjectService)
        {
            Name = "Disciplina Qualquer",
            Code = "ANY-01",
            WorkloadHours = 40
        };

        var result = await model.CreateSubjectAsync();

        Assert.False(result);
        Assert.True(model.IsForbidden);
    }

    [Fact]
    public async Task SubjectCreatePageModel_CreateSubjectAsync_WhileBusy_PreventsDoubleSubmission()
    {
        var model = new SubjectCreatePageModel(_fakeSubjectService)
        {
            Name = "Disciplina Teste",
            Code = "TST-01",
            WorkloadHours = 40,
            IsBusy = true
        };

        var result = await model.CreateSubjectAsync();

        Assert.False(result);
        Assert.Null(_fakeSubjectService.LastCreatedRequest);
    }

    [Fact]
    public void SubjectCreatePageModel_ResetForm_ClearsFields()
    {
        var model = new SubjectCreatePageModel(_fakeSubjectService)
        {
            Name = "Disciplina",
            Code = "DIS-01",
            WorkloadHours = 80,
            Description = "Desc",
            SchoolUnitId = "unit",
            CourseId = "course",
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
        Assert.Null(model.CourseId);
        Assert.Equal(SubjectStatus.Active, model.Status);
        Assert.Null(model.ErrorMessage);
        Assert.Null(model.SuccessMessage);
        Assert.False(model.IsCreated);
        Assert.False(model.IsForbidden);
        Assert.Null(model.CreatedSubject);
    }

    #endregion

    #region SubjectDetailsPageModel Tests

    [Fact]
    public async Task SubjectDetailsPageModel_LoadSubjectAsync_Success_PopulatesSubject()
    {
        var model = new SubjectDetailsPageModel(_fakeSubjectService);

        var result = await model.LoadSubjectAsync("subject-1");

        Assert.True(result);
        Assert.NotNull(model.Subject);
        Assert.Equal("Matemática Financeira", model.Subject.Name);
        Assert.Equal("MAT-101", model.Subject.Code);
        Assert.Equal(40, model.Subject.WorkloadHours);
        Assert.Equal(SubjectStatus.Active, model.Subject.Status);
        Assert.Null(model.ErrorMessage);
        Assert.False(model.IsNotFound);
        Assert.False(model.IsForbidden);
    }

    [Fact]
    public async Task SubjectDetailsPageModel_LoadSubjectAsync_NotFound_SetsIsNotFound()
    {
        var model = new SubjectDetailsPageModel(_fakeSubjectService);

        var result = await model.LoadSubjectAsync("non-existent-id");

        Assert.False(result);
        Assert.Null(model.Subject);
        Assert.True(model.IsNotFound);
        Assert.Equal("Disciplina não encontrada.", model.ErrorMessage);
    }

    [Fact]
    public async Task SubjectDetailsPageModel_LoadSubjectAsync_Forbidden_SetsIsForbidden()
    {
        _fakeSubjectService.ShouldSucceed = false;
        _fakeSubjectService.FailureStatusCode = 403;

        var model = new SubjectDetailsPageModel(_fakeSubjectService);

        var result = await model.LoadSubjectAsync("subject-1");

        Assert.False(result);
        Assert.Null(model.Subject);
        Assert.True(model.IsForbidden);
        Assert.Contains("Acesso negado", model.ErrorMessage);
    }

    [Fact]
    public async Task SubjectDetailsPageModel_LoadSubjectAsync_EmptyId_SetsNotFound()
    {
        var model = new SubjectDetailsPageModel(_fakeSubjectService);

        var result = await model.LoadSubjectAsync("");

        Assert.False(result);
        Assert.True(model.IsNotFound);
    }

    [Fact]
    public async Task SubjectDetailsPageModel_DeleteSubjectAsync_Success_SetsIsDeleted()
    {
        var model = new SubjectDetailsPageModel(_fakeSubjectService);
        await model.LoadSubjectAsync("subject-1");

        var result = await model.DeleteSubjectAsync();

        Assert.True(result);
        Assert.True(model.IsDeleted);
        Assert.NotNull(model.SuccessMessage);
    }

    [Fact]
    public async Task SubjectDetailsPageModel_DeleteSubjectAsync_Failure_SetsErrorMessage()
    {
        var model = new SubjectDetailsPageModel(_fakeSubjectService);
        await model.LoadSubjectAsync("subject-1");

        _fakeSubjectService.ShouldSucceed = false;
        _fakeSubjectService.FailureMessage = "Não é possível excluir disciplina ativa em turma";
        _fakeSubjectService.FailureStatusCode = 409;

        var result = await model.DeleteSubjectAsync();

        Assert.False(result);
        Assert.False(model.IsDeleted);
        Assert.Equal("Não é possível excluir disciplina ativa em turma", model.ErrorMessage);
    }

    #endregion

    #region SubjectEditPageModel Tests

    [Fact]
    public async Task SubjectEditPageModel_LoadSubjectAsync_Success_PopulatesFields()
    {
        var model = new SubjectEditPageModel(_fakeSubjectService);

        var result = await model.LoadSubjectAsync("subject-1");

        Assert.True(result);
        Assert.Equal("subject-1", model.Id);
        Assert.Equal("Matemática Financeira", model.Name);
        Assert.Equal("MAT-101", model.Code);
        Assert.Equal(40, model.WorkloadHours);
        Assert.Equal("Conceitos de juros, inflação e orçamento pessoal", model.Description);
        Assert.Equal("unit-central", model.SchoolUnitId);
        Assert.Equal("course-1", model.CourseId);
        Assert.Equal(SubjectStatus.Active, model.Status);
        Assert.False(model.IsNotFound);
        Assert.False(model.IsForbidden);
    }

    [Fact]
    public async Task SubjectEditPageModel_LoadSubjectAsync_NotFound_SetsIsNotFound()
    {
        var model = new SubjectEditPageModel(_fakeSubjectService);

        var result = await model.LoadSubjectAsync("id-inexistente");

        Assert.False(result);
        Assert.True(model.IsNotFound);
        Assert.Equal("Disciplina não encontrada.", model.ErrorMessage);
    }

    [Fact]
    public async Task SubjectEditPageModel_LoadSubjectAsync_Forbidden_SetsIsForbidden()
    {
        _fakeSubjectService.ShouldSucceed = false;
        _fakeSubjectService.FailureStatusCode = 403;

        var model = new SubjectEditPageModel(_fakeSubjectService);

        var result = await model.LoadSubjectAsync("subject-1");

        Assert.False(result);
        Assert.True(model.IsForbidden);
        Assert.Contains("Acesso negado", model.ErrorMessage);
    }

    [Theory]
    [InlineData("", "Nome", "COD-1", 40, "identificador da disciplina é obrigatório")]
    [InlineData("id-1", "", "COD-1", 40, "nome da disciplina é obrigatório")]
    [InlineData("id-1", "Nome", "", 40, "código da disciplina é obrigatório")]
    [InlineData("id-1", "Nome", "COD-1", 0, "carga horária deve ser maior que zero")]
    [InlineData("id-1", "Nome", "COD-1", -10, "carga horária deve ser maior que zero")]
    public void SubjectEditPageModel_Validate_InvalidFields_SetsErrorMessage(string id, string name, string code, int hours, string expectedSub)
    {
        var model = new SubjectEditPageModel(_fakeSubjectService)
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
    public async Task SubjectEditPageModel_SaveSubjectAsync_Success_SetsIsSaved()
    {
        var model = new SubjectEditPageModel(_fakeSubjectService);
        await model.LoadSubjectAsync("subject-1");

        model.Name = "Matemática Financeira e Estatística";
        model.WorkloadHours = 80;

        var result = await model.SaveSubjectAsync();

        Assert.True(result);
        Assert.True(model.IsSaved);
        Assert.NotNull(model.SuccessMessage);
        Assert.Null(model.ErrorMessage);

        // Verify updated in fake store
        var inStore = _fakeSubjectService.MockSubjects.First(s => s.Id == "subject-1");
        Assert.Equal("Matemática Financeira e Estatística", inStore.Name);
        Assert.Equal(80, inStore.WorkloadHours);
    }

    [Fact]
    public async Task SubjectEditPageModel_SaveSubjectAsync_ApiFailure_PreservesInputs()
    {
        var model = new SubjectEditPageModel(_fakeSubjectService);
        await model.LoadSubjectAsync("subject-1");

        model.Name = "Nome Modificado";
        model.Code = "COD-NOVO";

        _fakeSubjectService.ShouldSucceed = false;
        _fakeSubjectService.FailureMessage = "Código já em uso por outra disciplina";
        _fakeSubjectService.FailureStatusCode = 409;

        var result = await model.SaveSubjectAsync();

        Assert.False(result);
        Assert.False(model.IsSaved);
        Assert.Equal("Código já em uso por outra disciplina", model.ErrorMessage);

        // Preserves user changes on screen
        Assert.Equal("Nome Modificado", model.Name);
        Assert.Equal("COD-NOVO", model.Code);
    }

    [Fact]
    public async Task SubjectEditPageModel_SaveSubjectAsync_NotFound_SetsIsNotFound()
    {
        var model = new SubjectEditPageModel(_fakeSubjectService);
        await model.LoadSubjectAsync("subject-1");

        _fakeSubjectService.ShouldSucceed = false;
        _fakeSubjectService.FailureMessage = "Subject not found";
        _fakeSubjectService.FailureStatusCode = 404;

        var result = await model.SaveSubjectAsync();

        Assert.False(result);
        Assert.True(model.IsNotFound);
    }

    [Fact]
    public async Task SubjectEditPageModel_SaveSubjectAsync_WhileBusy_PreventsDoubleSubmission()
    {
        var model = new SubjectEditPageModel(_fakeSubjectService)
        {
            Id = "subject-1",
            Name = "Nome",
            Code = "COD-1",
            WorkloadHours = 40,
            IsBusy = true
        };

        var result = await model.SaveSubjectAsync();

        Assert.False(result);
        Assert.Null(_fakeSubjectService.LastUpdatedRequest);
    }

    #endregion
}
