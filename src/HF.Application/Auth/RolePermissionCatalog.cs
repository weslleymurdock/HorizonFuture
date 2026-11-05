namespace HF.Application.Auth;

/// <summary>
/// Defines the default permissions granted to each catalog role, and implements the
/// deterministic precedence evaluation rule for user capabilities:
/// 1. Explicit Revocation (Deny)
/// 2. Direct Grant (Allow)
/// 3. Group Membership (Allow)
/// 4. Role Inheritance (Allow)
/// 5. Default (Deny)
/// </summary>
public static class RolePermissionCatalog
{
    private static readonly HashSet<string> AllPermissionsSet = new(AppPermissions.All, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Default permission sets associated with each role.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> RolePermissions =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [AppRoles.Admin] = AllPermissionsSet,

            [AppRoles.Director] = new HashSet<string>(
            [
                // All entity CRUD
                AppPermissions.StudentsRead, AppPermissions.StudentsCreate, AppPermissions.StudentsUpdate, AppPermissions.StudentsDelete,
                AppPermissions.TeachersRead, AppPermissions.TeachersCreate, AppPermissions.TeachersUpdate, AppPermissions.TeachersDelete,
                AppPermissions.ManagersRead, AppPermissions.ManagersCreate, AppPermissions.ManagersUpdate, AppPermissions.ManagersDelete,
                AppPermissions.DirectorsRead, AppPermissions.DirectorsCreate, AppPermissions.DirectorsUpdate, AppPermissions.DirectorsDelete,
                AppPermissions.CollaboratorsRead, AppPermissions.CollaboratorsCreate, AppPermissions.CollaboratorsUpdate, AppPermissions.CollaboratorsDelete,
                AppPermissions.VolunteersRead, AppPermissions.VolunteersCreate, AppPermissions.VolunteersUpdate, AppPermissions.VolunteersDelete,
                AppPermissions.CoursesRead, AppPermissions.CoursesCreate, AppPermissions.CoursesUpdate, AppPermissions.CoursesDelete,
                AppPermissions.SubjectsRead, AppPermissions.SubjectsCreate, AppPermissions.SubjectsUpdate, AppPermissions.SubjectsDelete,
                AppPermissions.AssetsRead, AppPermissions.AssetsCreate, AppPermissions.AssetsUpdate, AppPermissions.AssetsDelete,
                AppPermissions.LiabilitiesRead, AppPermissions.LiabilitiesCreate, AppPermissions.LiabilitiesUpdate, AppPermissions.LiabilitiesDelete,
                AppPermissions.TeacherAssignmentsRead, AppPermissions.TeacherAssignmentsCreate, AppPermissions.TeacherAssignmentsUpdate, AppPermissions.TeacherAssignmentsDelete,
                AppPermissions.EnrollmentsRead, AppPermissions.EnrollmentsCreate, AppPermissions.EnrollmentsUpdate, AppPermissions.EnrollmentsDelete,
                AppPermissions.ClassSessionsRead, AppPermissions.ClassSessionsCreate, AppPermissions.ClassSessionsUpdate, AppPermissions.ClassSessionsDelete,
                AppPermissions.AttendancesRead, AppPermissions.AttendancesCreate, AppPermissions.AttendancesUpdate, AppPermissions.AttendancesDelete,
                // Role & permission admin (scoped to strictly subordinate ranks)
                AppPermissions.RolesRead, AppPermissions.RolesManage,
                AppPermissions.PermissionsRead, AppPermissions.PermissionsManage
            ], StringComparer.OrdinalIgnoreCase),

            [AppRoles.Coordinator] = new HashSet<string>(
            [
                // Read operations across NGO
                AppPermissions.StudentsRead, AppPermissions.TeachersRead, AppPermissions.ManagersRead,
                AppPermissions.CollaboratorsRead, AppPermissions.VolunteersRead, AppPermissions.CoursesRead,
                AppPermissions.SubjectsRead, AppPermissions.AssetsRead, AppPermissions.LiabilitiesRead,
                AppPermissions.TeacherAssignmentsRead, AppPermissions.EnrollmentsRead, AppPermissions.ClassSessionsRead,
                AppPermissions.AttendancesRead, AppPermissions.RolesRead, AppPermissions.PermissionsRead,

                // Academic & pedagogical management
                AppPermissions.StudentsCreate, AppPermissions.StudentsUpdate, AppPermissions.StudentsDelete,
                AppPermissions.TeachersCreate, AppPermissions.TeachersUpdate,
                AppPermissions.VolunteersCreate, AppPermissions.VolunteersUpdate,
                AppPermissions.CoursesCreate, AppPermissions.CoursesUpdate, AppPermissions.CoursesDelete,
                AppPermissions.SubjectsCreate, AppPermissions.SubjectsUpdate, AppPermissions.SubjectsDelete,
                AppPermissions.TeacherAssignmentsCreate, AppPermissions.TeacherAssignmentsUpdate, AppPermissions.TeacherAssignmentsDelete,
                AppPermissions.EnrollmentsCreate, AppPermissions.EnrollmentsUpdate, AppPermissions.EnrollmentsDelete,
                AppPermissions.ClassSessionsCreate, AppPermissions.ClassSessionsUpdate, AppPermissions.ClassSessionsDelete,
                AppPermissions.AttendancesCreate, AppPermissions.AttendancesUpdate, AppPermissions.AttendancesDelete
            ], StringComparer.OrdinalIgnoreCase),

            [AppRoles.Manager] = new HashSet<string>(
            [
                // Operational and administrative management
                AppPermissions.StudentsRead, AppPermissions.CoursesRead, AppPermissions.SubjectsRead,
                AppPermissions.ClassSessionsRead, AppPermissions.AttendancesRead, AppPermissions.VolunteersRead,
                AppPermissions.CollaboratorsRead, AppPermissions.CollaboratorsCreate, AppPermissions.CollaboratorsUpdate, AppPermissions.CollaboratorsDelete,
                AppPermissions.AssetsRead, AppPermissions.AssetsCreate, AppPermissions.AssetsUpdate, AppPermissions.AssetsDelete,
                AppPermissions.LiabilitiesRead, AppPermissions.LiabilitiesCreate, AppPermissions.LiabilitiesUpdate, AppPermissions.LiabilitiesDelete,
                AppPermissions.StudentsUpdate
            ], StringComparer.OrdinalIgnoreCase),

            [AppRoles.Coworker] = new HashSet<string>(
            [
                AppPermissions.StudentsRead,
                AppPermissions.CoursesRead,
                AppPermissions.SubjectsRead,
                AppPermissions.ClassSessionsRead, AppPermissions.ClassSessionsCreate, AppPermissions.ClassSessionsUpdate,
                AppPermissions.AttendancesRead, AppPermissions.AttendancesCreate, AppPermissions.AttendancesUpdate
            ], StringComparer.OrdinalIgnoreCase),

            [AppRoles.Volunteer] = new HashSet<string>(
            [
                AppPermissions.StudentsRead,
                AppPermissions.CoursesRead,
                AppPermissions.SubjectsRead,
                AppPermissions.ClassSessionsRead,
                AppPermissions.AttendancesRead, AppPermissions.AttendancesCreate, AppPermissions.AttendancesUpdate
            ], StringComparer.OrdinalIgnoreCase),

            [AppRoles.Reader] = new HashSet<string>(
            [
                AppPermissions.StudentsRead,
                AppPermissions.TeachersRead,
                AppPermissions.CoursesRead,
                AppPermissions.SubjectsRead,
                AppPermissions.ClassSessionsRead,
                AppPermissions.AttendancesRead
            ], StringComparer.OrdinalIgnoreCase)
        };

    /// <summary>
    /// Evaluates whether a user is authorized for a specific permission given their roles,
    /// direct grants, explicit revocations, and assigned permission groups.
    /// </summary>
    public static bool Evaluate(
        IEnumerable<string> userRoles,
        IEnumerable<string> directGrants,
        IEnumerable<string> explicitRevocations,
        IEnumerable<string> assignedGroups,
        string requestedPermission)
    {
        if (string.IsNullOrWhiteSpace(requestedPermission))
        {
            return false;
        }

        // 1. Explicit Revocation (Deny) takes highest precedence over everything
        var revocationsSet = explicitRevocations as ISet<string> ?? new HashSet<string>(explicitRevocations, StringComparer.OrdinalIgnoreCase);
        if (revocationsSet.Contains(requestedPermission))
        {
            return false;
        }

        // 2. Direct Grant (Allow)
        var grantsSet = directGrants as ISet<string> ?? new HashSet<string>(directGrants, StringComparer.OrdinalIgnoreCase);
        if (grantsSet.Contains(requestedPermission))
        {
            return true;
        }

        // 3. Permission Group Grant (Allow)
        foreach (var group in assignedGroups)
        {
            var groupPermissions = AppPermissionGroups.GetPermissionsForGroup(group);
            if (groupPermissions.Contains(requestedPermission, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        // 4. Role Inheritance (Allow)
        foreach (var role in userRoles)
        {
            if (RolePermissions.TryGetValue(role, out var permissions) && permissions.Contains(requestedPermission))
            {
                return true;
            }
        }

        // 5. Default (Deny)
        return false;
    }

    /// <summary>
    /// Computes the list of all effective permissions for a user according to the precedence rules.
    /// </summary>
    public static IReadOnlyList<string> GetEffectivePermissions(
        IEnumerable<string> userRoles,
        IEnumerable<string> directGrants,
        IEnumerable<string> explicitRevocations,
        IEnumerable<string> assignedGroups)
    {
        var effective = new List<string>();
        var revocationsSet = new HashSet<string>(explicitRevocations, StringComparer.OrdinalIgnoreCase);
        var grantsSet = new HashSet<string>(directGrants, StringComparer.OrdinalIgnoreCase);

        foreach (var permission in AppPermissions.All)
        {
            if (Evaluate(userRoles, grantsSet, revocationsSet, assignedGroups, permission))
            {
                effective.Add(permission);
            }
        }

        return effective;
    }
}

