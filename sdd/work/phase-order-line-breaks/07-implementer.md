Role: implementer
Commit: b48a47e
Verdict: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict. I worked from `00-lead.md` (maintainer answer 1, decisions 1–19), handovers 01–06, the change doc, and the specs. Every handed-over test failed on 297d497 as `04-test-implementer.md` and `06-test-implementer.md` describe, and passes on b48a47e. No assertion, test input, or expected output was changed. One test harness line was changed, see *Disputed tests or scope*.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/FormattingPipeline.cs` | `CreatePhases`: LineBreak → SwitchCaseBrace → BlankLine. Summary states the new order and the line-break/blank-line rule | FMT-PIPE-001, FMT-PIPE-025 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakContainedBlockRewriter.cs` | Removed `MoveTrailingConditionCommentToOwnLine`, `IsElseIfBranch`, the `isElseIfBranch` capture in `Visit`, and the now unused `_context` field and constructor parameter. Type and `NormalizeIfStatement` summaries updated | FMT-BREAK-005 (retired), FMT-BREAK-036 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/LineBreakPhase.cs` | Constructor call without the context | — |
| `src/Reihitsu.Formatter/Data/Enumerations/RootConstruct.cs`, `src/Reihitsu.Formatter/Utilities/ReihitsuFormatterHelpers.cs` | `RootConstruct.ElseIf` and its capture removed. The other flag values are unchanged | FMT-PIPE-024 |
| `src/Reihitsu.Core/BlankLineSpacingPolicy.cs` | New public `RequiresBlankLineBehindClosingBrace`, `RequiresBlankLineForStatementKind` (moved verbatim from the blank-line rewriter's private `IsAfterClosingBrace`/`NeedsBlankLineForStatementKind`), and `RequiresBlankLineBetweenStatements` (both checks, for two statements of one list) | FMT-BLANK-001 to FMT-BLANK-005, FMT-BREAK-006 |
| `src/Reihitsu.Formatter/Pipeline/BlankLines/Rewriter/BlankLineStatementSpacingRewriter.cs` | Reads the two moved predicates from `BlankLineSpacingPolicy`. The private copies are deleted | FMT-BLANK-001 to FMT-BLANK-005 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakBlockRewriter.cs` | `EnsureStatementsStartOnSeparateLines`: the empty-statement exemption applies only where `BlankLineSpacingPolicy.RequiresBlankLineBetweenStatements` is false | FMT-BREAK-006 |
| `src/Reihitsu.Formatter/Pipeline/SwitchCaseBraces/Rewriter/SwitchCaseBraceRewriter.cs` | `AddBraces`: the first statement's carried leading trivia keeps the blank lines in front of its first comment (`keepBlankLinesAboveFirstComment: true`, as for the terminal `break`). Both comments now say that the blank-line phase decides those blank lines after this phase | FMT-SWITCH-016, FMT-SWITCH-007 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/ChainLineBreakRewriter.cs` | Removed `BreaksLineBehindMultiLineCommentTail`, whose only reader was the blank-line phase. The `GetWrappedLayoutLineBreaks` remark is rewritten (decision 8) | FMT-BLANK-018, FMT-PIPE-025 |
| `src/Reihitsu.Formatter/Pipeline/BlankLines/Utilities/BraceBlankLinePolicy.cs` | `FindTokenLineStartIndex` no longer predicts the chain line break. It and `FindGluedCommentIndex` lose the now unused `context` parameter. The `RemovesBlankLinesBehindOwnLineComment` remark no longer says that the line-break phase runs later | FMT-BLANK-018, FMT-BLANK-020 |
| `src/Reihitsu.Formatter/Pipeline/BlankLines/Rewriter/BlankLineTokenCleanupRewriter.cs`, `.../BlankLineTriviaBoundaryRewriter.cs` | Call sites without `context` | — |
| `src/Reihitsu.Formatter/Pipeline/BlankLines/Utilities/BlankLineEditor.cs` | `EnsureBlankLineBeforeSection` summary: the line-break phase has already started the section's line. The body is unchanged | FMT-BLANK-004 (FC-031) |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Utilities/TokenGapNormalizer.cs` | Summary: the blank-line phase decides the kept blank lines after this phase, not before it | FMT-BREAK-030 (FC-031) |
| `src/Reihitsu.Formatter.Test/Unit/LineBreaks/ContainedBlockRewriterTests.cs` | Harness line only: `new LineBreakContainedBlockRewriter(bracePlacer, …)` without the removed context argument | — |

## Policy owners

- Statement-kind blank-line rule. `rg -n "case YieldStatementSyntax|YieldReturnStatement or SyntaxKind.YieldBreakStatement" --type cs -g '!*Test*'` finds `src/Reihitsu.Core/BlankLineSpacingPolicy.cs` (the one owner) and `SwitchCaseBraceRewriter.cs:395`. The second hit is an unrelated local-name walk. Both phases read `BlankLineSpacingPolicy`. The analyzers' per-rule bases (RH5010 and others) keep their own reporting conditions, as on `main` (see *Follow-up candidates*).
- Chain line-break prediction and the `else if` fact. `rg -n "BreaksLineBehindMultiLineCommentTail|IsElseIfBranch|RootConstruct.ElseIf" --type cs` returns no hits.
- Carried trivia across new braces. `rg -n "GetCarriedLeadingTrivia" --type cs` finds `BlockWrapTriviaUtilities` (owner), `SwitchCaseBraceRewriter` (both calls now `true`), and `ControlFlowBraceTransform` (`false`, unchanged; it is a structural transform and outside FMT-SWITCH-016).

## Tests run

All runs use `--configuration Debug` on b48a47e.

- `scripts/test.sh --project formatter` gave `Passed! - Failed: 0, Passed: 4343, Total: 4343`.
- `scripts/test.sh --project analyzer` gave `Passed! - Failed: 0, Passed: 6402, Total: 6402`. That includes RH5405/RH5406/RH5407 and `RH5104CommentsMustBeOnTheirOwnLineFormatterTests`.
- `scripts/test.sh --project core` gave `Passed! - Failed: 0, Passed: 563`.
- `scripts/test.sh --project architecture` gave `Passed! - Failed: 0, Passed: 92`. That includes `OwnershipMapMatchesRuntimePhaseOrder` and `RequirementCoverageTests`.
- `scripts/test.sh --project cli` gave `Passed! - Failed: 0, Passed: 251`.
- `scripts/format.sh` over the 16 changed `.cs` files formatted 0 of 16 files.
- No new warning in the Debug build. The pre-existing S1125 warnings are in `FormatCommandHandler.cs`, which this run does not touch.

## Disputed tests or scope

- **Test harness line.** `ContainedBlockRewriterTests.ExecuteContainedBlockRewriter` built the rewriter with the `FormattingContext` that only the removed move read. I dropped that argument rather than keep an unused field. No assertion, input, or expected value changed. If the lead prefers to leave the test file untouched, the alternative is to restore the constructor parameter and keep an unused field.
- **New public Core members.** There are three new public members on `BlankLineSpacingPolicy`, a shared helper with analyzer consumers, inside the component the run already changes (decision 6). `RequiresBlankLineForStatementKind` and `RequiresBlankLineBehindClosingBrace` were moved, not newly written. Decision 14 applies: they live on the existing `BlankLineSpacingPolicy`, not in a new type. `BlankLineSpacingPolicy` is owned by `sdd/specs/shared/blank-line-spacing.md`, while the conditions belong to FMT-BLANK-001 to FMT-BLANK-005. The spec checker should confirm that the code-owner listing names this location. The Core tests cover none of the three directly; the formatter tests reach them through both phases.
- **FMT-SWITCH-016 and comment-free trivia.** The wrap still drops the leading trivia of the first statement and of the terminal `break` when that trivia holds no comment (`BlockWrapTriviaUtilities.GetCarriedLeadingTrivia` returns nothing then). I read "every blank line in the trivia it carries" as the trivia the wrap carries, which is trivia with a comment. Carrying comment-free blank lines too, such as `B();`, a blank line, then `break;`, would hand the blank-line phase a blank line between `}` and the terminal `break`. Nothing I checked decides whether that blank line would stay, so it could change output. The handed-over tests pass under this reading. If the spec means every blank line in that gap, this is a spec-wording question, not a code change.

## Architecture findings

- **The switch-brace phase still places its braces itself.** Components: `SwitchCaseBraceRewriter.AddBraces` and the line-break phase (`LineBreakBlockRewriter.VisitSwitchSection` → `BracePlacer.NormalizeContainedBlock`, FMT-BREAK-008). Shared decision: which lines the braces of a section's block start. `AddBraces` writes its own end-of-line trivia around the new braces, through `BlockWrapTriviaUtilities.GetCarriedTrailingTrivia(..., endsLine: true)`, the leading `eolTrivia`, and `IsLastLineEndedByFollowingToken`. It does not call `BracePlacer`. The outputs agree (FMT-SWITCH-015 tests pass), so no input shows a disagreement. It is still a second statement of the placement, as FMT-SWITCH-015's Why names it, and `AGENTS.md`'s `SwitchCaseBracePhase` row lists `BracePlacer` as a guard (decision 15). I made the narrowest change and did not restructure. Routing the inserted block through `BracePlacer` needs more than a swap. A section without a terminal `break` would still need the token behind the section (the next label or the switch's `}`) moved to its own line, which lies outside the section, and FMT-SWITCH-007's comment carry would have to be re-proved. Either the structure changes in this run, or the `AGENTS.md` row is corrected and the structural change becomes a follow-up. That decision is the lead's.

## Follow-up candidates

- `ControlFlowBraceTransform` (structural transforms) still drops the blank lines in front of the first comment of a body it braces (`keepBlankLinesAboveFirstComment: false`). After the reorder the blank-line phase runs after it too, so the same carry as the switch wrap would be possible. This is outside FMT-SWITCH-016, and I did not check its output. I note it only as a candidate.
- `src/Reihitsu.Formatter.Test/Regression/LineBreaks/AutoPropertyTrailingCommentTests.cs:233`: the spec author found that this test comment states the old phase order. It is a test file, not mine to change.
- `BlankLineEditor.InsertBlankLineBeforeToken` still writes the line break that ends a line when the gap does not end its line. After the reorder, statements and sections no longer reach that path. It remains for a token behind a multi-line comment's closing line. Its body is unchanged and the observer tests pass. Whether that case is also the line-break phase's is not part of this run.
