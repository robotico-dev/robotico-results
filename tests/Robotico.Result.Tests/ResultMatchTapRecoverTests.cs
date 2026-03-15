namespace Robotico.Result.Tests;

public class ResultMatchTapRecoverTests
{
    [Fact]
    public void Match_success_uses_onSuccess()
    {
        Result<int> r = Result.Success(7);
        int x = r.Match(v => v + 1, _ => -1);
        Assert.Equal(8, x);
    }

    [Fact]
    public void Match_error_uses_onError()
    {
        Result<int> r = Result.Error<int>(new SimpleError("m"));
        int x = r.Match(v => v, e => -99);
        Assert.Equal(-99, x);
    }

    [Fact]
    public void Tap_success_invokes_action()
    {
        int side = 0;
        Result<int> r = Result.Success(3).Tap(v => side = v);
        Assert.Equal(3, side);
        Assert.True(r.IsSuccess(out int v));
        Assert.Equal(3, v);
    }

    [Fact]
    public void Tap_error_does_not_invoke()
    {
        int side = 0;
        Result<int> r = Result.Error<int>(new SimpleError("t")).Tap(v => side = v);
        Assert.Equal(0, side);
        Assert.True(r.IsError(out _));
    }

    [Fact]
    public void Recover_error_returns_fallback()
    {
        Result<int> r = Result.Error<int>(new SimpleError("r"));
        Result<int> recovered = r.Recover(_ => 100);
        Assert.True(recovered.IsSuccess(out int v));
        Assert.Equal(100, v);
    }

    [Fact]
    public void RecoverWith_constant_fallback()
    {
        Result<string> r = Result.Error<string>(new SimpleError("r"));
        Result<string> recovered = r.RecoverWith("default");
        Assert.True(recovered.IsSuccess(out string? s));
        Assert.Equal("default", s);
    }

    [Fact]
    public void Ensure_predicate_true_returns_same()
    {
        Result<int> r = Result.Success(5).Ensure(x => x > 0, "must be positive");
        Assert.True(r.IsSuccess(out int v));
        Assert.Equal(5, v);
    }

    [Fact]
    public void Ensure_predicate_false_returns_error()
    {
        Result<int> r = Result.Success(-1).Ensure(x => x > 0, "must be positive");
        Assert.True(r.IsError(out IError? e));
        Assert.Equal("must be positive", e!.Message);
    }
}
