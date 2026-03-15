using System.Collections.Immutable;

namespace Robotico.Result.Errors;

/// <summary>An error that aggregates multiple errors.</summary>
public sealed class AggregateError : Error
{
    /// <summary>Creates an aggregate error with the given message and errors.</summary>
    public AggregateError(string message, IEnumerable<Error> errors)
        : base(message, errors ?? [])
    {
        Errors = (errors ?? []).ToImmutableArray();
    }

    /// <summary>Creates an aggregate error with a default message.</summary>
    public AggregateError(IEnumerable<Error> errors)
        : this("Multiple errors occurred", errors)
    {
    }

    /// <summary>The aggregated errors.</summary>
    public ImmutableArray<Error> Errors { get; }
}
