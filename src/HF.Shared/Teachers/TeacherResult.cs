using System;
using System.Collections.Generic;
using System.Linq;

namespace HF.Shared.Teachers;

/// <summary>
/// Resultado da execução de uma operação de professores na UI.
/// </summary>
public class TeacherResult
{
    public bool Succeeded { get; set; }
    public string Message { get; set; } = string.Empty;
    public IReadOnlyList<string> Errors { get; set; } = Array.Empty<string>();
    public int? StatusCode { get; set; }

    public static TeacherResult Ok(string message = "") => new()
    {
        Succeeded = true,
        Message = message
    };

    public static TeacherResult Fail(string message, IEnumerable<string>? errors = null, int? statusCode = null) => new()
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
/// Resultado tipado da execução de uma operação de professores na UI.
/// </summary>
public class TeacherResult<T> : TeacherResult
{
    public T? Data { get; set; }

    public static TeacherResult<T> Ok(T data, string message = "") => new()
    {
        Succeeded = true,
        Data = data,
        Message = message
    };

    public static new TeacherResult<T> Fail(string message, IEnumerable<string>? errors = null, int? statusCode = null) => new()
    {
        Succeeded = false,
        Message = message,
        Errors = errors is not null
            ? errors.ToList()
            : (string.IsNullOrWhiteSpace(message) ? Array.Empty<string>() : new[] { message }),
        StatusCode = statusCode
    };
}

