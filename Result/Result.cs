namespace dotnet_result;

/// <summary>
/// This is an implementation of the Result type. It is a sum type that represents a value that may be either a success 
/// or an error.
/// </summary>
/// <value></value>
public record Result<T, E>
{
    /// <summary>
    /// This represents a successful result.
    /// </summary>
    /// <param name="Value">the result</param>
    /// <returns></returns>
    public record Ok(T Value) : Result<T, E>;

    /// <summary>
    /// This represents an error result.
    /// </summary>
    /// <param name="Error">the error</param>
    /// <returns></returns>
    public record Err(E Error) : Result<T, E>;

    /// <summary>
    /// Private constructor so no more subclasses can be created.
    /// </summary>
    private Result() { }

    /// <summary>
    /// Applies a function to the value of a successful result, otherwise propagates the error.
    /// </summary>
    /// <param name="map">The function to apply</param>
    /// <typeparam name="T2"></typeparam>
    /// <returns></returns>
    public Result<T2, E> Map<T2>(Func<T, T2> map) =>
        this switch
        {
            Ok(var value) => new Result<T2, E>.Ok(map(value)),
            Err(var error) => new Result<T2, E>.Err(error),
            _ => throw new Exception("Unexpected result type")
        };

    /// <summary>
    /// Applies a function to the error of an error result, otherwise propagates the successful result.
    /// </summary>
    /// <param name="errorMap">the function to apply</param>
    /// <typeparam name="E2"></typeparam>
    /// <returns></returns>
    public Result<T, E2> MapErr<E2>(Func<E, E2> errorMap) =>
        this switch
        {
            Ok(var value) => new Result<T, E2>.Ok(value),
            Err(var error) => new Result<T, E2>.Err(errorMap(error)),
            _ => throw new Exception("Unexpected result type")
        };

    /// <summary>
    /// Applies functions to either the value or the error of a result, depending on the result type.
    /// </summary>
    /// <param name="map">The function to apply to successful results</param>
    /// <param name="errorMap">The function to apply to error results</param>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="E2"></typeparam>
    /// <returns></returns>
    public Result<T2, E2> MapBoth<T2, E2>(Func<T, T2> map, Func<E, E2> errorMap) =>
        this switch
        {
            Ok(var value) => new Result<T2, E2>.Ok(map(value)),
            Err(var error) => new Result<T2, E2>.Err(errorMap(error)),
            _ => throw new Exception("Unexpected result type")
        };

    /// <summary>
    /// Applies action to the result
    /// </summary>
    /// <param name="okHandler">The action to run on a successful result</param>
    /// <param name="errorHandler">The action to run on an error</param>
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

    /// <summary>
    /// Like <see cref="Handle"> but with async handlers
    /// </summary>
    /// <param name="okHandler">the handler for the successful result</param>
    /// <param name="errorHandler">the handler for errors</param>
    /// <returns></returns>
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

    /// <summary>
    /// Unwraps the value of a successful result or throws an exception.
    /// </summary>
    /// <returns></returns>
    /// <exception>when the result is an error</exception>
    public T Unwrap() =>
        this switch
        {
            Ok(var value) => value,
            Err(var error) => throw new Exception($"Error: {error}"),
            _ => throw new Exception("Unexpected result type")
        };

    /// <summary>
    /// Unwraps the value of an error result or throws an exception.
    /// </summary>
    /// <returns></returns>
    /// <exception>when the result is a successful result</exception>
    public E UnwrapErr() =>
        this switch
        {
            Ok(var value) => throw new Exception($"Expected error, got {value}"),
            Err(var error) => error,
            _ => throw new Exception("Unexpected result type")
        };

    /// <summary>
    /// Return success if both results are successful, otherwise return the first error.
    /// </summary>
    /// <param name="other"></param>
    /// <typeparam name="U"></typeparam>
    /// <returns></returns>
    public Result<U, E> And<U>(Result<U, E> other) =>
        this switch
        {
            Ok(_) => other,
            Err(var error) => new Result<U, E>.Err(error),
            _ => throw new Exception("Unexpected result type")
        };

    /// <summary>
    /// Applies function if success, otherwise propagate error.
    /// </summary>
    /// <param name="f"></param>
    /// <typeparam name="U"></typeparam>
    /// <returns></returns>
    public Result<U, E> AndThen<U>(Func<T, Result<U, E>> f) =>
        this switch
        {
            Ok(var value) => f(value),
            Err(var error) => new Result<U, E>.Err(error),
            _ => throw new Exception("Unexpected result type")
        };

    /// <summary>
    /// Returns result if result is Error, otherwise the original successful result.
    /// </summary>
    /// <param name="other"></param>
    /// <typeparam name="F"></typeparam>
    /// <returns></returns>
    public Result<T, F> Or<F>(Result<T, F> other) =>
        this switch
        {
            Ok(var value) => new Result<T, F>.Ok(value),
            Err(_) => other,
            _ => throw new Exception("Unexpected result type")
        };

    /// <summary>
    /// Applies function if error, otherwise propagate success.
    /// </summary>
    /// <param name="f"></param>
    /// <typeparam name="F"></typeparam>
    /// <returns></returns>
    public Result<T, F> OrElse<F>(Func<E, Result<T, F>> f) =>
        this switch
        {
            Ok(var value) => new Result<T, F>.Ok(value),
            Err(var err) => f(err),
            _ => throw new Exception("Unexpected result type")
        };

    /// <summary>
    /// Returns true iff the result contains value
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool Contains(T value) =>
        this switch
        {
            Ok(var v) => Equals(v, value),
            _ => false
        };

    /// <summary>
    /// Returns true iff the result contains error
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public bool ContainsError(E error) =>
        this switch
        {
            Err(var e) => Equals(e, error),
            _ => false
        };

    /// <summary>
    /// Returns true iff the result is successful
    /// </summary>
    /// <value></value>
    public bool IsOk =>
        this switch
        {
            Ok(_) => true,
            _ => false
        };

    /// <summary>
    /// Returns true if the result is error
    /// </summary>
    /// <value></value>
    public bool IsErr =>
        this switch
        {
            Err(_) => true,
            _ => false
        };

    /// <summary>
    /// Return default if result is error, otherwise apply function to value
    /// </summary>
    /// <param name="def"></param>
    /// <param name="map"></param>
    /// <typeparam name="U"></typeparam>
    /// <returns></returns>
    public U MapOr<U>(U def, Func<T, U> map) =>
        this switch
        {
            Ok(var value) => map(value),
            _ => def
        };

    /// <summary>
    /// Return generated efault if result is error, otherwise apply function to the successful value
    /// </summary>
    /// <param name="def"></param>
    /// <param name="map"></param>
    /// <typeparam name="U"></typeparam>
    /// <returns></returns>
    public U MapOrElse<U>(Func<T, U> def, Func<E, U> map) =>
        this switch
        {
            Err(var error) => map(error),
            Ok(var value) => def(value),
            _ => throw new Exception("Unexpected result type")
        };
}
