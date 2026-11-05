using HF.Application.Auth;
using HF.Application.Auth.Requests;
using HF.Application.Auth.Validators;
using Xunit;

namespace HF.Application.Tests.Auth;

public class IdentityAdminValidatorsTests
{
    [Fact]
    public void AssignUserRoleRequestValidator_WhenSelfAssigning_FailsValidation()
    {
        var validator = new AssignUserRoleRequestValidator();
        var request = new AssignUserRoleRequest("user-1", "user-1", AppRoles.Director);

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("own roles"));
    }

    [Fact]
    public void AssignUserRoleRequestValidator_WhenAssigningAdmin_FailsValidation()
    {
        var validator = new AssignUserRoleRequestValidator();
        var request = new AssignUserRoleRequest("actor-1", "target-1", AppRoles.Admin);

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Admin role cannot be granted"));
    }

    [Fact]
    public void AssignUserRoleRequestValidator_WhenAssigningUnknownRole_FailsValidation()
    {
        var validator = new AssignUserRoleRequestValidator();
        var request = new AssignUserRoleRequest("actor-1", "target-1", "SuperHacker");

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("authorized catalog"));
    }

    [Fact]
    public void GrantUserPermissionRequestValidator_WhenUnknownPermission_FailsValidation()
    {
        var validator = new GrantUserPermissionRequestValidator();
        var request = new GrantUserPermissionRequest("actor-1", "target-1", "Arbitrary.Permission");

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("authorized catalog"));
    }

    [Fact]
    public void AssignUserPermissionGroupRequestValidator_WhenUnknownGroup_FailsValidation()
    {
        var validator = new AssignUserPermissionGroupRequestValidator();
        var request = new AssignUserPermissionGroupRequest("actor-1", "target-1", "UnknownGroup");

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("authorized catalog"));
    }
}

