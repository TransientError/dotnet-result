namespace dotnet_result;

public record Result<T, E>
{
    public record Ok(T Value) : Result<T, E>;

    public record Err(E Error) : Result<T, E>;

    private Result() { }

    public Result<T2, E> Map<T2>(Func<T, T2> map) =>
        this switch
        {
            Ok(var value) => new Result<T2, E>.Ok(map(value)),
            Err(var error) => new Result<T2, E>.Err(error),
            _ => throw new Exception("Unexpected result type")
        };

    public Result<T, E2> MapErr<E2>(Func<E, E2> errorMap) =>
        this switch
        {
            Ok(var value) => new Result<T, E2>.Ok(value),
            Err(var error) => new Result<T, E2>.Err(errorMap(error)),
            _ => throw new Exception("Unexpected result type")
        };

    public Result<T2, E2> MapBoth<T2, E2>(Func<T, T2> map, Func<E, E2> errorMap) =>
        this switch
        {
            Ok(var value) => new Result<T2, E2>.Ok(map(value)),
            Err(var error) => new Result<T2, E2>.Err(errorMap(error)),
            _ => throw new Exception("Unexpected result type")
        };

    public void Handle(Action<T> okHandler, Action<E> errorHandler)
    {
        switch (this)
        {
            case Ok(var value) ok:
                okHandler(value);
                break;
            case Err(var error) err:
                errorHandler(error);
                break;
            default:
                throw new Exception("Unexpected result type");
        }
    }

    public async Task HandleAsync(Func<T, Task> okHandler, Func<E, Task> errorHandler)
    {
        switch (this)
        {
            case Ok(var value) ok:
                await okHandler(value);
                break;
            case Err(var error) err:
                await errorHandler(error);
                break;
            default:
                throw new Exception("Unexpected result type");
        }
    }

    public T Unwrap() =>
        this switch
        {
            Ok(var value) => value,
            Err(var error) => throw new Exception($"Error: {error}"),
            _ => throw new Exception("Unexpected result type")
        };

    public E UnwrapErr() =>
        this switch
        {
            Ok(var value) => throw new Exception($"Expected error, got {value}"),
            Err(var error) => error,
            _ => throw new Exception("Unexpected result type")
        };

    public Result<U, E> And<U>(Result<U, E> other) =>
        this switch
        {
            Ok(_) => other,
            Err(var error) => new Result<U, E>.Err(error),
            _ => throw new Exception("Unexpected result type")
        };

    public Result<U, E> AndThen<U>(Func<T, Result<U, E>> f) =>
        this switch
        {
            Ok(var value) => f(value),
            Err(var error) => new Result<U, E>.Err(error),
            _ => throw new Exception("Unexpected result type")
        };

    public Result<T, F> Or<F>(Result<T, F> other) =>
        this switch
        {
            Ok(var value) => new Result<T, F>.Ok(value),
            Err(_) => other,
            _ => throw new Exception("Unexpected result type")
        };

    public Result<T, F> OrElse<F>(Func<E, Result<T, F>> f) =>
        this switch
        {
            Ok(var value) => new Result<T, F>.Ok(value),
            Err(var err) => f(err),
            _ => throw new Exception("Unexpected result type")
        };

    public bool Contains(T value) =>
        this switch
        {
            Ok(var v) => Equals(v, value),
            _ => false
        };

    public bool ContainsError(E error) =>
        this switch
        {
            Err(var e) => Equals(e, error),
            _ => false
        };

    public bool IsOk =>
        this switch
        {
            Ok(_) => true,
            _ => false
        };

    public bool IsErr =>
        this switch
        {
            Err(_) => true,
            _ => false
        };

    public U MapOr<U>(U def, Func<T, U> map) =>
        this switch
        {
            Ok(var value) => map(value),
            _ => def
        };

    public U MapOrElse<U>(Func<T, U> def, Func<E, U> map) =>
        this switch
        {
            Err(var error) => map(error),
            Ok(var value) => def(value),
            _ => throw new Exception("Unexpected result type")
        };
}
