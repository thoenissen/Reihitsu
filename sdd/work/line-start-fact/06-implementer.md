Role: implementer
Commit: cb59e98
Verdict: test disputed

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the scope the maintainer's decisions set; I worked from the handovers, the change doc, and the specs.

Every test listed in `04-test-implementer.md` passes: the formatter's 73 and the analyzer's 13 tests that were red at 16f6f92 are green, and the green ones stay green. Three earlier tests that the test implementer did not touch now fail. Each one asserts the old output that the accepted change doc changes, or the old bug; see *Disputed tests or scope*.

## Changes

### Shared facts (`Reihitsu.Core`, shared with the analyzers)

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/LineEndingUtilities.cs` | Adds the one line-terminator predicate covering all five C# terminators: `IsLineTerminator`, `GetLineTerminatorLength`, `CountLineTerminators`, `ContainsLineTerminator`, `EndsWithLineTerminator`, `SplitLines` | FMT-INDENT-006 |
| `src/Reihitsu.Core/SyntaxTokenPositionUtilities.cs` | `IsFirstOnLine`: a token without text is never first on its line, and the line break may be the one that ends a `///` comment, a directive, or disabled text (through `AreSeparatedByEndOfLine`). New `StartsLine`: the strict SHARED-BLANK-008 form, nothing but whitespace in front of the token on its line, built on `TokenGapAnalysis.StartsLineAtLeadingTriviaIndex` | SHARED-BLANK-008 |
| `src/Reihitsu.Core/SyntaxTriviaUtilities.cs` | `AreSeparatedByEndOfLine` counts trivia that ends its line (`EndsLine`). New `EndsLine`, and `EndsWithLineBreak`, which moved here from `BlankLineTriviaUtilities`. The private `CommentEndsOwnLine` copy is deleted | SHARED-BLANK-008, FMT-SPACE-001, RH6002-001/002 |
| `src/Reihitsu.Core/TokenGapAnalysis.cs` | The gap counter uses the shared terminator predicate; the private `GetLineBreakLength` is deleted | SHARED-BLANK-002 |
| `src/Reihitsu.Core/DocumentationCommentUtilities.cs` | The private `IsLineTerminator` copy and the CR/LF length logic are replaced by the shared predicate | — |

### Formatter

| File | What changed | Requirement |
|---|---|---|
| `Pipeline/Indentation/Utilities/LineStartIndex.cs` (new) | Names, for each physical line of a tree, the position where the line's text starts: the trivia that starts its line (SHARED-BLANK-002) or the token that starts it (SHARED-BLANK-008). Each element's line comes from Roslyn's line table. Built once per tree | FMT-INDENT-002, -006, -015 |
| `Pipeline/Indentation/Utilities/IndentationRewriter.cs` | Rewritten. The rewriter writes a line's column in front of the element `LineStartIndex` names and nowhere else on the line. It no longer counts `\n` (`CountLineBreaks`, `CountEmbeddedNewLines`, and `RebuildLeadingTrivia` are gone), and the `startsAtLineStart` copy is deleted. A `///` comment's continuation markers are realigned only when the comment starts its line. Trivia in front of a token without text (the end of the file) is placed; the token adds no column | FMT-INDENT-002, -006, -008, -015, FMT-DOC-003, SHARED-BLANK-008 |
| `Pipeline/Indentation/Utilities/LayoutComputer.cs` | `TryGetLineTextColumn` reads `LineStartIndex`, and `FindFirstTokenOnLine` is deleted. The end-of-file line gets the file's block column when no token with text stands on it (`IsAloneOnEndOfFileLine`). This keeps comments and `#endregion` behind the last `}` placed now that the end of the file is never first on its line. The alignment sweep sets `LayoutModel.AlignmentContributor` | SHARED-BLANK-008 |
| `Pipeline/Indentation/Utilities/LayoutModel.cs` | Records which alignment contributors placed each line through `Set` while the sweeps run: `AlignmentContributor` and `GetAlignmentContributors(int line)`. A contributor that only moves a range of lines (`ShiftRange`) is not counted. This is the observable property for the one-contributor-per-line test | issue *Done when* |
| `Pipeline/LineBreaks/Utilities/LineBreakTriviaUtilities.cs` | `HasLeadingEndOfLine` is the shared `IsFirstOnLine`; the detached-subtree fallback is kept. `HasTrailingEndOfLine` uses `EndsLine`. `CollapseTokenToSameLine` refuses when the directly preceding token has no text and the gap to it holds no line break, because the line break it would remove lies in front of that token. Without this, the line-break phase stripped the indentation of `>` behind `Dictionary<,` and the indentation phase wrote it back | FMT-INDENT-015, FC-012 |
| `Pipeline/LineBreaks/Rewriter/ChainLineBreakRewriter.cs` | Its gap check is `AreSeparatedByEndOfLine`, and its multi-line-comment check uses the shared terminator predicate. A stranded `///` before `.` or `?.` is no longer moved | FMT-DOC-003 |
| `Pipeline/LineBreaks/Rewriter/ListElementInteriorJoinRewriter.cs` | `CanJoin` uses `AreSeparatedByEndOfLine`; the terminator set is shared | — |
| `Pipeline/LineBreaks/Rewriter/SwitchCaseWhenLineBreakRewriter.cs`, `Pipeline/UsingDirectives/Utilities/UsingTrailingTriviaBuilder.cs`, `Pipeline/DocumentationComments/Utilities/DocCommentElementNormalizer.cs`, `Pipeline/StructuralTransforms/Rewriter/FieldDeclarationSplitTransform.cs` | Each private terminator set (`\n` only; `\r\n`/`\r`/`\n`; all five) or private `EndsLine` is replaced by the shared predicate | — |
| `Pipeline/HorizontalSpacing/Rewriter/HorizontalSpacingRewriter.cs` | The FMT-SPACE-015 exception in `TrimGapBeforeToken` is deleted. The shared `AreSeparatedByEndOfLine` now covers the gap | FMT-SPACE-001 |
| `Data/PrecedingTokenFacts.cs` | `EndsLine` uses `SyntaxTriviaUtilities.EndsLine` | — |
| `ReihitsuFormatter.cs`, `Data/RootListPosition.cs` | `StartsOnNewLineIncludingDocumentation` is replaced by `AreSeparatedByEndOfLine(previousToken, firstToken)`, which is equivalent and also covers directives and the shared terminators | — |
| `Utilities/ReihitsuFormatterHelpers.cs` | Split. Deleted: `StartsOnNewLine` and `StartsOnNewLineIncludingDocumentation`; the facade forwards `IsCommentTrivia`, `IsDirectiveOrDisabledTextTrivia`, `DocumentsFollowingCode`, and `HasCommentDirectlyAbove` (phases now call `Reihitsu.Core` directly); the BOM helpers. What remains is entry-point code: `DetectEndOfLine`, `HasSyntaxErrors`, `IsAutoGeneratedSource`, `ComputeBaseIndentLevel`, `ComputeTokenColumn`, `ComputeDocumentColumn` | issue *Done when* |
| `Pipeline/Core/Utilities/ByteOrderMarkUtilities.cs` (new) | The BOM helpers, used by the blank-line, cleanup, and indentation phases | — |
| `Pipeline/Core/Utilities/TriviaListUtilities.cs` (new) | `RemoveTrailingWhitespace`, the neutral owner of the former `LineBreakTriviaUtilities.StripTrailingWhitespace`. The structural transforms call it | — |
| `Pipeline/Core/Utilities/TokenContainmentUtilities.cs` (new) | `ContainsToken`, the neutral owner of the former `TokenLocator.ContainsToken`. The documentation-comment phase calls it | — |
| `Pipeline/BlankLines/Utilities/BlankLineTriviaUtilities.cs` | Deleted. Its only method moved to `SyntaxTriviaUtilities.EndsWithLineBreak` | — |
| `Data/Enumerations/ConfigurableStructuralTransforms.cs` | Moved from `Pipeline/StructuralTransforms/Enumerations/`; the namespace is now `Reihitsu.Formatter.Data.Enumerations` | issue *Proposed Behavior* |
| Using and call-site updates | `BlankLineCollapser`, `BlankLineTokenCleanupRewriter`, `BlankLineTriviaBoundaryRewriter`, `CleanupPhase`, `ControlFlowBraceTransform`, `ExpressionBodiedMethodTransform`, `ExpressionBodiedConstructorTransform`, `AccessorExpressionBodyTransform`, `ExpressionBodyToBlockConverter`, `SwitchCaseBraceRewriter`, `TokenGapNormalizer`, `TokenLocator`, `DocumentationCommentFormattingPhase`, `FormattingContext`, `StructuralTransformPhase` | — |

### Code fixes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedCodeFixProvider.cs` | `ReihitsuFormatterHelpers.StartsOnNewLine` is replaced by `SyntaxTokenPositionUtilities.IsFirstOnLine`. A labeled local function always has a preceding token, so the no-previous-token difference cannot occur | — |

No analyzer source changed. RH5204, RH5203, RH6002, RH5201, RH5112, RH5048, and `FluentChainCodeFixHelper` already asked `IsFirstOnLine`, `FluentChainLink.StartsLine`, or `AreSeparatedByEndOfLine`, so they follow the shared fact unchanged.

## Policy owners

- Line terminators. `rg -n "IsLineTerminator|GetLineTerminatorLength|IndexOfAny\(\['\\r'|IndexOf\('\\n'\)" src --glob '*.cs' --glob '!**/obj/**' --glob '!*.Test/**'`. Inside the formatter and Core, the only owner is `LineEndingUtilities`. Its callers are `DocumentationCommentUtilities`, `TokenGapAnalysis`, `SyntaxTriviaUtilities`, and the formatter files above. Outside the run's scope (analyzer rules, not line-start decisions): `RH5202…Analyzer.cs:100`, `RH5202…CodeFixProvider.cs:72`, `RH5401…CodeFixProvider.cs:49`, `RH5603…Analyzer.cs:57`, `RH8402…CodeFixProvider.cs`, and the copyright-header helpers in `Reihitsu.Analyzer/Data`.
- Token line start. `rg -n "static bool (IsFirstOnLine|StartsLine|AreSeparatedByEndOfLine|StartsOnNewLine)" src --glob '*.cs'`. Owners: `SyntaxTokenPositionUtilities.IsFirstOnLine`/`StartsLine`, `SyntaxTriviaUtilities.AreSeparatedByEndOfLine`, and the forwarder `LayoutComputer.IsFirstOnLine`. Remaining private copy: `AttributeTargetUtilities.StartsLine` (`src/Reihitsu.Core/AttributeTargetUtilities.cs:350`); see *Follow-up candidates*.
- Trivia line start. `TokenGapAnalysis.StartsLineAtLeadingTriviaIndex`, unchanged as owner. `LineStartIndex` and `CommentIndentationContributor` call it.
- Where a line's text starts (column). `LineStartIndex` is now the one owner for `LayoutComputer` and `IndentationRewriter`. Before, `TryGetLineTextColumn`/`FindFirstTokenOnLine` and the rewriter's own trivia walk each decided it.

## Tests run

Debug build, `scripts/build.sh --configuration Debug`: 0 warnings, 0 errors.

On cb59e98. The experiment under *Follow-up candidates* was reverted; analyzer, cli, and architecture were rerun after the revert:

- `scripts/test.sh --configuration Debug --project formatter`: `Failed: 3, Passed: 3237, Total: 3240`, the three disputed tests below.
- `scripts/test.sh --configuration Debug --project analyzer`: `Passed: 6264, Total: 6264`.
- `scripts/test.sh --configuration Debug --project core`: `Passed: 533`.
- `scripts/test.sh --configuration Debug --project cli`: `Passed: 251`.
- `scripts/test.sh --configuration Debug --project architecture`: `Passed: 89`.
- `scripts/test.sh --configuration Debug --project tooling`: `Passed: 125`.

## Disputed tests or scope

### Disputed tests (not changed)

1. **`Regression/Spacing/StrandedDocumentationTokenSpacingTests.RemovesGapBetweenBlockCommentAndTerminatorBelowStrayDocumentationComment`** (untagged; unchanged since `main`). Input `return 1 /// stray` / `/* b */ ;`. It expects `/* b */;`; the output is now `/* b */ ;`.
   - FMT-SPACE-001 as changed: when the right token's leading trivia holds the line break that ends a `///` comment, the whole gap stays unchanged.
   - The `//` counterpart (`return 1 // stray` / `/* b */ ;`) keeps `/* b */ ;` on today's formatter, checked with `scripts/trace.sh`. The change doc makes that output the target.
   - The test asserts the pre-change output.
2. **`Regression/FullPipeline/StrandedDocumentationCommentTextTests.TwoLineRemarksBehindArgumentKeepsItsText`** (FMT-DOC-003). Input `M(1 /// <remarks>a` / `/// b</remarks>` / `);`, with `)` in column 8 and the input expected unchanged. The output puts `)` in column 10.
   - The comment's text, including its continuation line in column 8, is kept as FMT-DOC-003 requires. Only the line behind it is placed (FMT-INDENT-002).
   - The `//` counterpart (`M(1 // x` / `);`) puts `)` in column 10 on today's formatter, aligned with the first argument. The test implementer changed the sibling `ClosingParenthesisBelowStrayDocumentationCommentIsAlignedWithFirstArgument` the same way. This one was missed.
3. **`Unit/Pipeline/PhaseStabilityCheckTests.CheckCatchesLineBreakPhaseStrippingIndentationOfClosingAngleBracketBehindOmittedTypeArgument`**. It asserts that the line-break phase alone strips the indentation of `>` behind `Dictionary<,`. That is the FC-012 reversal `01-spec-author.md` reported. The test implementer deleted its exception-list entry, and `ZeroWidthTokenLineStartTests` requires the reversal to be gone (the phase-level check runs in `AssertRuleResult`). The line-break phase now leaves the line alone, so this check test has lost its example. It needs another reversal input, or deletion.

### Scope

- **`TokenLocator` and `LineBreakTriviaUtilities.StripTrailingWhitespace` stay in `LineBreaks/` as forwarders** to the new `Pipeline/Core` owners. `TokenLocatorTests` and `LineBreakTriviaUtilitiesTests` call them by their `Pipeline.LineBreaks.Utilities` names, and moving them would require editing those tests. No phase other than LineBreaks uses them now. The test implementer can retarget the two tests; the forwarders can then be deleted.
- **Two cross-folder dependencies remain:**
  - `LineBreaks/Rewriter/ListElementInteriorJoinRewriter` uses `HorizontalSpacing`'s `SpacingPolicy` and `UnaryOperatorSpacingUtilities`. `SpacingPolicy` is FMT-SPACE's policy owner, not a neutral helper, so moving it is not one of the issue's neutral-helper moves. See *Architecture findings*.
  - `Data/RootListPosition` uses `TokenGapNormalizer.HasOwnLinePlacementOwner` from `LineBreaks`. That is a blank-line placement policy, and `Data` is not a phase.

  The issue's *Done when* ("no phase uses a type from another phase's folder") therefore holds for every pair except the first.
- **`ReihitsuFormatterHelpers.ComputeTokenColumn`** is used only by `TargetAttributePlacementCodeFixProviderBase` and by tests (`ReihitsuFormatterHelpersTests`, among others). It stays because the tests name it. It is the only code-fix-only member left.
- **`AGENTS.md`**: the `IndentationPhase` row of the phase-ownership table names `RebuildLeadingTrivia` and "previous-token trailing-EOL detection". Both are gone. The row should name `LineStartIndex` and `IndentationRewriter.Apply` instead. That is a text change for the spec author, and I did not make it.
- **One-contributor-per-line test**: `LayoutModel.GetAlignmentContributors(line)` returns the distinct `ILayoutContributor` types whose `Set` placed the line during the alignment sweeps of `LayoutComputer.Compute`. Block indentation, the root-column shift, comment alignment, and `ShiftRange` moves are not recorded. I did not check whether every line has at most one contributor today.

## Architecture findings

- **Line-break phase and horizontal-spacing phase share the spacing decision.** `ListElementInteriorJoinRewriter` (LineBreaks) calls `SpacingPolicy.GetDesiredSpacesAfter` and `UnaryOperatorSpacingUtilities.WouldGlueIntoDifferentOperator` to decide a join that the horizontal-spacing phase spaces later. This predates the run; I did not change it. It is the one remaining member of the issue's "no phase uses a type from another phase's folder".
- **The line-break phase's angle-bracket join and FMT-BREAK-014 disagree for omitted type arguments.** FMT-BREAK-014 says a wrapped type argument list is joined onto one line. `Dictionary<,` / `>` is not joined, because `GetFirstToken()` skips zero-width elements and the close-token collapse cannot reach the line break in front of an omitted argument. FMT-INDENT-015 and SHARED-BLANK-008 then specify the column of the line that remains wrapped. I made the collapse refuse explicitly rather than strip whitespace. Whether such a list should be joined is a question between FMT-BREAK-014 and the FMT-INDENT-015/SHARED-BLANK-008 examples, not one I decided.

## Follow-up candidates

- `AttributeTargetUtilities.StartsLine` (`src/Reihitsu.Core/AttributeTargetUtilities.cs:350`) is a private line-start copy: the previous token's trailing EOL, true without a previous token. Routing it to `SyntaxTokenPositionUtilities.IsFirstOnLine` failed 12 analyzer tests, for example `VerifyCodeFixJoinsParameterWhenBlankLineFollowsTheGroup`, so I reverted it. It is used on rewritten owners where the two notions differ. This is outside the issue's list.
- The analyzer-side terminator sets listed under *Policy owners* (RH5202, RH5401, RH5603, RH8402, copyright header) do not use the shared predicate.
- The follow-up candidates of `01`, `02`, and `03` stand.
