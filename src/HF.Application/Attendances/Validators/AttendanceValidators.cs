using FluentValidation;
using HF.Application.Attendances.Abstract;
using HF.Application.Attendances.Requests;

namespace HF.Application.Attendances.Validators;

/// <summary>Validates requests to create a new attendance record.</summary>
public sealed class CreateAttendanceRequestValidator : AbstractValidator<CreateAttendanceRequest>
{
    public CreateAttendanceRequestValidator(IAttendanceService attendanceService)
    {
        _ = RuleFor(x => x.ClassSessionId)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Class session identifier is required.");

        _ = RuleFor(x => x.EnrollmentId)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Enrollment identifier is required.");

        _ = RuleFor(x => x)
            .MustAsync(async (request, ct) => !await attendanceService.AttendanceExistsAsync(
                request.ClassSessionId,
                request.EnrollmentId,
                null,
                ct))
            .WithErrorCode("CONFLICT")
            .WithMessage("Attendance is already recorded for this student in the specified class session.")
            .MustAsync(async (request, ct) =>
            {
                var (isCompatible, _) = await attendanceService.ValidateAcademicCompatibilityAsync(
                    request.ClassSessionId,
                    request.EnrollmentId,
                    ct);
                return isCompatible;
            })
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("The student enrollment does not match the subject or academic period of the class session.");
    }
}

/// <summary>Validates requests to update an existing attendance record.</summary>
public sealed class UpdateAttendanceRequestValidator : AbstractValidator<UpdateAttendanceRequest>
{
    public UpdateAttendanceRequestValidator(IAttendanceService attendanceService)
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Attendance identifier is required.");

        _ = RuleFor(x => x.ClassSessionId)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Class session identifier is required.");

        _ = RuleFor(x => x.EnrollmentId)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Enrollment identifier is required.");

        _ = RuleFor(x => x)
            .MustAsync(async (request, ct) => !await attendanceService.AttendanceExistsAsync(
                request.ClassSessionId,
                request.EnrollmentId,
                request.Id,
                ct))
            .WithErrorCode("CONFLICT")
            .WithMessage("Attendance is already recorded for this student in the specified class session.")
            .MustAsync(async (request, ct) =>
            {
                var (isCompatible, _) = await attendanceService.ValidateAcademicCompatibilityAsync(
                    request.ClassSessionId,
                    request.EnrollmentId,
                    ct);
                return isCompatible;
            })
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("The student enrollment does not match the subject or academic period of the class session.");
    }
}

/// <summary>Validates requests to get an attendance record by identifier.</summary>
public sealed class GetAttendanceByIdRequestValidator : AbstractValidator<GetAttendanceByIdRequest>
{
    public GetAttendanceByIdRequestValidator()
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Attendance identifier is required.");
    }
}

/// <summary>Validates requests to delete an attendance record.</summary>
public sealed class DeleteAttendanceRequestValidator : AbstractValidator<DeleteAttendanceRequest>
{
    public DeleteAttendanceRequestValidator()
    {
        _ = RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("Attendance identifier is required.");
    }
}

