using FluentValidation;
using HF.Application.Courses.Abstract;
using HF.Application.TeacherAssignments.Abstract;
using HF.Application.TeacherAssignments.Requests;
using HF.Application.Teachers.Abstract;

namespace HF.Application.TeacherAssignments.Validators;

/// <summary>Validates requests to create a new teacher assignment.</summary>
public sealed class CreateTeacherAssignmentRequestValidator : AbstractValidator<CreateTeacherAssignmentRequest>
{
    public CreateTeacherAssignmentRequestValidator(
        ITeacherAssignmentService assignmentService,
        ITeacherService teacherService,
        ISubjectService subjectService)
    {
        _ = RuleFor(x => x.TeacherId)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Teacher identifier is required.")
            .MustAsync(async (teacherId, ct) => await teacherService.GetByIdAsync(teacherId, ct) is not null)
            .WithErrorCode("NOT_FOUND")
            .WithMessage("Referenced teacher does not exist.");

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

        _ = RuleFor(x => x)
            .MustAsync(async (request, ct) => !await assignmentService.AssignmentExistsAsync(
                request.TeacherId,
                request.SubjectId,
                request.AcademicYear,
                request.AcademicSemester,
                null,
                ct))
            .WithErrorCode("CONFLICT")
            .WithMessage("The teacher is already assigned to this subject for the specified academic period.");
    }
}

/// <summary>Validates requests to update an existing teacher assignment.</summary>
public sealed class UpdateTeacherAssignmentRequestValidator : AbstractValidator<UpdateTeacherAssignmentRequest>
{
    public UpdateTeacherAssignmentRequestValidator(
        ITeacherAssignmentService assignmentService,
        ITeacherService teacherService,
        ISubjectService subjectService)
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Teacher assignment identifier is required.");

        _ = RuleFor(x => x.TeacherId)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Teacher identifier is required.")
            .MustAsync(async (teacherId, ct) => await teacherService.GetByIdAsync(teacherId, ct) is not null)
            .WithErrorCode("NOT_FOUND")
            .WithMessage("Referenced teacher does not exist.");

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

        _ = RuleFor(x => x)
            .MustAsync(async (request, ct) => !await assignmentService.AssignmentExistsAsync(
                request.TeacherId,
                request.SubjectId,
                request.AcademicYear,
                request.AcademicSemester,
                request.Id,
                ct))
            .WithErrorCode("CONFLICT")
            .WithMessage("The teacher is already assigned to this subject for the specified academic period.");
    }
}

/// <summary>Validates requests to get a teacher assignment by identifier.</summary>
public sealed class GetTeacherAssignmentByIdRequestValidator : AbstractValidator<GetTeacherAssignmentByIdRequest>
{
    public GetTeacherAssignmentByIdRequestValidator()
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Teacher assignment identifier is required.");
    }
}

/// <summary>Validates requests to delete a teacher assignment.</summary>
public sealed class DeleteTeacherAssignmentRequestValidator : AbstractValidator<DeleteTeacherAssignmentRequest>
{
    public DeleteTeacherAssignmentRequestValidator()
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Teacher assignment identifier is required.");
    }
}

