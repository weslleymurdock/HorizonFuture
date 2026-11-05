using FluentValidation;
using HF.Application.Courses.Abstract;
using HF.Application.Enrollments.Abstract;
using HF.Application.Enrollments.Requests;
using HF.Application.Students.Abstract;

namespace HF.Application.Enrollments.Validators;

/// <summary>Validates requests to create a new enrollment.</summary>
public sealed class CreateEnrollmentRequestValidator : AbstractValidator<CreateEnrollmentRequest>
{
    public CreateEnrollmentRequestValidator(
        IEnrollmentService enrollmentService,
        IStudentService studentService,
        ISubjectService subjectService)
    {
        _ = RuleFor(x => x.StudentId)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Student identifier is required.")
            .MustAsync(async (studentId, ct) => await studentService.GetByIdAsync(studentId, ct) is not null)
            .WithErrorCode("NOT_FOUND")
            .WithMessage("Referenced student does not exist.");

        _ = RuleFor(x => x.SubjectId)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Subject identifier is required.")
            .MustAsync(async (subjectId, ct) => await subjectService.ExistsAsync(subjectId, ct))
            .WithErrorCode("NOT_FOUND")
            .WithMessage("Referenced subject does not exist.");

        _ = RuleFor(x => x.AcademicYear)
            .GreaterThanOrEqualTo(2000)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Academic year must be a valid four-digit year (>= 2000).");

        _ = RuleFor(x => x.AcademicSemester)
            .InclusiveBetween(1, 4)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Academic semester must be between 1 and 4.");

        _ = RuleFor(x => x.FinalGrade)
            .InclusiveBetween(0, 100)
            .When(x => x.FinalGrade.HasValue)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Final grade must be between 0 and 100.");

        _ = RuleFor(x => x)
            .MustAsync(async (request, ct) => !await enrollmentService.EnrollmentExistsAsync(
                request.StudentId,
                request.SubjectId,
                request.AcademicYear,
                request.AcademicSemester,
                null,
                ct))
            .WithErrorCode("CONFLICT")
            .WithMessage("The student is already enrolled in this subject for the specified academic period.");
    }
}

/// <summary>Validates requests to update an existing enrollment.</summary>
public sealed class UpdateEnrollmentRequestValidator : AbstractValidator<UpdateEnrollmentRequest>
{
    public UpdateEnrollmentRequestValidator(
        IEnrollmentService enrollmentService,
        IStudentService studentService,
        ISubjectService subjectService)
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Enrollment identifier is required.");

        _ = RuleFor(x => x.StudentId)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Student identifier is required.")
            .MustAsync(async (studentId, ct) => await studentService.GetByIdAsync(studentId, ct) is not null)
            .WithErrorCode("NOT_FOUND")
            .WithMessage("Referenced student does not exist.");

        _ = RuleFor(x => x.SubjectId)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Subject identifier is required.")
            .MustAsync(async (subjectId, ct) => await subjectService.ExistsAsync(subjectId, ct))
            .WithErrorCode("NOT_FOUND")
            .WithMessage("Referenced subject does not exist.");

        _ = RuleFor(x => x.AcademicYear)
            .GreaterThanOrEqualTo(2000)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Academic year must be a valid four-digit year (>= 2000).");

        _ = RuleFor(x => x.AcademicSemester)
            .InclusiveBetween(1, 4)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Academic semester must be between 1 and 4.");

        _ = RuleFor(x => x.FinalGrade)
            .InclusiveBetween(0, 100)
            .When(x => x.FinalGrade.HasValue)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Final grade must be between 0 and 100.");

        _ = RuleFor(x => x)
            .MustAsync(async (request, ct) => !await enrollmentService.EnrollmentExistsAsync(
                request.StudentId,
                request.SubjectId,
                request.AcademicYear,
                request.AcademicSemester,
                request.Id,
                ct))
            .WithErrorCode("CONFLICT")
            .WithMessage("The student is already enrolled in this subject for the specified academic period.");
    }
}

/// <summary>Validates requests to get an enrollment by identifier.</summary>
public sealed class GetEnrollmentByIdRequestValidator : AbstractValidator<GetEnrollmentByIdRequest>
{
    public GetEnrollmentByIdRequestValidator()
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Enrollment identifier is required.");
    }
}

/// <summary>Validates requests to delete an enrollment.</summary>
public sealed class DeleteEnrollmentRequestValidator : AbstractValidator<DeleteEnrollmentRequest>
{
    public DeleteEnrollmentRequestValidator()
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Enrollment identifier is required.");
    }
}

