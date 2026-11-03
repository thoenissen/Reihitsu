Role: implementer
Commit: 072b113
Verdict: tests pass

The prompt carried no intended fix, no suspected cause, and no hoped-for verdict. Where the rule lives follows lead decision 6: the formatter phase decides every active region directive through one predicate in Core, which RH5031's analyzer and code fix also reach.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/RegionDirectiveBlankLineUtilities.cs` | New `IsMissingRequiredBlankLineBefore(bool hasLineAbove, bool isLineAboveBlank, bool doesLineAboveEndWithOpenBrace)`, the one statement of the rule. The existing `SourceText` overload, which RH5031's analyzer and code fix call, now reads those three facts from the text and delegates to it, so their behavior is unchanged. | SHARED-BLANK-005, RH5031-001 |
| `src/Reihitsu.Formatter/Pipeline/BlankLines/Utilities/BlankLineEditor.cs` | Removed the formatter's restatement: both `IsExemptFromPrecedingBlankLineBeforeRegionDirective` overloads (the `{`-plus-conditional-directives and start-of-file exemptions), `HasBlankLineImmediatelyBeforeIndex`, and `EnsureBlankLineBeforeFirstDirective`. New `IsMissingRequiredBlankLineBeforeRegionDirective(trivia, lineStartIndex, …)` only reads the three facts about the one line directly above the directive from the trivia, and asks Core. New `EnsureBlankLineBeforeRegionDirectives` visits every active directive of the kind in the leading trivia, from last to first, not only the first one. | FMT-BLANK-013, SHARED-BLANK-005, SHARED-BLANK-007 |
| `src/Reihitsu.Formatter/Pipeline/BlankLines/Rewriter/BlankLineRegionDirectiveRewriter.cs` | Calls `EnsureBlankLineBeforeRegionDirectives` for `#region`. | FMT-BLANK-013 |
| `src/Reihitsu.Formatter/Pipeline/BlankLines/Rewriter/BlankLineTriviaBoundaryRewriter.cs` | Leading-trivia `#endregion`: calls `EnsureBlankLineBeforeRegionDirectives`. Trailing-trivia `#endregion` (`EnsureBlankLineBeforeEndRegionsInTrailingTrivia`, renamed from `…FirstEndRegion…`): decides every active directive through the same predicate, with the token itself as the token in front of the list. It used to take its `{` exemption from the token *before* the token. It still inserts a second line break when the directive does not start a line. The skeptic found that the parser never puts a directive there, so no fixture reaches this path; see *Disputed tests or scope*. | FMT-BLANK-013 |

Line-above facts the formatter reads (in `IsMissingRequiredBlankLineBeforeRegionDirective`):
- The directive's line starts the list: the line above is the preceding token's line. It exists when there is a preceding token, it is never blank, and it ends with `{` when that token is a brace with no trailing comment. Without a preceding token the directive is on line 0 and exempt.
- The trivia in front is a directive, a `///` comment, or disabled text, which carries its own line break: the line above holds that trivia.
- The trivia in front is an end of line: whitespace is skipped. A preceding trivia that ends a line (`SyntaxTriviaUtilities.EndsLine`) means the line above is blank, and any other trivia means it is content. When the list start is reached, the line above is blank when the list starts a line (no preceding token, or the preceding token ends its line). Otherwise it continues the preceding token's line.

## Policy owners

- `rg -n "IsMissingRequiredBlankLineBefore" src --glob '!*.Test/**'` finds the Core decision (`RegionDirectiveBlankLineUtilities`, both overloads, the text one delegating). Its callers are `RegionDirectiveBlankLineAnalyzerBase`, `RegionDirectiveBlankLineCodeFixProviderBase`, and `BlankLineEditor.IsMissingRequiredBlankLineBeforeRegionDirective`. The latter is called by `BlankLineEditor.EnsureBlankLineBeforeRegionDirectives` and `BlankLineTriviaBoundaryRewriter.EnsureBlankLineBeforeEndRegionsInTrailingTrivia`.
- `rg -n "IsExemptFromPrecedingBlankLineBeforeRegionDirective|HasBlankLineImmediatelyBeforeIndex|EnsureBlankLineBeforeFirstDirective|EnsureBlankLineBeforeFirstEndRegion" --glob '!sdd/work/**' .` finds nothing. No private copy of the exemption remains.
- SHARED-BLANK-006 (blank line after the directive) is out of the issue's scope and was not touched. It is still stated twice: `RegionDirectiveBlankLineUtilities.IsMissingRequiredBlankLineAfter` and `BlankLineRegionDirectiveRewriter.EnsureBlankLineAfterRegionDirectives`. No probe shows drift.

## Tests run

All in Debug on 072b113:

- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.BlankLines.RegionDirectiveLineAboveTests|FullyQualifiedName~Reihitsu.Formatter.Test.Regression.Regions.DisabledRegionLayoutTests"` gives `Passed! - Failed: 0, Passed: 25` (before the change: 17 failed, 8 passed, as the handover says).
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5031RegionDirectivesShouldBePrecededByABlankLine"` gives `Passed! - Failed: 0, Passed: 25` (before: 3 failed).
- `--project formatter` gives `Passed! - Failed: 0, Passed: 4402`.
- `--project analyzer` gives `Passed! - Failed: 0, Passed: 6411`.
- `--project core` gives `Passed! - Failed: 0, Passed: 607`.
- `--project architecture` gives `Passed! - Failed: 0, Passed: 92`.
- `--project cli` gives `Passed! - Failed: 0, Passed: 251`.
- `scripts/format.sh` on the four changed files formatted 0 of 4.
- `scripts/format.sh src --check` reports "0 of 1843 file(s) need formatting". The repository's own source holds no shape the change reformats, so the self-hosting formatter rows should not move.

## Disputed tests or scope

- **New public member in Core:** `RegionDirectiveBlankLineUtilities.IsMissingRequiredBlankLineBefore(bool, bool, bool)`. Decision 6 asks for a shared predicate, and the skeptic forecast this growth. The existing `SourceText` overload keeps its signature and behavior, so RH5031's analyzer and code fix needed no edit and stay unchanged.
- **Trailing-trivia `#endregion` path:** its behavior changed on an input no caller produces. The `{` exemption now comes from the token that owns the trivia (the line above), not from the token before it, and every directive is visited, not only the first. I routed it through the shared predicate so no second statement of the rule remains. No test covers it, and none should be written, per *Supported inputs*. If the lead prefers to delete the path instead, that is an option. It would need a check that no rewriter before the blank-line phase can move an `#endregion` into trailing trivia.
- No test disputed.

## Architecture findings

The skeptic's finding (SHARED-BLANK-005 stated in Core and again in the formatter) is resolved as decision 6 asks: the decision is stated once, in Core. Two fact readers remain, because the two sides have different inputs: Core reads `SourceText` lines, and the formatter reads trivia mid-rewrite. Each reader only answers "is there a line above, is it blank, does it end with an exempt `{`", so the exemption scope and first-of-kind drifts can no longer recur in one copy alone. The `{`-with-trailing-comment detail is read on both sides. Core gets it from `GetLineIndicesEndingWithToken`, and the formatter from `PrecedingTokenFacts.HasTrailingComment`. A restructuring that gives the phase a text view of the tree would remove the second reader, but it would break the node-level entry points, whose root has no document line 0. I do not propose it.

The second skeptic finding (FMT-BLANK-010 against FMT-BLANK-013, decided by rewriter order) is unchanged and remains lead follow-up 1.

## Follow-up candidates

- SHARED-BLANK-006 is stated twice (see *Policy owners*), the same structure this run removed for SHARED-BLANK-005. No drift is shown, so this is a candidate only if the lead wants the symmetric cleanup.
