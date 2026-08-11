namespace Robotico.Result.Errors;

/// <summary>An error that wraps an exception.</summary>
public sealed class ExceptionError(Exception exception) : Error(BaseMessage(exception))
{
    /// <summary>The wrapped exception.</summary>
    public Exception Exception { get; } = exception;

    private static string BaseMessage(Exception? ex)
    {
        ArgumentNullException.ThrowIfNull(ex);
        return ex.Message ?? "An exception occurred";
    }
}
