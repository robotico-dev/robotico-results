using System.Collections.Immutable;

namespace Robotico.Result.Errors;

/// <summary>
/// Represents validation errors with field-specific error messages.
/// Use for form validation, API input validation, etc.
/// </summary>
public sealed record ValidationError : IError
{
    /// <summary>Creates a validation error with field-specific error messages.</summary>
    /// <param name="errors">Dictionary mapping field names to their error messages.</param>
    /// <param name="message">Optional custom message. If null, generated from error count.</param>
    /// <param name="code">Error code. Defaults to "VAL_FAILED".</param>
    public ValidationError(
        IReadOnlyDictionary<string, string[]> errors,
        string? message = null,
        string code = "VAL_FAILED")
    {
        ArgumentNullException.ThrowIfNull(errors);
        if (errors.Count == 0)
        {
            throw new ArgumentException("At least one validation error is required", nameof(errors));
        }

        Errors = errors;
        int totalErrors = errors.Values.Sum(v => v.Length);
        Message = message ?? $"Validation failed with {totalErrors} error(s) across {errors.Count} field(s)";
        Code = code;
        Severity = ErrorSeverity.Warning;
        InnerError = null;
        Context = ImmutableDictionary<string, object>.Empty;
    }

    /// <summary>Creates a validation error for a single field.</summary>
    public static ValidationError ForField(string fieldName, string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(fieldName);
        ArgumentNullException.ThrowIfNull(errorMessage);
        Dictionary<string, string[]> errors = new Dictionary<string, string[]>
        {
            [fieldName] = [errorMessage]
        };
        return new ValidationError(errors, $"Validation failed for {fieldName}: {errorMessage}", $"VAL_{fieldName.ToUpperInvariant()}");
    }

    /// <summary>Creates a validation error for a single field with multiple messages.</summary>
    public static ValidationError ForField(string fieldName, params string[] errorMessages)
    {
        ArgumentNullException.ThrowIfNull(fieldName);
        ArgumentNullException.ThrowIfNull(errorMessages);
        if (errorMessages.Length == 0)
        {
            throw new ArgumentException("At least one error message is required", nameof(errorMessages));
        }

        Dictionary<string, string[]> errors = new Dictionary<string, string[]>
        {
            [fieldName] = errorMessages
        };
        string summary = errorMessages.Length == 1 ? errorMessages[0] : $"{errorMessages.Length} validation errors";
        return new ValidationError(errors, $"Validation failed for {fieldName}: {summary}", $"VAL_{fieldName.ToUpperInvariant()}");
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

    /// <summary>Field-specific validation errors.</summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; init; }
}
