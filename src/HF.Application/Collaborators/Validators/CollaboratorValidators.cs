using FluentValidation;
using HF.Application.Collaborators.Abstract;
using HF.Application.Collaborators.Requests;

namespace HF.Application.Collaborators.Validators;

/// <summary>Validates requests to create a new collaborator.</summary>
public sealed class CreateCollaboratorRequestValidator : AbstractValidator<CreateCollaboratorRequest>
{
    public CreateCollaboratorRequestValidator(ICollaboratorService collaboratorService)
    {
        _ = RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("User identifier is required.")
            .MustAsync((userId, ct) => collaboratorService.UserAvailableAsync(userId, null, ct))
            .WithErrorCode("CONFLICT")
            .WithMessage("User does not exist or is already linked to another collaborator profile.");

        _ = RuleFor(x => x.EmployeeNumber)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Employee number cannot be empty.")
            .MaximumLength(50)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Employee number cannot exceed 50 characters.")
            .MustAsync(async (empNum, ct) => !await collaboratorService.EmployeeNumberExistsAsync(empNum, null, ct))
            .WithErrorCode("CONFLICT")
            .WithMessage("The employee number is already in use.");

        _ = RuleFor(x => x.JobTitle)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Job title cannot be empty.")
            .MaximumLength(100)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Job title cannot exceed 100 characters.");

        _ = RuleFor(x => x.Department)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Department cannot be empty.")
            .MaximumLength(100)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Department cannot exceed 100 characters.");
    }
}

/// <summary>Validates requests to update an existing collaborator.</summary>
public sealed class UpdateCollaboratorRequestValidator : AbstractValidator<UpdateCollaboratorRequest>
{
    public UpdateCollaboratorRequestValidator(ICollaboratorService collaboratorService)
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Collaborator identifier is required.");

        _ = RuleFor(x => x.EmployeeNumber)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Employee number cannot be empty.")
            .MaximumLength(50)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Employee number cannot exceed 50 characters.")
            .MustAsync(async (request, empNum, ct) => !await collaboratorService.EmployeeNumberExistsAsync(empNum, request.Id, ct))
            .WithErrorCode("CONFLICT")
            .WithMessage("The employee number is already in use.");

        _ = RuleFor(x => x.JobTitle)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Job title cannot be empty.")
            .MaximumLength(100)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Job title cannot exceed 100 characters.");

        _ = RuleFor(x => x.Department)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Department cannot be empty.")
            .MaximumLength(100)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Department cannot exceed 100 characters.");
    }
}

/// <summary>Validates requests to get a collaborator by identifier.</summary>
public sealed class GetCollaboratorByIdRequestValidator : AbstractValidator<GetCollaboratorByIdRequest>
{
    public GetCollaboratorByIdRequestValidator()
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Collaborator identifier is required.");
    }
}

/// <summary>Validates requests to delete a collaborator.</summary>
public sealed class DeleteCollaboratorRequestValidator : AbstractValidator<DeleteCollaboratorRequest>
{
    public DeleteCollaboratorRequestValidator()
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Collaborator identifier is required.");
    }
}

