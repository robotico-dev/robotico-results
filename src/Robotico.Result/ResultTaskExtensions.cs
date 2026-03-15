using Robotico.Result.Errors;

namespace Robotico.Result;

/// <summary>Task extensions for Result types: MapAsync, BindAsync, MapErrorAsync, MatchAsync, GetValueAsync, ExpectSuccess.</summary>
public static class ResultTaskExtensions
{
    // --- Result (void) ---

    /// <summary>Maps a task of Result to a result with data.</summary>
    public static async Task<Result<TMapped>> MapAsync<TMapped>(this Task<Result> task, Func<TMapped> mapping)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(mapping);
        return (await task.ConfigureAwait(false)).Map(mapping);
    }

    /// <summary>Binds a task of Result.</summary>
    public static async Task<Result<TMapped>> BindAsync<TMapped>(this Task<Result> task, Func<Task<Result<TMapped>>> binding)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(binding);
        return await (await task.ConfigureAwait(false)).BindAsync(binding).ConfigureAwait(false);
    }

    /// <summary>Pattern match on a task of void Result.</summary>
    public static async Task<TResult> MatchAsync<TResult>(this Task<Result> task, Func<TResult> onSuccess, Func<IError, TResult> onError)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onError);
        return (await task.ConfigureAwait(false)).Match(onSuccess, onError);
    }

    /// <summary>Runs a side effect on success of a task of void Result; returns the same task result.</summary>
    public static async Task<Result> TapAsync(this Task<Result> task, Action action)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(action);
        return (await task.ConfigureAwait(false)).Tap(action);
    }

    /// <summary>Runs an async side effect on success of a task of void Result.</summary>
    public static async Task<Result> TapAsync(this Task<Result> task, Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(action);
        Result r = await task.ConfigureAwait(false);
        if (r.IsSuccess())
            await action().ConfigureAwait(false);
        return r;
    }

    /// <summary>Runs a side effect on error of a task of void Result.</summary>
    public static async Task<Result> TapErrorAsync(this Task<Result> task, Action<IError> action)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(action);
        return (await task.ConfigureAwait(false)).TapError(action);
    }

    /// <summary>Returns the task's result if success; otherwise the fallback result.</summary>
    public static async Task<Result> RecoverWithAsync(this Task<Result> task, Result fallback)
    {
        ArgumentNullException.ThrowIfNull(task);
        return (await task.ConfigureAwait(false)).RecoverWith(fallback);
    }

    // --- Result<TData> ---

    /// <summary>Maps a task of Result&lt;TData&gt;.</summary>
    public static async Task<Result<TMapped>> MapAsync<TData, TMapped>(this Task<Result<TData>> task, Func<TData, TMapped> mapping)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(mapping);
        return (await task.ConfigureAwait(false)).Map(mapping);
    }

    /// <summary>Async map for task of Result&lt;TData&gt;.</summary>
    public static async Task<Result<TMapped>> MapAsync<TData, TMapped>(this Task<Result<TData>> task, Func<TData, Task<TMapped>> mapping)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(mapping);
        return await (await task.ConfigureAwait(false)).MapAsync(mapping).ConfigureAwait(false);
    }

    /// <summary>Binds a task of Result&lt;TData&gt;.</summary>
    public static async Task<Result<TMapped>> BindAsync<TData, TMapped>(this Task<Result<TData>> task, Func<TData, Result<TMapped>> binding)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(binding);
        return (await task.ConfigureAwait(false)).Bind(binding);
    }

    /// <summary>Async bind for task of Result&lt;TData&gt;.</summary>
    public static async Task<Result<TMapped>> BindAsync<TData, TMapped>(this Task<Result<TData>> task, Func<TData, Task<Result<TMapped>>> binding)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(binding);
        return await (await task.ConfigureAwait(false)).BindAsync(binding).ConfigureAwait(false);
    }

    /// <summary>Pattern match on a task of Result&lt;TData&gt;.</summary>
    public static async Task<TResult> MatchAsync<TData, TResult>(this Task<Result<TData>> task, Func<TData, TResult> onSuccess, Func<IError, TResult> onError)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onError);
        return (await task.ConfigureAwait(false)).Match(onSuccess, onError);
    }

    /// <summary>Gets the value from a task of Result&lt;TData&gt;, or default.</summary>
    public static async Task<TData?> GetValueAsync<TData>(this Task<Result<TData>> task)
    {
        ArgumentNullException.ThrowIfNull(task);
        return (await task.ConfigureAwait(false)).GetValue();
    }

    /// <summary>Expects success on a task of Result&lt;TData&gt;; throws if error.</summary>
    public static async Task<TData> ExpectSuccessAsync<TData>(this Task<Result<TData>> task)
    {
        ArgumentNullException.ThrowIfNull(task);
        return (await task.ConfigureAwait(false)).ExpectSuccess();
    }

    /// <summary>Expects success with custom exception builder.</summary>
    public static async Task<TData> ExpectSuccessAsync<TData>(this Task<Result<TData>> task, Func<IError, Exception> exceptionBuilder)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(exceptionBuilder);
        return (await task.ConfigureAwait(false)).ExpectSuccess(exceptionBuilder);
    }

    // --- Result<TData, TError> ---

    /// <summary>Maps a task of Result&lt;TData, TError&gt;.</summary>
    public static async Task<Result<TMapped, TError>> MapAsync<TData, TError, TMapped>(this Task<Result<TData, TError>> task, Func<TData, TMapped> mapping)
        where TError : class, IError
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(mapping);
        return (await task.ConfigureAwait(false)).Map(mapping);
    }

    /// <summary>Async map for task of Result&lt;TData, TError&gt;.</summary>
    public static async Task<Result<TMapped, TError>> MapAsync<TData, TError, TMapped>(this Task<Result<TData, TError>> task, Func<TData, Task<TMapped>> mapping)
        where TError : class, IError
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(mapping);
        return await (await task.ConfigureAwait(false)).MapAsync(mapping).ConfigureAwait(false);
    }

    /// <summary>Binds a task of Result&lt;TData, TError&gt;.</summary>
    public static async Task<Result<TMapped, TError>> BindAsync<TData, TError, TMapped>(this Task<Result<TData, TError>> task, Func<TData, Result<TMapped, TError>> binding)
        where TError : class, IError
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(binding);
        return (await task.ConfigureAwait(false)).Bind(binding);
    }

    /// <summary>Async bind for task of Result&lt;TData, TError&gt;.</summary>
    public static async Task<Result<TMapped, TError>> BindAsync<TData, TError, TMapped>(this Task<Result<TData, TError>> task, Func<TData, Task<Result<TMapped, TError>>> binding)
        where TError : class, IError
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(binding);
        return await (await task.ConfigureAwait(false)).BindAsync(binding).ConfigureAwait(false);
    }

    /// <summary>Pattern match on a task of Result&lt;TData, TError&gt;.</summary>
    public static async Task<TResult> MatchAsync<TData, TError, TResult>(this Task<Result<TData, TError>> task, Func<TData, TResult> onSuccess, Func<TError, TResult> onError)
        where TError : class, IError
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onError);
        return (await task.ConfigureAwait(false)).Match(onSuccess, onError);
    }

    /// <summary>MapError for task of Result&lt;TData, TError&gt;.</summary>
    public static async Task<Result<TData, TMappedError>> MapErrorAsync<TData, TError, TMappedError>(this Task<Result<TData, TError>> task, Func<TError, TMappedError> errorMapping)
        where TError : class, IError
        where TMappedError : class, IError
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(errorMapping);
        return (await task.ConfigureAwait(false)).MapError(errorMapping);
    }

    /// <summary>Async MapError for task of Result&lt;TData, TError&gt;.</summary>
    public static async Task<Result<TData, TMappedError>> MapErrorAsync<TData, TError, TMappedError>(this Task<Result<TData, TError>> task, Func<TError, Task<TMappedError>> errorMapping)
        where TError : class, IError
        where TMappedError : class, IError
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(errorMapping);
        return await (await task.ConfigureAwait(false)).MapErrorAsync(errorMapping).ConfigureAwait(false);
    }

    /// <summary>Gets the value from a task of Result&lt;TData, TError&gt;, or default.</summary>
    public static async Task<TData?> GetValueAsync<TData, TError>(this Task<Result<TData, TError>> task)
        where TError : class, IError
    {
        ArgumentNullException.ThrowIfNull(task);
        return (await task.ConfigureAwait(false)).GetValue();
    }

    /// <summary>Expects success on a task of Result&lt;TData, TError&gt;; throws if error.</summary>
    public static async Task<TData> ExpectSuccessAsync<TData, TError>(this Task<Result<TData, TError>> task)
        where TError : class, IError
    {
        ArgumentNullException.ThrowIfNull(task);
        return (await task.ConfigureAwait(false)).ExpectSuccess();
    }

    /// <summary>Expects success with custom exception builder.</summary>
    public static async Task<TData> ExpectSuccessAsync<TData, TError>(this Task<Result<TData, TError>> task, Func<TError, Exception> exceptionBuilder)
        where TError : class, IError
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(exceptionBuilder);
        return (await task.ConfigureAwait(false)).ExpectSuccess(exceptionBuilder);
    }

    /// <summary>Tap (side effect on success) for task of Result&lt;TData, TError&gt;.</summary>
    public static async Task<Result<TData, TError>> TapAsync<TData, TError>(this Task<Result<TData, TError>> task, Action<TData> action)
        where TError : class, IError
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(action);
        return (await task.ConfigureAwait(false)).Tap(action);
    }

    /// <summary>Async tap on success for task of Result&lt;TData, TError&gt;.</summary>
    public static async Task<Result<TData, TError>> TapAsync<TData, TError>(this Task<Result<TData, TError>> task, Func<TData, Task> action)
        where TError : class, IError
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(action);
        return await (await task.ConfigureAwait(false)).TapAsync(action).ConfigureAwait(false);
    }

    /// <summary>TapError for task of Result&lt;TData, TError&gt;.</summary>
    public static async Task<Result<TData, TError>> TapErrorAsync<TData, TError>(this Task<Result<TData, TError>> task, Action<TError> action)
        where TError : class, IError
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(action);
        return (await task.ConfigureAwait(false)).TapError(action);
    }

    /// <summary>Async TapError for task of Result&lt;TData, TError&gt;.</summary>
    public static async Task<Result<TData, TError>> TapErrorAsync<TData, TError>(this Task<Result<TData, TError>> task, Func<TError, Task> action)
        where TError : class, IError
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(action);
        return await (await task.ConfigureAwait(false)).TapErrorAsync(action).ConfigureAwait(false);
    }

    /// <summary>Recover for task of Result&lt;TData, TError&gt;.</summary>
    public static async Task<Result<TData, TError>> RecoverAsync<TData, TError>(this Task<Result<TData, TError>> task, Func<TError, TData> fallback)
        where TError : class, IError
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(fallback);
        return (await task.ConfigureAwait(false)).Recover(fallback);
    }

    /// <summary>Async Recover for task of Result&lt;TData, TError&gt;.</summary>
    public static async Task<Result<TData, TError>> RecoverAsync<TData, TError>(this Task<Result<TData, TError>> task, Func<TError, Task<TData>> fallback)
        where TError : class, IError
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(fallback);
        return await (await task.ConfigureAwait(false)).RecoverAsync(fallback).ConfigureAwait(false);
    }

    /// <summary>RecoverWith for task of Result&lt;TData, TError&gt;.</summary>
    public static async Task<Result<TData, TError>> RecoverWithAsync<TData, TError>(this Task<Result<TData, TError>> task, TData fallbackValue)
        where TError : class, IError
    {
        ArgumentNullException.ThrowIfNull(task);
        return (await task.ConfigureAwait(false)).RecoverWith(fallbackValue);
    }

    /// <summary>Returns the task's result if success; otherwise the fallback result. For Result&lt;TData&gt;.</summary>
    public static async Task<Result<TData>> RecoverWithAsync<TData>(this Task<Result<TData>> task, Result<TData> fallback)
    {
        ArgumentNullException.ThrowIfNull(task);
        return (await task.ConfigureAwait(false)).RecoverWith(fallback);
    }

    /// <summary>Returns the task's result if success; otherwise the fallback result. For Result&lt;TData, TError&gt;.</summary>
    public static async Task<Result<TData, TError>> RecoverWithAsync<TData, TError>(this Task<Result<TData, TError>> task, Result<TData, TError> fallback)
        where TError : class, IError
    {
        ArgumentNullException.ThrowIfNull(task);
        return (await task.ConfigureAwait(false)).RecoverWith(fallback);
    }
}
