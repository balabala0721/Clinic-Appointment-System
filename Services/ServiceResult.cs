namespace ClinicSystem.Services;

/// <summary>
/// Services return this instead of throwing for "expected" failures
/// (slot taken, too late to cancel...). Controllers turn it into JSON or a message.
/// </summary>
public class ServiceResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = "";

    public static ServiceResult Ok(string message = "") => new() { Success = true, Message = message };
    public static ServiceResult Fail(string message) => new() { Success = false, Message = message };
}

public class ServiceResult<T> : ServiceResult
{
    public T? Value { get; init; }

    public static ServiceResult<T> Ok(T value, string message = "") =>
        new() { Success = true, Value = value, Message = message };

    public new static ServiceResult<T> Fail(string message) => new() { Success = false, Message = message };
}
