namespace Robotico.Result.Tests;

public class ResultErrorTypesTests
{
    // --- MapError extension (Result<TData> IError -> IError) ---

    [Fact]
    public void MapError_extension_success_returns_same()
    {
        Result<int> r = Result.Success(5);
        Result<int> mapped = Result.From(r.MapError(e => new SimpleError(e.Message + "_mapped")));
        Assert.True(mapped.IsSuccess(out int v));
        Assert.Equal(5, v);
    }

    [Fact]
    public void MapError_extension_error_transforms_error()
    {
        SimpleError err = new SimpleError("original");
        Result<int> r = Result.Error<int>(err);
        Result<int> mapped = Result.From(r.MapError(e => new SimpleError(e.Message + "_mapped", "MAPPED")));
        Assert.True(mapped.IsError(out IError? e));
        Assert.Equal("original_mapped", e!.Message);
        Assert.Equal("MAPPED", e.Code);
    }

    // --- ValidationError implicit to Result<TData> ---

    [Fact]
    public void Result_Error_from_ValidationError()
    {
        ValidationError validationErr = ValidationError.ForField("Email", "Invalid format");
        Result<int> r = Result.Error<int>(validationErr);
        Assert.True(r.IsError(out IError? e));
        Assert.Same(validationErr, e);
        Assert.Equal("VAL_EMAIL", e!.Code);
    }

    // --- DomainError<TCode> ---

    private enum TestDomainCode { NotFound, Unauthorized }

    private sealed class TestDomainError : DomainError<TestDomainCode>
    {
        public TestDomainError(TestDomainCode code, string message) : base(code, message) { }
    }

    [Fact]
    public void DomainError_Code_returns_domain_code_string()
    {
        TestDomainError err = new TestDomainError(TestDomainCode.NotFound, "Resource missing");
        Assert.Equal("NotFound", err.Code);
        Assert.Equal(TestDomainCode.NotFound, err.DomainCode);
    }

    [Fact]
    public void Result_with_DomainError_roundtrip()
    {
        TestDomainError err = new TestDomainError(TestDomainCode.Unauthorized, "Access denied");
        Result<int, TestDomainError> r = Result.Error<int, TestDomainError>(err);
        Assert.True(r.IsError(out TestDomainError? e));
        Assert.Same(err, e);
        Assert.Equal("Unauthorized", e!.Code);
    }

    // --- AggregateError<T> ---

    [Fact]
    public void AggregateError_of_T_holds_typed_errors()
    {
        Error e1 = new Error("a");
        Error e2 = new Error("b");
        AggregateError<Error> agg = new AggregateError<Error>("Combined", new[] { e1, e2 });
        Assert.Equal(2, agg.Errors.Length);
        Assert.Same(e1, agg.Errors[0]);
        Assert.Same(e2, agg.Errors[1]);
        Assert.Equal("Combined", agg.Message);
    }

    [Fact]
    public void GetErrorMessages_flattens_AggregateError()
    {
        AggregateError agg = new AggregateError("Top", new[] { new Error("inner1"), new Error("inner2") });
        List<string> messages = agg.GetErrorMessages().ToList();
        Assert.Equal(2, messages.Count);
        Assert.Contains("inner1", messages);
        Assert.Contains("inner2", messages);
    }
}
