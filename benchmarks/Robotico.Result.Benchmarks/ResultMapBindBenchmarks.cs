using BenchmarkDotNet.Attributes;
using Robotico.Result;
using Robotico.Result.Errors;

namespace Robotico.Result.Benchmarks;

/// <summary>
/// Benchmarks for <see cref="Result{T}"/> map/bind/match on success and error paths.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public sealed class ResultMapBindBenchmarks
{
    private static readonly Result<int> Success42 = Result.Success(42);
    private static readonly IError SampleError = new SimpleError("error");

    [Benchmark(Baseline = true)]
    public Result<int> Success_Map()
    {
        return Success42.Map(x => x + 1);
    }

    [Benchmark]
    public Result<string> Success_Map_ToString()
    {
        return Success42.Map(x => x.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    [Benchmark]
    public Result<int> Success_Bind()
    {
        return Success42.Bind(x => Result.Success(x + 1));
    }

    [Benchmark]
    public Result<int> Error_Map_Propagates()
    {
        Result<int> err = Result.Error<int>(SampleError);
        return err.Map(x => x + 1);
    }

    [Benchmark]
    public int Success_Match()
    {
        return Success42.Match(v => v, _ => 0);
    }
}
