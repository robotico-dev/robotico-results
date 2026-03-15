namespace Robotico.Result.Errors;

/// <summary>
/// Base interface for all errors reported using Result types.
/// Provides message, code, severity, inner error, and context for diagnostics and handling.
/// </summary>
public interface IError
{
    /// <summary>Gets the human-readable error message.</summary>
    string Message { get; }

    /// <summary>Gets the machine-readable error code (e.g. "AUTH_001", "VAL_REQUIRED").</summary>
    string Code { get; }

    /// <summary>Gets the error severity level.</summary>
    ErrorSeverity Severity { get; }

    /// <summary>Gets the inner error that caused this error, if any.</summary>
    IError? InnerError { get; }

    /// <summary>Gets contextual data associated with this error.</summary>
    IReadOnlyDictionary<string, object> Context { get; }
}
