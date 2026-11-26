Role: implementer
Commit: 8ef5087
Verdict: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's decisions 1 and 2.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineShifts.cs` | `IsHoleOpeningLine` summary: states that the test is deliberately open-brace-only and not `FormattingTextAnalysisUtilities.IsInterpolationBrace`, because the indentation in front of an opening brace that starts a line is literal text, while the indentation in front of a closing brace is leading trivia inside the hole, which the trivia edit moves. Text-only (proof below) | Decision 1; FC-015 |
| `src/Reihitsu.Formatter/Pipeline/HorizontalSpacing/Rules/SingleSpaceSpacingRule.cs` | `RequiresSpaceBeforeBrace` returns `FormattingTextAnalysisUtilities.IsInterpolationBrace(token) == false` instead of `token.Parent is not InterpolationSyntax`; adds `using Reihitsu.Core;`. Equivalent for the brace tokens the guard above admits; the summary still describes the code | Decision 2; FC-015 / FC-016 |

## Policy owners
`rg -n "Parent is (not )?InterpolationSyntax|OpenBraceToken == token|CloseBraceToken == token" src --type cs` after the change:
- `src/Reihitsu.Core/FormattingTextAnalysisUtilities.cs:347` — `IsInterpolationBrace`, the owner.
- `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineShifts.cs:281` — open-brace-only, a different question (decision 1), now documented.
- `src/Reihitsu.Analyzer/Rules/Layout/RH5025...Analyzer.cs:51` — `block.OpenBraceToken`, a block brace, not this policy.
No other private copy of the interpolation-brace check remains.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src`: 2 files changed, 8 insertions(+), 2 deletions(-). No new type or member.

## Deleted special cases
_N/A._

## Tests run
    scripts/build.sh --configuration Debug                       -> 0 Warning(s), 0 Error(s)
    scripts/verify-text-only.sh --head worktree --exclude src/Reihitsu.Formatter/Pipeline/HorizontalSpacing/Rules/SingleSpaceSpacingRule.cs
        -> RawStringLineShifts.cs: 2076 tokens identical, changed trivia: documentation; TEXT-ONLY PROOF: PASS
    scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Pipeline.RawStringMovedLineStartTests|FullyQualifiedName~RawStringTabIndentationTests|FullyQualifiedName~HorizontalSpacingRewriterTests|FullyQualifiedName~InterpolationAlignmentSpacingReproTests|FullyQualifiedName~HorizontalSpacingRequirementTests|FullyQualifiedName~RawStringInterpolationHoleTests"
        -> Passed! Failed: 0, Passed: 188
    scripts/test.sh --configuration Debug --project formatter    -> Passed! Failed: 0, Passed: 7286

`scripts/format.sh` over both files: Formatted 0 of 2. The pinning tests from 02 are unchanged and pass.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None_ beyond the `IsOwningConstruct` duplicate already in 00-lead.md.
