namespace Robotico.Result.Tests;

public class ResultTaskExtensionsTests
{
    [Fact]
    public async Task MapAsync_TaskOfResultT()
    {
        Task<Result<int>> t = Task.FromResult(Result.Success(5));
        Result<string> mapped = await t.MapAsync(x => (x * 2).ToString());
        Assert.True(mapped.IsSuccess(out string? s));
        Assert.Equal("10", s);
    }

    [Fact]
    public async Task BindAsync_TaskOfResultT()
    {
        Task<Result<int>> t = Task.FromResult(Result.Success(4));
        Result<string> bound = await t.BindAsync(x => Task.FromResult(Result.Success(x.ToString())));
        Assert.True(bound.IsSuccess(out string? s));
        Assert.Equal("4", s);
    }

    [Fact]
    public async Task ExpectSuccessAsync_throws_on_error()
    {
        Task<Result<int>> t = Task.FromResult(Result.Error<int>(new SimpleError("async")));
        await Assert.ThrowsAsync<ResultErrorException<IError>>(async () => await t.ExpectSuccessAsync());
    }

    [Fact]
    public async Task GetValueAsync_returns_value()
    {
        Task<Result<int>> t = Task.FromResult(Result.Success(99));
        int? v = await t.GetValueAsync();
        Assert.Equal(99, v);
    }

    [Fact]
    public async Task MatchAsync_TaskOfResultTData_success()
    {
        Task<Result<int>> t = Task.FromResult(Result.Success(42));
        int x = await t.MatchAsync(v => v * 2, _ => -1);
        Assert.Equal(84, x);
    }

    [Fact]
    public async Task MatchAsync_TaskOfResultTData_error()
    {
        Task<Result<int>> t = Task.FromResult(Result.Error<int>(new SimpleError("err")));
        int x = await t.MatchAsync(_ => 0, e => e.Message.Length);
        Assert.Equal(3, x);
    }

    [Fact]
    public async Task MatchAsync_TaskOfResultTDataTError_success()
    {
        Task<Result<int, SimpleError>> t = Task.FromResult(Result.Success<int, SimpleError>(7));
        string s = await t.MatchAsync(v => v.ToString(), _ => "error");
        Assert.Equal("7", s);
    }

    [Fact]
    public async Task MatchAsync_TaskOfResultTDataTError_error()
    {
        Task<Result<int, SimpleError>> t = Task.FromResult(Result.Error<int, SimpleError>(new SimpleError("fail")));
        string s = await t.MatchAsync(_ => "ok", e => e.Message);
        Assert.Equal("fail", s);
    }
}
