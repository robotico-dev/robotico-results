using System.Collections.Immutable;

namespace Robotico.Result.Tests;

public class ResultCollectCombineTryTests
{
    [Fact]
    public void Collect_all_success_returns_immutable_array()
    {
        Result<int>[] results = new[] { Result.Success(1), Result.Success(2), Result.Success(3) };
        Result<ImmutableArray<int>> collected = results.Collect();
        Assert.True(collected.IsSuccess(out ImmutableArray<int> arr));
        Assert.Equal(3, arr.Length);
        Assert.Equal(1, arr[0]);
        Assert.Equal(2, arr[1]);
        Assert.Equal(3, arr[2]);
    }

    [Fact]
    public void Collect_first_error_wins()
    {
        SimpleError err = new SimpleError("first");
        Result<int>[] results = new[] { Result.Success(1), Result.Error<int>(err), Result.Success(3) };
        Result<ImmutableArray<int>> collected = results.Collect();
        Assert.True(collected.IsError(out IError? e));
        Assert.Same(err, e);
    }

    [Fact]
    public void Sequence_same_as_Collect()
    {
        Result<string>[] results = new[] { Result.Success("a"), Result.Success("b") };
        Result<ImmutableArray<string>> seq = results.Sequence();
        Assert.True(seq.IsSuccess(out ImmutableArray<string> arr));
        Assert.Equal(2, arr.Length);
    }

    [Fact]
    public void ChooseSuccessful_filters_errors()
    {
        Result<int>[] results = new[]
        {
            Result.Success(1),
            Result.Error<int>(new SimpleError("x")),
            Result.Success(3)
        };
        List<int> chosen = results.ChooseSuccessful().ToList();
        Assert.Equal(2, chosen.Count);
        Assert.Equal(1, chosen[0]);
        Assert.Equal(3, chosen[1]);
    }

    [Fact]
    public void Combine_two_success()
    {
        Result<(int, string)> c = ResultUtilities.Combine(Result.Success(1), Result.Success("a"));
        Assert.True(c.IsSuccess(out (int, string) t));
        Assert.Equal(1, t.Item1);
        Assert.Equal("a", t.Item2);
    }

    [Fact]
    public void Combine_one_error_returns_aggregate()
    {
        SimpleError err = new SimpleError("e");
        Result<(int, string)> c = ResultUtilities.Combine(Result.Error<int>(err), Result.Success("a"));
        Assert.True(c.IsError(out IError? e));
        Assert.NotNull(e);
    }

    [Fact]
    public void Combine_both_fail_returns_aggregate_with_both_errors()
    {
        Error err1 = new Error("first");
        Error err2 = new Error("second");
        Result<(int, string)> c = ResultUtilities.Combine(Result.Error<int>(err1), Result.Error<string>(err2));
        Assert.True(c.IsError(out IError? e));
        Assert.NotNull(e);
        Assert.IsType<AggregateError>(e);
        AggregateError agg = (AggregateError)e!;
        Assert.Equal(2, agg.Errors.Length);
        Assert.Contains(agg.Errors, x => x.Message == "first");
        Assert.Contains(agg.Errors, x => x.Message == "second");
    }

    [Fact]
    public void Combine_enumerable_all_success_returns_immutable_array()
    {
        Result<int>[] results = new[] { Result.Success(1), Result.Success(2), Result.Success(3) };
        Result<ImmutableArray<int>> combined = ResultUtilities.Combine(results);
        Assert.True(combined.IsSuccess(out ImmutableArray<int> arr));
        Assert.Equal(3, arr.Length);
        Assert.Equal(1, arr[0]);
        Assert.Equal(2, arr[1]);
        Assert.Equal(3, arr[2]);
    }

    [Fact]
    public void Combine_enumerable_first_error_wins()
    {
        SimpleError err = new SimpleError("first");
        Result<int>[] results = new[] { Result.Success(1), Result.Error<int>(err), Result.Success(3) };
        Result<ImmutableArray<int>> combined = ResultUtilities.Combine(results);
        Assert.True(combined.IsError(out IError? e));
        Assert.Same(err, e);
    }

    [Fact]
    public void Try_success_returns_value()
    {
        Result<int> r = ResultUtilities.Try(() => 42);
        Assert.True(r.IsSuccess(out int v));
        Assert.Equal(42, v);
    }

    [Fact]
    public void Try_exception_returns_ExceptionError()
    {
        Result<int> r = ResultUtilities.Try<int>(() => throw new InvalidOperationException("bad"));
        Assert.True(r.IsError(out IError? e));
        Assert.IsType<ExceptionError>(e);
        Assert.Equal("bad", e!.Message);
    }

    [Fact]
    public async Task TryAsync_success()
    {
        Result<int> r = await ResultUtilities.TryAsync(async () => await Task.FromResult(10));
        Assert.True(r.IsSuccess(out int v));
        Assert.Equal(10, v);
    }
}
