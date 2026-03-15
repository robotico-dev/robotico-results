using System.Collections.Immutable;

namespace Robotico.Result.Errors;

/// <summary>
/// Simple implementation of <see cref="IError"/> with full control over all properties.
/// </summary>
public sealed record SimpleError : IError
{
    /// <summary>Creates a simple error with full control over all properties.</summary>
    public SimpleError(
        string message,
        string code = "ERROR",
        ErrorSeverity severity = ErrorSeverity.Error,
        IError? innerError = null,
        IReadOnlyDictionary<string, object>? context = null)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Severity = severity;
        InnerError = innerError;
        Context = context is null ? ImmutableDictionary<string, object>.Empty : ImmutableDictionary.CreateRange(context);
    }

    /// <summary>Creates a simple error with only a message.</summary>
    public SimpleError(string message)
        : this(message, "ERROR", ErrorSeverity.Error)
    {
    }

    /// <inheritdoc />
    public string Message { get; init; }

    /// <inheritdoc />
    public string Code { get; init; }

    /// <inheritdoc />
    public ErrorSeverity Severity { get; init; }

    /// <inheritdoc />
    public IError? InnerError { get; init; }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object> Context { get; init; }
}
