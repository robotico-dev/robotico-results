using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Robotico.Result.Errors;

/// <summary>
/// Base class for errors. Supports message, code, severity, and optional cause chain.
/// </summary>
/// <remarks>
/// <para><b>CausedBy vs InnerError</b>: <see cref="CausedBy"/> holds all previous errors that led to this one (for aggregation and logging).
/// <see cref="InnerError"/> (from <see cref="IError"/>) is set to the first element of <see cref="CausedBy"/> when using the constructor that accepts causes, so single-cause chains are navigable via InnerError.</para>
/// </remarks>
[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "'Error' is the standard domain term for this pattern. Renaming would harm API clarity and break consistency with Result/Error naming.")]
public class Error : IError
{
    /// <summary>Creates an error with the given message.</summary>
    public Error(string message)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Code = "ERROR";
        Severity = ErrorSeverity.Error;
        InnerError = null;
        Context = ImmutableDictionary<string, object>.Empty;
        CausedBy = ImmutableArray<Error>.Empty;
    }

    /// <summary>Creates an error with the given message and causes.</summary>
    public Error(string message, IEnumerable<Error> causedBy)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
        CausedBy = causedBy?.ToImmutableArray() ?? ImmutableArray<Error>.Empty;
        Code = "ERROR";
        Severity = ErrorSeverity.Error;
        InnerError = CausedBy.FirstOrDefault();
        Context = ImmutableDictionary<string, object>.Empty;
    }

    /// <summary>The error message.</summary>
    public string Message { get; }

    /// <summary>Previous errors that led to this error.</summary>
    public ImmutableArray<Error> CausedBy { get; }

    /// <inheritdoc />
    public virtual string Code { get; protected init; } = "ERROR";

    /// <inheritdoc />
    public ErrorSeverity Severity { get; protected init; }

    /// <inheritdoc />
    public IError? InnerError { get; protected init; }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object> Context { get; protected init; } = ImmutableDictionary<string, object>.Empty;
}
