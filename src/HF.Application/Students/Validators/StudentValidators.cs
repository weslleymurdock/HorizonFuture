using FluentValidation;
using HF.Application.Students.Abstract;
using HF.Application.Students.Requests;

namespace HF.Application.Students.Validators;

/// <summary>Validates requests to create a new student.</summary>
public sealed class CreateStudentRequestValidator : AbstractValidator<CreateStudentRequest>
{
    public CreateStudentRequestValidator(IStudentService studentService)
    {
        _ = RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("User identifier is required.")
            .MustAsync((userId, ct) => studentService.UserAvailableAsync(userId, null, ct))
            .WithErrorCode("CONFLICT")
            .WithMessage("User does not exist or is already linked to another student profile.");

        _ = RuleFor(x => x.RegistrationNumber)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Registration number cannot be empty.")
            .MaximumLength(50)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Registration number cannot exceed 50 characters.")
            .MustAsync((regNum, ct) => studentService.RegistrationNumberExistsAsync(regNum, null, ct).ContinueWith(t => !t.Result, ct))
            .WithErrorCode("CONFLICT")
            .WithMessage("The registration number is already in use.");

        _ = RuleFor(x => x.DateOfBirth)
            .Must(dob => dob < DateOnly.FromDateTime(DateTime.UtcNow))
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Date of birth must be in the past.");
    }
}

/// <summary>Validates requests to update an existing student.</summary>
public sealed class UpdateStudentRequestValidator : AbstractValidator<UpdateStudentRequest>
{
    public UpdateStudentRequestValidator(IStudentService studentService)
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Student identifier is required.");

        _ = RuleFor(x => x.RegistrationNumber)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Registration number cannot be empty.")
            .MaximumLength(50)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Registration number cannot exceed 50 characters.")
            .MustAsync(async (request, regNum, ct) => !await studentService.RegistrationNumberExistsAsync(regNum, request.Id, ct))
            .WithErrorCode("CONFLICT")
            .WithMessage("The registration number is already in use.");

        _ = RuleFor(x => x.DateOfBirth)
            .Must(dob => dob < DateOnly.FromDateTime(DateTime.UtcNow))
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("Date of birth must be in the past.");
    }
}

/// <summary>Validates requests to get a student by identifier.</summary>
public sealed class GetStudentByIdRequestValidator : AbstractValidator<GetStudentByIdRequest>
{
    public GetStudentByIdRequestValidator()
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Student identifier is required.");
    }
}

/// <summary>Validates requests to delete a student.</summary>
public sealed class DeleteStudentRequestValidator : AbstractValidator<DeleteStudentRequest>
{
    public DeleteStudentRequestValidator()
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Student identifier is required.");
    }
}

