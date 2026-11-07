Role: implementer
Commit: (this commit)
Verdict: tests pass

Repair round 1, audit findings #1 and #2 (`12-auditor.md`), decisions 8, 9, 11 (`00-lead.md`). The prompt carried no intended fix, suspected cause, or hoped-for verdict. No test was changed.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/GenericConstraintLineBreakRewriter.cs` | `IsWrapped`, `JoinGap`, and the separator test in `PlaceOneConstraintPerLine` decide on `SyntaxTriviaUtilities.AreSeparatedByEndOfLine` (the SHARED-BLANK-008 line break; a line break inside a multi-line comment separates nothing) instead of `TokenGapUtilities.HasLineBreakBetween`. `JoinUnstructuredWraps` keeps its gap selection and refusal filter and hands the gaps to the shared `LineBreakTriviaUtilities.JoinGaps`. Class remarks and the summaries of `JoinUnstructuredWraps`, `IsWrapped`, `JoinGap`, `PlaceOneConstraintPerLine` rewritten to line-start facts; the false sentence on the colon in `IsWrapped`'s summary now states what holds after the joins. Unused `Pipeline.Core.Utilities` using removed. | FMT-BREAK-037 (finding #1, decision 8) |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Utilities/LineBreakTriviaUtilities.cs` | New `JoinGaps<TNode>(node, gaps)`: the replacement-map batch join (chained joins, `CreateJoinedTrailingTrivia`, `RemoveLeadingEndOfLineAndWhitespace`, `ReplaceTokens`) moved here from the two rewriters. The caller selects and filters the gaps. | finding #2, decision 9 (no output change) |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/ListElementInteriorJoinRewriter.cs` | `JoinInteriorWraps` calls `JoinGaps` with a new lazy `SelectJoinableGaps` iterator that keeps the per-token cancellation check and `CanJoin`, in the same order. | FMT-BREAK-015 (finding #2, no output change) |

Case (e) (`IEquatable</* a⏎ b */T>`) now stays as written, as decision 11 and test `TypeArgumentBehindMultiLineCommentClosingLineDoesNotWrapClause` require: no line break separates its tokens. Case `S` gets its line break appended to the comma's trailing trivia behind the comment, as before; with the line-start test the comma now counts as not yet broken.

## Policy owners
- Line-break-in-gap fact: `rg -n "HasLineBreakBetween|AreSeparatedByEndOfLine" src/Reihitsu.Formatter` — the constraint rewriter now uses only `AreSeparatedByEndOfLine` (Core, SHARED-BLANK-008), as `ListElementInteriorJoinRewriter.CanJoin`, `ChainLineBreakRewriter`, `CollapseTokenToSameLine`'s empty-token path, and the indentation `LayoutComputer` do. `HasLineBreakBetween` remains in `LineBreakListRewriter`, `LineBreakBlockRewriter`, `MemberLineStartRewriter`, `BracePlacer`, `TokenGapNormalizer`; not changed (their requirements are not this run's).
- Batch join bookkeeping: `rg -n "CreateJoinedTrailingTrivia|RemoveLeadingEndOfLineAndWhitespace" src --glob '!*Test*'` — one replacement-map owner now (`LineBreakTriviaUtilities.JoinGaps`), used by both rewriters. `ChainLineBreakRewriter.CloseGap` keeps its own pending-token map (pre-existing on `main` at 99ae644; it shares that map with the chain's line-break insertions, so it does not fit `JoinGaps` without restructuring) — follow-up candidate below. The other hits are single-gap joins, not batch loops.

## Tests run
```shell
scripts/build.sh --configuration Debug                     # Build succeeded, 0 Warning(s), 0 Error(s)
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~ConstraintClause"
# Passed! - Failed: 0, Passed: 74, Total: 74
scripts/test.sh --configuration Debug
# Analyzer 7340/7340, Formatter 4589/4589, Core 660/660, Cli 251/251, ArchitectureTests 115/115, Tooling 126/126, Playground 17/17 — all passed
```
`scripts/format.sh` over the three changed files: formatted 0 of 3.

## Disputed tests or scope
`LineBreakTriviaUtilities.JoinGaps` is a new member of a shared helper with two consumers; decision 9 asked for it.

## Architecture findings
_None._

## Follow-up candidates
- `ChainLineBreakRewriter.CloseGap`/`GetPendingToken` keeps a third copy of the chained-join replacement-map bookkeeping (pre-existing on `main`). Folding it into `JoinGaps` needs the chain rewriter to separate its joins from its line-break insertions; internal refactor, no output change.
