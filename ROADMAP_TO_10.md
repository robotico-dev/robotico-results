# Roadmap to 10/10

Concrete steps to take Robotico.Result from **8.5/10** to **10/10** in implementation seniority.

**Status: 10/10 achieved.** All items below are done. This file is kept for history and reference.

---

## 1. API consistency (quick wins)

### 1.1 ValidationError argument handling
- **Done**: Use `ArgumentNullException.ThrowIfNull(errors)` in the constructor; keep `ArgumentException` only for "empty dictionary" (business rule).
- **Done**: In `ForField(fieldName, params string[] errorMessages)`, use `ArgumentNullException.ThrowIfNull(errorMessages)` and a separate check (or `ThrowIfNullOrEmpty`) for length so null vs empty is consistent with the rest of the codebase.

### 1.2 Combine null checks (optional)
- `Combine` currently doesn’t validate `r1, r2, ...` for null. Because `Result<T>` is a struct, arguments can’t be null, so this is optional; skip unless you want defensive checks for future refactors.

---

## 2. Tests

### 2.1 Guard / invalid-argument tests
- **Done**: Added `ResultGuardTests.cs` asserting:
  - `Result.Error` (void and `Result<TData>`) with null throws.
  - `Ensure` with null `predicate` or null `errorFactory` throws.
  - `Match` / `Tap` / `Recover` with null delegates throw.
  - `Collect` / `ChooseSuccessful` with null `results` throw (for `ChooseSuccessful`, enumeration triggers the check).
  - `ValidationError` constructor with null or empty dictionary, and `ForField` with null/empty arguments, throw as documented.
  - `ResultUtilities.Try` / `TryAsync` with null `func` or `action` throw.
- Ensures the public API enforces preconditions and doesn’t hide bugs.
- *Note*: For iterator methods (e.g. `ChooseSuccessful`), the null check runs when the sequence is enumerated; tests use `.ToList()` to trigger it.

### 2.2 Property-based or stress tests ✅ Done
- Added `ResultLawsTests.cs` with law-style tests (no extra package):
  - Map identity: `r.Map(x => x)` equals `r` (for success).
  - Bind with Success: `r.Bind(x => Result.Success(x))` equals `r`.
  - Error propagation: `Result.Error<T>(e).Map(f)` is still `Result.Error<T>(e)`.
- **Added `ResultPropertyTests.cs` (CsCheck)** — property-based law tests over generated inputs (Gen.Int, Gen.Const for errors): Map identity, Bind-Success identity, error propagation, RecoverWith; void Result and `Result<TData, TError>`.
- This pushes the implementation toward “provably consistent” and is a clear 10/10 differentiator.

---

## 3. Combine API (reduce duplication / extend)

### Option A — Keep 2–4 and document
- In XML and in `docs/design.adoc`, state why only 2–4: “Tuple return type; beyond 4 use `Collect`/`Sequence`.”
- No code change; clarity alone improves the score.

### Option B — Generalize ✅ Done
- Added `ResultUtilities.Combine<T>(IEnumerable<Result<T>> results)` returning `Result<ImmutableArray<T>>`; implemented via `Collect`. Use for 5+ results of the same type.
- Removes duplication and answers “why not 5+?” with “use `Combine(results)`.”
- **Option A also done**: XML `<remarks>` on 2-, 3-, and 4-arg `Combine` overloads point to `Combine(IEnumerable)` and `Collect` for 5+ results.

---

## 4. Documentation and polish

### 4.1 Design doc ✅ Done
- Added “Combining results” subsection in `docs/design.adoc`: when to use `Combine` (2–4 tuple, or N-way same type) vs `Collect`/`Sequence`.

### 4.2 AOT / trimming ✅ Done
- Added to README tagline: “Trim-friendly; no reflection on hot paths.”
- **CI trim validation**: Job `trim-validate` builds the library with `IsTrimmable=true` and `EnableTrimAnalyzer=true`; with `TreatWarningsAsErrors` any trim warning fails the build. Publish job depends on both `build-and-test` and `trim-validate`.

### 4.3 Changelog ✅ Done
- Added `CHANGELOG.md` (Keep a Changelog format); documents Unreleased additions and 1.0.0 initial release.

---

## 5. Checklist summary

| Area              | Action                                              | Impact   |
|-------------------|-----------------------------------------------------|----------|
| ValidationError   | Use ThrowIfNull; separate null vs empty validation   | High     |
| Guard tests       | New test class for null/invalid arguments            | High     |
| Property / laws   | FsCheck/CsCheck or Theory-based law tests            | Very high|
| Combine           | Document 2–4 only, or add N-way overload             | Medium   |
| Design doc        | “Combining results” + optional comparison sentence  | Medium   |
| AOT/trimming      | One-sentence note or CI trim test                   | Low–med  |
| Changelog         | CHANGELOG.md or Releases                            | Medium   |

---

## 6. Order of execution

1. **Do first**: ValidationError consistency + guard tests (fast, high impact). ✅ *Done.*
2. **Next**: Design doc “Combining results” + Combine decision (document or add N-way).
3. **Then**: Property-based or law tests (biggest step toward 10/10).
4. **Finally**: Changelog, optional AOT/trim note.

After 1–3, the library is at **9–9.5/10**. Property-based tests and small polish (changelog, AOT note) get it to **10/10**.

---

## 7. Reassessment (post–10/10)

| Criterion | Before | After |
|-----------|--------|--------|
| Property-based / law coverage | Law tests with fixed data | CsCheck PBT over Gen.Int / errors; void and typed-error variants |
| Trim claim | README only | CI job enforces trim-safe build (IsTrimmable + EnableTrimAnalyzer) |
| Combine API clarity | Design doc | XML remarks on 2–4 arg overloads pointing to N-way Combine/Collect |
| Benchmarks | Map, Bind, Match, Collect | + Combine (2/4, success/error), Try (success/throw), Ensure (pass/fail) |

**Rating: 10/10** — Senior implementation: documented trim validation in CI, property-based law tests, full benchmark coverage, and clear API guidance in XML.
