using System.Security.Claims;
using HF.Application.Auth;
using HF.Application.Tests.Common;
using HF.Infrastructure.Auth.Models;
using HF.Infrastructure.Auth.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HF.Application.Tests.Auth;

public class RoleHierarchyAndSecurityTests
{
    [Fact]
    public void RoleHierarchy_MustNotRelyOnAlphabeticalSorting()
    {
        // "Volunteer" starts with V, "Admin" starts with A.
        // In alphabetical sorting, "Volunteer" > "Admin".
        // In our authority hierarchy, Admin must be strictly superior.
        int adminRank = AppRoles.GetRank(AppRoles.Admin);
        int directorRank = AppRoles.GetRank(AppRoles.Director);
        int coordinatorRank = AppRoles.GetRank(AppRoles.Coordinator);
        int managerRank = AppRoles.GetRank(AppRoles.Manager);
        int coworkerRank = AppRoles.GetRank(AppRoles.Coworker);
        int volunteerRank = AppRoles.GetRank(AppRoles.Volunteer);
        int readerRank = AppRoles.GetRank(AppRoles.Reader);

        Assert.True(adminRank > directorRank);
        Assert.True(directorRank > coordinatorRank);
        Assert.True(coordinatorRank > managerRank);
        Assert.True(managerRank > coworkerRank);
        Assert.True(coworkerRank > volunteerRank);
        Assert.True(volunteerRank > readerRank);
        Assert.True(readerRank > 0);
    }

    [Fact]
    public void GetUserMaxRank_WithMultipleRoles_ReturnsMaximumRank()
    {
        string[] roles = [AppRoles.Reader, AppRoles.Coordinator, AppRoles.Volunteer];
        int maxRank = AppRoles.GetUserMaxRank(roles);

        Assert.Equal(AppRoles.Hierarchy[AppRoles.Coordinator], maxRank);
    }

    [Fact]
    public void GetUserMaxRank_WithNoRolesOrUnknownRoles_ReturnsZero()
    {
        Assert.Equal(0, AppRoles.GetUserMaxRank([]));
        Assert.Equal(0, AppRoles.GetUserMaxRank(["SuperUser", "Guest", "UnknownRole"]));
    }

    [Fact]
    public void PrecedenceRule_ExplicitRevocation_OverridesRoleInheritance()
    {
        // Coordinator has Students.Create by default
        bool beforeRevoke = RolePermissionCatalog.Evaluate(
            userRoles: [AppRoles.Coordinator],
            directGrants: [],
            explicitRevocations: [],
            assignedGroups: [],
            requestedPermission: AppPermissions.StudentsCreate);

        Assert.True(beforeRevoke);

        // Explicitly revoke Students.Create
        bool afterRevoke = RolePermissionCatalog.Evaluate(
            userRoles: [AppRoles.Coordinator],
            directGrants: [],
            explicitRevocations: [AppPermissions.StudentsCreate],
            assignedGroups: [],
            requestedPermission: AppPermissions.StudentsCreate);

        Assert.False(afterRevoke);
    }

    [Fact]
    public void PrecedenceRule_ExplicitRevocation_OverridesDirectGrantAndGroup()
    {
        bool allowed = RolePermissionCatalog.Evaluate(
            userRoles: [],
            directGrants: [AppPermissions.StudentsCreate],
            explicitRevocations: [AppPermissions.StudentsCreate],
            assignedGroups: [AppPermissionGroups.StudentAdministration],
            requestedPermission: AppPermissions.StudentsCreate);

        Assert.False(allowed);
    }

    [Fact]
    public void PrecedenceRule_DirectGrant_GrantsPermissionToRolelessUser()
    {
        bool allowed = RolePermissionCatalog.Evaluate(
            userRoles: [],
            directGrants: [AppPermissions.StudentsRead],
            explicitRevocations: [],
            assignedGroups: [],
            requestedPermission: AppPermissions.StudentsRead);

        Assert.True(allowed);
    }

    [Fact]
    public void PrecedenceRule_GroupGrant_GrantsAllGroupPermissions()
    {
        bool allowed = RolePermissionCatalog.Evaluate(
            userRoles: [AppRoles.Reader],
            directGrants: [],
            explicitRevocations: [],
            assignedGroups: [AppPermissionGroups.AcademicManagement],
            requestedPermission: AppPermissions.CoursesCreate);

        Assert.True(allowed);
    }

    [Fact]
    public void PrecedenceRule_DefaultIsDeny()
    {
        bool allowed = RolePermissionCatalog.Evaluate(
            userRoles: [AppRoles.Reader],
            directGrants: [],
            explicitRevocations: [],
            assignedGroups: [],
            requestedPermission: AppPermissions.DirectorsDelete);

        Assert.False(allowed);
    }

    // -------------------------------------------------------------
    // IdentityAdminService server-side validation tests
    // -------------------------------------------------------------

    private static (IdentityAdminService sut, UserManager<User> userManager, RoleManager<Role> roleManager, InMemoryUserStore store) CreateSut()
    {
        var store = new InMemoryUserStore();
        var roleStore = new InMemoryRoleStore();

        var userManager = new UserManager<User>(
            store,
            null!, null!, null!, null!, null!, null!, null!, null!);

        var roleManager = new RoleManager<Role>(
            roleStore,
            null!, null!, null!, null!);

        var sut = new IdentityAdminService(userManager, roleManager, NullLogger<IdentityAdminService>.Instance);
        return (sut, userManager, roleManager, store);
    }

    [Fact]
    public async Task AssignRole_WhenActorTriesSelfElevation_Fails()
    {
        var (sut, userManager, _, _) = CreateSut();

        var actor = new User("actor-admin") { Id = "user-1" };
        await userManager.CreateAsync(actor);
        await userManager.AddToRoleAsync(actor, AppRoles.Admin);

        var result = await sut.AssignRoleAsync("user-1", "user-1", AppRoles.Director, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("own roles", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AssignRole_WhenActorHasLowerOrEqualRankThanRole_Fails()
    {
        var (sut, userManager, _, _) = CreateSut();

        var actor = new User("actor-mgr") { Id = "actor-1" };
        var target = new User("target-user") { Id = "target-1" };
        await userManager.CreateAsync(actor);
        await userManager.CreateAsync(target);
        await userManager.AddToRoleAsync(actor, AppRoles.Manager);

        // Manager (rank 400) trying to assign Director (rank 600)
        var resultHigher = await sut.AssignRoleAsync("actor-1", "target-1", AppRoles.Director, CancellationToken.None);
        Assert.False(resultHigher.Success);
        Assert.Contains("cannot assign role", resultHigher.Message, StringComparison.OrdinalIgnoreCase);

        // Manager (rank 400) trying to assign Manager (rank 400)
        var resultEqual = await sut.AssignRoleAsync("actor-1", "target-1", AppRoles.Manager, CancellationToken.None);
        Assert.False(resultEqual.Success);
        Assert.Contains("cannot assign role", resultEqual.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AssignRole_WhenActorHasLowerOrEqualRankThanTargetUser_Fails()
    {
        var (sut, userManager, _, _) = CreateSut();

        var actor = new User("actor-coord") { Id = "actor-1" };
        var target = new User("target-director") { Id = "target-1" };
        await userManager.CreateAsync(actor);
        await userManager.CreateAsync(target);
        await userManager.AddToRoleAsync(actor, AppRoles.Coordinator); // 500
        await userManager.AddToRoleAsync(target, AppRoles.Director);    // 600

        // Coordinator trying to assign Volunteer to a Director
        var result = await sut.AssignRoleAsync("actor-1", "target-1", AppRoles.Volunteer, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("not strictly higher", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AssignRole_CannotGrantAdminRoleViaStandardManagement()
    {
        var (sut, userManager, _, _) = CreateSut();

        var actor = new User("actor-admin") { Id = "actor-1" };
        var target = new User("target-user") { Id = "target-1" };
        await userManager.CreateAsync(actor);
        await userManager.CreateAsync(target);
        await userManager.AddToRoleAsync(actor, AppRoles.Admin);

        var result = await sut.AssignRoleAsync("actor-1", "target-1", AppRoles.Admin, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("Admin role cannot be granted", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RemoveRole_CannotRemoveLastAdministrator()
    {
        var (sut, userManager, _, _) = CreateSut();

        var actor = new User("actor-admin") { Id = "actor-1" };
        var targetAdmin = new User("target-admin") { Id = "target-admin-1" };
        await userManager.CreateAsync(actor);
        await userManager.CreateAsync(targetAdmin);

        // Target is the ONLY admin in system
        await userManager.AddToRoleAsync(targetAdmin, AppRoles.Admin);
        await userManager.AddToRoleAsync(actor, AppRoles.Director); // director trying to remove admin

        var result = await sut.RemoveRoleAsync("actor-1", "target-admin-1", AppRoles.Admin, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task AssignRole_WhenActorHasStrictlyHigherRank_Succeeds()
    {
        var (sut, userManager, _, _) = CreateSut();

        var actor = new User("actor-director") { Id = "actor-1" };
        var target = new User("target-user") { Id = "target-1" };
        await userManager.CreateAsync(actor);
        await userManager.CreateAsync(target);
        await userManager.AddToRoleAsync(actor, AppRoles.Director); // 600

        // Director assigning Coordinator (500) to subordinate (0)
        var result = await sut.AssignRoleAsync("actor-1", "target-1", AppRoles.Coordinator, CancellationToken.None);

        Assert.True(result.Success);
        var inRole = await userManager.IsInRoleAsync(target, AppRoles.Coordinator);
        Assert.True(inRole);
    }

    [Fact]
    public async Task GrantAndRevokePermission_EnforcesPrecedenceAndRank()
    {
        var (sut, userManager, _, _) = CreateSut();

        var actor = new User("actor-director") { Id = "actor-1" };
        var target = new User("target-user") { Id = "target-1" };
        await userManager.CreateAsync(actor);
        await userManager.CreateAsync(target);
        await userManager.AddToRoleAsync(actor, AppRoles.Director); // 600

        // Director grants Students.Create to target
        var grantResult = await sut.GrantPermissionAsync("actor-1", "target-1", AppPermissions.StudentsCreate, CancellationToken.None);
        Assert.True(grantResult.Success);

        var permResult1 = await sut.GetUserPermissionsAsync("target-1", CancellationToken.None);
        Assert.True(permResult1.Success);
        Assert.Contains(AppPermissions.StudentsCreate, permResult1.Value!.DirectGrants);
        Assert.Contains(AppPermissions.StudentsCreate, permResult1.Value!.EffectivePermissions);

        // Director explicitly revokes Students.Create from target
        var revokeResult = await sut.RevokePermissionAsync("actor-1", "target-1", AppPermissions.StudentsCreate, CancellationToken.None);
        Assert.True(revokeResult.Success);

        var permResult2 = await sut.GetUserPermissionsAsync("target-1", CancellationToken.None);
        Assert.True(permResult2.Success);
        Assert.Contains(AppPermissions.StudentsCreate, permResult2.Value!.ExplicitRevocations);
        Assert.DoesNotContain(AppPermissions.StudentsCreate, permResult2.Value!.DirectGrants);
        Assert.DoesNotContain(AppPermissions.StudentsCreate, permResult2.Value!.EffectivePermissions);
    }

    [Fact]
    public async Task GrantPermission_WhenActorHasInsufficientRank_Fails()
    {
        var (sut, userManager, _, _) = CreateSut();

        var actor = new User("actor-volunteer") { Id = "actor-1" };
        var target = new User("target-user") { Id = "target-1" };
        await userManager.CreateAsync(actor);
        await userManager.CreateAsync(target);
        await userManager.AddToRoleAsync(actor, AppRoles.Volunteer); // 200

        // Volunteer trying to grant Students.Create (requires rank > 500)
        var result = await sut.GrantPermissionAsync("actor-1", "target-1", AppPermissions.StudentsCreate, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("insufficient", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AssignPermissionGroup_AssignsAndRemovesGroupSafely()
    {
        var (sut, userManager, _, _) = CreateSut();

        var actor = new User("actor-director") { Id = "actor-1" };
        var target = new User("target-user") { Id = "target-1" };
        await userManager.CreateAsync(actor);
        await userManager.CreateAsync(target);
        await userManager.AddToRoleAsync(actor, AppRoles.Director); // 600

        var assignResult = await sut.AssignPermissionGroupAsync(
            "actor-1",
            "target-1",
            AppPermissionGroups.StudentAdministration,
            CancellationToken.None);

        Assert.True(assignResult.Success);

        var userPerms = await sut.GetUserPermissionsAsync("target-1", CancellationToken.None);
        Assert.True(userPerms.Success);
        Assert.Contains(AppPermissionGroups.StudentAdministration, userPerms.Value!.AssignedGroups);
        Assert.Contains(AppPermissions.StudentsRead, userPerms.Value!.EffectivePermissions);

        var removeResult = await sut.RemovePermissionGroupAsync(
            "actor-1",
            "target-1",
            AppPermissionGroups.StudentAdministration,
            CancellationToken.None);

        Assert.True(removeResult.Success);

        var userPermsAfter = await sut.GetUserPermissionsAsync("target-1", CancellationToken.None);
        Assert.DoesNotContain(AppPermissionGroups.StudentAdministration, userPermsAfter.Value!.AssignedGroups);
    }
}

