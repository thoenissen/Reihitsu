Role: test implementer
Commit: 6bed9b2 (tests written and run against this head; the spec author's parallel commit 4be9fc4 changes Markdown only)
Verdict: tests written

Repair round 3: the test side of audit finding 7 (`17-auditor.md`), as lead decision 23 routes it. My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond naming the finding.

Expected values come from FMT-STRUCT-028 ("whitespace and line breaks behind the original list's `[` are dropped"; every created list keeps the specifier) and FMT-BREAK-032 (the accessor split in an accessor list over several lines, same split as FMT-STRUCT-028). They are also the outputs head produces, which the auditor measured in one pass under LF and CRLF.

## Tests

A = `src/Reihitsu.Formatter.Test/Unit/StructuralTransforms/AttributeListStructuralTransformTests.cs` (structural-phase output, LF and CRLF, no elastic trivia, FMT-PIPE-021 round trip); B = `src/Reihitsu.Formatter.Test/Regression/Structural/AttributeListStructuralFinalOutputTests.cs` (final output, `AssertRuleResult`).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| A `NothingBehindTheOpeningBracketStaysInTheFirstCreatedList`, new row `LineBreakBehindOpeningBracketInFrontOfSpecifier` (`[⏎        method: A, B]` → `[method: A][method: B]`) | FMT-STRUCT-028 | `AssertAttributeListText` | passes |
| A same test, new row `SpaceBehindOpeningBracketInFrontOfSpecifier` (`[ return: A, B]` → `[return: A][return: B]`) | FMT-STRUCT-028 | `AssertAttributeListText` | passes |
| B `AccessorSplitDropsTheLineBreakBehindTheOpeningBracketInFrontOfTheSpecifier` (`[⏎ method: C, D] get;` in an accessor list over several lines → `[method: C]⏎[method: D]⏎get;`) | FMT-BREAK-032 | `AssertRuleResult` | passes |

The test's summary in A now says "its specifier or attribute" and "also when a target specifier follows them"; its earlier rows are unchanged.

### Falsification

Since the production code exists, the tests cannot fail on this head. To show they guard the finding's line, I applied the auditor's mutation in a throwaway worktree of this head (session scratchpad, removed afterwards; the checked-out production code was never edited): `AttributeListShapeUtilities.cs:143` reverted to `var target = listToSplit.Target;`. Result: `FullyQualifiedName~AttributeListStructural` 2 failed, 99 passed.

| Test under mutation | Observed | Expected |
|---|---|---|
| A `LineBreakBehindOpeningBracketInFrontOfSpecifier` | `[        method: A][        method: B]` (LF; CRLF is not reached) | `[method: A][method: B]` |
| B `AccessorSplitDropsTheLineBreakBehindTheOpeningBracketInFrontOfTheSpecifier` | `[        method: C]⏎        [         method: D]…` | `[method: C]⏎        [method: D]…` |
| A `SpaceBehindOpeningBracketInFrontOfSpecifier` | passes | — |

The `[ return: A, B]` row does not discriminate the mutation. When the space is on the same line, Roslyn attaches it to the `[` token's trailing trivia, not the specifier's leading trivia, so the open-bracket branch drops it. Only a line break behind `[` puts whitespace into the specifier's leading trivia. I kept the row because the auditor asked for it and it checks a spec member, but finding 7 is guarded by the other two tests.

## Not covered, and why

- **Code-fix side**: the specifier branch is formatter-only (`BuildSplitAttributeLists`); the RH55xx split is a separate implementation (follow-up 6), and no code-fix behavior changes in this round.
- **`[⏎ param: …]` / `[⏎ typevar: …]`**: not split (FMT-STRUCT-029, FMT-BREAK-032), so they cannot reach the branch; the auditor proposed `method:` for the accessor row for this reason.
- **Failure classes**: FC-017 (finding 7): now covered on both owners that reach the branch, declaration and accessor. FC-010: LF/CRLF and the round trip in A, and LF/CRLF plus phase stability through `AssertRuleResult` in B. FC-016: covered with a specifier as well. The other classes are as in `13-test-implementer.md`; this round changes no production code, so none of them reaches new code.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~AttributeListStructural"
```

Result on head: 101 passed.
