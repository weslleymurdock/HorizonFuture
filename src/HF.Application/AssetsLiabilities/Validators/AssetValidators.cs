using FluentValidation;
using HF.Application.AssetsLiabilities.Abstract;
using HF.Application.AssetsLiabilities.Requests;

namespace HF.Application.AssetsLiabilities.Validators;

/// <summary>Validates requests to create a new asset.</summary>
public sealed class CreateAssetRequestValidator : AbstractValidator<CreateAssetRequest>
{
    public CreateAssetRequestValidator(IAssetService assetService)
    {
        _ = RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Asset name cannot be empty.")
            .MaximumLength(150)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Asset name cannot exceed 150 characters.");

        _ = RuleFor(x => x.AssetTag)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Asset tag cannot be empty.")
            .MaximumLength(50)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Asset tag cannot exceed 50 characters.")
            .MustAsync(async (tag, ct) => !await assetService.AssetTagExistsAsync(tag, null, ct))
            .WithErrorCode("CONFLICT")
            .WithMessage("Asset tag is already in use.");

        _ = RuleFor(x => x.AcquisitionValue)
            .GreaterThanOrEqualTo(0)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Acquisition value cannot be negative.");
    }
}

/// <summary>Validates requests to update an existing asset.</summary>
public sealed class UpdateAssetRequestValidator : AbstractValidator<UpdateAssetRequest>
{
    public UpdateAssetRequestValidator(IAssetService assetService)
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Asset identifier is required.");

        _ = RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Asset name cannot be empty.")
            .MaximumLength(150)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Asset name cannot exceed 150 characters.");

        _ = RuleFor(x => x.AssetTag)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Asset tag cannot be empty.")
            .MaximumLength(50)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Asset tag cannot exceed 50 characters.")
            .MustAsync(async (request, tag, ct) => !await assetService.AssetTagExistsAsync(tag, request.Id, ct))
            .WithErrorCode("CONFLICT")
            .WithMessage("Asset tag is already in use.");

        _ = RuleFor(x => x.AcquisitionValue)
            .GreaterThanOrEqualTo(0)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Acquisition value cannot be negative.");
    }
}

/// <summary>Validates requests to get an asset by identifier.</summary>
public sealed class GetAssetByIdRequestValidator : AbstractValidator<GetAssetByIdRequest>
{
    public GetAssetByIdRequestValidator()
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Asset identifier is required.");
    }
}

/// <summary>Validates requests to delete an asset.</summary>
public sealed class DeleteAssetRequestValidator : AbstractValidator<DeleteAssetRequest>
{
    public DeleteAssetRequestValidator()
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Asset identifier is required.");
    }
}

