using BenchmarkDotNet.Attributes;
using Robotico.Result;
using Robotico.Result.Errors;

namespace Robotico.Result.Benchmarks;

/// <summary>
/// Benchmarks for <see cref="ResultUtilities.Combine{T1,T2}"/> overloads.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public sealed class ResultCombineBenchmarks
{
    private static readonly Result<int> R1 = Result.Success(1);
    private static readonly Result<int> R2 = Result.Success(2);
    private static readonly Result<string> R3 = Result.Success("a");
    private static readonly Result<int> E1 = Result.Error<int>(new SimpleError("e1"));

    [Benchmark]
    public Result<(int, int)> Combine_Two_Success()
    {
        return ResultUtilities.Combine(R1, R2);
    }

    [Benchmark]
    public Result<(int, string)> Combine_Two_Mixed()
    {
        return ResultUtilities.Combine(R1, R3);
    }

    [Benchmark]
    public Result<(int, int, string, int)> Combine_Four_Success()
    {
        return ResultUtilities.Combine(R1, R2, R3, R1);
    }

    [Benchmark]
    public Result<(int, int)> Combine_Two_OneError()
    {
        return ResultUtilities.Combine(E1, R2);
    }
}
