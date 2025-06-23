namespace BalancerKube.Common.Models;

public record Result<T>
{
    private readonly T? _value;
    private readonly Exception? _exception;

    public Result(T value)
    {
        IsSuccess = true;
        _value = value;
    }

    public Result(Exception exception)
    {
        IsFaulted = true;
        _exception = exception;
    }

    public bool IsFaulted { get; private set; }

    public bool IsSuccess { get; private set; }

    public T? Value
    {
        get
        {
            if (IsSuccess)
            {
                return _value;
            }

            return default;
        }
    }

    public Exception? Exception
    {
        get
        {
            if (IsFaulted)
            {
                return _exception;
            }

            return null;
        }
    }

    public static Result<T> Failure(string exceptionMessage) => new Result<T>(new Exception(exceptionMessage));
    
    public static Result<T> Success(T value) => new Result<T>(value);
}
