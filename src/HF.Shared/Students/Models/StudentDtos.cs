using System;

namespace HF.Shared.Students.Models;

/// <summary>
/// Status of a student within the educational system.
/// </summary>
public enum StudentStatus
{
    Active = 1,
    Inactive = 2,
    Graduated = 3,
    Suspended = 4,
    Transferred = 5
}

/// <summary>
/// DTO representing a student returned by the API for UI presentation.
/// </summary>
public sealed record StudentItemDto(
    string Id,
    string UserId,
    string RegistrationNumber,
    DateOnly DateOfBirth,
    string? GuardianName,
    string? GuardianRelationship,
    string? GuardianPhone,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? SchoolUnitId,
    StudentStatus Status,
    string? SpecialNeeds,
    string? MedicalNotes,
    DateOnly EnrollmentDate,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? CreatedBy,
    string? UpdatedBy);

/// <summary>
/// DTO for creating a new student record.
/// </summary>
public sealed record CreateStudentDto(
    string UserId,
    string RegistrationNumber,
    DateOnly DateOfBirth,
    string? GuardianName = null,
    string? GuardianRelationship = null,
    string? GuardianPhone = null,
    string? EmergencyContactName = null,
    string? EmergencyContactPhone = null,
    string? SchoolUnitId = null,
    StudentStatus Status = StudentStatus.Active,
    string? SpecialNeeds = null,
    string? MedicalNotes = null,
    DateOnly? EnrollmentDate = null);

/// <summary>
/// DTO for updating an existing student record.
/// </summary>
public sealed record UpdateStudentDto(
    string Id,
    string RegistrationNumber,
    DateOnly DateOfBirth,
    string? GuardianName = null,
    string? GuardianRelationship = null,
    string? GuardianPhone = null,
    string? EmergencyContactName = null,
    string? EmergencyContactPhone = null,
    string? SchoolUnitId = null,
    StudentStatus Status = StudentStatus.Active,
    string? SpecialNeeds = null,
    string? MedicalNotes = null);

/// <summary>
/// Optional filters for querying students via GET /api/v1/students.
/// </summary>
public sealed record StudentFilterDto(
    string? SchoolUnitId = null,
    StudentStatus? Status = null);
