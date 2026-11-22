Role: implementer
Commit: e7aa90f
Verdict: tests pass

Repair round 2: decision 29 (auditor finding 1) and decision 31's finding 4 (`00-lead.md`, `22-auditor.md`), tests from `24-test-implementer.md`. The prompt named the change asked for, which matches the recorded decisions; it carried no suspected cause or hoped-for verdict beyond them.

The three tests 24 marks as failing failed on `f466f5c` as described there (the whole-project run in 24: 7130 tests, those 3 failing); after the change all pass.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Utilities/PatternLineStartUtilities.cs` | Now owns FMT-BREAK-022's case-label condition: `IsIntroducedByCaseLabel(node, rootConstruct)` moved here unchanged from `LineBreakInitializerRewriter`. `StartsOwnLine(token, rootConstruct)` takes the root facts and excludes the `{` of a property pattern a `case` label introduces, so the joins read the same condition as FMT-BREAK-022 | FMT-BREAK-022, FMT-BREAK-017, FMT-BREAK-041, FMT-PIPE-024 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakInitializerRewriter.cs` | `IsIntroducedByCaseLabel` removed; `VisitRecursivePattern` calls `PatternLineStartUtilities.IsIntroducedByCaseLabel`; comment states FMT-BREAK-022's wording (brace stays on the line it is written on) | FMT-BREAK-022 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/ContinuationJoinRewriter.cs` | Takes the `FormattingContext`; `IsJoinedGap` passes `RootConstruct` to `StartsOwnLine` (the visited node's parents are intact, the root's facts answer at the formatting root) | FMT-BREAK-041 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/BinaryOperatorLineBreakRewriter.cs` | `VisitBinaryPattern` captures, before the base visit, whether a case label introduces the chain (as `RootConstruct.CaseLabelPattern`), because a rebuilt chain has no parent; `NormalizePatternChain` passes that fact to `StartsOwnLine`, where the walk from a right operand ends at the (possibly rebuilt) chain and reads it through the existing `IsRootIn` mechanism | FMT-BREAK-017 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/LineBreakPhase.cs` | Passes the context to `ContinuationJoinRewriter` | – |
| `src/Reihitsu.Formatter/Utilities/ReihitsuFormatterHelpers.cs` | `ComputeDocumentLineStartColumn` returns `ComputeDocumentColumn(token) - (token.SpanStart - position)`: the BOM rule lives only in `ComputeDocumentColumn` (auditor's suggested change). The root-fact capture calls `PatternLineStartUtilities.IsIntroducedByCaseLabel`. Summary updated | FMT-PIPE-017, FMT-PIPE-024 |

Summaries and comments of every changed method re-read and updated (`StartsOwnLine`, `IsIntroducedByCaseLabel`, `IsJoinedGap`, `NormalizePatternChain`, `VisitBinaryPattern`, `VisitRecursivePattern`, `ComputeDocumentLineStartColumn`, the class summary of `PatternLineStartUtilities`). Finding 3 (summary of `NormalizeBinaryOperatorPosition`) is the cleaner's and was not touched.

## Policy owners
- Case-label condition: `rg -n "IsIntroducedByCaseLabel|CasePatternSwitchLabelSyntax" src/Reihitsu.Formatter --glob '*.cs'` — one definition, `PatternLineStartUtilities.IsIntroducedByCaseLabel`; readers: `LineBreakInitializerRewriter.VisitRecursivePattern`, `PatternLineStartUtilities.StartsOwnLine` (read by `ContinuationJoinRewriter` and `BinaryOperatorLineBreakRewriter`), `BinaryOperatorLineBreakRewriter.VisitBinaryPattern`, `ReihitsuFormatterHelpers` (root fact). The other `CasePatternSwitchLabelSyntax` hits (`SwitchCaseWhenLineBreakRewriter`, `CaseWhenClauseContributor`) concern the `when` clause, not this decision.
- Pattern delimiter on its own line: `StartsOwnLine` (joins) and `LineBreakInitializerRewriter.VisitRecursivePattern`/`VisitParenthesizedPattern` (FMT-BREAK-022's placement) now share the case-label condition. `BinaryOperatorLineBreakRewriter.IsMultiLineDelimitedPattern` stays: it decides "is the chain wrapped" (FMT-BREAK-017's definition, which the auditor found in agreement), not where a delimiter starts.
- Document column counting: `rg -n "uFEFF" src/Reihitsu.Formatter --glob '*.cs'` — the BOM-as-zero-columns rule is in `ComputeDocumentColumn` only; `ComputeDocumentLineStartColumn` keeps `﻿` only in its skip of leading characters. `LayoutComputer.GetOpeningQuotesColumnBeforeRawStringShift` (FMT-RAW-005, on `main`) is a different rule (the raw-string move counts the mark as a column).

## Production diff size
- Branch: `git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` → 21 files changed, 1385 insertions(+), 82 deletions(-). No new type.
- This round: 6 files changed, 75 insertions(+), 56 deletions(-).

## Deleted special cases
_N/A._ (No shape declared in this round; `LineBreakInitializerRewriter.IsIntroducedByCaseLabel` moved, not duplicated.)

## Tests run
```text
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~PatternChainLayoutTests|FullyQualifiedName~ContinuationJoinTests|FullyQualifiedName~LineBreakRequirementTests|FullyQualifiedName~UnsupportedAssignmentInArmCommentTests|FullyQualifiedName~ReihitsuFormatterNodeColumnTests"
  Passed! - Failed: 0, Passed: 168, Total: 168
scripts/test.sh --configuration Debug --project formatter       Passed! - Failed: 0, Passed: 7130, Total: 7130
scripts/test.sh --configuration Debug --project analyzer        Passed! - Failed: 0, Passed: 11935, Total: 11935
scripts/test.sh --configuration Debug --project architecture    Passed! - Failed: 0, Passed: 198, Total: 198
scripts/build.sh --configuration Debug                          0 warnings in the changed files after scripts/format.sh
```

## Disputed tests or scope
_None._ The constructor of `ContinuationJoinRewriter` gains a `FormattingContext` parameter (internal type, one caller).

## Architecture findings
_None._ This round carries out decision 29's structural change.

## Follow-up candidates
_None._
