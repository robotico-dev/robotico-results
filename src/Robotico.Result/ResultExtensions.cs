using System.Collections.Immutable;
using Robotico.Result.Errors;

namespace Robotico.Result;

/// <summary>
/// Extension methods for Result: Match, Tap, TapError, Recover, Ensure, Collect, Sequence, GetValueOrDefault.
/// Includes void Result and Result&lt;TData, TError&gt; overloads for full API consistency.
/// </summary>
public static class ResultExtensions
{
    // --- Match (void Result) ---

    /// <summary>Pattern match on void Result: executes one of two functions based on success/error.</summary>
    public static TResult Match<TResult>(this Result r, Func<TResult> onSuccess, Func<IError, TResult> onError)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onError);
        if (r.IsSuccess())
        {
            return onSuccess();
        }
        r.IsError(out IError? err);
        return onError(err!);
    }

    /// <summary>Pattern match on void Result: executes one of two actions.</summary>
    public static void Match(this Result r, Action onSuccess, Action<IError> onError)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onError);
        if (r.IsSuccess())
        {
            onSuccess();
        }
        else
        {
            r.IsError(out IError? err);
            onError(err!);
        }
    }

    // --- Tap / TapError / RecoverWith (void Result) ---

    /// <summary>Runs a side effect on success of void Result; returns the same result.</summary>
    public static Result Tap(this Result r, Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (r.IsSuccess())
        {
            action();
        }

        return r;
    }

    /// <summary>Runs a side effect on error of void Result; returns the same result.</summary>
    public static Result TapError(this Result r, Action<IError> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (r.IsError(out IError? err))
        {
            action(err!);
        }

        return r;
    }

    /// <summary>Returns this result if success; otherwise returns the fallback result.</summary>
    public static Result RecoverWith(this Result r, Result fallback) =>
        r.IsSuccess() ? r : fallback;

    // --- Match (Result<TData>) ---

    /// <summary>Pattern match: executes one of two functions based on success/error.</summary>
    public static TResult Match<TData, TResult>(this Result<TData> r, Func<TData, TResult> onSuccess, Func<IError, TResult> onError)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onError);
        return r.IsSuccess(out TData? data, out IError? err) ? onSuccess(data!) : onError(err!);
    }

    /// <summary>Pattern match: executes one of two actions.</summary>
    public static void Match<TData>(this Result<TData> r, Action<TData> onSuccess, Action<IError> onError)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onError);
        if (r.IsSuccess(out TData? data, out IError? err))
        {
            onSuccess(data!);
        }
        else
        {
            onError(err!);
        }
    }

    /// <summary>Pattern match for Result&lt;TData, TError&gt;.</summary>
    public static TResult Match<TData, TError, TResult>(this Result<TData, TError> r, Func<TData, TResult> onSuccess, Func<TError, TResult> onError)
        where TError : class, IError
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onError);
        return r.IsSuccess(out TData? data, out TError? err) ? onSuccess(data!) : onError(err!);
    }

    // --- Tap / TapError (Result<TData, TError>) ---

    /// <summary>Runs a side effect on success; returns the same result. For Result&lt;TData, TError&gt;.</summary>
    public static Result<TData, TError> Tap<TData, TError>(this Result<TData, TError> r, Action<TData> action)
        where TError : class, IError
    {
        ArgumentNullException.ThrowIfNull(action);
        if (r.IsSuccess(out TData? data))
        {
            action(data!);
        }

        return r;
    }

    /// <summary>Runs a side effect on error; returns the same result. For Result&lt;TData, TError&gt;.</summary>
    public static Result<TData, TError> TapError<TData, TError>(this Result<TData, TError> r, Action<TError> action)
        where TError : class, IError
    {
        ArgumentNullException.ThrowIfNull(action);
        if (r.IsError(out TError? err))
        {
            action(err!);
        }

        return r;
    }

    /// <summary>Async tap on success. For Result&lt;TData, TError&gt;.</summary>
    public static async Task<Result<TData, TError>> TapAsync<TData, TError>(this Result<TData, TError> r, Func<TData, Task> action)
        where TError : class, IError
    {
        ArgumentNullException.ThrowIfNull(action);
        if (r.IsSuccess(out TData? data))
        {
            await action(data!).ConfigureAwait(false);
        }

        return r;
    }

    /// <summary>Async tap on error. For Result&lt;TData, TError&gt;.</summary>
    public static async Task<Result<TData, TError>> TapErrorAsync<TData, TError>(this Result<TData, TError> r, Func<TError, Task> action)
        where TError : class, IError
    {
        ArgumentNullException.ThrowIfNull(action);
        if (r.IsError(out TError? err))
        {
            await action(err!).ConfigureAwait(false);
        }

        return r;
    }

    // --- Recover (Result<TData, TError>) ---

    /// <summary>Returns the result or a success with the fallback value from the error. For Result&lt;TData, TError&gt;.</summary>
    public static Result<TData, TError> Recover<TData, TError>(this Result<TData, TError> r, Func<TError, TData> fallback)
        where TError : class, IError
    {
        ArgumentNullException.ThrowIfNull(fallback);
        if (r.IsSuccess(out TData? data))
        {
            return r;
        }

        r.IsError(out TError? err);
        return Result.Success<TData, TError>(fallback(err!));
    }

    /// <summary>Returns the result or a success with the constant fallback value. For Result&lt;TData, TError&gt;.</summary>
    public static Result<TData, TError> RecoverWith<TData, TError>(this Result<TData, TError> r, TData fallbackValue)
        where TError : class, IError =>
        r.IsSuccess(out _) ? r : Result.Success<TData, TError>(fallbackValue);

    /// <summary>Async recover. For Result&lt;TData, TError&gt;.</summary>
    public static async Task<Result<TData, TError>> RecoverAsync<TData, TError>(this Result<TData, TError> r, Func<TError, Task<TData>> fallback)
        where TError : class, IError
    {
        ArgumentNullException.ThrowIfNull(fallback);
        if (r.IsSuccess(out _))
        {
            return r;
        }

        r.IsError(out TError? err);
        TData value = await fallback(err!).ConfigureAwait(false);
        return Result.Success<TData, TError>(value);
    }

    // --- Tap (Result<TData>) ---

    /// <summary>Runs a side effect on success; returns the same result.</summary>
    public static Result<TData> Tap<TData>(this Result<TData> r, Action<TData> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (r.IsSuccess(out TData? data))
        {
            action(data!);
        }

        return r;
    }

    /// <summary>Runs a side effect on error; returns the same result.</summary>
    public static Result<TData> TapError<TData>(this Result<TData> r, Action<IError> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (r.IsError(out IError? err))
        {
            action(err!);
        }

        return r;
    }

    /// <summary>Async tap on success.</summary>
    public static async Task<Result<TData>> TapAsync<TData>(this Result<TData> r, Func<TData, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (r.IsSuccess(out TData? data))
        {
            await action(data!).ConfigureAwait(false);
        }

        return r;
    }

    /// <summary>Async tap on error.</summary>
    public static async Task<Result<TData>> TapErrorAsync<TData>(this Result<TData> r, Func<IError, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (r.IsError(out IError? err))
        {
            await action(err!).ConfigureAwait(false);
        }

        return r;
    }

    // --- Recover ---

    /// <summary>Returns the result or a success with the fallback value from the error.</summary>
    public static Result<TData> Recover<TData>(this Result<TData> r, Func<IError, TData> fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);
        if (r.IsSuccess(out TData? data))
        {
            return r;
        }

        r.IsError(out IError? err);
        return Result.Success(fallback(err!));
    }

    /// <summary>Returns the result or a success with the constant fallback value.</summary>
    public static Result<TData> RecoverWith<TData>(this Result<TData> r, TData fallbackValue) =>
        r.IsSuccess(out _) ? r : Result.Success(fallbackValue);

    /// <summary>Returns this result if success; otherwise returns the fallback result.</summary>
    public static Result<TData> RecoverWith<TData>(this Result<TData> r, Result<TData> fallback) =>
        r.IsSuccess(out _) ? r : fallback;

    /// <summary>Returns this result if success; otherwise returns the fallback result. For Result&lt;TData, TError&gt;.</summary>
    public static Result<TData, TError> RecoverWith<TData, TError>(this Result<TData, TError> r, Result<TData, TError> fallback)
        where TError : class, IError =>
        r.IsSuccess(out _) ? r : fallback;

    /// <summary>Async recover.</summary>
    public static async Task<Result<TData>> RecoverAsync<TData>(this Result<TData> r, Func<IError, Task<TData>> fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);
        if (r.IsSuccess(out _))
        {
            return r;
        }

        r.IsError(out IError? err);
        TData value = await fallback(err!).ConfigureAwait(false);
        return Result.Success(value);
    }

    // --- Ensure ---

    /// <summary>Ensures a predicate holds on the success value; otherwise returns an error from the factory.</summary>
    public static Result<TData> Ensure<TData>(this Result<TData> r, Func<TData, bool> predicate, Func<TData, IError> errorFactory)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(errorFactory);
        if (r.IsError(out _))
        {
            return r;
        }

        r.IsSuccess(out TData? value);
        return predicate(value!) ? r : Result.Error<TData>(errorFactory(value!));
    }

    /// <summary>Ensures a predicate holds; uses a simple error message if not.</summary>
    public static Result<TData> Ensure<TData>(this Result<TData> r, Func<TData, bool> predicate, string errorMessage) =>
        r.Ensure(predicate, _ => new SimpleError(errorMessage));

    /// <summary>Ensures a predicate holds on the success value; otherwise returns an error from the factory. For Result&lt;TData, TError&gt;.</summary>
    public static Result<TData, TError> Ensure<TData, TError>(this Result<TData, TError> r, Func<TData, bool> predicate, Func<TData, TError> errorFactory)
        where TError : class, IError
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(errorFactory);
        if (r.IsError(out _))
        {
            return r;
        }

        r.IsSuccess(out TData? value);
        return predicate(value!) ? r : Result.Error<TData, TError>(errorFactory(value!));
    }

    // --- GetValueOrDefault ---

    /// <summary>Gets the value or default if error.</summary>
    public static TData? GetValueOrDefault<TData>(this Result<TData> r) =>
        r.IsSuccess(out TData? d) ? d : default;

    // --- Collect / Sequence ---

    /// <summary>Collects an enumerable of results into a single result of an immutable array. First error wins.</summary>
    public static Result<ImmutableArray<TData>> Collect<TData>(this IEnumerable<Result<TData>> results)
    {
        ArgumentNullException.ThrowIfNull(results);
        ImmutableArray<TData>.Builder builder = ImmutableArray.CreateBuilder<TData>();
        foreach (Result<TData> r in results)
        {
            if (r.IsSuccess(out TData? value))
            {
                builder.Add(value!);
            }
            else if (r.IsError(out IError? err))
            {
                return Result.Error<ImmutableArray<TData>>(err!);
            }
        }
        return Result.Success(builder.ToImmutable());
    }

    /// <summary>Alias for Collect: collects an enumerable of results into one result. First error wins.</summary>
    public static Result<ImmutableArray<TData>> Sequence<TData>(this IEnumerable<Result<TData>> results) =>
        results.Collect();

    /// <summary>Filters only successful values from an enumerable of results.</summary>
    public static IEnumerable<TData> ChooseSuccessful<TData>(this IEnumerable<Result<TData>> results)
    {
        ArgumentNullException.ThrowIfNull(results);
        foreach (Result<TData> r in results)
        {
            if (r.IsSuccess(out TData? value))
            {
                yield return value!;
            }
        }
    }

    // --- MapError for untyped Result<TData> (IError -> IError) ---

    /// <summary>Maps the error (untyped) to another IError.</summary>
    public static Result<TData> MapError<TData>(this Result<TData> r, Func<IError, IError> errorMapping)
    {
        ArgumentNullException.ThrowIfNull(errorMapping);
        if (r.IsSuccess(out TData? value))
        {
            return Result.Success(value!);
        }

        r.IsError(out IError? err);
        return Result.Error<TData>(errorMapping(err!));
    }
}
