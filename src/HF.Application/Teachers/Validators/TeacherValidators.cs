using FluentValidation;
using HF.Application.Teachers.Abstract;
using HF.Application.Teachers.Requests;

namespace HF.Application.Teachers.Validators;

/// <summary>Validates requests to create a new teacher.</summary>
public sealed class CreateTeacherRequestValidator : AbstractValidator<CreateTeacherRequest>
{
    public CreateTeacherRequestValidator(ITeacherService teacherService)
    {
        _ = RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("User identifier is required.")
            .MustAsync((userId, ct) => teacherService.UserAvailableAsync(userId, null, ct))
            .WithErrorCode("CONFLICT")
            .WithMessage("User does not exist or is already linked to another teacher profile.");

        _ = RuleFor(x => x.EmployeeNumber)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Employee number cannot be empty.")
            .MaximumLength(50)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Employee number cannot exceed 50 characters.")
            .MustAsync(async (empNum, ct) => !await teacherService.EmployeeNumberExistsAsync(empNum, null, ct))
            .WithErrorCode("CONFLICT")
            .WithMessage("The employee number is already in use.");

        _ = RuleFor(x => x.Specialty)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Specialty cannot be empty.")
            .MaximumLength(100)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Specialty cannot exceed 100 characters.");
    }
}

/// <summary>Validates requests to update an existing teacher.</summary>
public sealed class UpdateTeacherRequestValidator : AbstractValidator<UpdateTeacherRequest>
{
    public UpdateTeacherRequestValidator(ITeacherService teacherService)
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Teacher identifier is required.");

        _ = RuleFor(x => x.EmployeeNumber)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Employee number cannot be empty.")
            .MaximumLength(50)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Employee number cannot exceed 50 characters.")
            .MustAsync(async (request, empNum, ct) => !await teacherService.EmployeeNumberExistsAsync(empNum, request.Id, ct))
            .WithErrorCode("CONFLICT")
            .WithMessage("The employee number is already in use.");

        _ = RuleFor(x => x.Specialty)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Specialty cannot be empty.")
            .MaximumLength(100)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Specialty cannot exceed 100 characters.");
    }
}

/// <summary>Validates requests to get a teacher by identifier.</summary>
public sealed class GetTeacherByIdRequestValidator : AbstractValidator<GetTeacherByIdRequest>
{
    public GetTeacherByIdRequestValidator()
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Teacher identifier is required.");
    }
}

/// <summary>Validates requests to delete a teacher.</summary>
public sealed class DeleteTeacherRequestValidator : AbstractValidator<DeleteTeacherRequest>
{
    public DeleteTeacherRequestValidator()
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Teacher identifier is required.");
    }
}

