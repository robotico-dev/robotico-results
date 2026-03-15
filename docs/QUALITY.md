# Robotico.Result — Quality bar (senior/principal 10/10)

This document defines the target quality level for the Result library and how it aligns with **Robotico.Mediator** (robotico-mediator-csharp). Both repos target the same senior/principal bar (10/10).

## Seniority rating

**Current assessment: 10/10** (principal level). The implementation is production-ready, railway-oriented, well-tested (unit, theory, property-based with CsCheck), and documented. It serves as the quality bar for other Robotico libraries (including Robotico.Mediator).

## What meets the bar

- **Design**: Result types as values; single exception boundary (Try / ExpectSuccess); explicit conversions; trim/AOT considered.
- **SOLID**: SRP (Result vs extensions vs utilities vs errors); OCP (IError, new result/error types); small interfaces.
- **Testing**: xUnit, Coverlet, CsCheck (property-based); guard tests; sync and async; void, `Result<TData>`, `Result<TData, TError>`.
- **Documentation**: XML docs on public API, README; `docs/index.adoc`, `docs/architecture.adoc`, `docs/design.adoc`, `docs/trim-aot.adoc`.
- **Tooling**: TreatWarningsAsErrors, AnalysisLevel latest-all, EnforceCodeStyleInBuild (Directory.Build.props), .editorconfig; documented suppressions.

## How to reach / maintain 10/10

1. **SOLID & abstractions** — SRP/OCP; small interfaces (IError, result types, extensions).
2. **Error handling** — Result as values; single exception boundary (ExpectSuccess); no silent swallowing.
3. **Tests** — Unit + theory (InlineData) + property-based (CsCheck); guard and edge-case tests for all result types.
4. **Coverage** — Coverlet with documented command; optional CI gate (e.g. 90% line). See QUALITY.md coverage section.
5. **Documentation** — Full XML docs on public API; README; docs/ (index, architecture, design, trim-aot).
6. **Analyzers** — TreatWarningsAsErrors, latest analyzers, EnforceCodeStyleInBuild. Document any intentional suppressions.

## Shared bar with Robotico.Mediator

Both libraries aim for the same senior/principal bar (10/10). Criteria:

| Criterion            | Robotico.Result           | Robotico.Mediator              |
|----------------------|---------------------------|---------------------------------|
| SOLID & abstractions | Yes                       | Yes                             |
| Error handling       | Result + boundary         | Exceptions + Result             |
| Tests                | Unit + theory + CsCheck   | Unit + contract + edge cases     |
| Coverage             | Documented + optional gate | Documented + optional gate     |
| XML docs             | Full public API           | Full public API                 |
| Design docs          | index, architecture, design, trim-aot | index, architecture, design, trim-aot |
| Analyzers            | Latest, warnings as errors | Latest, warnings as errors      |
| Observability        | N/A (library)             | Logging + event IDs             |

## Coverage

- Run: `dotnet test tests/Robotico.Result.Tests/Robotico.Result.Tests.csproj -c Release --collect:"XPlat Code Coverage"`.
- Optional CI gate: fail build if line coverage is below a threshold (e.g. 90%); use Coverlet `/p:Threshold=90` when running tests with coverage.

## Fixes applied for 10/10 consistency

- **AggregateError&lt;T&gt; cause chain**: Constructors now call `base(message, errors ?? [])` so `CausedBy` and `InnerError` are set; `GetErrorMessages()` uses `CausedBy` so both `AggregateError` and `AggregateError<TError>` flatten correctly.
- **Context immutability**: `Error`, `SimpleError`, and `ValidationError` now use `ImmutableDictionary<string, object>.Empty` or `ImmutableDictionary.CreateRange(context)` so `Context` cannot be mutated by callers.
- **MatchAsync API symmetry**: `MatchAsync` added for `Task<Result<TData>>` and `Task<Result<TData, TError>>` so task extensions match sync `Match` for all three result types.

## Optional gaps (do not affect 10/10)

- **Solution format**: Repo uses `.slnx` (solution folder format); some CI/tooling may expect `.sln` — open `robotico-results.slnx` as the solution.
- **Integration tests**: Not applicable for a pure library; if a host or sample app is added later, integration tests can be added.
