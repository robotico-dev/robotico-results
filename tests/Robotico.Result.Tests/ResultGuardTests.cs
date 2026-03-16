using System.Globalization;

namespace Robotico.Result.Tests;

/// <summary>
/// Tests that public API rejects null or invalid arguments with appropriate exceptions.
/// </summary>
public class ResultGuardTests
{
    // --- Result factories ---

    [Fact]
    public void Error_void_throws_when_error_is_null()
    {
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() => Result.Error(null!));
        Assert.Equal("error", ex.ParamName);
    }

    [Fact]
    public void Error_TData_throws_when_error_is_null()
    {
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() => Result.Error<int>(null!));
        Assert.Equal("error", ex.ParamName);
    }

    // --- Ensure ---

    [Fact]
    public void Ensure_throws_when_predicate_is_null()
    {
        Result<int> r = Result.Success(42);
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() =>
            r.Ensure((Func<int, bool>)null!, _ => new SimpleError("x")));
        Assert.Equal("predicate", ex.ParamName);
    }

    [Fact]
    public void Ensure_throws_when_errorFactory_is_null()
    {
        Result<int> r = Result.Success(42);
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() =>
            r.Ensure(_ => true, (Func<int, IError>)null!));
        Assert.Equal("errorFactory", ex.ParamName);
    }

    [Fact]
    public void Ensure_with_message_throws_when_predicate_is_null()
    {
        Result<int> r = Result.Success(42);
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() =>
            r.Ensure((Func<int, bool>)null!, "msg"));
        Assert.Equal("predicate", ex.ParamName);
    }

    // --- Match (Result<TData>) ---

    [Fact]
    public void Match_throws_when_onSuccess_is_null()
    {
        Result<int> r = Result.Success(1);
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() =>
            r.Match(null!, _ => "err"));
        Assert.Equal("onSuccess", ex.ParamName);
    }

    [Fact]
    public void Match_throws_when_onError_is_null()
    {
        Result<int> r = Result.Success(1);
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() =>
            r.Match(v => v.ToString(CultureInfo.InvariantCulture), null!));
        Assert.Equal("onError", ex.ParamName);
    }

    // --- Tap / Recover ---

    [Fact]
    public void Tap_throws_when_action_is_null()
    {
        Result<int> r = Result.Success(1);
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() => r.Tap(null!));
        Assert.Equal("action", ex.ParamName);
    }

    [Fact]
    public void Recover_throws_when_fallback_is_null()
    {
        Result<int> r = Result.Error<int>(new SimpleError("e"));
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() => r.Recover(null!));
        Assert.Equal("fallback", ex.ParamName);
    }

    // --- Collect / Sequence ---

    [Fact]
    public void Collect_throws_when_results_is_null()
    {
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() =>
            ResultExtensions.Collect<int>(null!));
        Assert.Equal("results", ex.ParamName);
    }

    [Fact]
    public void ChooseSuccessful_throws_when_results_is_null()
    {
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() =>
            ResultExtensions.ChooseSuccessful<int>(null!).ToList());
        Assert.Equal("results", ex.ParamName);
    }

    // --- ValidationError ---

    [Fact]
    public void ValidationError_constructor_throws_when_errors_is_null()
    {
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() =>
            new ValidationError(null!));
        Assert.Equal("errors", ex.ParamName);
    }

    [Fact]
    public void ValidationError_constructor_throws_when_errors_is_empty()
    {
        Dictionary<string, string[]> empty = new Dictionary<string, string[]>();
        ArgumentException ex = Assert.Throws<ArgumentException>(() =>
            new ValidationError(empty));
        Assert.Equal("errors", ex.ParamName);
    }

    [Fact]
    public void ValidationError_ForField_throws_when_fieldName_is_null()
    {
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() =>
            ValidationError.ForField(null!, "msg"));
        Assert.Equal("fieldName", ex.ParamName);
    }

    [Fact]
    public void ValidationError_ForField_throws_when_errorMessage_is_null()
    {
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() =>
            ValidationError.ForField("f", (string)null!));
        Assert.Equal("errorMessage", ex.ParamName);
    }

    [Fact]
    public void ValidationError_ForField_params_throws_when_errorMessages_is_empty()
    {
        ArgumentException ex = Assert.Throws<ArgumentException>(() =>
            ValidationError.ForField("f"));
        Assert.Equal("errorMessages", ex.ParamName);
    }

    [Fact]
    public void ValidationError_ForField_params_throws_when_errorMessages_array_is_null()
    {
        string[]? nullArray = null;
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() =>
            ValidationError.ForField("f", nullArray!));
        Assert.Equal("errorMessages", ex.ParamName);
    }

    // --- ResultUtilities.Try ---

    [Fact]
    public void Try_throws_when_func_is_null()
    {
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() =>
            ResultUtilities.Try<int>(null!));
        Assert.Equal("func", ex.ParamName);
    }

    [Fact]
    public void Try_void_throws_when_action_is_null()
    {
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() =>
            ResultUtilities.Try(null!));
        Assert.Equal("action", ex.ParamName);
    }
}
