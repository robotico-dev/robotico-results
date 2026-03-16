namespace Robotico.Result.Tests;

/// <summary>
/// Tests for Ensure(Result&lt;TData, TError&gt;) and RecoverWith(result fallback) overloads.
/// </summary>
public class ResultEnsureAndRecoverWithTests
{
    // --- Ensure for Result<TData, TError> ---

    [Fact]
    public void Ensure_TDataTError_predicate_pass_returns_same_result()
    {
        Result<int, SimpleError> r = Result.Success<int, SimpleError>(42);
        Result<int, SimpleError> ensured = r.Ensure(x => x > 0, _ => new("must be positive"));
        Assert.True(ensured.IsSuccess(out int value));
        Assert.Equal(42, value);
    }

    [Fact]
    public void Ensure_TDataTError_predicate_fail_returns_error_from_factory()
    {
        Result<int, SimpleError> r = Result.Success<int, SimpleError>(42);
        SimpleError err = new("must be negative");
        Result<int, SimpleError> ensured = r.Ensure(x => x < 0, _ => err);
        Assert.True(ensured.IsError(out SimpleError? e));
        Assert.Same(err, e);
    }

    [Fact]
    public void Ensure_TDataTError_on_error_returns_same_error()
    {
        SimpleError original = new("original");
        Result<int, SimpleError> r = Result.Error<int, SimpleError>(original);
        Result<int, SimpleError> ensured = r.Ensure(x => x > 0, _ => new("other"));
        Assert.True(ensured.IsError(out SimpleError? e));
        Assert.Same(original, e);
    }

    [Fact]
    public void Ensure_TDataTError_throws_when_predicate_is_null()
    {
        Result<int, SimpleError> r = Result.Success<int, SimpleError>(1);
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() =>
            r.Ensure((Func<int, bool>)null!, _ => new("x")));
        Assert.Equal("predicate", ex.ParamName);
    }

    [Fact]
    public void Ensure_TDataTError_throws_when_errorFactory_is_null()
    {
        Result<int, SimpleError> r = Result.Success<int, SimpleError>(1);
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() =>
            r.Ensure(_ => true, (Func<int, SimpleError>)null!));
        Assert.Equal("errorFactory", ex.ParamName);
    }

    // --- RecoverWith(Result<TData> fallback) ---

    [Fact]
    public void RecoverWith_ResultTData_success_returns_same_result()
    {
        Result<int> r = Result.Success(10);
        Result<int> fallback = Result.Success(99);
        Result<int> recovered = r.RecoverWith(fallback);
        Assert.True(recovered.IsSuccess(out int value));
        Assert.Equal(10, value);
    }

    [Fact]
    public void RecoverWith_ResultTData_error_returns_fallback_result()
    {
        Result<int> r = Result.Error<int>(new SimpleError("e"));
        Result<int> fallback = Result.Success(99);
        Result<int> recovered = r.RecoverWith(fallback);
        Assert.True(recovered.IsSuccess(out int value));
        Assert.Equal(99, value);
    }

    [Fact]
    public void RecoverWith_ResultTData_error_fallback_also_error_returns_fallback_error()
    {
        SimpleError errFallback = new("fallback");
        Result<int> r = Result.Error<int>(new SimpleError("e"));
        Result<int> fallback = Result.Error<int>(errFallback);
        Result<int> recovered = r.RecoverWith(fallback);
        Assert.True(recovered.IsError(out IError? e));
        Assert.Same(errFallback, e);
    }

    // --- RecoverWith(Result<TData, TError> fallback) ---

    [Fact]
    public void RecoverWith_ResultTDataTError_success_returns_same_result()
    {
        Result<int, SimpleError> r = Result.Success<int, SimpleError>(10);
        Result<int, SimpleError> fallback = Result.Success<int, SimpleError>(99);
        Result<int, SimpleError> recovered = r.RecoverWith(fallback);
        Assert.True(recovered.IsSuccess(out int value));
        Assert.Equal(10, value);
    }

    [Fact]
    public void RecoverWith_ResultTDataTError_error_returns_fallback_result()
    {
        Result<int, SimpleError> r = Result.Error<int, SimpleError>(new SimpleError("e"));
        Result<int, SimpleError> fallback = Result.Success<int, SimpleError>(99);
        Result<int, SimpleError> recovered = r.RecoverWith(fallback);
        Assert.True(recovered.IsSuccess(out int value));
        Assert.Equal(99, value);
    }

    [Fact]
    public void RecoverWith_ResultTDataTError_error_fallback_also_error_returns_fallback_error()
    {
        SimpleError errFallback = new("fallback");
        Result<int, SimpleError> r = Result.Error<int, SimpleError>(new SimpleError("e"));
        Result<int, SimpleError> fallback = Result.Error<int, SimpleError>(errFallback);
        Result<int, SimpleError> recovered = r.RecoverWith(fallback);
        Assert.True(recovered.IsError(out SimpleError? e));
        Assert.Same(errFallback, e);
    }
}
