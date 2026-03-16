using System.Diagnostics.CodeAnalysis;
using Robotico.Result.Errors;

namespace Robotico.Result;

/// <summary>
/// A result type representing either success (no data) or an error.
/// Immutable value type; use Map, Bind, MapError to transform.
/// </summary>
/// <remarks>
/// <para><b>When to use</b>: Use <see cref="Result"/> for operations that either succeed (e.g. save, delete) or fail with an error—no success value is needed.</para>
/// <para><b>Exception boundary</b>: Convert to exception only at process or API boundaries via <see cref="ExpectSuccess()"/> or <see cref="ExpectSuccess(Func{IError, Exception})"/>.</para>
/// <para><b>Allocation</b>: This type is a <c>readonly struct</c>; no heap allocation in sync paths. Async methods (e.g. <see cref="MapAsync{TMapped}(Func{Task{TMapped}})"/>) allocate only for the returned <see cref="Task"/>.</para>
/// </remarks>
[SuppressMessage("Naming", "CA1724:Type names should not match namespaces", Justification = "Result is the standard domain term for this pattern; renaming would harm discoverability. Namespace Robotico.Result is the package identity.")]
public readonly partial struct Result : IEquatable<Result>
{
    internal Result(IError? errorValue)
    {
        ErrorValue = errorValue;
    }

    private IError? ErrorValue { get; }

    private bool IsSuccessState => ErrorValue is null;

    /// <summary>Creates a successful result with no data.</summary>
    public static Result Success() => new(null);

    /// <summary>Creates a successful result with data.</summary>
    public static Result<TData> Success<TData>(TData data) => new(data, null);

    /// <summary>Creates a successful result with data and typed error.</summary>
    public static Result<TData, TError> Success<TData, TError>(TData data)
        where TError : class, IError =>
        new(data, null);

    /// <summary>Creates an error result.</summary>
    public static Result Error(IError error)
    {
        ArgumentNullException.ThrowIfNull(error, nameof(error));
        return new(error);
    }

    /// <summary>Creates an error result with data type.</summary>
    public static Result<TData> Error<TData>(IError error)
    {
        ArgumentNullException.ThrowIfNull(error, nameof(error));
        return new(default!, error);
    }

    /// <summary>Creates an error result with data and typed error.</summary>
    public static Result<TData, TError> Error<TData, TError>(TError error)
        where TError : class, IError
    {
        ArgumentNullException.ThrowIfNull(error, nameof(error));
        return new(default!, error);
    }

    /// <summary>Creates a validation error result (void).</summary>
    public static Result ValidationError(IReadOnlyDictionary<string, string[]> errors, string? message = null, string code = "VAL_FAILED") =>
        Error(new ValidationError(errors, message, code));

    /// <summary>Creates a validation error result with data type.</summary>
    public static Result<TData> ValidationError<TData>(IReadOnlyDictionary<string, string[]> errors, string? message = null, string code = "VAL_FAILED") =>
        Error<TData>(new ValidationError(errors, message, code));

    // --- Explicit conversions (named alternates for CA2225; no implicit operators for CA1000) ---

    /// <summary>Converts a <see cref="Result{TData}"/> to a void <see cref="Result"/> (drops the value).</summary>
    public static Result FromVoid<TData>(Result<TData> r)
    {
        if (r.IsSuccess(out _))
        {
            return Success();
        }

        r.IsError(out IError? e);
        return Error(e!);
    }

    /// <summary>Converts a <see cref="Result{TData}"/> to <see cref="Result{TData, IError}"/> (typed error as IError).</summary>
    public static Result<TData, IError> FromWithIError<TData>(Result<TData> r) =>
        r.IsSuccess(out TData? d, out IError? e) ? Success<TData, IError>(d) : Error<TData, IError>(e!);

    /// <summary>Converts a <see cref="Result{TData, TError}"/> to a void <see cref="Result"/> (drops the value).</summary>
    public static Result FromVoid<TData, TError>(Result<TData, TError> r)
        where TError : class, IError
    {
        if (r.IsSuccess(out _))
        {
            return Success();
        }

        r.IsError(out TError? e);
        return Error(e!);
    }

    /// <summary>Converts a <see cref="Result{TData, TError}"/> to <see cref="Result{TData}"/> (erases the specific error type to IError).</summary>
    public static Result<TData> From<TData, TError>(Result<TData, TError> r)
        where TError : class, IError =>
        r.IsSuccess(out TData? d, out TError? e) ? Success(d) : Error<TData>(e!);

    /// <summary>Converts a <see cref="Result{TData, TError}"/> to <see cref="Result{TData, IError}"/>.</summary>
    public static Result<TData, IError> FromWithIError<TData, TError>(Result<TData, TError> r)
        where TError : class, IError =>
        r.IsSuccess(out TData? d, out TError? e) ? Success<TData, IError>(d) : Error<TData, IError>(e!);

    // --- Map / Bind (industry naming) ---

    /// <summary>Maps success to a value; errors propagate.</summary>
    public Result<TMapped> Map<TMapped>(Func<TMapped> mapping)
    {
        ArgumentNullException.ThrowIfNull(mapping);
        return IsSuccessState ? new Result<TMapped>(mapping(), null) : new Result<TMapped>(default!, ErrorValue);
    }

    /// <summary>Binds (flatMap): maps success to a result; errors propagate.</summary>
    public Result<TMapped> Bind<TMapped>(Func<Result<TMapped>> binding)
    {
        ArgumentNullException.ThrowIfNull(binding);
        return IsSuccessState ? binding() : new Result<TMapped>(default!, ErrorValue);
    }

    /// <summary>Maps both value and error to new types.</summary>
    public Result<TMappedData, TMappedError> Map<TMappedData, TMappedError>(
        Func<TMappedData> valueMapping,
        Func<IError, TMappedError> errorMapping)
        where TMappedError : class, IError
    {
        ArgumentNullException.ThrowIfNull(valueMapping);
        ArgumentNullException.ThrowIfNull(errorMapping);
        return IsSuccessState
            ? new Result<TMappedData, TMappedError>(valueMapping(), null)
            : new Result<TMappedData, TMappedError>(default!, errorMapping(ErrorValue!));
    }

    /// <summary>Binds with error mapping.</summary>
    public Result<TMappedData, TMappedError> Bind<TMappedData, TMappedError>(
        Func<Result<TMappedData, TMappedError>> valueBinding,
        Func<IError, TMappedError> errorMapping)
        where TMappedError : class, IError
    {
        ArgumentNullException.ThrowIfNull(valueBinding);
        ArgumentNullException.ThrowIfNull(errorMapping);
        return IsSuccessState ? valueBinding() : new Result<TMappedData, TMappedError>(default!, errorMapping(ErrorValue!));
    }

    /// <summary>Async map.</summary>
    public async Task<Result<TMapped>> MapAsync<TMapped>(Func<Task<TMapped>> mapping)
    {
        ArgumentNullException.ThrowIfNull(mapping);
        return IsSuccessState ? new Result<TMapped>(await mapping().ConfigureAwait(false), null) : new Result<TMapped>(default!, ErrorValue);
    }

    /// <summary>Async bind.</summary>
    public async Task<Result<TMapped>> BindAsync<TMapped>(Func<Task<Result<TMapped>>> binding)
    {
        ArgumentNullException.ThrowIfNull(binding);
        return IsSuccessState ? await binding().ConfigureAwait(false) : new Result<TMapped>(default!, ErrorValue);
    }

    /// <summary>Returns true if success.</summary>
    public bool IsSuccess() => IsSuccessState;

    /// <summary>Returns true if error; sets <paramref name="error"/> when true.</summary>
    public bool IsError([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out IError? error)
    {
        error = ErrorValue;
        return !IsSuccessState;
    }

    /// <summary>Throws if error; use at process/API boundaries only.</summary>
    /// <remarks>This is the main boundary between the Result world and the exception world. Prefer keeping Results inside your domain and calling <c>ExpectSuccess</c> only at controllers, entry points, or when integrating with exception-based APIs.</remarks>
    public void ExpectSuccess() => ExpectSuccess(e => new ResultErrorException<IError>(e));

    /// <summary>Throws the exception from <paramref name="exceptionBuilder"/> if error.</summary>
    public void ExpectSuccess(Func<IError, Exception> exceptionBuilder)
    {
        ArgumentNullException.ThrowIfNull(exceptionBuilder);
        if (IsError(out IError? err))
        {
            throw exceptionBuilder(err);
        }
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Result other && Equals(other);

    /// <summary>Indicates whether this instance and another void result are equal. Success equals success; error equals error when the same error instance.</summary>
    public bool Equals(Result other) =>
        IsSuccessState == other.IsSuccessState && (IsSuccessState || ReferenceEquals(ErrorValue, other.ErrorValue));

    /// <inheritdoc />
    public override int GetHashCode() => IsSuccessState ? 0 : (ErrorValue?.GetHashCode() ?? 0);

    /// <summary>Returns true if left and right are equal.</summary>
    public static bool operator ==(Result left, Result right) => left.Equals(right);

    /// <summary>Returns true if left and right are not equal.</summary>
    public static bool operator !=(Result left, Result right) => !left.Equals(right);
}
