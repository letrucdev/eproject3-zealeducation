namespace ZealEducation.API.Common.Models;

public class ApiResponse<T>
{
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }

    public static ApiResponse<T> Success(T? data, string message = "Success") => new()
    {
        Message = message,
        Data = data
    };

    public static ApiResponse<T> Error(string message, T? data = default) => new()
    {
        Message = message,
        Data = data
    };
}
