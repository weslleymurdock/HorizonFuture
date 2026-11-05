using FluentValidation;
using HF.Application.Volunteers.Abstract;
using HF.Application.Volunteers.Requests;

namespace HF.Application.Volunteers.Validators;

/// <summary>Validates requests to create a new volunteer.</summary>
public sealed class CreateVolunteerRequestValidator : AbstractValidator<CreateVolunteerRequest>
{
    public CreateVolunteerRequestValidator(IVolunteerService volunteerService)
    {
        _ = RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("User identifier is required.")
            .MustAsync((userId, ct) => volunteerService.UserAvailableAsync(userId, null, ct))
            .WithErrorCode("CONFLICT")
            .WithMessage("User does not exist or is already linked to another volunteer profile.");

        _ = RuleFor(x => x.AvailableHoursPerWeek)
            .GreaterThanOrEqualTo(0)
            .When(x => x.AvailableHoursPerWeek.HasValue)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Available hours per week cannot be negative.");

        _ = RuleFor(x => x)
            .Must(x => !x.EndDate.HasValue || x.EndDate.Value >= (x.StartDate ?? DateOnly.FromDateTime(DateTime.UtcNow)))
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("End date must be on or after start date.");
    }
}

/// <summary>Validates requests to update an existing volunteer.</summary>
public sealed class UpdateVolunteerRequestValidator : AbstractValidator<UpdateVolunteerRequest>
{
    public UpdateVolunteerRequestValidator()
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Volunteer identifier is required.");

        _ = RuleFor(x => x.AvailableHoursPerWeek)
            .GreaterThanOrEqualTo(0)
            .When(x => x.AvailableHoursPerWeek.HasValue)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Available hours per week cannot be negative.");

        _ = RuleFor(x => x)
            .Must(x => !x.EndDate.HasValue || !x.StartDate.HasValue || x.EndDate.Value >= x.StartDate.Value)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("End date must be on or after start date.");
    }
}

/// <summary>Validates requests to get a volunteer by identifier.</summary>
public sealed class GetVolunteerByIdRequestValidator : AbstractValidator<GetVolunteerByIdRequest>
{
    public GetVolunteerByIdRequestValidator()
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Volunteer identifier is required.");
    }
}

/// <summary>Validates requests to delete a volunteer.</summary>
public sealed class DeleteVolunteerRequestValidator : AbstractValidator<DeleteVolunteerRequest>
{
    public DeleteVolunteerRequestValidator()
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Volunteer identifier is required.");
    }
}

