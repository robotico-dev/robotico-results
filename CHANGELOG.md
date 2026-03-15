# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- `ResultUtilities.Combine<T>(IEnumerable<Result<T>> results)` — N-way combine for many results of the same type; returns `Result<ImmutableArray<T>>`. First error wins (same semantics as `Collect`).
- Design doc subsection "Combining results" — when to use tuple `Combine`, N-way `Combine`, and `Collect`/`Sequence`.
- Guard tests (`ResultGuardTests`) — null/invalid argument checks for public API.
- Law tests (`ResultLawsTests`) — Map identity, Bind-Success identity, error propagation for Map/Bind.
- Property-based tests (`ResultPropertyTests`, CsCheck) — functor/monad laws over generated inputs (Map identity, Bind-Success identity, error propagation, RecoverWith; void and `Result<TData, TError>`).
- CI job `trim-validate` — build library with `IsTrimmable` and `EnableTrimAnalyzer`; fails on trim warnings (validates trim-friendly claim).
- XML remarks on `Combine` (2-, 3-, 4-arg) — point to `Combine(IEnumerable)` / `Collect` for 5+ results of the same type.
- Benchmarks: `ResultCombineBenchmarks` (Combine 2/4 success, one error), `ResultTryEnsureBenchmarks` (Try success/throw, Ensure pass/fail).

### Changed

- `ValidationError` constructor and `ForField(string, params string[])` now use `ArgumentNullException.ThrowIfNull` for null parameters; empty validation is a separate `ArgumentException`.

## [1.0.0] — Initial release

- Result types: `Result`, `Result<TData>`, `Result<TData, TError>` (readonly structs).
- Map, Bind, MapError, Match, Tap, TapError, Recover, RecoverWith, Ensure.
- Collect, Sequence, ChooseSuccessful.
- Combine (2, 3, 4 results → tuple).
- Try / TryAsync, ExpectSuccess, `ResultErrorException<TError>`.
- Error types: IError, Error, SimpleError, ValidationError, ExceptionError, DomainError, AggregateError.
- Task extensions: MapAsync, BindAsync, MatchAsync, TapAsync, RecoverWithAsync, GetValueAsync, ExpectSuccessAsync.
- Explicit conversions: FromVoid, From, FromWithIError.

[Unreleased]: https://github.com/robotico/robotico-results/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/robotico/robotico-results/releases/tag/v1.0.0
