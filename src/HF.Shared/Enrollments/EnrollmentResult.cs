using System;
using System.Collections.Generic;
using System.Linq;

namespace HF.Shared.Enrollments;

/// <summary>
/// Result of executing an enrollment operation in the UI layer.
/// </summary>
public class EnrollmentResult
{
    public bool Succeeded { get; set; }
    public string Message { get; set; } = string.Empty;
    public IReadOnlyList<string> Errors { get; set; } = Array.Empty<string>();
    public int? StatusCode { get; set; }

    public static EnrollmentResult Ok(string message = "") => new()
    {
        Succeeded = true,
        Message = message
    };

    public static EnrollmentResult Fail(string message, IEnumerable<string>? errors = null, int? statusCode = null) => new()
    {
        Succeeded = false,
        Message = message,
        Errors = errors is not null
            ? errors.ToList()
            : (string.IsNullOrWhiteSpace(message) ? Array.Empty<string>() : new[] { message }),
        StatusCode = statusCode
    };
}

/// <summary>
/// Typed result of executing an enrollment operation in the UI layer.
/// </summary>
public class EnrollmentResult<T> : EnrollmentResult
{
    public T? Data { get; set; }

    public static EnrollmentResult<T> Ok(T data, string message = "") => new()
    {
        Succeeded = true,
        Data = data,
        Message = message
    };

    public static new EnrollmentResult<T> Fail(string message, IEnumerable<string>? errors = null, int? statusCode = null) => new()
    {
        Succeeded = false,
        Message = message,
        Errors = errors is not null
            ? errors.ToList()
            : (string.IsNullOrWhiteSpace(message) ? Array.Empty<string>() : new[] { message }),
        StatusCode = statusCode
    };
}

