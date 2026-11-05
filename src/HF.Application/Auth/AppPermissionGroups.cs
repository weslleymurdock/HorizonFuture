namespace HF.Application.Auth;

/// <summary>
/// Predefined, stable named groups of application permissions.
/// Clients cannot create arbitrary groups.
/// </summary>
public static class AppPermissionGroups
{
    public const string AcademicManagement = "AcademicManagement";
    public const string StudentAdministration = "StudentAdministration";
    public const string FinancialManagement = "FinancialManagement";
    public const string ClassroomOperations = "ClassroomOperations";
    public const string GeneralAuditor = "GeneralAuditor";

    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> GroupDefinitions =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [AcademicManagement] =
            [
                AppPermissions.CoursesRead,
                AppPermissions.CoursesCreate,
                AppPermissions.CoursesUpdate,
                AppPermissions.CoursesDelete,
                AppPermissions.SubjectsRead,
                AppPermissions.SubjectsCreate,
                AppPermissions.SubjectsUpdate,
                AppPermissions.SubjectsDelete,
                AppPermissions.TeacherAssignmentsRead,
                AppPermissions.TeacherAssignmentsCreate,
                AppPermissions.TeacherAssignmentsUpdate,
                AppPermissions.TeacherAssignmentsDelete
            ],

            [StudentAdministration] =
            [
                AppPermissions.StudentsRead,
                AppPermissions.StudentsCreate,
                AppPermissions.StudentsUpdate,
                AppPermissions.StudentsDelete,
                AppPermissions.EnrollmentsRead,
                AppPermissions.EnrollmentsCreate,
                AppPermissions.EnrollmentsUpdate,
                AppPermissions.EnrollmentsDelete
            ],

            [FinancialManagement] =
            [
                AppPermissions.AssetsRead,
                AppPermissions.AssetsCreate,
                AppPermissions.AssetsUpdate,
                AppPermissions.AssetsDelete,
                AppPermissions.LiabilitiesRead,
                AppPermissions.LiabilitiesCreate,
                AppPermissions.LiabilitiesUpdate,
                AppPermissions.LiabilitiesDelete
            ],

            [ClassroomOperations] =
            [
                AppPermissions.ClassSessionsRead,
                AppPermissions.ClassSessionsCreate,
                AppPermissions.ClassSessionsUpdate,
                AppPermissions.ClassSessionsDelete,
                AppPermissions.AttendancesRead,
                AppPermissions.AttendancesCreate,
                AppPermissions.AttendancesUpdate,
                AppPermissions.AttendancesDelete
            ],

            [GeneralAuditor] =
            [
                AppPermissions.StudentsRead,
                AppPermissions.TeachersRead,
                AppPermissions.ManagersRead,
                AppPermissions.DirectorsRead,
                AppPermissions.CollaboratorsRead,
                AppPermissions.VolunteersRead,
                AppPermissions.CoursesRead,
                AppPermissions.SubjectsRead,
                AppPermissions.AssetsRead,
                AppPermissions.LiabilitiesRead,
                AppPermissions.TeacherAssignmentsRead,
                AppPermissions.EnrollmentsRead,
                AppPermissions.ClassSessionsRead,
                AppPermissions.AttendancesRead
            ]
        };

    /// <summary>
    /// Minimum authority rank required to assign or remove each permission group.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, int> MinimumGrantAuthority =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [AcademicManagement] = 500, // Coordinator+
            [StudentAdministration] = 500, // Coordinator+
            [FinancialManagement] = 400, // Manager+
            [ClassroomOperations] = 300, // Coworker+
            [GeneralAuditor] = 400 // Manager+
        };

    public static readonly IReadOnlyList<string> All =
    [
        AcademicManagement,
        StudentAdministration,
        FinancialManagement,
        ClassroomOperations,
        GeneralAuditor
    ];

    public static bool IsValidGroup(string? groupName) =>
        groupName != null && GroupDefinitions.ContainsKey(groupName);

    public static IReadOnlyList<string> GetPermissionsForGroup(string groupName) =>
        GroupDefinitions.TryGetValue(groupName, out var permissions) ? permissions : [];

    public static int GetRequiredGrantRank(string groupName) =>
        MinimumGrantAuthority.TryGetValue(groupName, out int rank) ? rank : int.MaxValue;
}

