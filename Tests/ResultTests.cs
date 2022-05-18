using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dotnet_result.Tests;

[TestClass]
public class ResultTests
{
    [TestMethod]
    public void BasicUsageTest()
    {
        var okResult = Divide(1, 1);
        var errResult = Divide(1, 0);

        Assert.AreEqual(okResult.Unwrap(), 1);
        Assert.AreEqual(errResult.UnwrapErr(), "divisor cannot be 0");
    }

    [TestMethod]
    public void ShouldMapFunctions()
    {
        var okResult = Divide(1, 1);
        var intResult = okResult.Map(x => x + 2).Unwrap();
        Assert.AreEqual(intResult, 3);
    }

    [TestMethod]
    public void ShouldMapErrors()
    {
        var errResult = Divide(1, 0);
        var stringResult = errResult.MapErr(x => x.ToUpper()).UnwrapErr();
        Assert.AreEqual(stringResult, "DIVISOR CANNOT BE 0");
    }

    [TestMethod]
    public void ShouldMapBoth()
    {
        var okResult = Divide(1, 1);
        var intResult = okResult.MapBoth(x => x + 2, x => x.ToUpper()).Unwrap();
        var errResult = Divide(1, 0);
        var stringResult = errResult.MapBoth(x => x + 2, x => x.ToUpper()).UnwrapErr();
        Assert.AreEqual(intResult, 3);
        Assert.AreEqual(stringResult, "DIVISOR CANNOT BE 0");
    }

    [TestMethod]
    public void ShouldHandle()
    {
        var okResult = Divide(1, 1);
        var errResult = Divide(1, 0);

        var okHandler = new Action<int>(x => Assert.AreEqual(x, 1));
        var errHandler = new Action<string>(x => Assert.AreEqual(x, "divisor cannot be 0"));

        okResult.Handle(okHandler, errHandler);
        errResult.Handle(okHandler, errHandler);
    }

    [TestMethod]
    public async Task ShouldHandleAsync()
    {
        var okResult = Divide(1, 1);
        var errResult = Divide(1, 0);

        var okHandler = new Func<int, Task>(
            x =>
            {
                var task = new Task(() => Assert.AreEqual(x, 1));
                task.Start();
                return task;
            }
        );
        var errHandler = new Func<string, Task>(
            x =>
            {
                var task = new Task(() => Assert.AreEqual(x, "divisor cannot be 0"));
                task.Start();
                return task;
            }
        );

        await okResult.HandleAsync(okHandler, errHandler);
        await errResult.HandleAsync(okHandler, errHandler);
    }

    [TestMethod]
    public void TestAnd()
    {
        var okResult = Divide(1, 1);
        var errResult = Divide(1, 0);

        var okAndErrResult = okResult.And(errResult);
        var okAndOkResult = okResult.And(okResult);

        Assert.AreEqual(okAndErrResult.UnwrapErr(), "divisor cannot be 0");
        Assert.AreEqual(okAndOkResult.Unwrap(), 1);
    }

    [TestMethod]
    public void TestAndThen()
    {
        var okResult = Divide(1, 1);
        var errResult = Divide(1, 0);

        var okAndErrResult = okResult.AndThen(x => errResult);
        var okAndOkResult = okResult.AndThen(x => okResult);

        Assert.AreEqual(okAndErrResult.UnwrapErr(), "divisor cannot be 0");
        Assert.AreEqual(okAndOkResult.Unwrap(), 1);
    }

    [TestMethod]
    public void TestOr()
    {
        var okResult = Divide(1, 1);
        var errResult = Divide(1, 0);

        var okOrErrResult = okResult.Or(errResult);
        var errOrErrResult = errResult.Or(errResult);

        Assert.AreEqual(okOrErrResult.Unwrap(), 1);
        Assert.AreEqual(errOrErrResult.UnwrapErr(), "divisor cannot be 0");
    }

    [TestMethod]
    public void TestOrElse()
    {
        var okResult = Divide(1, 1);
        var errResult = Divide(1, 0);

        var okOrErrResult = okResult.OrElse(x => errResult);
        var errOrErrResult = errResult.OrElse(x => errResult);

        Assert.AreEqual(okOrErrResult.Unwrap(), 1);
        Assert.AreEqual(errOrErrResult.UnwrapErr(), "divisor cannot be 0");
    }

    [TestMethod]
    public void TestContains()
    {
        var okResult = Divide(1, 1);
        var errResult = Divide(1, 0);

        Assert.IsTrue(okResult.Contains(1));
        Assert.IsFalse(okResult.Contains(2));
        Assert.IsTrue(errResult.ContainsError("divisor cannot be 0"));
        Assert.IsFalse(errResult.ContainsError("divisor cannot be 1"));
    }

    [TestMethod]
    public void TestIsOkIsErr()
    {
        var okResult = Divide(1, 1);
        var errResult = Divide(1, 0);

        Assert.IsTrue(okResult.IsOk);
        Assert.IsFalse(okResult.IsErr);
        Assert.IsFalse(errResult.IsOk);
        Assert.IsTrue(errResult.IsErr);
    }

    [TestMethod]
    public void TestMapOr()
    {
        var okResult = Divide(1, 1);
        var errResult = Divide(1, 0);

        var okOrErrResult = okResult.MapOr(0, x => x + 2);
        var errOrErrResult = errResult.MapOr(0, x => x + 2);

        Assert.AreEqual(okOrErrResult, 3);
        Assert.AreEqual(errOrErrResult, 0);
    }

    [TestMethod]
    public void TestMapOrElse()
    {
        var okResult = Divide(1, 1);
        var errResult = Divide(1, 0);

        var okOrErrResult = okResult.MapOrElse(x => x + 2, _ => 0);
        var errOrErrResult = errResult.MapOrElse(x => x + 2, _ => 0);

        Assert.AreEqual(okOrErrResult, 3);
        Assert.AreEqual(errOrErrResult, 0);
    }

    private static Result<int, string> Divide(int numerator, int divisor)
    {
        if (divisor == 0)
        {
            return new Result<int, string>.Err("divisor cannot be 0");
        }

        return new Result<int, string>.Ok(numerator / divisor);
    }
}
