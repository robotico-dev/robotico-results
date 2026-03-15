namespace Robotico.Result.Errors;

/// <summary>An error that wraps an exception.</summary>
public sealed class ExceptionError : Error
{
    /// <summary>Creates an exception error wrapping the given exception.</summary>
    public ExceptionError(Exception exception)
        : base(exception?.Message ?? "An exception occurred")
    {
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
    }

    /// <summary>The wrapped exception.</summary>
    public Exception Exception { get; }
}
