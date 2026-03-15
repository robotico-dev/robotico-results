namespace Robotico.Result.Tests;

/// <summary>
/// Property-style and parameterized tests for Result laws using [Theory] and [InlineData].
/// </summary>
public class ResultLawsTheoryTests
{
    [Theory]
    [InlineData(42)]
    [InlineData(0)]
    [InlineData(-1)]
    public void Map_identity_success_preserves_any_value(int value)
    {
        Result<int> r = Result.Success(value);
        Result<int> mapped = r.Map(x => x);
        Assert.True(mapped.IsSuccess(out int result));
        Assert.Equal(value, result);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("")]
    public void Map_identity_success_preserves_string_value(string value)
    {
        Result<string> r = Result.Success(value);
        Result<string> mapped = r.Map(x => x);
        Assert.True(mapped.IsSuccess(out string? result));
        Assert.Equal(value, result);
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(10, 20, 30)]
    public void Bind_Success_identity_success_preserves_value(int a, int b, int expectedSum)
    {
        Result<int> r = Result.Success(a);
        Result<int> bound = r.Bind(x => Result.Success(x + b));
        Assert.True(bound.IsSuccess(out int value));
        Assert.Equal(expectedSum, value);
    }

    [Theory]
    [InlineData(7)]
    [InlineData(0)]
    [InlineData(-5)]
    public void RecoverWith_success_returns_same_value(int value)
    {
        Result<int> r = Result.Success(value);
        Result<int> recovered = r.RecoverWith(99);
        Assert.True(recovered.IsSuccess(out int result));
        Assert.Equal(value, result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(99)]
    [InlineData(-1)]
    public void RecoverWith_error_returns_fallback_value(int fallback)
    {
        Result<int> r = Result.Error<int>(new SimpleError("e"));
        Result<int> recovered = r.RecoverWith(fallback);
        Assert.True(recovered.IsSuccess(out int result));
        Assert.Equal(fallback, result);
    }

    [Theory]
    [InlineData(2, 3, 6)]
    [InlineData(1, 10, 10)]
    public void Map_composition_success(int initial, int scale, int expected)
    {
        Result<int> r = Result.Success(initial);
        Result<int> mapped = r.Map(x => x * scale);
        Assert.True(mapped.IsSuccess(out int value));
        Assert.Equal(expected, value);
    }
}
