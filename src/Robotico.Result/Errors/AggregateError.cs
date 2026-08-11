using System.Collections.Immutable;

namespace Robotico.Result.Errors;

/// <summary>An error that aggregates multiple errors.</summary>
public sealed class AggregateError(string message, IEnumerable<Error> errors) : Error(message, errors ?? [])
{
    /// <summary>Creates an aggregate error with a default message.</summary>
    public AggregateError(IEnumerable<Error> errors)
        : this("Multiple errors occurred", errors)
    {
    }

    /// <summary>The aggregated errors.</summary>
    public ImmutableArray<Error> Errors { get; } = [.. errors ?? []];
}
