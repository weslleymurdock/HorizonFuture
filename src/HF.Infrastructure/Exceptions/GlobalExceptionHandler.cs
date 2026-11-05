using System;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HF.Infrastructure.Exceptions;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(
            exception,
            "Unhandled exception: {Message} | Route: {Path}",
            exception.Message,
            httpContext.Request.Path);

        var (statusCode, title, detail) = MapException(exception);

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        if (exception is ValidationException ve)
        {
            problemDetails.Extensions["errors"] = ve.Errors;
        }

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });
    }

    private static (int StatusCode, string Title, string Detail) MapException(Exception exception) =>
        exception switch
        {
            KeyNotFoundException ex => (
                StatusCodes.Status404NotFound,
                "Resource Not Found",
                ex.Message),

            ValidationException ex => (
                StatusCodes.Status422UnprocessableEntity,
                "Validation Error",
                ex.Message),

            ArgumentNullException ex => (
                StatusCodes.Status400BadRequest,
                "Bad Request",
                ex.Message),

            UnauthorizedAccessException ex => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized Access",
                ex.Message),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error happened when processing your request.")
        };
}