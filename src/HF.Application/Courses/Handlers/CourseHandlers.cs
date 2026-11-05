using HF.Application.Common.Responses;
using HF.Application.Courses.Abstract;
using HF.Application.Courses.Requests;
using HF.Application.Courses.Responses;
using Mediator;

namespace HF.Application.Courses.Handlers;

/// <summary>
/// Mediator handlers for course operations.
/// </summary>
public sealed class CourseHandlers(ICourseService courseService)
    : IRequestHandler<CreateCourseRequest, ResponseBase<CourseResponse>>,
      IRequestHandler<GetCourseByIdRequest, ResponseBase<CourseResponse>>,
      IRequestHandler<ListCoursesRequest, ResponseBase<IReadOnlyList<CourseResponse>>>,
      IRequestHandler<UpdateCourseRequest, ResponseBase<CourseResponse>>,
      IRequestHandler<DeleteCourseRequest, ResponseBase<bool>>
{
    /// <inheritdoc />
    public async ValueTask<ResponseBase<CourseResponse>> Handle(
        CreateCourseRequest request,
        CancellationToken cancellationToken)
    {
        var result = await courseService.CreateAsync(request, cancellationToken);
        return ResponseBase<CourseResponse>.SuccessResponse(result, "Course created successfully.");
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<CourseResponse>> Handle(
        GetCourseByIdRequest request,
        CancellationToken cancellationToken)
    {
        var result = await courseService.GetByIdAsync(request.Id, cancellationToken);
        return result is null
            ? ResponseBase<CourseResponse>.FailureResponse("Course not found.")
            : ResponseBase<CourseResponse>.SuccessResponse(result);
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<IReadOnlyList<CourseResponse>>> Handle(
        ListCoursesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await courseService.ListAsync(request, cancellationToken);
        return ResponseBase<IReadOnlyList<CourseResponse>>.SuccessResponse(result);
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<CourseResponse>> Handle(
        UpdateCourseRequest request,
        CancellationToken cancellationToken)
    {
        var result = await courseService.UpdateAsync(request, cancellationToken);
        return result is null
            ? ResponseBase<CourseResponse>.FailureResponse("Course not found.")
            : ResponseBase<CourseResponse>.SuccessResponse(result, "Course updated successfully.");
    }

    /// <inheritdoc />
    public async ValueTask<ResponseBase<bool>> Handle(
        DeleteCourseRequest request,
        CancellationToken cancellationToken)
    {
        var (succeeded, error) = await courseService.DeleteAsync(request.Id, cancellationToken);
        return succeeded
            ? ResponseBase<bool>.SuccessResponse(true, "Course deleted successfully.")
            : ResponseBase<bool>.FailureResponse(error ?? "Could not delete course.");
    }
}

