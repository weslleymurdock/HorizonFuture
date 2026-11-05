using HF.Application.Collaborators.Requests;
using HF.Application.Collaborators.Services;
using HF.Application.Directors.Requests;
using HF.Application.Directors.Services;
using HF.Application.Managers.Requests;
using HF.Application.Managers.Services;
using HF.Application.Teachers.Requests;
using HF.Application.Teachers.Services;
using HF.Application.Tests.Common;
using HF.Application.Volunteers.Requests;
using HF.Application.Volunteers.Services;
using HF.Domain.Collaborators;
using HF.Domain.Directors;
using HF.Domain.Managers;
using HF.Domain.TeacherAssignments;
using HF.Domain.Teachers;
using HF.Domain.Volunteers;
using Xunit;

namespace HF.Application.Tests.Staff;

public class StaffServiceTests
{
    private readonly FakeIdentityService _identity = new();

    public StaffServiceTests()
    {
        _identity.AddUser("user-teacher");
        _identity.AddUser("user-manager");
        _identity.AddUser("user-director");
        _identity.AddUser("user-collab");
        _identity.AddUser("user-vol");
    }

    [Fact]
    public async Task TeacherService_CreateAndPreventDeletionWhenAssigned()
    {
        var teacherRepo = new InMemoryRepository<Teacher>();
        var assignRepo = new InMemoryRepository<TeacherAssignment>();
        var sut = new TeacherService(teacherRepo, assignRepo, _identity);

        var request = new CreateTeacherRequest("user-teacher", "EMP-T1", "Arts");
        var created = await sut.CreateAsync(request, CancellationToken.None);

        Assert.NotNull(created);
        Assert.Equal("EMP-T1", created.EmployeeNumber);

        // Add assignment
        await assignRepo.AddAsync(new TeacherAssignment { TeacherId = created.Id }, CancellationToken.None);

        var (succeeded, error) = await sut.DeleteAsync(created.Id, CancellationToken.None);
        Assert.False(succeeded);
        Assert.Contains("assignment history", error);
    }

    [Fact]
    public async Task ManagerService_CreateAndUpdate_ShouldSucceed()
    {
        var repo = new InMemoryRepository<Manager>();
        var sut = new ManagerService(repo, _identity);

        var created = await sut.CreateAsync(new CreateManagerRequest("user-manager", "MGR-01", "Operations", "Ops Coordinator"), CancellationToken.None);
        Assert.Equal("Operations", created.Department);

        var updated = await sut.UpdateAsync(new UpdateManagerRequest(created.Id, "MGR-01", "Pedagogical", "Pedagogical Lead"), CancellationToken.None);
        Assert.NotNull(updated);
        Assert.Equal("Pedagogical", updated.Department);
    }

    [Fact]
    public async Task DirectorService_CreateAndGet_ShouldSucceed()
    {
        var repo = new InMemoryRepository<Director>();
        var sut = new DirectorService(repo, _identity);

        var created = await sut.CreateAsync(new CreateDirectorRequest("user-director", "Executive", "President"), CancellationToken.None);
        Assert.Equal("Executive", created.Directorate);

        var fetched = await sut.GetByIdAsync(created.Id, CancellationToken.None);
        Assert.NotNull(fetched);
    }

    [Fact]
    public async Task CollaboratorService_CreateAndDelete_ShouldSucceed()
    {
        var repo = new InMemoryRepository<Collaborator>();
        var sut = new CollaboratorService(repo, _identity);

        var created = await sut.CreateAsync(new CreateCollaboratorRequest("user-collab", "COL-10", "Social Worker", "Social Assistance"), CancellationToken.None);
        Assert.Equal("Social Worker", created.JobTitle);

        var (succeeded, _) = await sut.DeleteAsync(created.Id, CancellationToken.None);
        Assert.True(succeeded);
        Assert.Empty(repo.Items);
    }

    [Fact]
    public async Task VolunteerService_CreateAndList_ShouldSucceed()
    {
        var repo = new InMemoryRepository<Volunteer>();
        var sut = new VolunteerService(repo, _identity);

        var created = await sut.CreateAsync(new CreateVolunteerRequest("user-vol", "Mentoring", 4), CancellationToken.None);
        Assert.Equal(4, created.AvailableHoursPerWeek);

        var list = await sut.ListAsync(new ListVolunteersRequest(), CancellationToken.None);
        Assert.Single(list);
    }
}

