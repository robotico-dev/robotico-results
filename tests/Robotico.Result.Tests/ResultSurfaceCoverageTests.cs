using System.Collections.Immutable;
using System.Globalization;

namespace Robotico.Result.Tests;

/// <summary>Exercises Result / Result&lt;T&gt; / Result&lt;T,E&gt;, extensions, Task extensions, and ResultUtilities branches for CI line coverage.</summary>
public sealed class ResultSurfaceCoverageTests
{
    [Fact]
    public void ValidationError_TData_overload()
    {
        IReadOnlyDictionary<string, string[]> dict = new Dictionary<string, string[]> { ["x"] = ["bad"] };
        Result<int> r = Result.ValidationError<int>(dict, "msg", "CODE");
        Assert.True(r.IsError(out IError? e));
        Assert.IsType<ValidationError>(e);
    }

    [Fact]
    public void FromVoid_and_FromWithIError_from_ResultT()
    {
        Assert.True(Result.FromVoid(Result.Success(1)).IsSuccess());
        SimpleError err = new("e");
        Result v = Result.FromVoid(Result.Error<int>(err));
        Assert.True(v.IsError(out IError? e));
        Assert.Same(err, e);

        Result<int, IError> ie = Result.FromWithIError(Result.Success(2));
        Assert.True(ie.IsSuccess(out int d));
        Assert.Equal(2, d);
        Result<int, IError> ie2 = Result.FromWithIError(Result.Error<int>(err));
        Assert.True(ie2.IsError(out IError? e2));
        Assert.Same(err, e2);
    }

    [Fact]
    public void FromVoid_From_FromWithIError_typed_error()
    {
        Result<int, SimpleError> ok = Result.Success<int, SimpleError>(5);
        Assert.True(Result.FromVoid(ok).IsSuccess());
        SimpleError se = new("se");
        Result<int, SimpleError> bad = Result.Error<int, SimpleError>(se);
        Result vv = Result.FromVoid(bad);
        Assert.True(vv.IsError(out IError? ev));
        Assert.Same(se, ev);

        Result<int> erased = Result.From(ok);
        Assert.True(erased.IsSuccess(out int x));
        Assert.Equal(5, x);
        Result<int> erasedE = Result.From(bad);
        Assert.True(erasedE.IsError(out IError? ee));
        Assert.Same(se, ee);

        Result<int, IError> wi = Result.FromWithIError(ok);
        Assert.True(wi.IsSuccess(out int d));
        Assert.Equal(5, d);
    }

    [Fact]
    public void Void_Result_Map_and_Bind_with_error_mapping()
    {
        Result ok = Result.Success();
        Result<int, SimpleError> m = ok.Map(() => 1, _ => new SimpleError("mapped"));
        Assert.True(m.IsSuccess(out int v));
        Assert.Equal(1, v);

        Result err = Result.Error(new SimpleError("base"));
        Result<int, SimpleError> me = err.Map(() => 0, _ => new SimpleError("fromErr"));
        Assert.True(me.IsError(out SimpleError? se));
        Assert.Equal("fromErr", se!.Message);

        Result<int, SimpleError> b = ok.Bind(() => Result.Success<int, SimpleError>(3), _ => new SimpleError("b"));
        Assert.True(b.IsSuccess(out int bv));
        Assert.Equal(3, bv);
        Result<int, SimpleError> be = err.Bind(() => Result.Success<int, SimpleError>(0), e => new SimpleError(e.Message + "!"));
        Assert.True(be.IsError(out SimpleError? bse));
        Assert.Equal("base!", bse!.Message);
    }

    [Fact]
    public async Task Void_Result_MapAsync_BindAsync()
    {
        Result ok = Result.Success();
        Result<string> ma = await ok.MapAsync(async () =>
        {
            await Task.Yield();
            return "a";
        });
        Assert.True(ma.IsSuccess(out string? s));
        Assert.Equal("a", s);

        Result<string> ba = await ok.BindAsync(async () =>
        {
            await Task.Yield();
            return Result.Success("b");
        });
        Assert.True(ba.IsSuccess(out string? sb));
        Assert.Equal("b", sb);
    }

    [Fact]
    public void Void_Result_ExpectSuccess_and_Equality()
    {
        Result.Success().ExpectSuccess();
        SimpleError err = new("x");
        Assert.Throws<ResultErrorException<IError>>(() => Result.Error(err).ExpectSuccess());
        Assert.Throws<InvalidOperationException>(() => Result.Error(err).ExpectSuccess(_ => new InvalidOperationException("ioe")));

        Result a = Result.Success();
        Result b = Result.Success();
        Assert.True(a.Equals((object)b));
        Assert.True(a.Equals(b));
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(0, a.GetHashCode());

        Assert.False(a.Equals(err));
        Result e1 = Result.Error(err);
        Result e2 = Result.Error(err);
        Assert.True(e1.Equals(e2));
        Assert.Equal(err.GetHashCode(), e1.GetHashCode());
        Assert.True(e1 == e2);
    }

    [Fact]
    public void Void_extensions_Match_Tap_TapError_RecoverWith()
    {
        int side = 0;
        Result.Success().Match(() => side = 1, _ => side = -1);
        Assert.Equal(1, side);

        side = 0;
        SimpleError err = new("te");
        Result.Error(err).Match(() => side = 1, _ => side = 2);
        Assert.Equal(2, side);

        Assert.True(Result.Success().Tap(() => { }).IsSuccess());
        side = 0;
        Result.Error(err).TapError(e => side = e.Message.Length);
        Assert.Equal(2, side);

        Result fb = Result.Success();
        Assert.True(Result.Error(err).RecoverWith(fb).IsSuccess());
    }

    [Fact]
    public async Task ResultT_IsError_overloads_MapError_async_MapErrorAsync_BindAsync()
    {
        Result<int> ok = Result.Success(4);
        Assert.False(ok.IsError());
        Assert.False(ok.IsError(out int d, out IError? e));
        Assert.Equal(4, d);

        SimpleError err = new("m");
        Result<int> bad = Result.Error<int>(err);
        Assert.True(bad.IsError());
        Assert.True(bad.IsError(out int bd, out IError? be));
        Assert.Equal(0, bd);
        Assert.Same(err, be);

        Result<int, SimpleError> mapped = ok.MapError(_ => new SimpleError("me"));
        Assert.True(mapped.IsSuccess(out int mv));
        Assert.Equal(4, mv);

        Result<int, SimpleError> mappedE = bad.MapError(e => new SimpleError(e.Message + "2"));
        Assert.True(mappedE.IsError(out SimpleError? mse));
        Assert.Equal("m2", mse!.Message);

        Result<string> ma = await ok.MapAsync(x => Task.FromResult(x.ToString(CultureInfo.InvariantCulture)));
        Assert.True(ma.IsSuccess(out string? ms));
        Assert.Equal("4", ms);

        Result<string> ba = await ok.BindAsync(x => Task.FromResult(Result.Success(x.ToString(CultureInfo.InvariantCulture))));
        Assert.True(ba.IsSuccess(out string? bs));
        Assert.Equal("4", bs);

        Result<int, SimpleError> mea = await bad.MapErrorAsync(e => Task.FromResult(new SimpleError(e.Message + "a")));
        Assert.True(mea.IsError(out SimpleError? ase));
        Assert.Equal("ma", ase!.Message);
    }

    [Fact]
    public void ResultT_ExpectSuccess_Equality_operators_GetValue()
    {
        Assert.Equal(9, Result.Success(9).ExpectSuccess());
        SimpleError err = new("ex");
        Assert.Throws<ResultErrorException<IError>>(() => Result.Error<int>(err).ExpectSuccess());
        Assert.Throws<FormatException>(() => Result.Error<int>(err).ExpectSuccess(_ => new FormatException()));

        Result<int> a = Result.Success(1);
        Result<int> b = Result.Success(1);
        Assert.True(a.Equals((object)b));
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.NotEqual(Result.Error<int>(err).GetHashCode(), a.GetHashCode());

        Assert.Equal(0, Result.Error<int>(err).GetValue());
    }

    [Fact]
    public void ResultT_extensions_Match_action_TapError_GetValueOrDefault_MapError_RecoverAsync()
    {
        int side = 0;
        Result.Success(3).Match(v => side = v, _ => side = -1);
        Assert.Equal(3, side);

        side = 0;
        Result.Error<int>(new SimpleError("q")).TapError(_ => side = 5);
        Assert.Equal(5, side);

        Assert.Equal(7, Result.Success(7).GetValueOrDefault());
        Assert.Equal(0, Result.Error<int>(new SimpleError("z")).GetValueOrDefault());

        Result<int> mapped = ResultExtensions.MapError(
            Result.Error<int>(new SimpleError("orig")),
            e => new SimpleError(e.Message + "x"));
        Assert.True(mapped.IsError(out IError? me));
        Assert.Equal("origx", me!.Message);

        Result<int> rw = Result.Error<int>(new SimpleError("r")).RecoverWith(Result.Success(11));
        Assert.True(rw.IsSuccess(out int rv));
        Assert.Equal(11, rv);
    }

    [Fact]
    public async Task ResultT_extensions_RecoverAsync_TapAsync_TapErrorAsync()
    {
        Result<int> r = await Result.Error<int>(new SimpleError("a")).RecoverAsync(async e =>
        {
            await Task.Yield();
            return e.Message.Length;
        });
        Assert.True(r.IsSuccess(out int v));
        Assert.Equal(1, v);

        int tap = 0;
        Result<int> t = await Result.Success(2).TapAsync(async x =>
        {
            await Task.Yield();
            tap = x;
        });
        Assert.Equal(2, tap);
        Assert.True(t.IsSuccess(out int tv));
        Assert.Equal(2, tv);

        int te = 0;
        await Result.Error<int>(new SimpleError("tapE")).TapErrorAsync(async err =>
        {
            await Task.Yield();
            te = err.Code.Length;
        });
        Assert.True(te > 0);
    }

    [Fact]
    public async Task ResultTE_all_Is_out_GetValue_ExpectSuccess_Map_Bind_MapError_async()
    {
        Result<int, SimpleError> ok = Result.Success<int, SimpleError>(8);
        Assert.True(ok.IsSuccess());
        Assert.False(ok.IsError());
        Assert.True(ok.IsSuccess(out int d, out SimpleError? ne));
        Assert.Equal(8, d);
        Assert.Null(ne);

        SimpleError se = new("E");
        Result<int, SimpleError> bad = Result.Error<int, SimpleError>(se);
        Assert.True(bad.IsError(out SimpleError? oe));
        Assert.Same(se, oe);
        Assert.True(bad.IsError(out int bd, out SimpleError? be));
        Assert.Equal(0, bd);
        Assert.Same(se, be);

        Assert.Equal(8, ok.GetValue());
        Assert.Equal(0, bad.GetValue());

        Assert.Equal(8, ok.ExpectSuccess());
        Assert.Throws<ResultErrorException<SimpleError>>(() => bad.ExpectSuccess());
        Assert.Throws<TimeoutException>(() => bad.ExpectSuccess(_ => new TimeoutException()));

        Result<string, SimpleError> sm = ok.Map(x => x.ToString(CultureInfo.InvariantCulture));
        Assert.True(sm.IsSuccess(out string? ss));
        Assert.Equal("8", ss);

        Result<string, SimpleError> sb = ok.Bind(x => Result.Success<string, SimpleError>(x.ToString(CultureInfo.InvariantCulture)));
        Assert.True(sb.IsSuccess(out string? s2));
        Assert.Equal("8", s2);

        Result<int, SimpleError> me = bad.MapError(e => new SimpleError(e.Message + "M"));
        Assert.True(me.IsError(out SimpleError? mse));
        Assert.Equal("EM", mse!.Message);

        Result<string, SimpleError> sma = await ok.MapAsync(x => Task.FromResult(x.ToString(CultureInfo.InvariantCulture)));
        Assert.True(sma.IsSuccess(out string? asv));
        Assert.Equal("8", asv);

        Result<string, SimpleError> sba = await ok.BindAsync(x => Task.FromResult(Result.Success<string, SimpleError>(x.ToString(CultureInfo.InvariantCulture))));
        Assert.True(sba.IsSuccess(out string? abv));
        Assert.Equal("8", abv);

        Result<int, SimpleError> mea = await bad.MapErrorAsync(e => Task.FromResult(new SimpleError(e.Message + "A")));
        Assert.True(mea.IsError(out SimpleError? ase));
        Assert.Equal("EA", ase!.Message);
    }

    [Fact]
    public void ResultTE_extensions_and_Equality()
    {
        Result<int, SimpleError> ok = Result.Success<int, SimpleError>(1);
        int s = ok.Match(v => v * 2, _ => 0);
        Assert.Equal(2, s);
        int s2 = Result.Error<int, SimpleError>(new SimpleError("m")).Match(_ => 0, e => e.Message.Length);
        Assert.Equal(1, s2);

        int tap = 0;
        ok.Tap(v => tap = v);
        Assert.Equal(1, tap);
        tap = 0;
        Result.Error<int, SimpleError>(new SimpleError("t")).TapError(e => tap = e.Message.Length);
        Assert.Equal(1, tap);

        Result<int, SimpleError> rec = Result.Error<int, SimpleError>(new SimpleError("r")).Recover(_ => 99);
        Assert.True(rec.IsSuccess(out int rv));
        Assert.Equal(99, rv);
        Result<int, SimpleError> rc = Result.Error<int, SimpleError>(new SimpleError("r2")).RecoverWith(5);
        Assert.True(rc.IsSuccess(out int rv2));
        Assert.Equal(5, rv2);
        Result<int, SimpleError> rf = ok.RecoverWith(Result.Error<int, SimpleError>(new SimpleError("fb")));
        Assert.True(rf.IsSuccess(out int okv));
        Assert.Equal(1, okv);
        Result<int, SimpleError> rfb = Result.Error<int, SimpleError>(new SimpleError("x")).RecoverWith(Result.Success<int, SimpleError>(7));
        Assert.True(rfb.IsSuccess(out int seven));
        Assert.Equal(7, seven);

        Result<int, SimpleError> a = Result.Success<int, SimpleError>(3);
        Result<int, SimpleError> b = Result.Success<int, SimpleError>(3);
        Assert.True(a.Equals((object)b));
        Assert.True(a == b);
        Assert.False(a != b);
        SimpleError same = new("same");
        Result<int, SimpleError> e1 = Result.Error<int, SimpleError>(same);
        Result<int, SimpleError> e2 = Result.Error<int, SimpleError>(same);
        Assert.True(e1.Equals(e2));
    }

    [Fact]
    public async Task ResultTE_extensions_async_Ensure()
    {
        Result<int, SimpleError> ok = Result.Success<int, SimpleError>(2);
        Result<int, SimpleError> t = await ok.TapAsync(async x =>
        {
            await Task.Yield();
            _ = x;
        });
        Assert.True(t.IsSuccess(out int tv));
        Assert.Equal(2, tv);

        Result<int, SimpleError> te = await Result.Error<int, SimpleError>(new SimpleError("e")).TapErrorAsync(async err =>
        {
            await Task.Yield();
            _ = err;
        });
        Assert.True(te.IsError(out _));

        Result<int, SimpleError> ra = await Result.Error<int, SimpleError>(new SimpleError("z")).RecoverAsync(async e =>
        {
            await Task.Yield();
            return e.Message.Length;
        });
        Assert.True(ra.IsSuccess(out int len));
        Assert.Equal(1, len);

        Result<int, SimpleError> ens = ok.Ensure(x => x > 0, _ => new SimpleError("bad"));
        Assert.True(ens.IsSuccess(out int ev));
        Assert.Equal(2, ev);
        Result<int, SimpleError> enf = ok.Ensure(x => x > 10, v => new SimpleError(v.ToString(CultureInfo.InvariantCulture)));
        Assert.True(enf.IsError(out SimpleError? ese));
        Assert.Equal("2", ese!.Message);
    }

    [Fact]
    public async Task ResultTaskExtensions_void_Result_overloads()
    {
        Task<Result> ok = Task.FromResult(Result.Success());
        Result<int> m = await ok.MapAsync(() => 10);
        Assert.True(m.IsSuccess(out int mv));
        Assert.Equal(10, mv);

        Result<string> b = await ok.BindAsync(() => Task.FromResult(Result.Success("x")));
        Assert.True(b.IsSuccess(out string? bs));
        Assert.Equal("x", bs);

        int match = await ok.MatchAsync(() => 1, _ => 2);
        Assert.Equal(1, match);

        int tap = 0;
        Result rt = await ok.TapAsync(() => tap = 3);
        Assert.Equal(3, tap);
        Assert.True(rt.IsSuccess());

        int at = 0;
        Result rt2 = await ok.TapAsync(async () =>
        {
            await Task.Yield();
            at = 4;
        });
        Assert.Equal(4, at);

        SimpleError err = new("te");
        int et = 0;
        await Task.FromResult(Result.Error(err)).TapErrorAsync(e => et = e.Message.Length);
        Assert.Equal(2, et);

        Result rec = await Task.FromResult(Result.Error(err)).RecoverWithAsync(Result.Success());
        Assert.True(rec.IsSuccess());
    }

    [Fact]
    public async Task ResultTaskExtensions_ResultT_extra_overloads()
    {
        Task<Result<int>> ok = Task.FromResult(Result.Success(5));
        Result<string> m = await ok.MapAsync(x => Task.FromResult(x.ToString(CultureInfo.InvariantCulture)));
        Assert.True(m.IsSuccess(out string? ms));
        Assert.Equal("5", ms);

        Result<string> b = await ok.BindAsync(x => Task.FromResult(Result.Success(x.ToString(CultureInfo.InvariantCulture))));
        Assert.True(b.IsSuccess(out string? bs));
        Assert.Equal("5", bs);

        Result<int> errTask = await Task.FromResult(Result.Error<int>(new SimpleError("me")));
        Result<int, SimpleError> me = await errTask.MapErrorAsync(e => Task.FromResult(new SimpleError(e.Message + "!")));
        Assert.True(me.IsError(out SimpleError? se));
        Assert.Equal("me!", se!.Message);

        Assert.Equal(5, await ok.GetValueAsync());

        Result<int> rw = await Task.FromResult(Result.Error<int>(new SimpleError("rw"))).RecoverWithAsync(Result.Success(12));
        Assert.True(rw.IsSuccess(out int v));
        Assert.Equal(12, v);

        await Assert.ThrowsAsync<ResultErrorException<IError>>(async () =>
            await Task.FromResult(Result.Error<int>(new SimpleError("e"))).ExpectSuccessAsync());
        await Assert.ThrowsAsync<IOException>(async () =>
            await Task.FromResult(Result.Error<int>(new SimpleError("e"))).ExpectSuccessAsync(_ => new IOException()));
    }

    [Fact]
    public async Task ResultTaskExtensions_ResultTE_overloads()
    {
        Task<Result<int, SimpleError>> ok = Task.FromResult(Result.Success<int, SimpleError>(6));
        Result<string, SimpleError> m = await ok.MapAsync(x => x.ToString(CultureInfo.InvariantCulture));
        Assert.True(m.IsSuccess(out string? s));
        Assert.Equal("6", s);

        Result<string, SimpleError> ma = await ok.MapAsync(x => Task.FromResult(x.ToString(CultureInfo.InvariantCulture)));
        Assert.True(ma.IsSuccess(out string? sa));
        Assert.Equal("6", sa);

        Result<string, SimpleError> b = await ok.BindAsync(x => Result.Success<string, SimpleError>(x.ToString(CultureInfo.InvariantCulture)));
        Assert.True(b.IsSuccess(out string? bs));
        Assert.Equal("6", bs);

        Result<string, SimpleError> ba = await ok.BindAsync(x => Task.FromResult(Result.Success<string, SimpleError>(x.ToString(CultureInfo.InvariantCulture))));
        Assert.True(ba.IsSuccess(out string? bas));
        Assert.Equal("6", bas);

        int mm = await ok.MatchAsync(x => x, _ => -1);
        Assert.Equal(6, mm);

        Result<int, SimpleError> me = await Task.FromResult(Result.Error<int, SimpleError>(new SimpleError("q"))).MapErrorAsync(e => new SimpleError(e.Message + "z"));
        Assert.True(me.IsError(out SimpleError? qe));
        Assert.Equal("qz", qe!.Message);

        Result<int, SimpleError> mea = await Task.FromResult(Result.Error<int, SimpleError>(new SimpleError("q"))).MapErrorAsync(e => Task.FromResult(new SimpleError(e.Message + "Z")));
        Assert.True(mea.IsError(out SimpleError? qe2));
        Assert.Equal("qZ", qe2!.Message);

        Assert.Equal(6, await ok.GetValueAsync());

        await Assert.ThrowsAsync<ResultErrorException<SimpleError>>(async () =>
            await Task.FromResult(Result.Error<int, SimpleError>(new SimpleError("e"))).ExpectSuccessAsync());

        Result<int, SimpleError> t = await ok.TapAsync(x => _ = x);
        Assert.True(t.IsSuccess(out int tv));
        Assert.Equal(6, tv);
        Result<int, SimpleError> ta = await ok.TapAsync(async x =>
        {
            await Task.Yield();
            _ = x;
        });
        Assert.True(ta.IsSuccess(out int tav));
        Assert.Equal(6, tav);

        Result<int, SimpleError> te = await Task.FromResult(Result.Error<int, SimpleError>(new SimpleError("t"))).TapErrorAsync(e => _ = e.Message);
        Assert.True(te.IsError(out _));
        Result<int, SimpleError> tea = await Task.FromResult(Result.Error<int, SimpleError>(new SimpleError("t"))).TapErrorAsync(async e =>
        {
            await Task.Yield();
            _ = e;
        });
        Assert.True(tea.IsError(out _));

        Result<int, SimpleError> r = await Task.FromResult(Result.Error<int, SimpleError>(new SimpleError("r"))).RecoverAsync(e => e.Message.Length);
        Assert.True(r.IsSuccess(out int len));
        Assert.Equal(1, len);
        Result<int, SimpleError> ra = await Task.FromResult(Result.Error<int, SimpleError>(new SimpleError("r"))).RecoverAsync(async e =>
        {
            await Task.Yield();
            return e.Message.Length;
        });
        Assert.True(ra.IsSuccess(out int len2));
        Assert.Equal(1, len2);

        Result<int, SimpleError> rw = await Task.FromResult(Result.Error<int, SimpleError>(new SimpleError("w"))).RecoverWithAsync(0);
        Assert.True(rw.IsSuccess(out int z));
        Assert.Equal(0, z);
        Result<int, SimpleError> rw2 = await Task.FromResult(Result.Error<int, SimpleError>(new SimpleError("w"))).RecoverWithAsync(Result.Success<int, SimpleError>(9));
        Assert.True(rw2.IsSuccess(out int nine));
        Assert.Equal(9, nine);
    }

    [Fact]
    public async Task ResultUtilities_Try_void_and_TryAsync_void()
    {
        Result r = ResultUtilities.Try(() => { });
        Assert.True(r.IsSuccess());

        Result re = ResultUtilities.Try(() => throw new InvalidOperationException("a"));
        Assert.True(re.IsError(out IError? e));
        Assert.IsType<ExceptionError>(e);

        Result rf = ResultUtilities.Try(() => throw new InvalidOperationException("b"), ex => new SimpleError(ex.Message));
        Assert.True(rf.IsError(out IError? se));
        Assert.Equal("b", se!.Message);

        Result ra = await ResultUtilities.TryAsync(async () =>
        {
            await Task.Yield();
        });
        Assert.True(ra.IsSuccess());

        Result rae = await ResultUtilities.TryAsync(async () =>
        {
            await Task.Yield();
            throw new InvalidOperationException("c");
        });
        Assert.True(rae.IsError(out IError? aee));
        Assert.IsType<ExceptionError>(aee);
    }

    [Fact]
    public void ResultUtilities_Combine_three_four_and_mixed_SimpleError_aggregate()
    {
        Result<(int, string, bool)> t3 = ResultUtilities.Combine(Result.Success(1), Result.Success("a"), Result.Success(true));
        Assert.True(t3.IsSuccess(out (int, string, bool) v3));
        Assert.True(v3.Item3);

        Error e1 = new("a");
        Error e2 = new("b");
        Error e3 = new("c");
        Result<(int, string, bool)> f3 = ResultUtilities.Combine(Result.Error<int>(e1), Result.Success("x"), Result.Success(false));
        Assert.True(f3.IsError(out _));

        Result<(int, int, int, int)> t4 = ResultUtilities.Combine(Result.Success(1), Result.Success(2), Result.Success(3), Result.Success(4));
        Assert.True(t4.IsSuccess(out (int, int, int, int) v4));
        Assert.Equal(4, v4.Item4);

        SimpleError s1 = new("s1");
        SimpleError s2 = new("s2");
        Result<(int, string)> mix = ResultUtilities.Combine(Result.Error<int>(s1), Result.Error<string>(s2));
        Assert.True(mix.IsError(out IError? agg));
        Assert.IsType<SimpleError>(agg);
        Assert.Contains("2 total", agg!.Message, StringComparison.Ordinal);
    }
}
