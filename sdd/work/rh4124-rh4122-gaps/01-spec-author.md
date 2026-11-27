Role: spec author
Commit: d127281
Verdict: change doc written

Change doc: `sdd/changes/2026-11/single-letter-range-variables-and-extension-receivers.md` (Status: Draft). Specs: `sdd/specs/analyzer/RH4124.md`, `sdd/specs/analyzer/RH4122.md`, a non-goal line in `sdd/specs/analyzer/RH4125.md`, ledger `sdd/specs/requirement-ids.txt`. Rule pages: `documentation/rules/RH4124.md` and `documentation/rules/RH4122.md`, *Description* only — the report grows in the normal case; the examples stay.

My prompt carried no intended fix or hoped-for verdict beyond the issue's recorded maintainer decisions, which the spec follows.

## Changed requirements

- RH4124-003 — Single-letter query range variable names are reported — added. Boundary by cause: a query clause that declares a range variable with an identifier of its own (`from` incl. the first clause and an explicitly typed one, `let`, `join`, `into` of a `join` and of a query continuation behind `select`/`group`); one diagnostic per declaration on its identifier; uses in later clauses are not reported. `_` exempt by SHARED-SINGLE-001 (`from _ in others` compiles and is a range variable named `_`); `@a` counts as `a`.
- RH4122-004 — Single-letter receiver parameters of extension blocks are reported — added. Named receiver only (also generic `extension<T>(T[] a)`), one diagnostic per block on the identifier; `extension(int)` without a name is not reported (example on each side, FC-002).
- RH4124 and RH4122 summary lines, RH4124 non-goals (local and foreach variables belong to RH4125/RH4126), RH4125 non-goals (range variables belong to RH4124) — clarified.

`scripts/test.sh --project architecture`: one expected failure, `EveryRequirementOfAnEnforcedSpecIsCovered` — RH4122-004 and RH4124-003 have no tagged test yet; the test implementer adds them. Everything else passes.

## Examples for the tests

RH4124-003 (both examples; the negative one has longer names), RH4122-004 (both examples; the positive one also holds an extension method `this int s`, already reported under RH4122-001 — keep it or drop it from the test, it only shows the two forms side by side). All examples compile (scratch net11 project, LangVersion preview, extension blocks need C# 14 — RH4105's tests already compile `extension(...)` with the default test setup). Today's analyzer cannot report them: RH4124 registers only `ParameterSyntax` of anonymous functions (query clauses have none), and RH4122's `IsSupportedParameter` rejects a parameter whose containing symbol is the extension type, so the tests reproduce the gap.

## Output questions

**1 · RH4124 · Single-letter lambda and anonymous method parameter names should not be used — title and message on a query range variable**

Input:
```csharp
var doubled = from x in values
              select x * 2;
```

Before (`main`): not reported.

After, A: reported on `x` with the unchanged title and message "Single-letter lambda and anonymous method parameter names should not be used".

After, B (recommended): reported on `x` with title and message "Single-letter lambda, anonymous method, and query range variable names should not be used"; the rule page heading, the spec title, and the package README row (`src/Reihitsu.Analyzer.Package/README.MD`) follow.

Why B: the message then names what is reported at `from x`, where no lambda is written. A is consistent with "checked like lambda parameters" and changes no text; the decision does not need the answer to proceed — the tests assert the resource's `MessageFormat`, not a literal — so the run can continue, and B is a wording repair (resource `<value>`, rule page H1, README row, spec title) the spec author writes after the answer.

Besides this, the change doc's visible output changes (the two new reports and the two rule-page description sentences) follow the maintainer's recorded decisions; the lead decides whether they need asking again.

## Internal choices

- New requirements RH4124-003 and RH4122-004 instead of widening RH4124-001/RH4122-001, so the new tests carry their own tags; alternative: extend the -001 lists.
- No SHARED-DIAG-007 `Uses` row for the nameless receiver: it is simply not reported (no name to be a single letter), as RH4105-004 does for casing; alternative: cite SHARED-DIAG-007, which is about locating, not suppressing.
- Code owners unchanged; the RH4124 analyzer's class name stays (internal); alternative: rename it with a title change if 1B is chosen — not needed for B either.

## Follow-up candidates

_None._
