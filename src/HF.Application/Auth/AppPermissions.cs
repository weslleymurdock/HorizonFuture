namespace HF.Application.Auth;

/// <summary>
/// Centralized catalog of stable application permissions and authorization policies.
/// </summary>
public static class AppPermissions
{
    // Students
    public const string StudentsRead = "Students.Read";
    public const string StudentsCreate = "Students.Create";
    public const string StudentsUpdate = "Students.Update";
    public const string StudentsDelete = "Students.Delete";

    // Teachers
    public const string TeachersRead = "Teachers.Read";
    public const string TeachersCreate = "Teachers.Create";
    public const string TeachersUpdate = "Teachers.Update";
    public const string TeachersDelete = "Teachers.Delete";

    // Managers
    public const string ManagersRead = "Managers.Read";
    public const string ManagersCreate = "Managers.Create";
    public const string ManagersUpdate = "Managers.Update";
    public const string ManagersDelete = "Managers.Delete";

    // Directors
    public const string DirectorsRead = "Directors.Read";
    public const string DirectorsCreate = "Directors.Create";
    public const string DirectorsUpdate = "Directors.Update";
    public const string DirectorsDelete = "Directors.Delete";

    // Collaborators
    public const string CollaboratorsRead = "Collaborators.Read";
    public const string CollaboratorsCreate = "Collaborators.Create";
    public const string CollaboratorsUpdate = "Collaborators.Update";
    public const string CollaboratorsDelete = "Collaborators.Delete";

    // Volunteers
    public const string VolunteersRead = "Volunteers.Read";
    public const string VolunteersCreate = "Volunteers.Create";
    public const string VolunteersUpdate = "Volunteers.Update";
    public const string VolunteersDelete = "Volunteers.Delete";

    // Courses
    public const string CoursesRead = "Courses.Read";
    public const string CoursesCreate = "Courses.Create";
    public const string CoursesUpdate = "Courses.Update";
    public const string CoursesDelete = "Courses.Delete";

    // Subjects
    public const string SubjectsRead = "Subjects.Read";
    public const string SubjectsCreate = "Subjects.Create";
    public const string SubjectsUpdate = "Subjects.Update";
    public const string SubjectsDelete = "Subjects.Delete";

    // Assets
    public const string AssetsRead = "Assets.Read";
    public const string AssetsCreate = "Assets.Create";
    public const string AssetsUpdate = "Assets.Update";
    public const string AssetsDelete = "Assets.Delete";

    // Liabilities
    public const string LiabilitiesRead = "Liabilities.Read";
    public const string LiabilitiesCreate = "Liabilities.Create";
    public const string LiabilitiesUpdate = "Liabilities.Update";
    public const string LiabilitiesDelete = "Liabilities.Delete";

    // Teacher Assignments
    public const string TeacherAssignmentsRead = "TeacherAssignments.Read";
    public const string TeacherAssignmentsCreate = "TeacherAssignments.Create";
    public const string TeacherAssignmentsUpdate = "TeacherAssignments.Update";
    public const string TeacherAssignmentsDelete = "TeacherAssignments.Delete";

    // Enrollments
    public const string EnrollmentsRead = "Enrollments.Read";
    public const string EnrollmentsCreate = "Enrollments.Create";
    public const string EnrollmentsUpdate = "Enrollments.Update";
    public const string EnrollmentsDelete = "Enrollments.Delete";

    // Class Sessions
    public const string ClassSessionsRead = "ClassSessions.Read";
    public const string ClassSessionsCreate = "ClassSessions.Create";
    public const string ClassSessionsUpdate = "ClassSessions.Update";
    public const string ClassSessionsDelete = "ClassSessions.Delete";

    // Attendances
    public const string AttendancesRead = "Attendances.Read";
    public const string AttendancesCreate = "Attendances.Create";
    public const string AttendancesUpdate = "Attendances.Update";
    public const string AttendancesDelete = "Attendances.Delete";

    // Identity and Access Administration
    public const string RolesRead = "Roles.Read";
    public const string RolesManage = "Roles.Manage";
    public const string PermissionsRead = "Permissions.Read";
    public const string PermissionsManage = "Permissions.Manage";

    public static readonly IReadOnlyList<string> All =
    [
        StudentsRead, StudentsCreate, StudentsUpdate, StudentsDelete,
        TeachersRead, TeachersCreate, TeachersUpdate, TeachersDelete,
        ManagersRead, ManagersCreate, ManagersUpdate, ManagersDelete,
        DirectorsRead, DirectorsCreate, DirectorsUpdate, DirectorsDelete,
        CollaboratorsRead, CollaboratorsCreate, CollaboratorsUpdate, CollaboratorsDelete,
        VolunteersRead, VolunteersCreate, VolunteersUpdate, VolunteersDelete,
        CoursesRead, CoursesCreate, CoursesUpdate, CoursesDelete,
        SubjectsRead, SubjectsCreate, SubjectsUpdate, SubjectsDelete,
        AssetsRead, AssetsCreate, AssetsUpdate, AssetsDelete,
        LiabilitiesRead, LiabilitiesCreate, LiabilitiesUpdate, LiabilitiesDelete,
        TeacherAssignmentsRead, TeacherAssignmentsCreate, TeacherAssignmentsUpdate, TeacherAssignmentsDelete,
        EnrollmentsRead, EnrollmentsCreate, EnrollmentsUpdate, EnrollmentsDelete,
        ClassSessionsRead, ClassSessionsCreate, ClassSessionsUpdate, ClassSessionsDelete,
        AttendancesRead, AttendancesCreate, AttendancesUpdate, AttendancesDelete,
        RolesRead, RolesManage, PermissionsRead, PermissionsManage
    ];

    /// <summary>
    /// Minimum authority rank required to grant or revoke each permission individually.
    /// An actor must hold an authority rank strictly greater than this value and strictly greater than the target user's rank.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, int> MinimumGrantAuthority = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
        // Students
        [StudentsRead] = 100,
        [StudentsCreate] = 500,
        [StudentsUpdate] = 400,
        [StudentsDelete] = 500,

        // Teachers
        [TeachersRead] = 100,
        [TeachersCreate] = 500,
        [TeachersUpdate] = 500,
        [TeachersDelete] = 600,

        // Managers
        [ManagersRead] = 500,
        [ManagersCreate] = 600,
        [ManagersUpdate] = 600,
        [ManagersDelete] = 600,

        // Directors
        [DirectorsRead] = 500,
        [DirectorsCreate] = 600,
        [DirectorsUpdate] = 600,
        [DirectorsDelete] = 600,

        // Collaborators
        [CollaboratorsRead] = 400,
        [CollaboratorsCreate] = 400,
        [CollaboratorsUpdate] = 400,
        [CollaboratorsDelete] = 500,

        // Volunteers
        [VolunteersRead] = 200,
        [VolunteersCreate] = 400,
        [VolunteersUpdate] = 400,
        [VolunteersDelete] = 500,

        // Courses
        [CoursesRead] = 100,
        [CoursesCreate] = 500,
        [CoursesUpdate] = 500,
        [CoursesDelete] = 500,

        // Subjects
        [SubjectsRead] = 100,
        [SubjectsCreate] = 500,
        [SubjectsUpdate] = 500,
        [SubjectsDelete] = 500,

        // Assets
        [AssetsRead] = 400,
        [AssetsCreate] = 400,
        [AssetsUpdate] = 400,
        [AssetsDelete] = 500,

        // Liabilities
        [LiabilitiesRead] = 400,
        [LiabilitiesCreate] = 400,
        [LiabilitiesUpdate] = 400,
        [LiabilitiesDelete] = 500,

        // Teacher Assignments
        [TeacherAssignmentsRead] = 300,
        [TeacherAssignmentsCreate] = 500,
        [TeacherAssignmentsUpdate] = 500,
        [TeacherAssignmentsDelete] = 500,

        // Enrollments
        [EnrollmentsRead] = 200,
        [EnrollmentsCreate] = 500,
        [EnrollmentsUpdate] = 500,
        [EnrollmentsDelete] = 500,

        // Class Sessions
        [ClassSessionsRead] = 100,
        [ClassSessionsCreate] = 300,
        [ClassSessionsUpdate] = 300,
        [ClassSessionsDelete] = 500,

        // Attendances
        [AttendancesRead] = 100,
        [AttendancesCreate] = 200,
        [AttendancesUpdate] = 200,
        [AttendancesDelete] = 500,

        // Roles & Permissions administration
        [RolesRead] = 500,
        [RolesManage] = 600,
        [PermissionsRead] = 500,
        [PermissionsManage] = 600
    };

    public static bool IsValidPermission(string? permissionName) =>
        permissionName != null && MinimumGrantAuthority.ContainsKey(permissionName);

    public static int GetRequiredGrantRank(string permissionName) =>
        MinimumGrantAuthority.TryGetValue(permissionName, out int rank) ? rank : int.MaxValue;
}

