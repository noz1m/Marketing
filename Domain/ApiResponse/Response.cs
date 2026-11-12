using System.Net;

namespace Domain.ApiResponse;

public class Response<T>(bool isSuccess, T? data, string message, HttpStatusCode statusCode)
{
    public bool IsSuccess { get; } = isSuccess;
    public T? Data { get; } = data;
    public string Message { get; } = message;
    public int StatusCode { get; } = (int)statusCode;

    public static Response<T> Success(T? data, string message = "Request completed successfully.")
        => new(true, data, message, HttpStatusCode.OK);

    public static Response<T> Created(T data, string message = "Resource created successfully.")
        => new(true, data, message, HttpStatusCode.Created);

    public static Response<T> Fail(string message, HttpStatusCode statusCode)
        => new(false, default, message, statusCode);
}
