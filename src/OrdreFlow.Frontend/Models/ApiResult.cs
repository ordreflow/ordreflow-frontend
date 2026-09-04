namespace OrdreFlow.Frontend.Models;

public record ApiResult<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public IReadOnlyList<ApiError> Errors { get; }

    private ApiResult(bool isSuccess, T? value, IReadOnlyList<ApiError> errors)
    {
        IsSuccess = isSuccess;
        Value = value;
        Errors = errors;
    }

    public static ApiResult<T> Success(T value) =>
        new(true, value, Array.Empty<ApiError>());

    public static ApiResult<T> Failure(IReadOnlyList<ApiError> errors) =>
        new(false, default, errors);
}
