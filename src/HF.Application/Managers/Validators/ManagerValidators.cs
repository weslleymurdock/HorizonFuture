using FluentValidation;
using HF.Application.Managers.Abstract;
using HF.Application.Managers.Requests;

namespace HF.Application.Managers.Validators;

/// <summary>Validates requests to create a new manager.</summary>
public sealed class CreateManagerRequestValidator : AbstractValidator<CreateManagerRequest>
{
    public CreateManagerRequestValidator(IManagerService managerService)
    {
        _ = RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("User identifier is required.")
            .MustAsync((userId, ct) => managerService.UserAvailableAsync(userId, null, ct))
            .WithErrorCode("CONFLICT")
            .WithMessage("User does not exist or is already linked to another manager profile.");

        _ = RuleFor(x => x.EmployeeNumber)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Employee number cannot be empty.")
            .MaximumLength(50)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Employee number cannot exceed 50 characters.")
            .MustAsync(async (empNum, ct) => !await managerService.EmployeeNumberExistsAsync(empNum, null, ct))
            .WithErrorCode("CONFLICT")
            .WithMessage("The employee number is already in use.");

        _ = RuleFor(x => x.Department)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Department cannot be empty.")
            .MaximumLength(100)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Department cannot exceed 100 characters.");

        _ = RuleFor(x => x.RoleTitle)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Role title cannot be empty.")
            .MaximumLength(100)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Role title cannot exceed 100 characters.");
    }
}

/// <summary>Validates requests to update an existing manager.</summary>
public sealed class UpdateManagerRequestValidator : AbstractValidator<UpdateManagerRequest>
{
    public UpdateManagerRequestValidator(IManagerService managerService)
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Manager identifier is required.");

        _ = RuleFor(x => x.EmployeeNumber)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Employee number cannot be empty.")
            .MaximumLength(50)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Employee number cannot exceed 50 characters.")
            .MustAsync(async (request, empNum, ct) => !await managerService.EmployeeNumberExistsAsync(empNum, request.Id, ct))
            .WithErrorCode("CONFLICT")
            .WithMessage("The employee number is already in use.");

        _ = RuleFor(x => x.Department)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Department cannot be empty.")
            .MaximumLength(100)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Department cannot exceed 100 characters.");

        _ = RuleFor(x => x.RoleTitle)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Role title cannot be empty.")
            .MaximumLength(100)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Role title cannot exceed 100 characters.");
    }
}

/// <summary>Validates requests to get a manager by identifier.</summary>
public sealed class GetManagerByIdRequestValidator : AbstractValidator<GetManagerByIdRequest>
{
    public GetManagerByIdRequestValidator()
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Manager identifier is required.");
    }
}

/// <summary>Validates requests to delete a manager.</summary>
public sealed class DeleteManagerRequestValidator : AbstractValidator<DeleteManagerRequest>
{
    public DeleteManagerRequestValidator()
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Manager identifier is required.");
    }
}

