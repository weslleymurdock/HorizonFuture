using FluentValidation;
using HF.Application.Courses.Abstract;
using HF.Application.Courses.Requests;

namespace HF.Application.Courses.Validators;

/// <summary>Validates requests to create a new course.</summary>
public sealed class CreateCourseRequestValidator : AbstractValidator<CreateCourseRequest>
{
    public CreateCourseRequestValidator(ICourseService courseService)
    {
        _ = RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Course name cannot be empty.")
            .MaximumLength(150)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Course name cannot exceed 150 characters.");

        _ = RuleFor(x => x.Code)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Course code cannot be empty.")
            .MaximumLength(30)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Course code cannot exceed 30 characters.")
            .MustAsync(async (code, ct) => !await courseService.CodeExistsAsync(code, null, ct))
            .WithErrorCode("CONFLICT")
            .WithMessage("Course code is already in use.");

        _ = RuleFor(x => x.WorkloadHours)
            .GreaterThan(0)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Workload hours must be greater than zero.");
    }
}

/// <summary>Validates requests to update an existing course.</summary>
public sealed class UpdateCourseRequestValidator : AbstractValidator<UpdateCourseRequest>
{
    public UpdateCourseRequestValidator(ICourseService courseService)
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Course identifier is required.");

        _ = RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Course name cannot be empty.")
            .MaximumLength(150)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Course name cannot exceed 150 characters.");

        _ = RuleFor(x => x.Code)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Course code cannot be empty.")
            .MaximumLength(30)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Course code cannot exceed 30 characters.")
            .MustAsync(async (request, code, ct) => !await courseService.CodeExistsAsync(code, request.Id, ct))
            .WithErrorCode("CONFLICT")
            .WithMessage("Course code is already in use.");

        _ = RuleFor(x => x.WorkloadHours)
            .GreaterThan(0)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Workload hours must be greater than zero.");
    }
}

/// <summary>Validates requests to get a course by identifier.</summary>
public sealed class GetCourseByIdRequestValidator : AbstractValidator<GetCourseByIdRequest>
{
    public GetCourseByIdRequestValidator()
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Course identifier is required.");
    }
}

/// <summary>Validates requests to delete a course.</summary>
public sealed class DeleteCourseRequestValidator : AbstractValidator<DeleteCourseRequest>
{
    public DeleteCourseRequestValidator()
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Course identifier is required.");
    }
}

