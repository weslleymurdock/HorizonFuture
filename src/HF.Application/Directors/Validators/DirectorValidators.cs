using FluentValidation;
using HF.Application.Directors.Abstract;
using HF.Application.Directors.Requests;

namespace HF.Application.Directors.Validators;

/// <summary>Validates requests to create a new director.</summary>
public sealed class CreateDirectorRequestValidator : AbstractValidator<CreateDirectorRequest>
{
    public CreateDirectorRequestValidator(IDirectorService directorService)
    {
        _ = RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("User identifier is required.")
            .MustAsync((userId, ct) => directorService.UserAvailableAsync(userId, null, ct))
            .WithErrorCode("CONFLICT")
            .WithMessage("User does not exist or is already linked to another director profile.");

        _ = RuleFor(x => x.Directorate)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Directorate cannot be empty.")
            .MaximumLength(100)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Directorate cannot exceed 100 characters.");

        _ = RuleFor(x => x.PositionTitle)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Position title cannot be empty.")
            .MaximumLength(100)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Position title cannot exceed 100 characters.");

        _ = RuleFor(x => x)
            .Must(x => !x.MandateEnd.HasValue || x.MandateEnd.Value >= (x.MandateStart ?? DateOnly.FromDateTime(DateTime.UtcNow)))
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Mandate end date must be on or after mandate start date.");
    }
}

/// <summary>Validates requests to update an existing director.</summary>
public sealed class UpdateDirectorRequestValidator : AbstractValidator<UpdateDirectorRequest>
{
    public UpdateDirectorRequestValidator()
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Director identifier is required.");

        _ = RuleFor(x => x.Directorate)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Directorate cannot be empty.")
            .MaximumLength(100)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Directorate cannot exceed 100 characters.");

        _ = RuleFor(x => x.PositionTitle)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Position title cannot be empty.")
            .MaximumLength(100)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Position title cannot exceed 100 characters.");

        _ = RuleFor(x => x)
            .Must(x => !x.MandateEnd.HasValue || !x.MandateStart.HasValue || x.MandateEnd.Value >= x.MandateStart.Value)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Mandate end date must be on or after mandate start date.");
    }
}

/// <summary>Validates requests to get a director by identifier.</summary>
public sealed class GetDirectorByIdRequestValidator : AbstractValidator<GetDirectorByIdRequest>
{
    public GetDirectorByIdRequestValidator()
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Director identifier is required.");
    }
}

/// <summary>Validates requests to delete a director.</summary>
public sealed class DeleteDirectorRequestValidator : AbstractValidator<DeleteDirectorRequest>
{
    public DeleteDirectorRequestValidator()
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Director identifier is required.");
    }
}

