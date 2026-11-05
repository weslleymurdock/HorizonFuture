using HF.Domain.Students;

namespace HF.Application.Students.Responses;

/// <summary>
/// Data contract representing a student.
/// </summary>
public sealed record StudentResponse(
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
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string CreatedBy,
    string? UpdatedBy);

