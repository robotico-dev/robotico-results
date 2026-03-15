using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Robotico.Result.Errors;

namespace Robotico.Result;

/// <summary>Utilities: Combine (multiple results into one), Try (exception to Result).</summary>
/// <remarks>
/// <para><b>Combine</b>: Use the overloads for 2, 3, or 4 results when you need a tuple. For combining N results of the same type into one result of a collection, use <see cref="Combine{T}(IEnumerable{Result{T}})"/> or <see cref="ResultExtensions.Collect{TData}(IEnumerable{Result{TData}})"/> / <see cref="ResultExtensions.Sequence{TData}(IEnumerable{Result{TData}})"/>.</para>
/// <para><b>Try / exception boundary</b>: <see cref="Try{T}(Func{T}, Func{Exception, IError}?)"/> and <see cref="TryAsync{T}(Func{Task{T}}, Func{Exception, IError}?)"/> are the intended way to convert exception-throwing code into Results. Use <c>ExpectSuccess</c> only when leaving the Result world (e.g. at API or process boundaries).</para>
/// </remarks>
public static class ResultUtilities
{
    /// <summary>Combines an enumerable of results of the same type into a single result of an immutable array. First error wins. Use for 5+ results; for 2–4 use the tuple overloads.</summary>
    public static Result<ImmutableArray<T>> Combine<T>(IEnumerable<Result<T>> results)
    {
        ArgumentNullException.ThrowIfNull(results);
        return results.Collect();
    }

    /// <summary>Combines two results into a single result containing a tuple. All errors are aggregated on failure.</summary>
    /// <remarks>For 5+ results of the same type, use <see cref="Combine{T}(IEnumerable{Result{T}})"/> or <see cref="ResultExtensions.Collect{TData}(IEnumerable{Result{TData}})"/>.</remarks>
    public static Result<(T1, T2)> Combine<T1, T2>(Result<T1> r1, Result<T2> r2)
    {
        bool ok1 = r1.IsSuccess(out T1? v1, out IError? e1);
        bool ok2 = r2.IsSuccess(out T2? v2, out IError? e2);
        if (ok1 && ok2)
            return Result.Success((v1!, v2!));
        return Result.Error<(T1, T2)>(ToAggregate(CollectErrorList(e1, ok1, e2, ok2)));
    }

    /// <summary>Combines three results into a single result containing a tuple. All errors are aggregated on failure.</summary>
    /// <remarks>For 5+ results of the same type, use <see cref="Combine{T}(IEnumerable{Result{T}})"/> or <see cref="ResultExtensions.Collect{TData}(IEnumerable{Result{TData}})"/>.</remarks>
    public static Result<(T1, T2, T3)> Combine<T1, T2, T3>(Result<T1> r1, Result<T2> r2, Result<T3> r3)
    {
        bool ok1 = r1.IsSuccess(out T1? v1, out IError? e1);
        bool ok2 = r2.IsSuccess(out T2? v2, out IError? e2);
        bool ok3 = r3.IsSuccess(out T3? v3, out IError? e3);
        if (ok1 && ok2 && ok3)
            return Result.Success((v1!, v2!, v3!));
        return Result.Error<(T1, T2, T3)>(ToAggregate(CollectErrorList(e1, ok1, e2, ok2, e3, ok3)));
    }

    /// <summary>Combines four results into a single result containing a tuple. All errors are aggregated on failure.</summary>
    /// <remarks>For 5+ results of the same type, use <see cref="Combine{T}(IEnumerable{Result{T}})"/> or <see cref="ResultExtensions.Collect{TData}(IEnumerable{Result{TData}})"/>.</remarks>
    public static Result<(T1, T2, T3, T4)> Combine<T1, T2, T3, T4>(Result<T1> r1, Result<T2> r2, Result<T3> r3, Result<T4> r4)
    {
        bool ok1 = r1.IsSuccess(out T1? v1, out IError? e1);
        bool ok2 = r2.IsSuccess(out T2? v2, out IError? e2);
        bool ok3 = r3.IsSuccess(out T3? v3, out IError? e3);
        bool ok4 = r4.IsSuccess(out T4? v4, out IError? e4);
        if (ok1 && ok2 && ok3 && ok4)
            return Result.Success((v1!, v2!, v3!, v4!));
        return Result.Error<(T1, T2, T3, T4)>(ToAggregate(CollectErrorList(e1, ok1, e2, ok2, e3, ok3, e4, ok4)));
    }

    private static List<IError> CollectErrorList(IError? e1, bool ok1, IError? e2, bool ok2)
    {
        List<IError> errors = new List<IError>();
        if (!ok1 && e1 != null) errors.Add(e1);
        if (!ok2 && e2 != null) errors.Add(e2);
        return errors;
    }

    private static List<IError> CollectErrorList(IError? e1, bool ok1, IError? e2, bool ok2, IError? e3, bool ok3)
    {
        List<IError> errors = CollectErrorList(e1, ok1, e2, ok2);
        if (!ok3 && e3 != null) errors.Add(e3);
        return errors;
    }

    private static List<IError> CollectErrorList(IError? e1, bool ok1, IError? e2, bool ok2, IError? e3, bool ok3, IError? e4, bool ok4)
    {
        List<IError> errors = CollectErrorList(e1, ok1, e2, ok2, e3, ok3);
        if (!ok4 && e4 != null) errors.Add(e4);
        return errors;
    }

    private static IError ToAggregate(List<IError> errors)
    {
        if (errors.Count == 1)
            return errors[0];
        List<Error> asErrors = errors.OfType<Error>().ToList();
        if (asErrors.Count == errors.Count)
            return new AggregateError($"Multiple errors occurred ({errors.Count} total)", asErrors);
        return new SimpleError($"Multiple errors occurred ({errors.Count} total)", "AGG_ERRORS", ErrorSeverity.Error, context: new Dictionary<string, object> { ["Errors"] = errors });
    }

    /// <summary>Executes a function and converts exceptions to a Result.</summary>
    /// <remarks>Use this at the boundary of exception-throwing code (e.g. external libraries). Optional <paramref name="errorFactory"/> lets you map exceptions to your error types.</remarks>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Try() is an exception boundary; any exception must be converted to Result. Catching only specific types would let others escape.")]
    public static Result<T> Try<T>(Func<T> func, Func<Exception, IError>? errorFactory = null)
    {
        ArgumentNullException.ThrowIfNull(func);
        try
        {
            return Result.Success(func());
        }
        catch (Exception ex)
        {
            IError err = errorFactory != null ? errorFactory(ex) : new ExceptionError(ex);
            return Result.Error<T>(err);
        }
    }

    /// <summary>Executes an async function and converts exceptions to a Result.</summary>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "TryAsync() is an exception boundary; any exception must be converted to Result. Catching only specific types would let others escape.")]
    public static async Task<Result<T>> TryAsync<T>(Func<Task<T>> func, Func<Exception, IError>? errorFactory = null)
    {
        ArgumentNullException.ThrowIfNull(func);
        try
        {
            return Result.Success(await func().ConfigureAwait(false));
        }
        catch (Exception ex)
        {
            IError err = errorFactory != null ? errorFactory(ex) : new ExceptionError(ex);
            return Result.Error<T>(err);
        }
    }

    /// <summary>Executes an action and converts exceptions to a void Result.</summary>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Try() is an exception boundary; any exception must be converted to Result. Catching only specific types would let others escape.")]
    public static Result Try(Action action, Func<Exception, IError>? errorFactory = null)
    {
        ArgumentNullException.ThrowIfNull(action);
        try
        {
            action();
            return Result.Success();
        }
        catch (Exception ex)
        {
            IError err = errorFactory != null ? errorFactory(ex) : new ExceptionError(ex);
            return Result.Error(err);
        }
    }

    /// <summary>Executes an async action and converts exceptions to a void Result.</summary>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "TryAsync() is an exception boundary; any exception must be converted to Result. Catching only specific types would let others escape.")]
    public static async Task<Result> TryAsync(Func<Task> action, Func<Exception, IError>? errorFactory = null)
    {
        ArgumentNullException.ThrowIfNull(action);
        try
        {
            await action().ConfigureAwait(false);
            return Result.Success();
        }
        catch (Exception ex)
        {
            IError err = errorFactory != null ? errorFactory(ex) : new ExceptionError(ex);
            return Result.Error(err);
        }
    }
}
