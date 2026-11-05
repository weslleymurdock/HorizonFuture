using FluentValidation;
using HF.Application.Courses.Abstract;
using HF.Application.Courses.Requests;

namespace HF.Application.Courses.Validators;

/// <summary>Validates requests to create a new subject.</summary>
public sealed class CreateSubjectRequestValidator : AbstractValidator<CreateSubjectRequest>
{
    public CreateSubjectRequestValidator(ISubjectService subjectService, ICourseService courseService)
    {
        _ = RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Subject name cannot be empty.")
            .MaximumLength(150)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Subject name cannot exceed 150 characters.");

        _ = RuleFor(x => x.Code)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Subject code cannot be empty.")
            .MaximumLength(30)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Subject code cannot exceed 30 characters.")
            .MustAsync(async (code, ct) => !await subjectService.CodeExistsAsync(code, null, ct))
            .WithErrorCode("CONFLICT")
            .WithMessage("Subject code is already in use.");

        _ = RuleFor(x => x.WorkloadHours)
            .GreaterThan(0)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Workload hours must be greater than zero.");

        _ = RuleFor(x => x.CourseId)
            .MustAsync(async (courseId, ct) => courseId is null || await courseService.ExistsAsync(courseId, ct))
            .WithErrorCode("NOT_FOUND")
            .WithMessage("Referenced course does not exist.");
    }
}

/// <summary>Validates requests to update an existing subject.</summary>
public sealed class UpdateSubjectRequestValidator : AbstractValidator<UpdateSubjectRequest>
{
    public UpdateSubjectRequestValidator(ISubjectService subjectService, ICourseService courseService)
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Subject identifier is required.");

        _ = RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Subject name cannot be empty.")
            .MaximumLength(150)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Subject name cannot exceed 150 characters.");

        _ = RuleFor(x => x.Code)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Subject code cannot be empty.")
            .MaximumLength(30)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Subject code cannot exceed 30 characters.")
            .MustAsync(async (request, code, ct) => !await subjectService.CodeExistsAsync(code, request.Id, ct))
            .WithErrorCode("CONFLICT")
            .WithMessage("Subject code is already in use.");

        _ = RuleFor(x => x.WorkloadHours)
            .GreaterThan(0)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Workload hours must be greater than zero.");

        _ = RuleFor(x => x.CourseId)
            .MustAsync(async (courseId, ct) => courseId is null || await courseService.ExistsAsync(courseId, ct))
            .WithErrorCode("NOT_FOUND")
            .WithMessage("Referenced course does not exist.");
    }
}

/// <summary>Validates requests to get a subject by identifier.</summary>
public sealed class GetSubjectByIdRequestValidator : AbstractValidator<GetSubjectByIdRequest>
{
    public GetSubjectByIdRequestValidator()
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Subject identifier is required.");
    }
}

/// <summary>Validates requests to delete a subject.</summary>
public sealed class DeleteSubjectRequestValidator : AbstractValidator<DeleteSubjectRequest>
{
    public DeleteSubjectRequestValidator()
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Subject identifier is required.");
    }
}

