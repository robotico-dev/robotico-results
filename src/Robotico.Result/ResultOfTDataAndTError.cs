using System.Diagnostics.CodeAnalysis;
using Robotico.Result.Errors;

namespace Robotico.Result;

/// <summary>
/// A result type representing either success with data or a strongly-typed error.
/// Immutable value type; use Map, Bind, MapError to transform.
/// </summary>
[SuppressMessage("Naming", "CA1724:Type names should not match namespaces", Justification = "Result is the standard domain term for this pattern; renaming would harm discoverability. Namespace Robotico.Result is the package identity.")]
public readonly partial struct Result<TData, TError> : IEquatable<Result<TData, TError>>
    where TError : class, IError
{
    internal Result(TData successValue, TError? errorValue)
    {
        SuccessValue = successValue;
        ErrorValue = errorValue;
    }

    private TData SuccessValue { get; }

    private TError? ErrorValue { get; }

    private bool IsSuccessState => ErrorValue is null;

    /// <summary>Maps the success value to a new type; errors propagate.</summary>
    public Result<TMapped, TError> Map<TMapped>(Func<TData, TMapped> mapping)
    {
        ArgumentNullException.ThrowIfNull(mapping);
        return IsSuccessState ? new Result<TMapped, TError>(mapping(SuccessValue), null) : new Result<TMapped, TError>(default!, ErrorValue);
    }

    /// <summary>Binds: maps the success value to a result; errors propagate.</summary>
    public Result<TMapped, TError> Bind<TMapped>(Func<TData, Result<TMapped, TError>> binding)
    {
        ArgumentNullException.ThrowIfNull(binding);
        return IsSuccessState ? binding(SuccessValue) : new Result<TMapped, TError>(default!, ErrorValue);
    }

    /// <summary>Maps the error to a new error type.</summary>
    public Result<TData, TMappedError> MapError<TMappedError>(Func<TError, TMappedError> errorMapping)
        where TMappedError : class, IError
    {
        ArgumentNullException.ThrowIfNull(errorMapping);
        return IsSuccessState ? new Result<TData, TMappedError>(SuccessValue, null) : new Result<TData, TMappedError>(default!, errorMapping(ErrorValue!));
    }

    /// <summary>Async map.</summary>
    public async Task<Result<TMapped, TError>> MapAsync<TMapped>(Func<TData, Task<TMapped>> mapping)
    {
        ArgumentNullException.ThrowIfNull(mapping);
        return IsSuccessState ? new Result<TMapped, TError>(await mapping(SuccessValue).ConfigureAwait(false), null) : new Result<TMapped, TError>(default!, ErrorValue);
    }

    /// <summary>Async bind.</summary>
    public async Task<Result<TMapped, TError>> BindAsync<TMapped>(Func<TData, Task<Result<TMapped, TError>>> binding)
    {
        ArgumentNullException.ThrowIfNull(binding);
        return IsSuccessState ? await binding(SuccessValue).ConfigureAwait(false) : new Result<TMapped, TError>(default!, ErrorValue);
    }

    /// <summary>Async map error.</summary>
    public async Task<Result<TData, TMappedError>> MapErrorAsync<TMappedError>(Func<TError, Task<TMappedError>> errorMapping)
        where TMappedError : class, IError
    {
        ArgumentNullException.ThrowIfNull(errorMapping);
        return IsSuccessState ? new Result<TData, TMappedError>(SuccessValue, null) : new Result<TData, TMappedError>(default!, await errorMapping(ErrorValue!).ConfigureAwait(false));
    }

    /// <summary>Returns true if success.</summary>
    public bool IsSuccess() => IsSuccessState;

    /// <summary>Returns true if success and sets <paramref name="data"/>.</summary>
    public bool IsSuccess([MaybeNullWhen(false)] out TData data)
    {
        data = SuccessValue!;
        return IsSuccessState;
    }

    /// <summary>Returns true if success and sets <paramref name="data"/> and <paramref name="error"/>.</summary>
    public bool IsSuccess([MaybeNullWhen(false)] out TData data, [NotNullWhen(false)] out TError? error)
    {
        data = SuccessValue!;
        error = ErrorValue;
        return IsSuccessState;
    }

    /// <summary>Returns true if error.</summary>
    public bool IsError() => !IsSuccessState;

    /// <summary>Returns true if error and sets <paramref name="error"/>.</summary>
    public bool IsError([NotNullWhen(true)] out TError? error)
    {
        error = ErrorValue;
        return !IsSuccessState;
    }

    /// <summary>Returns true if error and sets both out parameters.</summary>
    public bool IsError([MaybeNullWhen(true)] out TData data, [NotNullWhen(true)] out TError? error)
    {
        data = SuccessValue!;
        error = ErrorValue;
        return !IsSuccessState;
    }

    /// <summary>Gets the value if success, otherwise default.</summary>
    public TData? GetValue() => IsSuccess(out TData? d) ? d : default;

    /// <summary>Throws if error; returns the value. Use at process/API boundaries only.</summary>
    public TData ExpectSuccess() => ExpectSuccess(e => new ResultErrorException<TError>(e));

    /// <summary>Throws the exception from <paramref name="exceptionBuilder"/> if error; returns the value otherwise.</summary>
    public TData ExpectSuccess(Func<TError, Exception> exceptionBuilder)
    {
        ArgumentNullException.ThrowIfNull(exceptionBuilder);
        if (IsError(out _, out TError? err))
        {
            throw exceptionBuilder(err);
        }

        return SuccessValue!;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Result<TData, TError> other && Equals(other);

    /// <summary>Indicates whether this instance and another result are equal. Success equals success when values are equal; error equals error when the same error instance.</summary>
    public bool Equals(Result<TData, TError> other)
    {
        if (IsSuccessState != other.IsSuccessState)
        {
            return false;
        }

        return IsSuccessState
            ? EqualityComparer<TData>.Default.Equals(SuccessValue, other.SuccessValue)
            : ReferenceEquals(ErrorValue, other.ErrorValue);
    }

    /// <inheritdoc />
    public override int GetHashCode() =>
        IsSuccessState ? (SuccessValue?.GetHashCode() ?? 0) : (ErrorValue?.GetHashCode() ?? 0);

    /// <summary>Returns true if left and right are equal.</summary>
    public static bool operator ==(Result<TData, TError> left, Result<TData, TError> right) => left.Equals(right);

    /// <summary>Returns true if left and right are not equal.</summary>
    public static bool operator !=(Result<TData, TError> left, Result<TData, TError> right) => !left.Equals(right);
}
