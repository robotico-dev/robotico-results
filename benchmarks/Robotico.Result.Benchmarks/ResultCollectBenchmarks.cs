using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;
using Robotico.Result;
using Robotico.Result.Errors;

namespace Robotico.Result.Benchmarks;

/// <summary>
/// Benchmarks for <see cref="ResultExtensions.Collect{T}(Result{T}[])"/>.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public sealed class ResultCollectBenchmarks
{
    [Params(10, 100)]
    public int N { get; set; }

    [Benchmark]
    public Result<ImmutableArray<int>> Collect_AllSuccess()
    {
        Result<int>[] results = Enumerable.Range(0, N).Select(i => Result.Success(i)).ToArray();
        return results.Collect();
    }

    [Benchmark]
    public Result<ImmutableArray<int>> Collect_FirstError()
    {
        Result<int>[] results = Enumerable.Range(0, N)
            .Select(i => i == N / 2 ? Result.Error<int>(new SimpleError("e")) : Result.Success(i))
            .ToArray();
        return results.Collect();
    }
}
