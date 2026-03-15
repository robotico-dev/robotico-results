using System.Collections.Immutable;

namespace Robotico.Result.Errors;

/// <summary>An error that aggregates multiple errors of a specific type.</summary>
/// <typeparam name="TError">The type of errors being aggregated.</typeparam>
public sealed class AggregateError<TError> : Error
    where TError : Error
{
    /// <summary>Creates an aggregate error with the given message and errors.</summary>
    public AggregateError(string message, IEnumerable<TError> errors)
        : base(message, errors ?? [])
    {
        Errors = (errors ?? []).ToImmutableArray();
    }

    /// <summary>Creates an aggregate error with a default message.</summary>
    public AggregateError(IEnumerable<TError> errors)
        : this("Multiple errors occurred", errors)
    {
    }

    /// <summary>The aggregated errors.</summary>
    public ImmutableArray<TError> Errors { get; }
}
