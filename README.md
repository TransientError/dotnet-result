[![dotnet-result](https://img.shields.io/nuget/v/dotnet-result.svg?label=dotnet-result)](https://www.nuget.org/packages/dotnet-result/)

## What is this?
This is a simple C# implementation of the Result type. It's roughly modeled after the [Rust Result type](https://doc.rust-lang.org/std/result/enum.Result.html).

## Why results over exceptions?
### Using the Result type prevents you from missing handling error cases
Returning Results from values allows the typechecker to make sure the user is handling errors at compile time.
For example consider the following code:

```csharp
private static int Divide(int a, int b)
{
    if (b == 0)
    {
        throw new DivideByZeroException();
    }

    return a / b;
}

private static Result<int, string> ResultDivide(int a, int b)
{
    if (b == 0)
    {
        return Result.Fail("Cannot divide by zero");
    }

    return Result.Success(a / b);
}
```

Now when you call `Divide`, you can easily pass the returned int to a new function and completely forget about the error case. On the other hand, using `ResultDivide`, in order to use the resulting int, you need to handle the error case.

```csharp
int dividend = Divide(10, 0); // throws DivideByZeroException
Console.WriteLine(dividend + 1); // compiles
```

```csharp
Result<int, string> result = ResultDivide(10, 0);
result + 1; // does not compile
result.Map(x => x + 1); // does compile
result.Handle(x => Console.WriteLine(x)), e => Console.WriteLine(e); // does compile but we must handle the error case
```
### Using the result type allows the library users to know what kind of errors can happen
Consider
```csharp
public static int Divide(int a, int b)
{
    if (b == 0)
    {
        throw new DivideByZeroException();
    }

    return a / b;
}

public enum Error
{
    DivideByZero
}

public static Result<int, Error> ResultDivide(int a, int b)
{
    if (b == 0)
    {
        return Result.Fail(Error.DivideByZero);
    }

    return Result.Success(a / b);
}
```
Imagine if this function was vended via a dll. You would have no way to confirm what kind of Exception can be thrown. However, if you use a Result Type with an enum, all users will know what error cases are possible.
