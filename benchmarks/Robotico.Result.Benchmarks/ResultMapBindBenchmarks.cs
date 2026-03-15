using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;
using Robotico.Result;
using Robotico.Result.Errors;

namespace Robotico.Result.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class ResultMapBindBenchmarks
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
        return Success42.Map(x => x.ToString());
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

[MemoryDiagnoser]
[ShortRunJob]
public class ResultCollectBenchmarks
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

[MemoryDiagnoser]
[ShortRunJob]
public class ResultCombineBenchmarks
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

[MemoryDiagnoser]
[ShortRunJob]
public class ResultTryEnsureBenchmarks
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
