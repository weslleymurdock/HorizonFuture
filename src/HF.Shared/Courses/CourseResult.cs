namespace HF.Shared.Courses;

/// <summary>
/// Resultado da execução de uma operação de cursos na UI.
/// </summary>
public class CourseResult
{
    public bool Succeeded { get; set; }
    public string Message { get; set; } = string.Empty;
    public IReadOnlyList<string> Errors { get; set; } = Array.Empty<string>();
    public int? StatusCode { get; set; }

    public static CourseResult Ok(string message = "") => new()
    {
        Succeeded = true,
        Message = message
    };

    public static CourseResult Fail(string message, IEnumerable<string>? errors = null, int? statusCode = null) => new()
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
/// Resultado tipado da execução de uma operação de cursos na UI.
/// </summary>
public class CourseResult<T> : CourseResult
{
    public T? Data { get; set; }

    public static CourseResult<T> Ok(T data, string message = "") => new()
    {
        Succeeded = true,
        Data = data,
        Message = message
    };

    public static new CourseResult<T> Fail(string message, IEnumerable<string>? errors = null, int? statusCode = null) => new()
    {
        Succeeded = false,
        Message = message,
        Errors = errors is not null
            ? errors.ToList()
            : (string.IsNullOrWhiteSpace(message) ? Array.Empty<string>() : new[] { message }),
        StatusCode = statusCode
    };
}

