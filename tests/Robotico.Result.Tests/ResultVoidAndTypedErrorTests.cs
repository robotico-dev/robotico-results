namespace Robotico.Result.Tests;

public class ResultVoidAndTypedErrorTests
{
    // --- void Result: Map / Bind / MapAsync / BindAsync ---

    [Fact]
    public void Result_void_Map_success_returns_mapped()
    {
        Result r = Result.Success();
        Result<string> mapped = r.Map(() => "ok");
        Assert.True(mapped.IsSuccess(out string? s));
        Assert.Equal("ok", s);
    }

    [Fact]
    public void Result_void_Map_error_propagates()
    {
        SimpleError err = new("v");
        Result r = Result.Error(err);
        Result<string> mapped = r.Map(() => "ok");
        Assert.True(mapped.IsError(out IError? e));
        Assert.Same(err, e);
    }

    [Fact]
    public void Result_void_Bind_success_returns_bound()
    {
        Result r = Result.Success();
        Result<int> bound = r.Bind(() => Result.Success(42));
        Assert.True(bound.IsSuccess(out int v));
        Assert.Equal(42, v);
    }

    [Fact]
    public void Result_void_Bind_error_propagates()
    {
        SimpleError err = new("b");
        Result r = Result.Error(err);
        Result<int> bound = r.Bind(() => Result.Success(1));
        Assert.True(bound.IsError(out IError? e));
        Assert.Same(err, e);
    }

    [Fact]
    public async Task Result_void_MapAsync_success_returns_mapped()
    {
        Result r = Result.Success();
        Result<string> mapped = await r.MapAsync(async () => await Task.FromResult("async"));
        Assert.True(mapped.IsSuccess(out string? s));
        Assert.Equal("async", s);
    }

    [Fact]
    public async Task Result_void_BindAsync_success_returns_bound()
    {
        Result r = Result.Success();
        Result<int> bound = await r.BindAsync(async () => await Task.FromResult(Result.Success(7)));
        Assert.True(bound.IsSuccess(out int v));
        Assert.Equal(7, v);
    }

    [Fact]
    public async Task Result_void_MapAsync_error_propagates()
    {
        SimpleError err = new("ma");
        Result r = Result.Error(err);
        Result<string> mapped = await r.MapAsync(async () => await Task.FromResult("x"));
        Assert.True(mapped.IsError(out IError? e));
        Assert.Same(err, e);
    }

    [Fact]
    public async Task Result_void_BindAsync_error_propagates()
    {
        SimpleError err = new("ba");
        Result r = Result.Error(err);
        Result<int> bound = await r.BindAsync(async () => await Task.FromResult(Result.Success(1)));
        Assert.True(bound.IsError(out IError? e));
        Assert.Same(err, e);
    }

    // --- void Result: Match / Tap / TapError / RecoverWith ---

    [Fact]
    public void Result_void_Match_success_returns_onSuccess()
    {
        Result r = Result.Success();
        int x = r.Match(() => 1, _ => 0);
        Assert.Equal(1, x);
    }

    [Fact]
    public void Result_void_Match_error_returns_onError()
    {
        SimpleError err = new("e");
        Result r = Result.Error(err);
        int x = r.Match(() => 1, e => e.Message.Length);
        Assert.Equal(1, x);
    }

    [Fact]
    public void Result_void_Match_actions_success_calls_onSuccess()
    {
        Result r = Result.Success();
        int side = 0;
        r.Match(() => side = 1, _ => side = -1);
        Assert.Equal(1, side);
    }

    [Fact]
    public void Result_void_Match_actions_error_calls_onError()
    {
        SimpleError err = new("x");
        Result r = Result.Error(err);
        int side = 0;
        r.Match(() => side = 1, _ => side = -1);
        Assert.Equal(-1, side);
    }

    [Fact]
    public void Result_void_Tap_success_invokes_action()
    {
        Result r = Result.Success();
        int n = 0;
        Result t = r.Tap(() => n = 1);
        Assert.True(t.IsSuccess());
        Assert.Equal(1, n);
    }

    [Fact]
    public void Result_void_Tap_error_does_not_invoke_action()
    {
        Result r = Result.Error(new SimpleError("e"));
        int n = 0;
        Result t = r.Tap(() => n = 1);
        Assert.True(t.IsError(out _));
        Assert.Equal(0, n);
    }

    [Fact]
    public void Result_void_TapError_error_invokes_action()
    {
        SimpleError err = new("e");
        Result r = Result.Error(err);
        IError? captured = null;
        Result t = r.TapError(e => captured = e);
        Assert.Same(err, captured);
        Assert.True(t.IsError(out _));
    }

    [Fact]
    public void Result_void_RecoverWith_success_returns_this()
    {
        Result r = Result.Success();
        Result fallback = Result.Error(new SimpleError("f"));
        Result recovered = r.RecoverWith(fallback);
        Assert.True(recovered.IsSuccess());
    }

    [Fact]
    public void Result_void_RecoverWith_error_returns_fallback()
    {
        Result r = Result.Error(new SimpleError("e"));
        Result fallback = Result.Success();
        Result recovered = r.RecoverWith(fallback);
        Assert.True(recovered.IsSuccess());
    }

    // --- Result<TData, TError>: Match ---

    [Fact]
    public void Result_TData_TError_Match_success_uses_onSuccess()
    {
        Result<int, SimpleError> r = Result.Success<int, SimpleError>(10);
        int x = r.Match(v => v * 2, _ => -1);
        Assert.Equal(20, x);
    }

    [Fact]
    public void Result_TData_TError_Match_error_uses_onError()
    {
        SimpleError err = new("typed");
        Result<int, SimpleError> r = Result.Error<int, SimpleError>(err);
        int x = r.Match(v => v, e => e.Message.Length);
        Assert.Equal(5, x);
    }

    // --- Result<TData, TError>: Tap / TapError / Recover / RecoverWith ---

    [Fact]
    public void Result_TData_TError_Tap_success_invokes_action()
    {
        Result<int, SimpleError> r = Result.Success<int, SimpleError>(42);
        int value = 0;
        Result<int, SimpleError> t = r.Tap(v => value = v);
        Assert.True(t.IsSuccess(out int v));
        Assert.Equal(42, v);
        Assert.Equal(42, value);
    }

    [Fact]
    public void Result_TData_TError_TapError_error_invokes_action()
    {
        SimpleError err = new("tap");
        Result<int, SimpleError> r = Result.Error<int, SimpleError>(err);
        SimpleError? captured = null;
        Result<int, SimpleError> t = r.TapError(e => captured = e);
        Assert.Same(err, captured);
    }

    [Fact]
    public void Result_TData_TError_Recover_success_returns_this()
    {
        Result<int, SimpleError> r = Result.Success<int, SimpleError>(10);
        Result<int, SimpleError> recovered = r.Recover(_ => 0);
        Assert.True(recovered.IsSuccess(out int v));
        Assert.Equal(10, v);
    }

    [Fact]
    public void Result_TData_TError_Recover_error_returns_fallback_value()
    {
        Result<int, SimpleError> r = Result.Error<int, SimpleError>(new SimpleError("e"));
        Result<int, SimpleError> recovered = r.Recover(e => e.Message.Length);
        Assert.True(recovered.IsSuccess(out int v));
        Assert.Equal(1, v);
    }

    [Fact]
    public void Result_TData_TError_RecoverWith_success_returns_this()
    {
        Result<int, SimpleError> r = Result.Success<int, SimpleError>(7);
        Result<int, SimpleError> recovered = r.RecoverWith(0);
        Assert.True(recovered.IsSuccess(out int v));
        Assert.Equal(7, v);
    }

    [Fact]
    public void Result_TData_TError_RecoverWith_error_returns_fallback_value()
    {
        Result<int, SimpleError> r = Result.Error<int, SimpleError>(new SimpleError("e"));
        Result<int, SimpleError> recovered = r.RecoverWith(99);
        Assert.True(recovered.IsSuccess(out int v));
        Assert.Equal(99, v);
    }

    [Fact]
    public async Task Task_Result_void_MatchAsync_RecoverWithAsync()
    {
        Result r = Result.Success();
        int x = await Task.FromResult(r).MatchAsync(() => 1, _ => 0);
        Assert.Equal(1, x);

        Result err = Result.Error(new SimpleError("e"));
        Result recovered = await Task.FromResult(err).RecoverWithAsync(Result.Success());
        Assert.True(recovered.IsSuccess());
    }

    [Fact]
    public async Task Task_Result_TData_TError_RecoverWithAsync()
    {
        Result<int, SimpleError> r = Result.Error<int, SimpleError>(new SimpleError("e"));
        Result<int, SimpleError> recovered = await Task.FromResult(r).RecoverWithAsync(100);
        Assert.True(recovered.IsSuccess(out int v));
        Assert.Equal(100, v);
    }
}
