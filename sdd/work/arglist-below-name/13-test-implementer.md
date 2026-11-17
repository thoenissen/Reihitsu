Role: test implementer
Commit: 31a1502
Verdict: tests written

Repair round 1 (audit 11, finding 1, "Missing" row of *Assertion adequacy*), on the maintainer's answer 5A (`00-lead.md`). Inputs: `00-lead.md`, `11-auditor.md` finding 1, `12-spec-author.md`, `05-test-implementer.md`, FMT-BREAK-039's new example in `sdd/specs/formatter/line-breaks.md`, change 7 of `sdd/changes/2026-11/argument-list-below-name.md`. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

Expected values (FC-035): the requirement example's output verbatim (extracted from `line-breaks.md`); change 7's RH5302 *after* verbatim; every other expected value is the output of `main`'s CLI (worktree of eb89003, the run's merge base, `scripts/format.sh`) for the same input with only the line break in front of the argument list's `(` removed by hand. The branch CLI gives the same outputs.

Expected now: **all new tests pass on the branch** (the answer 5A matches the implementation; these tests lock it). Each was watched failing on `main` (the new tests copied into an eb89003 worktree): all 7 formatter tests and both RH5302 cases fail on the first-pass / fix-output assertion — the chain members on the one-argument-per-line list, the non-chain members on the kept gap — with no compile, markup, or stability failure.

## Tests

`src/Reihitsu.Formatter.Test/Regression/LineBreaks/ArgumentListLineStartTests.cs` (`AssertRuleResult` = LF and CRLF, second pass, per-phase stability):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RequirementExampleJudgesListAsTheLineItBecomesAroundArgumentListInNestedConstruct (the new example: hole chain + non-chain, query clause, array initializer) | FMT-BREAK-039 (FC-022, FC-027) | AssertRuleResult | pass |
| ChainArgumentListInInterpolationHoleOfLaterArgumentLeavesListOnOneLine (`Baz(2, $"{x.ToString⏎()}")`) | FMT-BREAK-039 (FC-022) | AssertRuleResult | pass |
| ChainArgumentListInQueryClauseLeavesListOnOneLine (`select v.ToString⏎()`) | FMT-BREAK-039 (FC-027) | AssertRuleResult | pass |
| ArgumentListInArrayInitializerLeavesListOnOneLine (`new[] { Bar⏎(1) }`, non-chain) | FMT-BREAK-039 (FC-027) | AssertRuleResult | pass |
| ChainArgumentListInStackallocInitializerLeavesListOnOneLine (`stackalloc int[] { x.GetHashCode⏎() }.Length`) | FMT-BREAK-039 (FC-027) | AssertRuleResult | pass |
| ListWithLineBreakBetweenArgumentsKeepsOneArgumentPerLineAroundJoinedInterpolationHole (`Baz($"{Bar⏎(1)}",⏎2)` → `2` aligned under `$`) | FMT-BREAK-039 (FC-003: "only line break" boundary) | AssertRuleResult | pass |
| CollectionExpressionInListElementIsJudgedAsWrittenAroundJoinedArgumentList (`Baz([Bar⏎(1), 2], 3)` → one element and one argument per line) | FMT-BREAK-039 (FC-002, FC-003: the requirement's stated other side) | AssertRuleResult | pass |

`src/Reihitsu.Analyzer.Test/Formatting/RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzerTests.cs`:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| VerifyFixJoinsArgumentListInInterpolationHoleAndKeepsOperandListOnOneLine (LF, CRLF; change 7's RH5302 example) | FMT-BREAK-039 (FC-007) | VerifyCodeFixWithEndOfLine | pass |

Existing tests that assert an output change 7 changes: none. Searched the test projects for an argument list's `(` below its name inside an interpolation hole, a query clause, or an array/`stackalloc` initializer of a list element (`rg -n -U '\{[^}\n]*\w\n\s*\(' src --glob '*.cs'` and `rg -n -U 'select [^\n]*\w\n\s*\(' src --glob '*.cs'`, test projects, outside bin/obj, plus the earlier scan of `05-test-implementer.md`). Hits other than this round's tests: `ArgumentListInInterpolationHoleJoins` (hole in a `return`, no surrounding list), the dictionary element initializer of the second requirement example, and the run's RH5301/RH5303/RH5305 fix tests (object, collection initializers and collection expressions, not a list around a nested construct) — none asserts a list layout change 7 changes; all unchanged. This agrees with the auditor ("No test asserts any member").

## Scenarios (bug fix)

_Not a bug fix._

## Not covered, and why

- `Baz($"{x.⏎ToString()}", 2)` (a member-name split, not an argument-list gap, inside a hole): decision 8 drops it; asserting `main`'s one-argument-per-line output would lock a layout the run does not specify.
- Non-chain query clause and chain array initializer as separate tests: both are in the requirement example test; the chain/non-chain arms of each nested construct reach the same owner (`ArgumentListLineStartRewriter`) only partly — the separate tests above pick one arm per construct plus the example, which covers both arms for the hole, the query clause, and the array initializer; `stackalloc` is covered for the chain arm only (rare input; the non-chain arm is the same gap in the same initializer kind as `new[] { Bar⏎(1) }`).
- Code fix for the non-chain member: change 7 states one code-fix example; the fix reaches the layout only through the formatter (change 6), whose non-chain cases are pinned above.
- Comment/directive in the gap inside a nested construct: the blocking predicate is the shared one pinned by the existing FMT-BREAK-039 comment, directive, and disabled-text tests; nothing in change 7 adds a condition on it.
- FC-013/FC-014: no rule reports the shape (RH5102 reports nothing on these inputs, `12-spec-author.md`).
- FC-038 to FC-043: no shape declared.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~ArgumentListLineStartTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5302LogicalExpressions"
```

Both green on the branch (50 and 34 passed).

## Follow-up candidates

_None._ Note for the lead: `origin/main` has moved to 1102aa5 since the audit (merge base still eb89003).
