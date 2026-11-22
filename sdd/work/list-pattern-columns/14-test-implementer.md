Role: test implementer
Commit: 637b531
Verdict: tests written

Repair round 1, decision 8 of `00-lead.md`: bring the tests that copy the FMT-INDENT-026 examples corrected in e9b6e21 in line with the corrected text. No expected column changed; both tests passed before and pass after.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `PatternAnchorBehindWrapperTests.SpecExampleBracesBehindAndOrNotStandUnderTheOutermostPatternsFirstToken` (8A example) | FMT-INDENT-026 | `AssertRuleResult` | passes; `M` and `N` now take `P o` in input and expected, as in the spec. `PropertyType` + each literal equals the spec's example blocks character for character (checked by script, modulo the raw-string indentation) |
| `PatternAnchorBehindWrapperTests.SpecExampleTypedAndParenthesizedPatternsBehindAndOrNotStandUnderTheOutermostPatternsFirstToken` (9A example, constants `TypedAndParenthesizedInput` / `TypedAndParenthesizedExpected`) | FMT-INDENT-026 | `AssertRuleResult` | passes; both constants regenerated from the spec's blocks: class `S` added, `L(S a, S b)`, `null or P { … }` before `not Q { … }` in the arm and in `switch (o)`, `case not (1 or 2)` last in `switch (i)`. Equal to the spec blocks character for character |

The change doc `sdd/changes/2026-11/pattern-anchor-behind-wrapper.md` carries the same 8A and 9A blocks as the spec, so the two tests match it too. The RH5206 code-fix test already reads `B: 4` (lines 616, 645, 695, 731 of `RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzerTests.cs`), as the corrected pair in `12-spec-author.md`; no change.

## Scenarios (bug fix)
_Not applicable: repair round on spec-example tests._

## Not covered, and why
- FMT-INDENT-026's first example (`bool M(string value)`, `value is ( { Length: > 0 } );`): no test copies it. Searched `rg` for `M(object value)`, `M(string value)`, `return value is$`, and every `Length: > 0` in the test projects, and every test tagged FMT-INDENT-026. Its columns (`(` and `)` under `is`, `{` four right, `Length` one level deeper) are pinned by `RecursivePatternAlignmentTests.ParenthesizedRecursivePatternBreaksParenthesisOntoOwnLine`, whose input differs (`object value`, `Length` and `Count` subpatterns), so no test was added; the type change `object` to `string` affects no test.
- Other tests in `PatternAnchorBehindWrapperTests` that use `M(object o)` / `L(object[] a)` are the file's own focused tests, not copies of a spec example; formatter tests do not compile their input, and the decision covers only spec-example copies, so they are left as they are.

## Commands
```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.Indentation.PatternAnchorBehindWrapperTests"
scripts/test.sh --project formatter   # full: 7188 passed, 0 failed
scripts/test.sh --project analyzer    # full: 11939 passed, 0 failed
```

## Follow-up candidates
_None._
