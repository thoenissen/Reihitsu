Role: implementer
Commit: c861527
Verdict: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

## Changes

Every restructuring keeps behavior: an extracted block or a flattened condition, evaluated in the same order with the same side effects. No public or internal member was added, renamed, or removed; none of the names in the `AGENTS.md` phase table changed (FC-036).

| File | What changed | Requirement |
|---|---|---|
| `Release.ruleset` | RH2009 `None` → `Error` | change doc, *The Debug build reports the rule as a warning* |
| `src/Reihitsu.Core/RegionDirectiveBlankLineUtilities.cs` | `RemoveDirectiveLines` (29): loop body moved into `IsBlankLineLeftOverByRemoval`, with `IsBlankOrRemovedLine` (run bounds) and `TouchesRemoval(..., out isDoubled)` (the two scans of the run). The run bounds still read the growing `removedLines`, the touch/doubled scans still read the caller's `removedLineIndices`, as before | change doc, *Members are restructured* |
| `src/Reihitsu.Core/DeclarationModifierUtilities.cs` | `ReplaceAccessibilityModifiers` (22): per-modifier branch chain moved into `AddFollowingModifier(tokens, modifier, ref carriedLeadingTrivia)` | same |
| `src/Reihitsu.Tooling/LaneClassifier.cs` | `TryGetRequiredLane` (23): `TryGetProductionLane`, `TryGetToolingLane`, `GetTextLane`; a failed lookup still yields `Lane.Docs` and the same reason strings | same |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/IndentationRewriter.cs` | `ApplyIndentation` (22): the trivia line start with its `///` continuation handling moved into `WriteTriviaLineStart` (continuation lines recorded before the markers are aligned; the alignment is pure) | same |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineShifts.cs` | `ApplyEdits` (16): leading-trivia edit moved into `ApplyLeadingTriviaEdit` | same |
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Utilities/AttributeListShapeUtilities.cs` | `BuildSplitAttributeLists` (18): per-attribute list creation moved into `CreateSplitAttributeList` | same |
| `src/Reihitsu.Formatter/Pipeline/BlankLines/Rewriter/BlankLineTokenCleanupRewriter.cs` | `CollapseLeadingBlankLines` (19): run scan moved into `FindLeadingBlankLineRunEnd` (`afterEndOfLine` is now `endOfLineCount > 0`, the same fact). `VisitToken` (17): the first `removesBlankLinesDirectlyBefore` block moved into the instance method `RemoveBlankLinesDirectlyBeforeToken`, its comments with it ("the branch above" now names `VisitToken`) | same |
| `src/Reihitsu.Analyzer/Rules/Clarity/RH3002TypeArgumentListUtilities.cs` | `WouldJoinTypeArgumentListAcrossArguments` (20): backward search moved into `IsPrecededByOpeningArgument`, the nested `if` became one condition. `ScanTypeArgumentList` (18): suffix scan moved into `ScanTypeArgumentSuffixes`. `GetDeclarationEnd` (16): the name-widening loop moved into `GetWholeName` | same |
| `src/Reihitsu.Analyzer/Rules/Documentation/RH8203InheritdocMustBeUsedWithInheritingClassAnalyzer.cs` | `OnDeclaration` (17): partial-part branch moved into `GetInheritdocOfPart(..., out laterInheritdocNode)` | same |
| `src/Reihitsu.Analyzer/Rules/Documentation/RH8303ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzer.cs` | `OnSyntaxTree` (17): per-line checks moved into `AnalyzeDocumentationLine` (`continue` → `return`), comments unchanged | same |
| `src/Reihitsu.Analyzer/Rules/Layout/RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.cs` | `AlignCommentIndentation` (19): trivia loop moved into `AlignLeadingComments`, which returns whether a multi-line comment ends on the token's line | same |
| `src/Reihitsu.Analyzer/Base/NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase.cs` | `OnTypeDeclaration` (17): the partial-member check became `IsReportedPart` and joins the relevance check in one condition (same order: relevance first) | same |
| `src/Reihitsu.Analyzer.CodeFixes/Base/TargetAttributePlacementCodeFixProviderBase.cs` | `PlaceAttributeListAsync` (16): bracket trivia moved into `CreateCloseBracketTrailingTrivia`, with its comment. `HasUnusableEnclosingScope` (18): brace switch moved into `GetScopeBraces`, column check into `IsOffItsDepthColumn` | same |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Documentation/RH8201InheritdocShouldBeUsedCodeFixProvider.cs` | `ReplaceDocumentation` (17): the state-machine iterator became index-based: `FindFirstDocumentationComment`, then the trivia in front of it, `<inheritdoc/>`, and for a `/** */` comment `DropLineBreakBehindComment` (drops whitespace and the line break, or writes the comment's indentation when something followed `*/` on its line or the list ends), then the rest unchanged. Same output for every list: nothing behind the replaced comment was ever replaced again | same |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Documentation/RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderCodeFixProvider.cs` | `GetLeadingHeaderSpan` (22): trivia scan moved into `ScanLeadingTrivia`, returning a named tuple; the inner `if/else` became `isBehindLastComment = IsKind(Whitespace)` then `if` | same |

Every new method is private, sits in the private part of its `Methods` region (or `Private methods` region in `IndentationRewriter`), and has full documentation comments; the summaries of the restructured members were re-read and still describe them (FC-031).

## Policy owners

- `rg -n "static int (Find|IndexOf)\w*Documentation\w*\(SyntaxTriviaList" src --glob '!*Test*'`: only the new `FindFirstDocumentationComment`; it calls the owner `SyntaxTriviaUtilities.IsDocumentationCommentTrivia`, as the old loop did.
- `rg -n "IsBlankOrRemoved|GetWholeName|GetOutermostName|GetScopeBraces|FindLeadingBlankLine" src --glob '!*Test*'`: only the new helpers. `IsBlankOrRemovedLine` calls the owner `FormattingTextAnalysisUtilities.IsBlankLine`.
- `GetScopeBraces` is the pre-existing private mirror of `SyntaxIndentationUtilities.IsIndentingAncestor`'s brace kinds, moved out unchanged; no new copy (FC-015).

## Production diff size

`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 15 files changed, 829 insertions(+), 505 deletions(-). No new type.

## Deleted special cases

_N/A._

## Tests run

```bash
scripts/build.sh --configuration Debug
scripts/test.sh --configuration Debug --no-build --project core        # Passed 812/812
scripts/test.sh --configuration Debug --no-build --project tooling     # Passed 204/204
scripts/test.sh --configuration Debug --no-build --project formatter   # Passed 6828/6828
scripts/test.sh --configuration Debug --no-build --project analyzer    # Passed 11878/11878
scripts/build.sh                                                        # Release, 0 warnings, 0 errors
scripts/test.sh --no-build --project selfhosting --filter "FullyQualifiedName~RulesetTests|FullyQualifiedName~AnalyzerViolationReportTests|Name~RH2008|Name~RH2111|Name~RH2009"   # Passed 55/55
scripts/test.sh --no-build --project selfhosting                       # 6157 passed, 1 failed (below)
```

`ReleaseRulesetReportsRuleAsError("RH2009")` and the RH2009 self-hosting row now pass. The full self-hosting run (21 min) passed every analyzer row, so no rule reports the new helpers (FC-032). Its one failure is `WorkingDocumentsDirectoryContainsNoFiles`, which reports this run's own `sdd/work/enforce-rh2009/` files. That is expected while the run is in progress, and it clears when the lead deletes `sdd/work/` before the PR is ready. `scripts/format.sh` changed none of the 15 files.

## Disputed tests or scope

_None._

## Architecture findings

_None._

## Follow-up candidates

_None._
