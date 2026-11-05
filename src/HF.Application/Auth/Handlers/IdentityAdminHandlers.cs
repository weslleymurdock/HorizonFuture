using HF.Application.Auth.Abstract;
using HF.Application.Auth.Requests;
using HF.Application.Auth.Responses;
using HF.Application.Common.Responses;
using Mediator;

namespace HF.Application.Auth.Handlers;

/// <summary>
/// Mediator handlers for role and permission administration requests.
/// </summary>
public sealed class IdentityAdminHandlers(IIdentityAdminService identityAdminService)
    : IRequestHandler<ListCatalogRolesRequest, ResponseBase<IReadOnlyList<RoleCatalogResponse>>>,
      IRequestHandler<ListCatalogPermissionsRequest, ResponseBase<PermissionCatalogResponse>>,
      IRequestHandler<GetUserRolesRequest, ResponseBase<UserRolesResponse>>,
      IRequestHandler<AssignUserRoleRequest, ResponseBase<bool>>,
      IRequestHandler<RemoveUserRoleRequest, ResponseBase<bool>>,
      IRequestHandler<GetUserPermissionsRequest, ResponseBase<UserPermissionsResponse>>,
      IRequestHandler<GrantUserPermissionRequest, ResponseBase<bool>>,
      IRequestHandler<RevokeUserPermissionRequest, ResponseBase<bool>>,
      IRequestHandler<ClearUserPermissionRequest, ResponseBase<bool>>,
      IRequestHandler<AssignUserPermissionGroupRequest, ResponseBase<bool>>,
      IRequestHandler<RemoveUserPermissionGroupRequest, ResponseBase<bool>>
{
    public async ValueTask<ResponseBase<IReadOnlyList<RoleCatalogResponse>>> Handle(
        ListCatalogRolesRequest request,
        CancellationToken cancellationToken)
    {
        return await identityAdminService.ListCatalogRolesAsync(cancellationToken);
    }

    public async ValueTask<ResponseBase<PermissionCatalogResponse>> Handle(
        ListCatalogPermissionsRequest request,
        CancellationToken cancellationToken)
    {
        return await identityAdminService.ListCatalogPermissionsAsync(cancellationToken);
    }

    public async ValueTask<ResponseBase<UserRolesResponse>> Handle(
        GetUserRolesRequest request,
        CancellationToken cancellationToken)
    {
        return await identityAdminService.GetUserRolesAsync(request.TargetUserId, cancellationToken);
    }

    public async ValueTask<ResponseBase<bool>> Handle(
        AssignUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        return await identityAdminService.AssignRoleAsync(
            request.ActorUserId,
            request.TargetUserId,
            request.RoleName,
            cancellationToken);
    }

    public async ValueTask<ResponseBase<bool>> Handle(
        RemoveUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        return await identityAdminService.RemoveRoleAsync(
            request.ActorUserId,
            request.TargetUserId,
            request.RoleName,
            cancellationToken);
    }

    public async ValueTask<ResponseBase<UserPermissionsResponse>> Handle(
        GetUserPermissionsRequest request,
        CancellationToken cancellationToken)
    {
        return await identityAdminService.GetUserPermissionsAsync(request.TargetUserId, cancellationToken);
    }

    public async ValueTask<ResponseBase<bool>> Handle(
        GrantUserPermissionRequest request,
        CancellationToken cancellationToken)
    {
        return await identityAdminService.GrantPermissionAsync(
            request.ActorUserId,
            request.TargetUserId,
            request.PermissionName,
            cancellationToken);
    }

    public async ValueTask<ResponseBase<bool>> Handle(
        RevokeUserPermissionRequest request,
        CancellationToken cancellationToken)
    {
        return await identityAdminService.RevokePermissionAsync(
            request.ActorUserId,
            request.TargetUserId,
            request.PermissionName,
            cancellationToken);
    }

    public async ValueTask<ResponseBase<bool>> Handle(
        ClearUserPermissionRequest request,
        CancellationToken cancellationToken)
    {
        return await identityAdminService.ClearPermissionOverrideAsync(
            request.ActorUserId,
            request.TargetUserId,
            request.PermissionName,
            cancellationToken);
    }

    public async ValueTask<ResponseBase<bool>> Handle(
        AssignUserPermissionGroupRequest request,
        CancellationToken cancellationToken)
    {
        return await identityAdminService.AssignPermissionGroupAsync(
            request.ActorUserId,
            request.TargetUserId,
            request.GroupName,
            cancellationToken);
    }

    public async ValueTask<ResponseBase<bool>> Handle(
        RemoveUserPermissionGroupRequest request,
        CancellationToken cancellationToken)
    {
        return await identityAdminService.RemovePermissionGroupAsync(
            request.ActorUserId,
            request.TargetUserId,
            request.GroupName,
            cancellationToken);
    }
}

