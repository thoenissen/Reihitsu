Role: spec author
Commit: db24264
Verdict: spec written

## Changed requirements

- RH5401-004 — The fix is withheld when the property stays on several lines — clarified (example only): the third member of the example is renamed from `int C` to `int D`. `int C` inside `class C` is CS0542 (member named like its enclosing type), so the example did not compile. The example now matches `RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzerTests.VerifyFixIsWithheldWhenPropertyStaysOnSeveralLines` line for line. The normative text is unchanged.

The change doc `sdd/changes/2026-11/expression-body-arrow-comment-unsupported.md` does not repeat this example (its RH5401 references are prose only; its `class C` examples declare `A`, `_a`, `_b`), so it is unchanged.

## Examples for the tests

- RH5401-004 — already covered by the test named above, which uses `D`; no test change needed.

## Output questions

_None._

## Internal choices

- Renamed to `D` to match the existing test (lead decision 7), rather than renaming the enclosing type.

## Follow-up candidates

_None._

Check: the example is identical to the test fixture, which compiles; no requirement-ID reference, `Uses` row, or code owner changed, so the architecture tests were not rerun.
