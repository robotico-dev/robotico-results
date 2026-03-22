using BenchmarkDotNet.Attributes;
using Robotico.Result;

namespace Robotico.Result.Benchmarks;

/// <summary>
/// Benchmarks for <see cref="ResultUtilities.Try{T}(Func{T})"/> and <see cref="ResultExtensions.Ensure{T}(Result{T}, Func{T, bool}, string)"/>.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public sealed class ResultTryEnsureBenchmarks
{
    [Benchmark]
    public Result<int> Try_Success()
    {
        return ResultUtilities.Try(() => 42);
    }

    [Benchmark]
    public Result<int> Try_Throws()
    {
        return ResultUtilities.Try<int>(() => throw new InvalidOperationException("bench"));
    }

    [Benchmark]
    public Result<int> Ensure_Pass()
    {
        return Result.Success(42).Ensure(x => x > 0, "must be positive");
    }

    [Benchmark]
    public Result<int> Ensure_Fail()
    {
        return Result.Success(42).Ensure(x => x < 0, "must be negative");
    }
}
