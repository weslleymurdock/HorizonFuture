using HF.Application.Auth.Abstract;
using HF.Application.Common.Abstractions.Repositories;
using HF.Application.Students.Abstract;
using HF.Application.Students.Requests;
using HF.Application.Students.Responses;
using HF.Domain.Enrollments;
using HF.Domain.Students;

namespace HF.Application.Students.Services;

/// <summary>
/// Implements application use cases and business rules for students.
/// </summary>
public sealed class StudentService(
    IRepository<Student> studentRepository,
    IRepository<Enrollment> enrollmentRepository,
    IIdentityService identityService)
    : IStudentService
{
    /// <inheritdoc />
    public async Task<StudentResponse> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken)
    {
        var student = new Student
        {
            UserId = request.UserId,
            PersonId = request.PersonId?.Trim(),
            RegistrationNumber = request.RegistrationNumber.Trim(),
            DateOfBirth = request.DateOfBirth,
            GuardianName = request.GuardianName?.Trim(),
            GuardianRelationship = request.GuardianRelationship?.Trim(),
            GuardianPhone = request.GuardianPhone?.Trim(),
            EmergencyContactName = request.EmergencyContactName?.Trim(),
            EmergencyContactPhone = request.EmergencyContactPhone?.Trim(),
            SchoolUnitId = request.SchoolUnitId?.Trim(),
            Status = request.Status,
            SpecialNeeds = request.SpecialNeeds?.Trim(),
            MedicalNotes = request.MedicalNotes?.Trim(),
            EnrollmentDate = request.EnrollmentDate ?? DateOnly.FromDateTime(DateTime.UtcNow)
        };

        await studentRepository.AddAsync(student, cancellationToken);
        return MapToResponse(student);
    }

    /// <inheritdoc />
    public async Task<StudentResponse?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var student = await studentRepository.GetByIdAsync(id, [], cancellationToken);
        return student is null ? null : MapToResponse(student);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<StudentResponse>> ListAsync(ListStudentsRequest request, CancellationToken cancellationToken)
    {
        var students = await studentRepository.ListAsync(
            s => (request.SchoolUnitId == null || s.SchoolUnitId == request.SchoolUnitId) &&
                 (request.Status == null || s.Status == request.Status),
            [],
            cancellationToken);

        return students.Select(MapToResponse).ToList();
    }

    /// <inheritdoc />
    public async Task<StudentResponse?> UpdateAsync(UpdateStudentRequest request, CancellationToken cancellationToken)
    {
        var student = await studentRepository.GetTrackedByIdAsync(request.Id, [], cancellationToken);
        if (student is null)
        {
            return null;
        }

        student.RegistrationNumber = request.RegistrationNumber.Trim();
        student.PersonId = request.PersonId?.Trim();
        student.DateOfBirth = request.DateOfBirth;
        student.GuardianName = request.GuardianName?.Trim();
        student.GuardianRelationship = request.GuardianRelationship?.Trim();
        student.GuardianPhone = request.GuardianPhone?.Trim();
        student.EmergencyContactName = request.EmergencyContactName?.Trim();
        student.EmergencyContactPhone = request.EmergencyContactPhone?.Trim();
        student.SchoolUnitId = request.SchoolUnitId?.Trim();
        student.Status = request.Status;
        student.SpecialNeeds = request.SpecialNeeds?.Trim();
        student.MedicalNotes = request.MedicalNotes?.Trim();
        student.UpdatedAt = DateTime.UtcNow;

        await studentRepository.UpdateAsync(student, cancellationToken);
        return MapToResponse(student);
    }

    /// <inheritdoc />
    public async Task<(bool Succeeded, string? Error)> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var student = await studentRepository.GetTrackedByIdAsync(id, [], cancellationToken);
        if (student is null)
        {
            return (false, "Student not found.");
        }

        bool hasEnrollments = await enrollmentRepository.ExistsAsync(e => e.StudentId == id, cancellationToken);
        if (hasEnrollments)
        {
            return (false, "Cannot delete student with existing academic enrollment history.");
        }

        await studentRepository.RemoveAsync(student, cancellationToken);
        return (true, null);
    }

    /// <inheritdoc />
    public async Task<bool> RegistrationNumberExistsAsync(string registrationNumber, string? excludeId, CancellationToken cancellationToken)
    {
        var trimmed = registrationNumber.Trim();
        return await studentRepository.ExistsAsync(
            s => s.RegistrationNumber == trimmed && (excludeId == null || s.Id != excludeId),
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> UserAvailableAsync(string userId, string? excludeId, CancellationToken cancellationToken)
    {
        var user = await identityService.GetInfoAsync(userId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        bool alreadyAssigned = await studentRepository.ExistsAsync(
            s => s.UserId == userId && (excludeId == null || s.Id != excludeId),
            cancellationToken);

        return !alreadyAssigned;
    }

    /// <inheritdoc />
    public async Task<bool> HasEnrollmentsAsync(string studentId, CancellationToken cancellationToken)
    {
        return await enrollmentRepository.ExistsAsync(e => e.StudentId == studentId, cancellationToken);
    }

    private static StudentResponse MapToResponse(Student student) =>
        new(
            student.Id,
            student.UserId,
            student.RegistrationNumber,
            student.DateOfBirth,
            student.GuardianName,
            student.GuardianRelationship,
            student.GuardianPhone,
            student.EmergencyContactName,
            student.EmergencyContactPhone,
            student.SchoolUnitId,
            student.Status,
            student.SpecialNeeds,
            student.MedicalNotes,
            student.EnrollmentDate,
            student.CreatedAt,
            student.UpdatedAt,
            student.CreatedBy,
            student.UpdatedBy);
}
