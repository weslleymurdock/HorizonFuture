using FluentValidation;
using HF.Application.Auth.Requests;

namespace HF.Application.Auth.Validators;

public sealed class AssignUserRoleRequestValidator : AbstractValidator<AssignUserRoleRequest>
{
    public AssignUserRoleRequestValidator()
    {
        _ = RuleFor(x => x.ActorUserId)
            .NotEmpty().WithMessage("Actor user identifier is required.");

        _ = RuleFor(x => x.TargetUserId)
            .NotEmpty().WithMessage("Target user identifier is required.")
            .Must((request, targetId) => targetId != request.ActorUserId)
            .WithMessage("Actors cannot alter their own roles.");

        _ = RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("Role name is required.")
            .Must(AppRoles.IsValidRole).WithMessage("Role does not exist in authorized catalog.")
            .Must(role => !string.Equals(role, AppRoles.Admin, StringComparison.OrdinalIgnoreCase))
            .WithMessage("The Admin role cannot be granted through regular role management.");
    }
}

public sealed class RemoveUserRoleRequestValidator : AbstractValidator<RemoveUserRoleRequest>
{
    public RemoveUserRoleRequestValidator()
    {
        _ = RuleFor(x => x.ActorUserId)
            .NotEmpty().WithMessage("Actor user identifier is required.");

        _ = RuleFor(x => x.TargetUserId)
            .NotEmpty().WithMessage("Target user identifier is required.")
            .Must((request, targetId) => targetId != request.ActorUserId)
            .WithMessage("Actors cannot alter their own roles.");

        _ = RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("Role name is required.")
            .Must(AppRoles.IsValidRole).WithMessage("Role does not exist in authorized catalog.");
    }
}

public sealed class GrantUserPermissionRequestValidator : AbstractValidator<GrantUserPermissionRequest>
{
    public GrantUserPermissionRequestValidator()
    {
        _ = RuleFor(x => x.ActorUserId)
            .NotEmpty().WithMessage("Actor user identifier is required.");

        _ = RuleFor(x => x.TargetUserId)
            .NotEmpty().WithMessage("Target user identifier is required.")
            .Must((request, targetId) => targetId != request.ActorUserId)
            .WithMessage("Actors cannot grant permissions to themselves.");

        _ = RuleFor(x => x.PermissionName)
            .NotEmpty().WithMessage("Permission name is required.")
            .Must(AppPermissions.IsValidPermission).WithMessage("Permission does not exist in authorized catalog.");
    }
}

public sealed class RevokeUserPermissionRequestValidator : AbstractValidator<RevokeUserPermissionRequest>
{
    public RevokeUserPermissionRequestValidator()
    {
        _ = RuleFor(x => x.ActorUserId)
            .NotEmpty().WithMessage("Actor user identifier is required.");

        _ = RuleFor(x => x.TargetUserId)
            .NotEmpty().WithMessage("Target user identifier is required.")
            .Must((request, targetId) => targetId != request.ActorUserId)
            .WithMessage("Actors cannot revoke permissions from themselves.");

        _ = RuleFor(x => x.PermissionName)
            .NotEmpty().WithMessage("Permission name is required.")
            .Must(AppPermissions.IsValidPermission).WithMessage("Permission does not exist in authorized catalog.");
    }
}

public sealed class ClearUserPermissionRequestValidator : AbstractValidator<ClearUserPermissionRequest>
{
    public ClearUserPermissionRequestValidator()
    {
        _ = RuleFor(x => x.ActorUserId)
            .NotEmpty().WithMessage("Actor user identifier is required.");

        _ = RuleFor(x => x.TargetUserId)
            .NotEmpty().WithMessage("Target user identifier is required.")
            .Must((request, targetId) => targetId != request.ActorUserId)
            .WithMessage("Actors cannot clear permission overrides on themselves.");

        _ = RuleFor(x => x.PermissionName)
            .NotEmpty().WithMessage("Permission name is required.")
            .Must(AppPermissions.IsValidPermission).WithMessage("Permission does not exist in authorized catalog.");
    }
}

public sealed class AssignUserPermissionGroupRequestValidator : AbstractValidator<AssignUserPermissionGroupRequest>
{
    public AssignUserPermissionGroupRequestValidator()
    {
        _ = RuleFor(x => x.ActorUserId)
            .NotEmpty().WithMessage("Actor user identifier is required.");

        _ = RuleFor(x => x.TargetUserId)
            .NotEmpty().WithMessage("Target user identifier is required.")
            .Must((request, targetId) => targetId != request.ActorUserId)
            .WithMessage("Actors cannot assign permission groups to themselves.");

        _ = RuleFor(x => x.GroupName)
            .NotEmpty().WithMessage("Group name is required.")
            .Must(AppPermissionGroups.IsValidGroup).WithMessage("Permission group does not exist in authorized catalog.");
    }
}

public sealed class RemoveUserPermissionGroupRequestValidator : AbstractValidator<RemoveUserPermissionGroupRequest>
{
    public RemoveUserPermissionGroupRequestValidator()
    {
        _ = RuleFor(x => x.ActorUserId)
            .NotEmpty().WithMessage("Actor user identifier is required.");

        _ = RuleFor(x => x.TargetUserId)
            .NotEmpty().WithMessage("Target user identifier is required.")
            .Must((request, targetId) => targetId != request.ActorUserId)
            .WithMessage("Actors cannot remove permission groups from themselves.");

        _ = RuleFor(x => x.GroupName)
            .NotEmpty().WithMessage("Group name is required.")
            .Must(AppPermissionGroups.IsValidGroup).WithMessage("Permission group does not exist in authorized catalog.");
    }
}

public sealed class GetUserRolesRequestValidator : AbstractValidator<GetUserRolesRequest>
{
    public GetUserRolesRequestValidator()
    {
        _ = RuleFor(x => x.TargetUserId)
            .NotEmpty().WithMessage("Target user identifier is required.");
    }
}

public sealed class GetUserPermissionsRequestValidator : AbstractValidator<GetUserPermissionsRequest>
{
    public GetUserPermissionsRequestValidator()
    {
        _ = RuleFor(x => x.TargetUserId)
            .NotEmpty().WithMessage("Target user identifier is required.");
    }
}

