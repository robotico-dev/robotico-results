# Robotico.Result

Industry-standard Result and error types for .NET 8 and .NET 10. Railway-oriented, immutable, **zero package dependencies** (no NuGet dependencies when you install). Trim-friendly; no reflection on hot paths.

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![C#](https://img.shields.io/badge/C%23-12-239120?logo=csharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![NuGet](https://img.shields.io/nuget/v/Robotico.Result.svg)](https://www.nuget.org/packages/Robotico.Result/)
[![Build](https://github.com/robotico-dev/robotico-results/actions/workflows/publish.yml/badge.svg)](https://github.com/robotico-dev/robotico-results/actions/workflows/publish.yml)

## Features

- **Result types**: `Result` (void), `Result<TData>`, `Result<TData, TError>` — all `readonly struct`
- **Industry naming**: `Map`, `Bind`, `MapError` (no LINQ-style `Select`)
- **Pattern matching**: `Match(onSuccess, onError)` for all three result types (void, `Result<TData>`, `Result<TData, TError>`)
- **Side effects**: `Tap`, `TapError`, `TapAsync`, `TapErrorAsync` (void and both generic variants)
- **Recovery**: `Recover`, `RecoverWith`, `RecoverAsync`; void and typed results have `RecoverWith(fallback)` (value or full result fallback)
- **Validation**: `Ensure(predicate, errorFactory)` for `Result<TData>` and `Result<TData, TError>`
- **Aggregation**: `Collect`, `Sequence`, `ChooseSuccessful`
- **Combining**: `Combine(r1, r2)` … `Combine(r1, r2, r3, r4)` (tuples); `Combine(results)` for N results of same type → `Result<ImmutableArray<T>>`
- **Exception boundary**: `Try`, `TryAsync`, `ExpectSuccess`, `ResultErrorException<TError>`
- **Errors**: `IError`, `Error`, `SimpleError`, `ValidationError`, `ExceptionError`, `DomainError<TCode>`, `AggregateError`
- **ValidationError**: `Result.ValidationError(errors)`, `ValidationError.ForField(name, message)`; use <c>Result.Error&lt;TData&gt;(validationError)</c> for a result
- **Explicit conversions**: <c>Result.FromVoid(r)</c>, <c>Result.From(r)</c>, <c>Result.FromWithIError(r)</c> to convert between result types (no implicit operators; CA1000/CA2225 compliant)
- **Task extensions**: `MapAsync`, `BindAsync`, `MapErrorAsync`, `MatchAsync`, `TapAsync`, `TapErrorAsync`, `RecoverWithAsync`, `GetValueAsync`, `ExpectSuccessAsync`

## Installation

```bash
dotnet add package Robotico.Result
```

## Quick start

```csharp
using Robotico.Result;
using Robotico.Result.Errors;

// Success / error
Result<int> ok = Result.Success(42);
Result<int> err = Result.Error<int>(new SimpleError("Something went wrong"));

// Map / Bind
Result<string> mapped = ok.Map(x => x.ToString());
Result<int> bound = ok.Bind(x => ParseUserId(x));

// Match
string message = result.Match(
    v => $"Value: {v}",
    e => $"Error: {e.Message}");

// Recover
Result<int> safe = result.RecoverWith(0);

// Try (exception → Result)
Result<int> r = ResultUtilities.Try(() => int.Parse(input));
Result r2 = ResultUtilities.TryAsync(async () => await client.SendAsync());
```

## Choosing a result type

- **`Result`** — No success value (e.g. save, delete). Success or error only.
- **`Result<TData>`** — Success carries a value; errors are `IError`. Use when callers don't need a specific error type.
- **`Result<TData, TError>`** — Success + value and a *strongly typed* error (e.g. `ValidationError`, `DomainError<TCode>`). Use when callers switch on or handle specific error types.

See the design doc (`docs/design.adoc`) for full guidance.

## Documentation

Detailed architecture and design docs (AsciiDoc + Mermaid) are in the `docs/` folder:

- **Architecture** (`docs/architecture.adoc`) — Type hierarchy, data flow, exception boundary (with Mermaid diagrams).
- **Design** (`docs/design.adoc`) — Design principles, when to use which type, API naming.
- **Trim and AOT** (`docs/trim-aot.adoc`) — Trim-friendly and AOT-compatible; no reflection on hot paths.

To build HTML from the AsciiDoc sources (e.g. with Asciidoctor):

```bash
asciidoctor docs/index.adoc -o docs/index.html
asciidoctor docs/architecture.adoc -o docs/architecture.html
asciidoctor docs/design.adoc -o docs/design.html
asciidoctor docs/trim-aot.adoc -o docs/trim-aot.html
```

## Versioning

We follow [Semantic Versioning](https://semver.org/). Version **1.0.0** is the first stable release. No breaking changes in minor/patch versions.

## Building, testing, and benchmarks

From the repo root:

```bash
dotnet restore
dotnet build -c Release
dotnet test tests/Robotico.Result.Tests/Robotico.Result.Tests.csproj -c Release
```

With coverage (Coverlet):

```bash
dotnet test tests/Robotico.Result.Tests/Robotico.Result.Tests.csproj -c Release --collect:"XPlat Code Coverage"
```

Optional CI gate (fail if line coverage below threshold):

```bash
dotnet test tests/Robotico.Result.Tests/Robotico.Result.Tests.csproj -c Release --collect:"XPlat Code Coverage" /p:CollectCoverage=true /p:Threshold=90 /p:ThresholdType=line
```

Run benchmarks (BenchmarkDotNet):

```bash
dotnet run -c Release -p benchmarks/Robotico.Result.Benchmarks/Robotico.Result.Benchmarks.csproj -- --filter "*"
```

Or open `robotico-results.slnx` in your IDE and build from there.

## License

See repository license file.
