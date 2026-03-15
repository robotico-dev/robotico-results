namespace Robotico.Result.Errors;

/// <summary>Error severity levels.</summary>
public enum ErrorSeverity
{
    /// <summary>Informational message, not an actual error.</summary>
    Info = 0,

    /// <summary>Warning about potential issue, operation succeeded.</summary>
    Warning = 1,

    /// <summary>Operation failed, may be recoverable.</summary>
    Error = 2,

    /// <summary>Critical system failure, requires immediate attention.</summary>
    Critical = 3
}
