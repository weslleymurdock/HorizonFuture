using FluentValidation;
using HF.Application.AssetsLiabilities.Requests;

namespace HF.Application.AssetsLiabilities.Validators;

/// <summary>Validates requests to create a new liability.</summary>
public sealed class CreateLiabilityRequestValidator : AbstractValidator<CreateLiabilityRequest>
{
    public CreateLiabilityRequestValidator()
    {
        _ = RuleFor(x => x.Title)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Liability title cannot be empty.")
            .MaximumLength(150)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Liability title cannot exceed 150 characters.");

        _ = RuleFor(x => x.Creditor)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Creditor cannot be empty.")
            .MaximumLength(150)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Creditor cannot exceed 150 characters.");

        _ = RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Amount must be greater than zero.");

        _ = RuleFor(x => x.PaidAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.PaidAmount.HasValue)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Paid amount cannot be negative.");
    }
}

/// <summary>Validates requests to update an existing liability.</summary>
public sealed class UpdateLiabilityRequestValidator : AbstractValidator<UpdateLiabilityRequest>
{
    public UpdateLiabilityRequestValidator()
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Liability identifier is required.");

        _ = RuleFor(x => x.Title)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Liability title cannot be empty.")
            .MaximumLength(150)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Liability title cannot exceed 150 characters.");

        _ = RuleFor(x => x.Creditor)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Creditor cannot be empty.")
            .MaximumLength(150)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Creditor cannot exceed 150 characters.");

        _ = RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Amount must be greater than zero.");

        _ = RuleFor(x => x.PaidAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.PaidAmount.HasValue)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Paid amount cannot be negative.");
    }
}

/// <summary>Validates requests to get a liability by identifier.</summary>
public sealed class GetLiabilityByIdRequestValidator : AbstractValidator<GetLiabilityByIdRequest>
{
    public GetLiabilityByIdRequestValidator()
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Liability identifier is required.");
    }
}

/// <summary>Validates requests to delete a liability.</summary>
public sealed class DeleteLiabilityRequestValidator : AbstractValidator<DeleteLiabilityRequest>
{
    public DeleteLiabilityRequestValidator()
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Liability identifier is required.");
    }
}

