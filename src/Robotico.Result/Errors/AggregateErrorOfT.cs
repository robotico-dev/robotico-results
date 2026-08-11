using System.Collections.Immutable;

namespace Robotico.Result.Errors;

/// <summary>An error that aggregates multiple errors of a specific type.</summary>
/// <typeparam name="TError">The type of errors being aggregated.</typeparam>
public sealed class AggregateError<TError>(string message, IEnumerable<TError> errors) : Error(message, errors ?? [])
    where TError : Error
{
    /// <summary>Creates an aggregate error with a default message.</summary>
    public AggregateError(IEnumerable<TError> errors)
        : this("Multiple errors occurred", errors)
    {
    }

    /// <summary>The aggregated errors.</summary>
    public ImmutableArray<TError> Errors { get; } = [.. errors ?? []];
}
