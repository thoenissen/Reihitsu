Role: implementer
Commit: fa7fb0f
Verdict: test disputed

The prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for. Every handed-over test now passes, and so does every test of the formatter, architecture, core, CLI, tooling, and playground projects. Two existing analyzer tests fail. Both assert output that the amended FMT-INDENT-030 rules out, so I did not change them; see "Disputed tests or scope".

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/TabReplacement/TabReplacementPhase.cs` (new) | New first phase. It replaces each tab in the `WhitespaceTrivia` of `root.DescendantTokens()` with four spaces, and each tab in the whitespace between the tokens of a `DirectiveTriviaSyntax`. This is the scope that `CleanupPhase.NormalizeWhitespaceTabs` and `NormalizeDirectiveTriviaTabs` had. The phase rebuilds only the tokens and directives that contain such a tab. Without one, it returns the input node itself (see note 1). | FMT-TAB-001, FMT-TAB-002, FMT-TAB-003, FMT-PIPE-016 |
| `src/Reihitsu.Formatter/Pipeline/Cleanup/CleanupPhase.cs` | Removed the tab code: `NormalizeWhitespaceTabs`, `NormalizeDirectiveTriviaTabs`, and `NormalizeDirectiveTrivia`. Removed an unused `using`. Updated the summary. | FMT-CLEAN-004, FMT-PIPE-015 |
| `src/Reihitsu.Formatter/Pipeline/FormattingPipeline.cs` | `TabReplacementPhase` now runs first. `RawStringAlignmentPhase` is removed. Updated the order summary. | FMT-PIPE-001 |
| `src/Reihitsu.Formatter/Pipeline/RawStringAlignment/RawStringAlignmentPhase.cs` | Deleted. | FMT-PIPE-001 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineShifts.cs` (moved from `RawStringAlignment/Utilities`, namespace now `…Indentation.Utilities`) | Removed both `GetColumnAfterTabReplacement` overloads. Added `MoveLines(root, getShift, ct)` with private helpers `ComputeMovedLiteral`, `GetIndentationLength`, and `IsNestedInRawStringHole`. These are the replacement logic of the deleted phase, but they take the shift from the caller instead of measuring it again from the text. A hole line's whitespace is counted as its length, because it holds no tab. Updated the summaries. | FMT-RAW-001 to FMT-RAW-008, FMT-PIPE-016 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLiteral.cs` (moved, namespace only) | Updated the summary. | FMT-RAW |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/IndentationRewriter.cs` | `Apply` places every line as before. It then calls `RawStringLineShifts.MoveLines` with `LayoutModel.GetRawStringShift`, which is the same shift the anchors were measured with. The model numbers lines in the input's tree, while the rewritten node is the root of its own tree, so `Apply` adds the line offset between the two (note 2). | FMT-RAW-005, FMT-INDENT-030, FMT-PIPE-015 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/LayoutComputer.cs` | `GetColumnBeforeRawStringShift` measures a token as follows: the line's column, plus the token's distance from the start of the line's text. The start of the text is the first comment in front of the line's first token on that line, or else that token. The new private `TryGetLineTextColumn` returns false when the line starts inside text that began on an earlier line: the previous token, its trailing trivia, or a multi-line comment of the first token. The token then keeps its column. This matches what `IndentationRewriter.RebuildLeadingTrivia` writes. It uses `FullSpan`, so the `/**` of a structured documentation comment counts. `RegisterRawStringHoleLines` now registers the column at which the hole line's text starts, without tab counting. Updated the summaries and the comment in `Compute`. | FMT-INDENT-030, FMT-RAW-005, FMT-RAW-007 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/LayoutModel.cs` | Documentation only. The move is now the phase's own move, and the comments no longer mention tab counting. Removed the old `using`. | FC-031 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/IndentationPhase.cs`, `Contributors/CommentIndentationContributor.cs`, `HorizontalSpacing/Utilities/TrailingWhitespaceWriter.cs` | Summaries only. The behavior is unchanged. | FC-031 |

Note 1. In the first version of the phase, every token was rebuilt. That detached a node formatted by `FormatNodeInDocumentAsync` from its document before any other phase ran. Four existing tests failed because later phases lost the previous token outside the target:
- `ReihitsuFormatterNodeBlankLineTests.MovesSingleLineDocumentationBehindCaseLabel`
- `…BehindOpeningBraceOfType`
- `…BehindOpeningBraceOfMethodBody`
- `FormatNodeWithPreviousTokenOutsideTargetPreservesTrailingDocumentation`

Rebuilding only the tokens that hold a tab fixed all four. The case that remains is a target whose own whitespace holds a tab. The phase still detaches that target first; see follow-up 1.

Note 2. Before the fix, a temporary probe (not committed) called `new IndentationPhase().Execute` on an attached `MethodDeclarationSyntax` that starts on document line 2. It left the literal unmoved. The full pipeline did not show this, because the target always reached the indentation phase already detached by an earlier phase. With the offset, the probe gave content and closing marker in column 16. No committed test covers the attached input.

The production paths are the ones the handovers name: `TabReplacementPhase`, `CleanupPhase`, `FormattingPipeline`, `IndentationPhase`, `LayoutComputer`, `LayoutModel`, `IndentationRewriter`, `RawStringAlignmentPhase`, `RawStringLineShifts`, `RawStringLiteral`, and the summaries of `TrailingWhitespaceWriter` and `CommentIndentationContributor`. The change adds no public API, no dependency, and no change to a helper shared with another consumer. The spec owner names did not change. The only new owner type is `TabReplacementPhase`, which `tab-replacement.md` already names. Moving `RawStringLineShifts` and `RawStringLiteral` changes their namespace but not their type names. No test references that namespace.

## Policy owners

- Tab replacement: `rg -n "'\\t'|\"\\t\"" src/Reihitsu.Formatter --glob '*.cs'`. The only matches are in `TabReplacementPhase`, plus `DocCommentElementNormalizer.cs:159` (`TrimStart(' ', '\t')` on documentation-comment text, which FMT-TAB-003 keeps). No other copy of the replacement exists.
- Tab-width counting: `rg -n "GetColumnAfterTabReplacement" src` finds nothing. Every caller is gone.
- Raw-string shift: `rg -n "GetRawStringShift|RawStringLineShifts|GetShift\(" src/Reihitsu.Formatter`. `RawStringLineShifts.GetShift` computes the shift. `LayoutModel.GetRawStringShift` exposes it to the contributors and to `IndentationRewriter.Apply`, which applies it through `RawStringLineShifts.MoveLines`. Nothing in the formatter measures it again. The RH5202 code fix keeps its own copy (`RealignContent`), which the change doc leaves out of scope (thoenissen/Reihitsu#1458).
- Column of a token on its line: `rg -n "static int (Compute|Get)\w*Column|FirstTokenOnLine" src/Reihitsu.Formatter src/Reihitsu.Core`. The matches:
  - `LayoutComputer.GetColumnBeforeRawStringShift`, `TryGetLineTextColumn`, and `FindFirstTokenOnLine`: the owner.
  - `SyntaxTokenPositionUtilities.GetColumn`: the raw text column.
  - `ReihitsuFormatterHelpers.ComputeTokenColumn`: the node-level offset, outside the phases and unchanged (change doc inventory).
  - `MethodChainAlignmentContributor` and `PatternAnchor`: both read through `GetAdjustedColumn`.
  - On the analyzer side, `InitializerLayoutAnalysisUtilities.IsAlignedAt` treats a line's leading comment as the line's anchor. It is the analyzer's own policy and is not shared with the formatter; see the disputed tests.

## Tests run

- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~TabReplacement|FullyQualifiedName~PhaseColumnOwnershipTests|FullyQualifiedName~FormattingPipelineTests|FullyQualifiedName~IndentationPhaseRawStringMoveTests|FullyQualifiedName~RawStringAlignmentPipelineTests|FullyQualifiedName~CommentLedLineAlignmentTests|FullyQualifiedName~WhitespaceCleanupTests|FullyQualifiedName~CleanupPhaseTests|FullyQualifiedName~RawStringInterpolationHoleTests"`
  - before: `Failed: 41, Passed: 97, Total: 138`, as `04-test-implementer.md` states
  - after: `Passed! - Failed: 0, Passed: 138, Total: 138`
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5202RawStringLiteralsShouldBeFormattedCorrectlyFormatterTests|FullyQualifiedName~RH5601UseTabsCorrectlyFormatterTests"` → `Passed! - Failed: 0, Passed: 9, Total: 9`
- `scripts/test.sh --configuration Debug --project formatter` → `Passed! - Failed: 0, Passed: 2951, Total: 2951`
- `scripts/test.sh --configuration Debug --project architecture` → `Passed! - Failed: 0, Passed: 84, Total: 84`. This includes `EveryCodeOwnerNamesAnExistingType` and `OwnershipMapMatchesRuntimePhaseOrder`.
- `scripts/test.sh --configuration Debug --project core` → `Passed! - Failed: 0, Passed: 533`
- `scripts/test.sh --configuration Debug --project cli` → `Passed! - Failed: 0, Passed: 250`
- `scripts/test.sh --configuration Debug --project tooling` → `Passed! - Failed: 0, Passed: 120`
- `scripts/test.sh --configuration Debug --project playground` → `Passed! - Failed: 0, Passed: 17`
- `scripts/test.sh --configuration Debug --no-build --project analyzer` → `Failed! - Failed: 2, Passed: 6178, Total: 6180`. The two failures are listed below. This was run on the tree of fa7fb0f.

`scripts/format.sh --force` was run on every changed production file. The Debug build shows no warning in the changed files. The Release build is the cleaner's.

## Disputed tests or scope

Two existing analyzer tests fail. They are the same shape under two rules:
- `Reihitsu.Analyzer.Test.Formatter.Formatting.RH5303CollectionInitializerShouldBeFormattedCorrectlyFormatterTests.VerifyFormatterKeepsCommentPrefixedComplexElementStable`
- `Reihitsu.Analyzer.Test.Formatter.Formatting.RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyFormatterTests.VerifyFormatterKeepsCommentPrefixedComplexElementStable`

**What they assert.** Each calls `VerifyFormatter(source)` and expects formatter output equal to its input. That input is stable today. In the RH5303 test, a comment leads the line of a complex element initializer's opening brace, and the members and the closing line are indented from the comment's column (lines shortened):

```
                     {
                         /* Keep element. */ {
                             /* Keep key. */ existingKey,

                             /* Keep value. */ existingValue
                         /* Keep close. */ }
                     };
```

The opening brace `{` is in column 45. The members start in column 29, which is 25 + 4: one level deeper than the comment in column 25. The closing line starts in column 25.

**What the formatter now produces.** The members start one level deeper than the brace's own column, and the closing line takes the brace's column:

```
                         /* Keep element. */ {
                                                 /* Keep key. */ existingKey,

                                                 /* Keep value. */ existingValue
                                             /* Keep close. */ }
```

The members are in column 49 and the closing line in column 45. The RH5304 test differs only in its columns: the brace is in 57, members 41 become 61, and the closing line 37 becomes 57.

**What the spec says.**
- FMT-INDENT-023: "for a complex element initializer … the closing brace MUST be indented to the column of the opening brace … the members that start a line MUST be indented one level deeper than the opening brace's column."
- FMT-INDENT-030 as amended: "The column of an anchor is the column it has in this phase's output, counting everything that stands in front of it on its line. When a comment stands in front of a line's first token, on that token's line … an anchor on that line is measured behind the comment."
- The opening brace here is the first token of its line, and a comment stands in front of it. So the brace's column is 45, and FMT-INDENT-023 requires members in column 49. The output the tests expect violates FMT-INDENT-023 read with the amended FMT-INDENT-030.
- That puts it in the second kind the change doc names under "Changed output is only output that violates an existing requirement today". The change doc's table of changed outputs does not list this input.

**Why they disagree, and what else is affected.** The RH5303 and RH5304 analyzers measure a different column. `InitializerLayoutAnalysisUtilities.FindFirstComplexElementMisalignment` and `IsAlignedAt` (`src/Reihitsu.Analyzer/Core/InitializerLayoutAnalysisUtilities.cs:24`, `:82`) take a leading comment's column as the line's anchor, and expect the members at that column plus four. Read from the code, not run: the analyzers would report the new formatter output (FC-013, SHARED-DIAG-010).

So the run has to choose one reading of the brace's column:
- **The brace's own column.** The tests and the RH5303/RH5304 analyzers must change. That is new scope: an analyzer change.
- **The line's column.** FMT-INDENT-030 must exempt an anchor that is the first token of its line, as `TryGetPlacedColumn` already does for the comment alignment. The implementation would then measure behind the comment only for a token that is not first on its line.

This is a spec decision (escalation 4), not mine. I left both tests unchanged.

No other input in the test suites changed output.

## Architecture findings

_None._ The disagreement above is between the formatter and the RH5303/RH5304 analyzers about one column. P11 and SHARED-DIAG-010 govern that relationship, and `squad/README.md` (*Architecture findings*) treats it as an ordinary finding.

## Follow-up candidates

1. **A node-level target whose whitespace holds a tab is detached by the first phase.**
   - `FormatNodeInDocumentAsync` passes the attached target to the pipeline. When the target's own whitespace holds a tab, `TabReplacementPhase` rebuilds it.
   - Later phases that read the previous token outside the target then no longer see it. Examples are the relocation of a `///` comment behind a preceding token, and `PrecedingTokenFacts` beyond what `FormattingContext.RootPrecedingToken` carries.
   - Before this run, tabs were replaced last, so this could not happen. Any earlier phase that rewrote the target detached it in the same way.
   - No test combines a tab in the target with a `///` comment behind the token in front of the target.
   - Structural cause: phases receive a node, and "attached to its document" is lost by whichever phase rewrites first.
2. **No committed test runs the indentation phase on an attached node with a raw string literal** (note 2). The pipeline always detaches the node before that phase, but the phase no longer depends on that.
3. The skeptic's follow-ups 1–4 in `02-skeptic.md` still stand. This run did not change them.
