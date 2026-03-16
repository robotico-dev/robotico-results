namespace Robotico.Result.Errors;

/// <summary>Extension methods for error types.</summary>
public static class ErrorExtensions
{
    /// <summary>
    /// Recursively gets all error messages from a concrete Error, flattening aggregates.
    /// </summary>
    public static IEnumerable<string> GetErrorMessages(this Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        if (error.CausedBy.Length > 0)
        {
            foreach (Error e in error.CausedBy)
            {
                foreach (string msg in e.GetErrorMessages())
                {
                    yield return msg;
                }
            }
            yield break;
        }

        yield return error.Message;
    }

    /// <summary>
    /// Recursively gets all error messages from an IError, flattening aggregates when possible.
    /// </summary>
    public static IEnumerable<string> GetErrorMessages(this IError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        if (error is Error concrete)
        {
            return concrete.GetErrorMessages();
        }

        return [error.Message];
    }
}
