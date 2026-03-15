using System.Diagnostics.CodeAnalysis;
using Robotico.Result.Errors;

namespace Robotico.Result;

/// <summary>
/// Exception that wraps an <see cref="IError"/> for use at process/API boundaries
/// when converting a Result to an exception (e.g. after ExpectSuccess).
/// </summary>
/// <typeparam name="TError">The error type wrapped by this exception.</typeparam>
[SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "Primary constructor (TError error) is the intended API. Standard string/inner constructors do not fit this type; serialization is rarely needed at Result boundaries.")]
public sealed class ResultErrorException<TError>(TError error) : Exception(error?.Message ?? "An error occurred")
    where TError : class, IError
{
    /// <summary>The error wrapped by this exception.</summary>
    public TError Error { get; } = error ?? throw new ArgumentNullException(nameof(error));
}
