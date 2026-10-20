Role: test implementer
Commit: c5b4bcf999cb8523816f4e8005b18d0bec8171e9
Verdict: tests written

## Scope

This round repairs finding 1 of `42-auditor.md` (low, FC-031). The tagged test named in that finding had a summary and a name that still claimed parentheses are added "only where the comparison would bind differently". RH3001-004 no longer makes that claim; spec author b4b7cb9 dropped it. Only the summary and the name changed. The assertions, the inputs, and the `[Requirement("RH3001-004")]` tag are unchanged, and no production code changed.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3001NotOperatorShouldNotBeUsedAnalyzerTests.FixParenthesizesTheComparisonWhereItWouldBindDifferently` (was `…TheComparisonOnlyWhereItWouldBindDifferently`) | RH3001-004; FC-031 | `Verify(testCode, fixedCode)`, unchanged | pass |

**The new summary** covers only the test's own three inputs:

- the comparison is parenthesized as the right operand of `==`;
- no parentheses are added as the left operand of `&&`;
- the parentheses of an already parenthesized operand are kept.

**Where the old name was used.** `rg` over `src/`, `sdd/specs/`, `sdd/changes/`, `squad/`, and `documentation/` found the old name only in that test file. Build outputs under `bin/` and `obj/` also contain it; they are stale artifacts and not tracked. Handovers under `sdd/work/` still quote the old name as a historical record, and I left them unchanged.

## Scenarios (bug fix)

None. The repair changes only the summary and the name.

## Not covered, and why

- **The two untagged tests that the finding cites as contradicting the old summary** (`FixOfNegationAfterUnsignedRightShiftAssignmentAddsParentheses`, `FixOfNegationAfterNullCoalescingAssignmentAddsParentheses`). They are unchanged. Their summaries describe what they pin.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Clarity.RH3001NotOperatorShouldNotBeUsedAnalyzerTests"
scripts/test.sh --project architecture
```

**Results on c5b4bcf.** The RH3001 class passed 34/34 and the architecture tests passed 72/72. `scripts/format.sh` changed nothing in the test file (0 of 1 formatted).
