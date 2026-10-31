Role: test implementer
Commit: a1ca9d4
Verdict: tests written

This handover settles the dispute in `08-implementer.md` about `HorizontalSpacingRequirementTests.OneSpaceAfterEachOfNestedFixedKeywords`. The prompt carried no intended fix.

## Re-derivation

FMT-INDENT-004 lists `fixed` among the statements whose unbraced embedded statement is indented one level deeper than its owner. In `fixed (…)` / `fixed (…)` / `{ }`, the second `fixed` statement is the embedded statement of the first. Its line and its block therefore belong at column 12, not column 8.

My expected output kept them at column 8. That is wrong against FMT-INDENT-004. The implementer is right.

The test targets FMT-SPACE-012, and its point is two `fixed` tokens in one input (FC-026). The input now already places the inner statement and its block at column 12, so the only difference between input and expected output is the space behind each `fixed`. The test carries no FMT-INDENT-004 tag, because indentation is not what it verifies.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `src/Reihitsu.Formatter.Test/Regression/Spacing/HorizontalSpacingRequirementTests.OneSpaceAfterEachOfNestedFixedKeywords` (corrected: inner `fixed` and its block at column 12 in both the input and the expected output) | FMT-SPACE-012 (FC-026) | `AssertRuleResult` | green on dd2f609 |

`scripts/test.sh --project formatter --filter "FullyQualifiedName~HorizontalSpacingRequirementTests"` gives `Passed: 22, Failed: 0` on a1ca9d4, with the implementer's dd2f609 in the history.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~HorizontalSpacingRequirementTests"
```
