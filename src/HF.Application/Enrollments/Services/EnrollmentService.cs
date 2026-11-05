using HF.Application.Common.Abstractions.Repositories;
using HF.Application.Enrollments.Abstract;
using HF.Application.Enrollments.Requests;
using HF.Application.Enrollments.Responses;
using HF.Domain.Attendances;
using HF.Domain.Courses;
using HF.Domain.Enrollments;
using HF.Domain.Students;

namespace HF.Application.Enrollments.Services;

/// <summary>
/// Implements application use cases and business rules for enrollments.
/// </summary>
public sealed class EnrollmentService(
    IRepository<Enrollment> enrollmentRepository,
    IRepository<Student> studentRepository,
    IRepository<Subject> subjectRepository,
    IRepository<Attendance> attendanceRepository)
    : IEnrollmentService
{
    /// <inheritdoc />
    public async Task<EnrollmentResponse> CreateAsync(CreateEnrollmentRequest request, CancellationToken cancellationToken)
    {
        var studentExists = await studentRepository.ExistsAsync(s => s.Id == request.StudentId, cancellationToken);
        if (!studentExists)
        {
            throw new KeyNotFoundException("Referenced student does not exist.");
        }

        var subjectExists = await subjectRepository.ExistsAsync(s => s.Id == request.SubjectId, cancellationToken);
        if (!subjectExists)
        {
            throw new KeyNotFoundException("Referenced subject does not exist.");
        }

        var enrollment = new Enrollment
        {
            StudentId = request.StudentId.Trim(),
            SubjectId = request.SubjectId.Trim(),
            AcademicYear = request.AcademicYear,
            AcademicSemester = request.AcademicSemester,
            EnrollmentDate = request.EnrollmentDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            Status = request.Status,
            FinalGrade = request.FinalGrade
        };

        await enrollmentRepository.AddAsync(enrollment, cancellationToken);
        return MapToResponse(enrollment);
    }

    /// <inheritdoc />
    public async Task<EnrollmentResponse?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var enrollment = await enrollmentRepository.GetByIdAsync(id, [], cancellationToken);
        return enrollment is null ? null : MapToResponse(enrollment);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EnrollmentResponse>> ListAsync(ListEnrollmentsRequest request, CancellationToken cancellationToken)
    {
        var enrollments = await enrollmentRepository.ListAsync(
            e => (request.StudentId == null || e.StudentId == request.StudentId) &&
                 (request.SubjectId == null || e.SubjectId == request.SubjectId) &&
                 (request.AcademicYear == null || e.AcademicYear == request.AcademicYear) &&
                 (request.AcademicSemester == null || e.AcademicSemester == request.AcademicSemester) &&
                 (request.Status == null || e.Status == request.Status),
            [],
            cancellationToken);

        return enrollments.Select(MapToResponse).ToList();
    }

    /// <inheritdoc />
    public async Task<EnrollmentResponse?> UpdateAsync(UpdateEnrollmentRequest request, CancellationToken cancellationToken)
    {
        var enrollment = await enrollmentRepository.GetTrackedByIdAsync(request.Id, [], cancellationToken);
        if (enrollment is null)
        {
            return null;
        }

        var studentExists = await studentRepository.ExistsAsync(s => s.Id == request.StudentId, cancellationToken);
        if (!studentExists)
        {
            throw new KeyNotFoundException("Referenced student does not exist.");
        }

        var subjectExists = await subjectRepository.ExistsAsync(s => s.Id == request.SubjectId, cancellationToken);
        if (!subjectExists)
        {
            throw new KeyNotFoundException("Referenced subject does not exist.");
        }

        enrollment.StudentId = request.StudentId.Trim();
        enrollment.SubjectId = request.SubjectId.Trim();
        enrollment.AcademicYear = request.AcademicYear;
        enrollment.AcademicSemester = request.AcademicSemester;
        enrollment.EnrollmentDate = request.EnrollmentDate;
        enrollment.Status = request.Status;
        enrollment.FinalGrade = request.FinalGrade;
        enrollment.UpdatedAt = DateTime.UtcNow;

        await enrollmentRepository.UpdateAsync(enrollment, cancellationToken);
        return MapToResponse(enrollment);
    }

    /// <inheritdoc />
    public async Task<(bool Succeeded, string? Error)> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var enrollment = await enrollmentRepository.GetTrackedByIdAsync(id, [], cancellationToken);
        if (enrollment is null)
        {
            return (false, "Enrollment not found.");
        }

        bool hasAttendances = await attendanceRepository.ExistsAsync(a => a.EnrollmentId == id, cancellationToken);
        if (hasAttendances)
        {
            return (false, "Cannot delete enrollment with associated attendance records.");
        }

        await enrollmentRepository.RemoveAsync(enrollment, cancellationToken);
        return (true, null);
    }

    /// <inheritdoc />
    public async Task<bool> EnrollmentExistsAsync(string studentId, string subjectId, int academicYear, int academicSemester, string? excludeId, CancellationToken cancellationToken)
    {
        return await enrollmentRepository.ExistsAsync(
            e => e.StudentId == studentId &&
                 e.SubjectId == subjectId &&
                 e.AcademicYear == academicYear &&
                 e.AcademicSemester == academicSemester &&
                 (excludeId == null || e.Id != excludeId),
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken)
    {
        return await enrollmentRepository.ExistsAsync(e => e.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> HasAttendancesAsync(string enrollmentId, CancellationToken cancellationToken)
    {
        return await attendanceRepository.ExistsAsync(a => a.EnrollmentId == enrollmentId, cancellationToken);
    }

    private static EnrollmentResponse MapToResponse(Enrollment enrollment) =>
        new(
            enrollment.Id,
            enrollment.StudentId,
            enrollment.SubjectId,
            enrollment.AcademicYear,
            enrollment.AcademicSemester,
            enrollment.EnrollmentDate,
            enrollment.Status,
            enrollment.FinalGrade,
            enrollment.CreatedAt,
            enrollment.UpdatedAt,
            enrollment.CreatedBy,
            enrollment.UpdatedBy);
}

