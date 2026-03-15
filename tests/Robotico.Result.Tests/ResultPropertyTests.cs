using CsCheck;

namespace Robotico.Result.Tests;

/// <summary>
/// Property-based tests for Result laws using CsCheck. Verifies functor/monad laws over many generated inputs.
/// </summary>
public class ResultPropertyTests
{
    // --- Result&lt;TData&gt;: Map identity (r.Map(x => x) === r) ---

    [Fact]
    public void Map_identity_success_holds_for_any_int()
    {
        Gen.Int.Sample(n =>
        {
            Result<int> r = Result.Success(n);
            Result<int> mapped = r.Map(x => x);
            return mapped.Equals(r);
        });
    }

    [Fact]
    public void Map_identity_error_holds_for_any_error()
    {
        Gen.Const(new SimpleError("e")).Sample(e =>
        {
            Result<int> r = Result.Error<int>(e);
            Result<int> mapped = r.Map(x => x);
            return mapped.IsError(out IError? e2) && ReferenceEquals(e, e2);
        });
    }

    [Fact]
    public void Map_identity_success_holds_for_generated_strings()
    {
        Gen.Int.Select(i => i.ToString()).Sample(s =>
        {
            Result<string> r = Result.Success(s);
            Result<string> mapped = r.Map(x => x);
            return mapped.Equals(r);
        });
    }

    // --- Result&lt;TData&gt;: Bind with Success is identity (r.Bind(x => Result.Success(x)) === r) ---

    [Fact]
    public void Bind_Success_identity_success_holds_for_any_int()
    {
        Gen.Int.Sample(n =>
        {
            Result<int> r = Result.Success(n);
            Result<int> bound = r.Bind(x => Result.Success(x));
            return bound.Equals(r);
        });
    }

    [Fact]
    public void Bind_Success_identity_error_holds_for_any_error()
    {
        Gen.Const(new SimpleError("bind")).Sample(e =>
        {
            Result<int> r = Result.Error<int>(e);
            Result<int> bound = r.Bind(x => Result.Success(x));
            return bound.IsError(out IError? e2) && ReferenceEquals(e, e2);
        });
    }

    // --- Result&lt;TData&gt;: Map propagates error (Result.Error(e).Map(f) === Result.Error(e)) ---

    [Fact]
    public void Map_error_propagates_unchanged_for_any_error_and_func()
    {
        SimpleError e = new SimpleError("err");
        Gen.Int.Sample(n =>
        {
            Result<int> r = Result.Error<int>(e);
            Result<string> mapped = r.Map(x => (n + x).ToString());
            return mapped.IsError(out IError? err) && ReferenceEquals(e, err);
        });
    }

    // --- Result&lt;TData&gt;: RecoverWith success returns same result ---

    [Fact]
    public void RecoverWith_success_returns_same_for_any_int()
    {
        Gen.Int.Sample(n =>
        {
            Result<int> r = Result.Success(n);
            Result<int> recovered = r.RecoverWith(-1);
            return recovered.Equals(r);
        });
    }

    [Fact]
    public void RecoverWith_error_returns_fallback_for_any_fallback()
    {
        Gen.Int.Sample(fallback =>
        {
            Result<int> r = Result.Error<int>(new SimpleError("e"));
            Result<int> recovered = r.RecoverWith(fallback);
            return recovered.IsSuccess(out int v) && v == fallback;
        });
    }

    // --- Void Result: Map/Bind propagate error ---

    [Fact]
    public void Void_Map_error_propagates_for_any_error()
    {
        Gen.Const(new SimpleError("v")).Sample(e =>
        {
            Result r = Result.Error(e);
            Result<string> mapped = r.Map(() => "ok");
            return mapped.IsError(out IError? e2) && ReferenceEquals(e, e2);
        });
    }

    [Fact]
    public void Void_Bind_error_propagates_for_any_error()
    {
        Gen.Const(new SimpleError("v")).Sample(e =>
        {
            Result r = Result.Error(e);
            Result<int> bound = r.Bind(() => Result.Success(1));
            return bound.IsError(out IError? e2) && ReferenceEquals(e, e2);
        });
    }

    // --- Result&lt;TData, TError&gt;: Map identity and error propagation ---

    [Fact]
    public void ResultTypedError_Map_identity_success_holds_for_any_int()
    {
        Gen.Int.Sample(n =>
        {
            Result<int, SimpleError> r = Result.Success<int, SimpleError>(n);
            Result<int, SimpleError> mapped = r.Map(x => x);
            return mapped.Equals(r);
        });
    }

    [Fact]
    public void ResultTypedError_Map_error_propagates_unchanged()
    {
        Gen.Const(new SimpleError("typed")).Sample(e =>
        {
            Result<int, SimpleError> r = Result.Error<int, SimpleError>(e);
            Result<string, SimpleError> mapped = r.Map(x => x.ToString());
            return mapped.IsError(out SimpleError? e2) && ReferenceEquals(e, e2);
        });
    }

    // --- Result<TData, TError>: Bind with Success is identity ---

    [Fact]
    public void ResultTypedError_Bind_Success_identity_success_holds_for_any_int()
    {
        Gen.Int.Sample(n =>
        {
            Result<int, SimpleError> r = Result.Success<int, SimpleError>(n);
            Result<int, SimpleError> bound = r.Bind(x => Result.Success<int, SimpleError>(x));
            return bound.Equals(r);
        });
    }

    [Fact]
    public void ResultTypedError_Bind_Success_identity_error_holds_for_any_error()
    {
        Gen.Const(new SimpleError("bind-typed")).Sample(e =>
        {
            Result<int, SimpleError> r = Result.Error<int, SimpleError>(e);
            Result<int, SimpleError> bound = r.Bind(x => Result.Success<int, SimpleError>(x));
            return bound.IsError(out SimpleError? e2) && ReferenceEquals(e, e2);
        });
    }

    // --- Result<TData, TError>: MapError identity and propagation ---

    [Fact]
    public void ResultTypedError_MapError_identity_success_unchanged()
    {
        Gen.Int.Sample(n =>
        {
            Result<int, SimpleError> r = Result.Success<int, SimpleError>(n);
            Result<int, SimpleError> mapped = r.MapError(e => e);
            return mapped.Equals(r);
        });
    }

    [Fact]
    public void ResultTypedError_MapError_error_propagates_unchanged()
    {
        Gen.Const(new SimpleError("maperr")).Sample(e =>
        {
            Result<int, SimpleError> r = Result.Error<int, SimpleError>(e);
            Result<int, SimpleError> mapped = r.MapError(err => err);
            return mapped.IsError(out SimpleError? e2) && ReferenceEquals(e, e2);
        });
    }

    // --- Result<TData, TError>: RecoverWith ---

    [Fact]
    public void ResultTypedError_RecoverWith_success_returns_same_for_any_int()
    {
        Gen.Int.Sample(n =>
        {
            Result<int, SimpleError> r = Result.Success<int, SimpleError>(n);
            Result<int, SimpleError> recovered = r.RecoverWith(-1);
            return recovered.Equals(r);
        });
    }

    [Fact]
    public void ResultTypedError_RecoverWith_error_returns_fallback_for_any_fallback()
    {
        Gen.Int.Sample(fallback =>
        {
            Result<int, SimpleError> r = Result.Error<int, SimpleError>(new SimpleError("e"));
            Result<int, SimpleError> recovered = r.RecoverWith(fallback);
            return recovered.IsSuccess(out int v) && v == fallback;
        });
    }
}
