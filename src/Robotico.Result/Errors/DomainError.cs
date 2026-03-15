namespace Robotico.Result.Errors;

/// <summary>
/// Base class for domain errors with a strongly-typed code (e.g. enum).
/// </summary>
/// <typeparam name="TCode">The type of the domain error code.</typeparam>
public abstract class DomainError<TCode> : Error
{
    /// <summary>Creates a domain error with the given code and message.</summary>
    protected DomainError(TCode code, string message)
        : base(message)
    {
        DomainCode = code;
    }

    /// <summary>Creates a domain error with the given code, message, and causes.</summary>
    protected DomainError(TCode code, string message, IEnumerable<Error> causedBy)
        : base(message, causedBy)
    {
        DomainCode = code;
    }

    /// <summary>Domain-specific error code.</summary>
    public TCode DomainCode { get; }

    /// <inheritdoc />
    public override string Code => DomainCode?.ToString() ?? "ERROR";
}
