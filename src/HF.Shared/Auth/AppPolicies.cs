namespace HF.Shared.Auth;

/// <summary>
/// Canonical constants for authorization policies matching backend permission names exactly.
/// </summary>
public static class AppPolicies
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

    // Enrollments
    public const string EnrollmentsRead = "Enrollments.Read";
    public const string EnrollmentsCreate = "Enrollments.Create";
    public const string EnrollmentsUpdate = "Enrollments.Update";
    public const string EnrollmentsDelete = "Enrollments.Delete";

    // Identity and Access Management
    public const string RolesRead = "Roles.Read";
    public const string RolesManage = "Roles.Manage";
    public const string PermissionsRead = "Permissions.Read";
    public const string PermissionsManage = "Permissions.Manage";

    public static readonly System.Collections.Generic.IReadOnlyList<string> All = new[]
    {
        StudentsRead, StudentsCreate, StudentsUpdate, StudentsDelete,
        TeachersRead, TeachersCreate, TeachersUpdate, TeachersDelete,
        CoursesRead, CoursesCreate, CoursesUpdate, CoursesDelete,
        SubjectsRead, SubjectsCreate, SubjectsUpdate, SubjectsDelete,
        EnrollmentsRead, EnrollmentsCreate, EnrollmentsUpdate, EnrollmentsDelete,
        RolesRead, RolesManage, PermissionsRead, PermissionsManage
    };
}

