using System.Globalization;

namespace Robotico.Result.Tests;

/// <summary>
/// Tests that Result obeys standard functor/monad-style laws: identity and error propagation.
/// </summary>
public class ResultLawsTests
{
    // --- Map identity: r.Map(x => x) === r (for success) ---

    [Fact]
    public void Map_identity_success_preserves_value()
    {
        Result<int> r = Result.Success(42);
        Result<int> mapped = r.Map(x => x);
        Assert.True(mapped.IsSuccess(out int value));
        Assert.Equal(42, value);
    }

    [Fact]
    public void Map_identity_error_preserves_error()
    {
        SimpleError err = new SimpleError("e");
        Result<int> r = Result.Error<int>(err);
        Result<int> mapped = r.Map(x => x);
        Assert.True(mapped.IsError(out IError? e));
        Assert.Same(err, e);
    }

    // --- Bind with Success is identity: r.Bind(x => Result.Success(x)) === r ---

    [Fact]
    public void Bind_Success_identity_success_preserves_value()
    {
        Result<int> r = Result.Success(7);
        Result<int> bound = r.Bind(x => Result.Success(x));
        Assert.True(bound.IsSuccess(out int value));
        Assert.Equal(7, value);
    }

    [Fact]
    public void Bind_Success_identity_error_preserves_error()
    {
        SimpleError err = new SimpleError("fail");
        Result<int> r = Result.Error<int>(err);
        Result<int> bound = r.Bind(x => Result.Success(x));
        Assert.True(bound.IsError(out IError? e));
        Assert.Same(err, e);
    }

    // --- Map propagates error: Result.Error<T>(e).Map(f) === Result.Error<TMapped>(e) (same error) ---

    [Fact]
    public void Map_error_propagates_error_unchanged()
    {
        SimpleError err = new SimpleError("x");
        Result<int> r = Result.Error<int>(err);
        Result<string> mapped = r.Map(x => x.ToString(CultureInfo.InvariantCulture));
        Assert.True(mapped.IsError(out IError? e));
        Assert.Same(err, e);
    }

    [Fact]
    public void Bind_error_propagates_error_unchanged()
    {
        SimpleError err = new SimpleError("y");
        Result<int> r = Result.Error<int>(err);
        Result<string> bound = r.Bind(x => Result.Success(x.ToString(CultureInfo.InvariantCulture)));
        Assert.True(bound.IsError(out IError? e));
        Assert.Same(err, e);
    }

    // --- RecoverWith success returns same result ---

    [Fact]
    public void RecoverWith_success_returns_same_result()
    {
        Result<int> r = Result.Success(10);
        Result<int> recovered = r.RecoverWith(0);
        Assert.True(recovered.IsSuccess(out int value));
        Assert.Equal(10, value);
    }

    [Fact]
    public void RecoverWith_error_returns_fallback_value()
    {
        Result<int> r = Result.Error<int>(new SimpleError("e"));
        Result<int> recovered = r.RecoverWith(99);
        Assert.True(recovered.IsSuccess(out int value));
        Assert.Equal(99, value);
    }

    // --- Void Result: Bind propagates error ---

    [Fact]
    public void Void_Bind_error_propagates()
    {
        SimpleError err = new SimpleError("v");
        Result r = Result.Error(err);
        Result<int> bound = r.Bind(() => Result.Success(1));
        Assert.True(bound.IsError(out IError? e));
        Assert.Same(err, e);
    }

    [Fact]
    public void Void_Map_error_propagates()
    {
        SimpleError err = new SimpleError("m");
        Result r = Result.Error(err);
        Result<string> mapped = r.Map(() => "ok");
        Assert.True(mapped.IsError(out IError? e));
        Assert.Same(err, e);
    }
}
