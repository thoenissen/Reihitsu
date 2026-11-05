Role: implementer
Commit: see the commit that adds this file (parent 90d44e2e)
Verdict: tests pass

Repair round 1, lead decisions 21 and 22 (code part): auditor findings 1, 2 and 4 (`14-auditor.md`) and architecture decision 21. My prompt named the work and carried no intended fix, suspected cause, or hoped-for verdict. No test was changed.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Core/Utilities/TokenGapUtilities.cs` | The one statement of "the gap holds a line break" (a line break inside a comment that spans lines counts). `HasLineBreakBetween` keeps its body, its summary now says it is that statement; new overload `HasLineBreak(SyntaxTriviaList)` asks the same question of a gap a transform builds, through the same `TokenGapAnalysis` | Decision 21 |
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/FieldDeclarationSplitTransform.cs` | `BuildTrailingTrivia` also writes the line break behind the comments that trailed a comma when the gap it built holds a line break by the shared statement (a `/* */` that spans lines), because the line-break phase does not break such a gap. One-line comments and no comment still leave the break to the line-break phase. Summary rewritten | FMT-STRUCT-026 (finding 1) |
| `src/Reihitsu.Formatter/ReihitsuFormatter.cs` | `FormatSplitFieldInContext` decides "a member shares the target's first/last line" with `TokenGapUtilities.HasLineBreakBetween` (the line-break phase's statement, used by `MemberLineStartRewriter`) instead of `SyntaxTriviaUtilities.AreSeparatedByEndOfLine`; comment added. The private `GetColumn` copy is deleted and `SyntaxTokenPositionUtilities.GetColumn` is called | FMT-PIPE-029 (finding 2), finding 4 |

The member-in-front case `m` (first field at column 11) needed no change of its own: with the split's line break, the second field starts its line and `memberIndentation` reads the member column.

## Policy owners

- `rg -n "HasLineBreakBetween|AreSeparatedByEndOfLine|OfTriviaRange\(.*\)\.HasLineBreak" src/Reihitsu.Formatter/ReihitsuFormatter.cs src/Reihitsu.Formatter/Pipeline/StructuralTransforms src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/MemberLineStartRewriter.cs`: the three components of decision 21 now all go through `TokenGapUtilities` (`MemberLineStartRewriter:96`, `ReihitsuFormatter.cs:472/474`, the transform through `HasLineBreak`). The remaining `AreSeparatedByEndOfLine` at `ReihitsuFormatter.cs:186` (`FormatNodeInDocumentAsync`, `keepsLeadingTrivia`) is pre-existing and outside the decision (auditor hint; no caller-reached input known). Left unchanged.
- `rg -n "static int GetColumn" src`: `SyntaxTokenPositionUtilities.GetColumn` (Core, now used), `LayoutComputer.GetColumn` (pre-existing, indentation phase), `PatternAnchor.GetColumn` (pattern, different signature). The new copy is gone.

## Tests run

Debug, on 90d44e2e plus this change.

- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~StructuralTransformSeparatorTests|FullyQualifiedName~FieldDeclarationSplitTests|FullyQualifiedName~ReihitsuFormatterFieldSplitTests"`: Passed 154, Failed 0 (15 failed before, as 16-test-implementer says).
- `scripts/test.sh --configuration Debug --project formatter`: Passed 4499, Failed 0.
- `scripts/test.sh --configuration Debug --project analyzer`: Passed 6498, Failed 0 (includes the 5 new RH7101 tests).
- `scripts/test.sh --configuration Debug --project architecture`: Passed 115, Failed 0.
- `scripts/test.sh --configuration Debug --project core`: Passed 607; `--project cli`: Passed 251.

`scripts/format.sh` over the three files: 0 of 3 changed.

## Disputed tests or scope

_None._ `TokenGapUtilities` gains one internal member; no public member or dependency is added.

## Architecture findings

_None new._ Decision 21 is carried out: the transform no longer has its own rule for whether a gap holds a line break, and the context entry point no longer re-decides the line-break phase's member-start question with a different predicate. The transform still writes the one line break no later phase writes, which FMT-STRUCT-026 now states.

## Follow-up candidates

_None new._ The auditor's hint (`FormatNodeInDocumentAsync`'s `keepsLeadingTrivia` with `AreSeparatedByEndOfLine`) stays unverified; no caller-reached input is known, so it is not a candidate under *Supported inputs*.
