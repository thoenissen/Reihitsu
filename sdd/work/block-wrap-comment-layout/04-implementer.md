Role: implementer
Commit: f5b464d
Verdict: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Decision 5 of `00-lead.md` (one shared carrier of a wrap's comment trivia, separators as caller input, `ControlFlowBraceTransform` output unchanged) is carried out; decision 3 (directive guards stay) is respected: `SectionCarriesDirectives` and `CanWrap` are unchanged in logic, only their comments now name the move instead of a rebuild.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Core/Utilities/BlockWrapTriviaUtilities.cs` (new) | The one carrier of a wrap's comment trivia. `GetCarriedLeadingTrivia(leading, keepBlankLinesAboveFirstComment)`: everything from the first comment's line on, as written (or the whole list when the flag is set); nothing when no comment. `GetCarriedTrailingTrivia(trailing, lineBreak, endsLine)`: with `endsLine`, the trivia up to and including the last comment as written, then `lineBreak`; without, as written plus `lineBreak` only behind a final single-line comment | FMT-PIPE-013, FMT-SWITCH-007 |
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/ControlFlowBraceTransform.cs` | Its private `GetCarriedLeadingTrivia` / `GetCarriedTrailingTrivia` moved verbatim into the shared carrier; `WrapWithBraces` calls it with `keepBlankLinesAboveFirstComment: false`, `endsLine: false`. Output unchanged | FMT-PIPE-013, FMT-STRUCT-026 |
| `src/Reihitsu.Formatter/Pipeline/SwitchCaseBraces/Rewriter/SwitchCaseBraceRewriter.cs` | Removed the rebuilds `BuildLeadingTrivia`, `BuildTrailingTrivia`, `BuildLabelTrailingTrivia`, `BuildTerminalBreakLeadingTrivia`, `CollectComments`. `AddBraces` now carries: last label trailing (`endsLine: true`, so the label's line ends there and the open brace no longer carries a leading line break); first statement leading (line break + carried, no blank lines above the first comment); last statement trailing (`endsLine: true`); terminal `break` leading (line break + carried with the blank lines the blank-line phase placed above the first comment); terminal `break` trailing (`endsLine: true`). Comments of `AddBraces` and `SectionCarriesDirectives` updated to the move | FMT-SWITCH-007, FMT-PIPE-013, FMT-PIPE-004, FMT-SWITCH-010 (guard unchanged) |

## Policy owners
- `rg -n "EndsLine\(.*\[.*- 1\]\)" src/Reihitsu.Formatter` — the carrier, and `FieldDeclarationSplitTransform.cs:134` (field split's own run start, a different decision; not a block wrap).
- `rg -ln "SyntaxFactory.Block\(" src/Reihitsu.Formatter` — `ControlFlowBraceTransform`, `SwitchCaseBraceRewriter` (both now use the carrier), and `ExpressionBodyToBlockConverter` (expression-body conversion; see Follow-up candidates).
- `rg -n "BuildLeadingTrivia|BuildTrailingTrivia|BuildLabelTrailingTrivia|BuildTerminalBreakLeadingTrivia|CollectComments" src` — remaining hits only in `FieldDeclarationSplitTransform` (unrelated private methods of the same name).

## Tests run
- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~BracedSwitchSectionCommentLayoutTests|FullyQualifiedName~BracedElseBodyCommentLayoutTests|FullyQualifiedName~TerminalBreakCommentTests|FullyQualifiedName~SwitchCaseBrace|FullyQualifiedName~PipelineRequirementTests"` — Passed: 147, Failed: 0 (before the change: 23 of the 27 handed-over tests failed, as `03-test-implementer.md` states).
- `scripts/test.sh --configuration Debug --project formatter` — Passed: 3545, Failed: 0.
- `scripts/test.sh --configuration Debug --project analyzer` — Passed: 6339, Failed: 0.
- `scripts/test.sh --configuration Debug --project cli` — Passed: 251, Failed: 0.
- `scripts/test.sh --configuration Debug --project architecture` — Passed: 92, Failed: 0.
- `scripts/build.sh --configuration Debug` — 0 errors; its one warning (RH5603, final newline of the new file) was removed by `scripts/format.sh` on the three changed files.

## Disputed tests or scope
- Scope: one new internal static class in `Reihitsu.Formatter` with two consumers. It is the shared helper decision 5 asked for; net production lines −196.
- Disputed tests: _None._

## Architecture findings
_None_ beyond decision 5, which this change carries out.

## Follow-up candidates
- `ExpressionBodyToBlockConverter` is a third carrier of comment trivia across inserted braces (01-skeptic: agrees on every traced layout). Its trivia shape differs (arrow and expression become a `return` statement), so it was not routed through the carrier; whether it should share it is a structural follow-up, not a defect.
