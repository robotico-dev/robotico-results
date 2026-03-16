using System.Globalization;

namespace Robotico.Result.Tests;

public class ResultBasicsTests
{
    [Fact]
    public void Success_void_IsSuccess()
    {
        Result r = Result.Success();
        Assert.True(r.IsSuccess());
        Assert.False(r.IsError(out _));
    }

    [Fact]
    public void Error_void_IsError()
    {
        SimpleError err = new("fail");
        Result r = Result.Error(err);
        Assert.True(r.IsError(out IError? e));
        Assert.Same(err, e);
        Assert.False(r.IsSuccess());
    }

    [Fact]
    public void Success_TData_IsSuccess_and_value()
    {
        Result<int> r = Result.Success(42);
        Assert.True(r.IsSuccess(out int value));
        Assert.Equal(42, value);
        Assert.Equal(42, r.GetValue());
    }

    [Fact]
    public void Error_TData_IsError()
    {
        SimpleError err = new("oops");
        Result<int> r = Result.Error<int>(err);
        Assert.True(r.IsError(out IError? e));
        Assert.Same(err, e);
        Assert.False(r.IsSuccess());
    }

    [Fact]
    public void Map_preserves_success()
    {
        Result<int> r = Result.Success(3);
        Result<string> mapped = r.Map(x => (x * 2).ToString(CultureInfo.InvariantCulture));
        Assert.True(mapped.IsSuccess(out string? s));
        Assert.Equal("6", s);
    }

    [Fact]
    public void Map_propagates_error()
    {
        SimpleError err = new("x");
        Result<int> r = Result.Error<int>(err);
        Result<string> mapped = r.Map(x => x.ToString(CultureInfo.InvariantCulture));
        Assert.True(mapped.IsError(out IError? e));
        Assert.Same(err, e);
    }

    [Fact]
    public void Bind_chains_success()
    {
        Result<int> r = Result.Success(2);
        Result<string> bound = r.Bind(x => Result.Success((x + 1).ToString(CultureInfo.InvariantCulture)));
        Assert.True(bound.IsSuccess(out string? s));
        Assert.Equal("3", s);
    }

    [Fact]
    public void Bind_propagates_error()
    {
        SimpleError err = new("y");
        Result<int> r = Result.Error<int>(err);
        Result<string> bound = r.Bind(x => Result.Success(x.ToString(CultureInfo.InvariantCulture)));
        Assert.True(bound.IsError(out IError? e));
        Assert.Same(err, e);
    }

    [Fact]
    public void MapError_transforms_error()
    {
        Result<int> r = Result.Error<int>(new SimpleError("a"));
        Result<int, SimpleError> mapped = r.MapError<SimpleError>(e => new(e.Message + "!", e.Code, e.Severity));
        Assert.True(mapped.IsError(out SimpleError? e));
        Assert.Equal("a!", e!.Message);
    }

    [Fact]
    public void ExpectSuccess_throws_on_error()
    {
        SimpleError err = new("z");
        Result<int> r = Result.Error<int>(err);
        ResultErrorException<IError> ex = Assert.Throws<ResultErrorException<IError>>(() => r.ExpectSuccess());
        Assert.Same(err, ex.Error);
    }

    [Fact]
    public void Result_Success_from_value()
    {
        Result<int> r = Result.Success(10);
        Assert.True(r.IsSuccess(out int v));
        Assert.Equal(10, v);
    }

    [Fact]
    public void Result_Error_from_IError()
    {
        SimpleError err = new("e");
        Result<int> r = Result.Error<int>(err);
        Assert.True(r.IsError(out IError? e));
        Assert.Same(err, e);
    }

    [Fact]
    public void ValidationError_factory()
    {
        Result r = Result.ValidationError(new Dictionary<string, string[]> { ["Email"] = ["Invalid format"] });
        Assert.True(r.IsError(out IError? e));
        Assert.IsType<ValidationError>(e);
        Assert.Equal("VAL_FAILED", e!.Code);
    }
}
