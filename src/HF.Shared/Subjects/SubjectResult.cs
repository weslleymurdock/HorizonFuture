using System;
using System.Collections.Generic;
using System.Linq;

namespace HF.Shared.Subjects;

/// <summary>
/// Result of executing a subject operation in the UI layer.
/// </summary>
public class SubjectResult
{
    public bool Succeeded { get; set; }
    public string Message { get; set; } = string.Empty;
    public IReadOnlyList<string> Errors { get; set; } = Array.Empty<string>();
    public int? StatusCode { get; set; }

    public static SubjectResult Ok(string message = "") => new()
    {
        Succeeded = true,
        Message = message
    };

    public static SubjectResult Fail(string message, IEnumerable<string>? errors = null, int? statusCode = null) => new()
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
/// Typed result of executing a subject operation in the UI layer.
/// </summary>
public class SubjectResult<T> : SubjectResult
{
    public T? Data { get; set; }

    public static SubjectResult<T> Ok(T data, string message = "") => new()
    {
        Succeeded = true,
        Data = data,
        Message = message
    };

    public static new SubjectResult<T> Fail(string message, IEnumerable<string>? errors = null, int? statusCode = null) => new()
    {
        Succeeded = false,
        Message = message,
        Errors = errors is not null
            ? errors.ToList()
            : (string.IsNullOrWhiteSpace(message) ? Array.Empty<string>() : new[] { message }),
        StatusCode = statusCode
    };
}

