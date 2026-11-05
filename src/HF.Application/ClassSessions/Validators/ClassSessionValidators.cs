using FluentValidation;
using HF.Application.ClassSessions.Requests;
using HF.Application.TeacherAssignments.Abstract;

namespace HF.Application.ClassSessions.Validators;

/// <summary>Validates requests to create a new class session.</summary>
public sealed class CreateClassSessionRequestValidator : AbstractValidator<CreateClassSessionRequest>
{
    public CreateClassSessionRequestValidator(ITeacherAssignmentService assignmentService)
    {
        _ = RuleFor(x => x.TeacherAssignmentId)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Teacher assignment identifier is required.")
            .MustAsync(async (assignmentId, ct) => await assignmentService.ExistsAsync(assignmentId, ct))
            .WithErrorCode("NOT_FOUND")
            .WithMessage("Referenced teacher assignment does not exist.");

        _ = RuleFor(x => x.DurationMinutes)
            .GreaterThan(0)
            .When(x => x.DurationMinutes.HasValue)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Duration in minutes must be greater than zero.");
    }
}

/// <summary>Validates requests to update an existing class session.</summary>
public sealed class UpdateClassSessionRequestValidator : AbstractValidator<UpdateClassSessionRequest>
{
    public UpdateClassSessionRequestValidator(ITeacherAssignmentService assignmentService)
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Class session identifier is required.");

        _ = RuleFor(x => x.TeacherAssignmentId)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Teacher assignment identifier is required.")
            .MustAsync(async (assignmentId, ct) => await assignmentService.ExistsAsync(assignmentId, ct))
            .WithErrorCode("NOT_FOUND")
            .WithMessage("Referenced teacher assignment does not exist.");

        _ = RuleFor(x => x.DurationMinutes)
            .GreaterThan(0)
            .When(x => x.DurationMinutes.HasValue)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Duration in minutes must be greater than zero.");
    }
}

/// <summary>Validates requests to get a class session by identifier.</summary>
public sealed class GetClassSessionByIdRequestValidator : AbstractValidator<GetClassSessionByIdRequest>
{
    public GetClassSessionByIdRequestValidator()
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Class session identifier is required.");
    }
}

/// <summary>Validates requests to delete a class session.</summary>
public sealed class DeleteClassSessionRequestValidator : AbstractValidator<DeleteClassSessionRequest>
{
    public DeleteClassSessionRequestValidator()
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Class session identifier is required.");
    }
}

