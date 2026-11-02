Role: implementer
Commit: see the commit that adds this handover (production change and handover in one commit)
Verdict: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond pointing at `00-lead.md` decisions 28 and 29, which are the lead's recorded decisions. I worked from `00-lead.md`, `07-implementer.md`, `14-auditor.md` (findings 1 and 2), and `15-test-implementer.md`. The handed-over failing tests (8 phase-alone rows of `LineBreakPhaseCommentTailStatementTests` and `BlankLinePhaseChangesOnlyBlankLines(StatementsBehindMultiLineCommentTail)`) failed on fd8e1fc as `15-test-implementer.md` describes and pass now. No test file was changed.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakBlockRewriter.cs` | `EnsureStatementsStartOnSeparateLines` (decision 28, option B of 14-auditor finding 1): for a pair where `BlankLineSpacingPolicy.RequiresBlankLineBetweenStatements` is true and the gap already holds a line break, it now asks `TokenGapNormalizer.EnsureTokenStartsLine`, which decides "starts its line" comment-aware (`TokenGapAnalysis.StartsLineAtLeadingTriviaIndex`, SHARED-COMMENT-012) and puts the line break in front of the statement when it stands behind a multi-line comment's closing line. Pairs without a blank-line rule keep today's `HasLineBreakBetween` check, so `A(); /* x⏎ */ B();` and a terminal `break` behind a comment tail stay as on `main` (option A's follow-up). Summary updated (FC-031) | FMT-BREAK-006, SHARED-COMMENT-012, FMT-PIPE-025 |
| `src/Reihitsu.Core/BlankLineSpacingPolicy.cs` | `RequiresBlankLineAfterClosingBrace` (decision 29, finding 2) reads its rule part from `RequiresBlankLineBehindClosingBrace(lastToken.Kind(), IsTerminalDirectSwitchSectionBreak(nextStatement))`; its layout conditions (existing blank line, same line) stay separate. Behavior unchanged. Summary updated | SHARED-BLANK-003, SHARED-BLANK-004 |

The blank-line phase was not changed: with the line break now written by the line-break phase, `BlankLineEditor.InsertBlankLineBeforeToken` finds a terminal line break in front of these statements and adds only the blank line.

## Policy owners

- `rg -n "RequiresBlankLineBehindClosingBrace|RequiresBlankLineAfterClosingBrace|RequiresBlankLineBetweenStatements" src --glob '!*.Test*'`: the closing-brace rule is stated once, in `RequiresBlankLineBehindClosingBrace`; `RequiresBlankLineAfterClosingBrace` (RH5030, RH3208 fix), `RequiresBlankLineBetweenStatements` (line-break phase), and `BlankLineStatementSpacingRewriter` (blank-line phase) all call it.
- `rg -n "StartsLineAtLeadingTriviaIndex|HasTerminalLineBreak" src`: the statement split now reaches SHARED-COMMENT-012's line start through `TokenGapNormalizer.EnsureTokenStartsLine`, the same owner sections, labels, and first statements use; no new copy.

## Tests run

```shell
scripts/build.sh --configuration Debug                       # 0 Warning(s), 0 Error(s)
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~StatementBehindMultiLineCommentTests|FullyQualifiedName~LineBreakPhaseCommentTailStatementTests|FullyQualifiedName~PhaseLineBreakOwnershipTests"
# Passed! - Failed: 0, Passed: 43
scripts/test.sh --configuration Debug --project core --filter "FullyQualifiedName~BlankLineSpacingPolicyTests"
# Passed! - Failed: 0, Passed: 62
scripts/test.sh --configuration Debug
# Analyzer 6402, Formatter 4372, Core 607, Cli 251, Architecture 92, Tooling 125, Playground 17 — all passed, 0 failed
```

`scripts/format.sh` over both changed files: formatted 0 of 2.

## Disputed tests or scope

_None._ The change stays in the two files decisions 28 and 29 name; no new public member, dependency, or helper.

## Architecture findings

_None._ Decision 28's finding (two phases owning the statement-ending line break behind a comment tail) is resolved for every pair a blank-line rule separates: the line-break phase writes it, the blank-line phase adds only the blank line.

## Follow-up candidates

- The existing follow-up "`BlankLineEditor.InsertBlankLineBeforeToken` still writes a line-ending line break when a token sits behind a multi-line comment's closing line" is now unreachable for statements whose pair needs a blank line; it remains for the other callers of that method, unchanged.
- Option A's members (`A(); /* x⏎ */ B();`, terminal `break` behind a comment tail) stay as on `main`, as decision 28 records.
