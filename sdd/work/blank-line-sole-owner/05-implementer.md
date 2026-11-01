Role: implementer
Commit: 39521e5
Verdict: test disputed

Issue thoenissen/Reihitsu#1496, lane behavior change. The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore. No test assertion, input, or expected output was changed; the one test edit is the call-site change the test implementer announced (`TokenGapNormalizerTests.NormalizeGapBeforeTokenInsertsMissingLineBreak` drops `blankLineCount: 0`, since the parameter is gone).

Result at 39521e5: formatter 3611 passed, 3 failed (all three below under *Disputed tests or scope*); analyzer 6350/6350, core 535/535, CLI 251/251, architecture 92/92, tooling 125/125. `scripts/format.sh --check src` reports 0 of 1784 files needing formatting, so the self-hosting scan should see no new output. Debug build has no warnings.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Utilities/TokenGapNormalizer.cs` | Blank-line half removed: no `blankLineCount`, no `NormalizeLeadingGap`, `NormalizeTrailingRun`, `NormalizeLeadingGapWithoutContent`, `HasOwnLinePlacementOwner`, or region guard. The three public entry points share `PlaceTokenOnOwnLine`: a gap with a line break is left as it is; a gap without one gets one line break in front of its trivia. Behind a documentation comment, `TryPlaceTokenBehindLeadingDocumentationComment` still moves a token glued behind other code onto its own line, but no longer removes blank lines | FMT-BREAK-030; Done-when "`TokenGapNormalizer` takes no blank-line count" |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakBlockRewriter.cs` | Run collapse (`blankLineCount: 1`) and its guard `GapCarriesDirectiveOrDisabledText` removed | FMT-BREAK-030; Done-when "`LineBreakBlockRewriter` has no run collapse" |
| `BracePlacer.cs`, `LineBreakInitializerRewriter.cs`, `MemberLineStartRewriter.cs` | Call sites drop `blankLineCount: 0` | FMT-BREAK-030 |
| `src/Reihitsu.Formatter/Data/RootListPosition.cs` | `KeepsBlankLineAboveBlockStatement` and the block case removed; a block statement gets a list position like any other statement; no line-break code referenced | FMT-PIPE-024; Done-when "`RootListPosition` calls no line-break code" |
| `src/Reihitsu.Core/BlankLineSpacingPolicy.cs` | New `IsBlockStatementAfterStatement(BlockSyntax)`: a block in the statement list of a block or switch section that is not the first statement | FMT-BLANK-017, RH5025-005 (one owner for both) |
| `src/Reihitsu.Analyzer/Rules/Layout/RH5025…Analyzer.cs` | The brace predicate excludes the opening brace of such a block | RH5025-005 |
| `src/Reihitsu.Formatter/Pipeline/BlankLines/Utilities/BraceBlankLinePolicy.cs` (new) | Owner of the token facts the blank-line subphases read: block-statement brace (detached root uses `RootListPosition.HasPreviousStatement`), the FMT-BLANK-018/020 token set, the FMT-BLANK-019 closing tokens | FMT-BLANK-017–020 |
| `BlankLineTokenCleanupRewriter.cs` | FMT-BLANK-008/009 cleanup skips the block-statement brace and also covers the FMT-BLANK-019 closing `]`/`)`; new FMT-BLANK-020 removal behind the last own-line comment; new FMT-BLANK-018 removal of the blank lines above a glued comment (none when a directive or disabled text is in the gap); `RemoveExcessBlankLinesAfterIndex` generalized to `RemoveExcessBlankLinesBetweenIndices`; stale comments about `TokenGapNormalizer` updated | FMT-BLANK-009, 017, 018, 019, 020 |
| `BlankLineTriviaBoundaryRewriter.cs` | FMT-BLANK-011 inserts no blank line above an ordinary comment that starts its line glued to an FMT-BLANK-018 token or to the brace of a block statement that follows a statement | FMT-BLANK-011, 017, 018 |

## Policy owners

- `rg -n "IsBlockStatementAfterStatement|IsOpeningBraceOfBlockStatementAfterStatement" src --glob '!*Test*'`: `BlankLineSpacingPolicy.IsBlockStatementAfterStatement` (Core, the one owner), read by the RH5025 analyzer and by `BraceBlankLinePolicy.IsOpeningBraceOfBlockStatementAfterStatement` (adds only the detached-root fact).
- `rg -n "KeepsGluedCommentWithoutBlankLine|IsClosingTokenOfPatternOrCollectionExpression" src --glob '!*Test*'`: `BraceBlankLinePolicy` only, read by the cleanup and boundary rewriters.
- `rg -n "IsAssignedCollectionInitializer" src --glob '!*Test*'`: `LineBreakInitializerRewriter.IsAssignedCollectionInitializer` (existing `internal static`), now also read by `BraceBlankLinePolicy`; no copy.
- `rg -n "SpansMultipleLines" src/Reihitsu.Formatter`: `LineBreakTriviaUtilities.SpansMultipleLines`, reused by `BraceBlankLinePolicy`.
- `rg -n "HasOwnLinePlacementOwner|GapCarriesDirectiveOrDisabledText|KeepsBlankLineAboveBlockStatement|blankLineCount" src/Reihitsu.Formatter`: no hits left outside `BlankLineTriviaBoundaryRewriter`'s own local variable.
- Collapse of runs to one blank line: `BlankLineCollapser` (FMT-BLANK-014) remains the only owner after the line-break copy was removed.

## Tests run

```shell
scripts/build.sh --configuration Debug                              # 0 warnings, 0 errors
scripts/test.sh --configuration Debug --project formatter           # Failed: 3, Passed: 3611, Total: 3614
scripts/test.sh --configuration Debug --project analyzer            # Passed: 6350
scripts/test.sh --configuration Debug --project core                # Passed: 535
scripts/test.sh --configuration Debug --project cli                 # Passed: 251
scripts/test.sh --configuration Debug --project architecture        # Passed: 92
scripts/test.sh --configuration Debug --project tooling             # Passed: 125
scripts/format.sh --check src                                       # 0 of 1784 file(s) need formatting
```

The failing three:

```
Failed LineBreaksEndingPragmaAndRegionDirectivesAreRewritten
Failed PlacesBlankLineBelowDirectiveAndKeepsAuthorBlankLineBelowDirective
Failed SeparatesBlockStatementsThatFollowAStatementInsideSwitchSection
```

## Disputed tests or scope

### D1 — `BlockStatementBlankLineTests.PlacesBlankLineBelowDirectiveAndKeepsAuthorBlankLineBelowDirective` (test wrong)

The input puts the block statement under `#if DEBUG`, but `AssertRuleResult` is called without parse options, so `DEBUG` is undefined and `{ B(); }` is disabled text. The expected output inserts a blank line between `#if DEBUG` and disabled text, which no formatter rule may do (disabled text is never touched). With an active branch the implementation gives exactly the expected output: the same fixture with `#if !DEBUG` traces to `#if !DEBUG⏎⏎{` in pass 1 and is stable in pass 2. The test needs `CSharpParseOptions.Default.WithPreprocessorSymbols("DEBUG")` (as `StructuralTransformSeparatorTests` does) or `#if !DEBUG`; the expected value itself is right. Route to the test implementer. The spec example of FMT-BLANK-017 and change-doc entry 2 use the same `#if DEBUG` input; the spec author may want to note that the branch is active there.

### D2 — `BlockStatementBlankLineTests.SeparatesBlockStatementsThatFollowAStatementInsideSwitchSection` (test wrong)

The expected output keeps the multi-line switch section unbraced. The switch-case brace phase braces it (FMT-SWITCH, `HasAnyMultiLineSection`), on the base commit e0ecaf5 as well: formatted there with `scripts/format.sh --force`, the same input becomes `case 1:⏎{⏎if (a > 0) … }⏎break;`. The test implementer's "fails: blank lines missing" did not mention that difference. Inside the braces, the implementation gives the FMT-BLANK-017 blank lines the test asks for (`}⏎⏎{`, author's `a++;⏎⏎{` kept). The input needs a shape the switch phase leaves unbraced, or the expected value needs the braces. Route to the test implementer.

### D3 — `ReihitsuFormatterLineEndingTests.LineBreaksEndingPragmaAndRegionDirectivesAreRewritten` (existing FMT-EOL-002 test; visible output change not listed)

Input (CRLF/LF mixed in the test; shown with LF):

```csharp
class C
{
    #region Fields

#pragma warning disable CS0169
    int x;
#pragma warning restore CS0169

    #endregion // Fields
}
```

Base (e0ecaf5): a blank line is inserted between `int x;` and `#pragma warning restore CS0169`. Now: no blank line there; everything else equal. The base's blank line came from the line-break phase normalizing the gap before the class's `}` (its last content is a region directive, so it was normalized "as if empty" and a line break was prepended in front of the gap's first directive), the same mechanism as change-doc visible output change 10, which the maintainer answered for a glued comment only. FMT-BREAK-030 forbids the line-break phase from inserting it, and no FMT-BLANK rule asks for a blank line in front of a directive (FMT-BLANK-016 argues against one), so keeping the base output would need a new blank-line rule.

Class, measured by formatting the same files at e0ecaf5 and at 39521e5: a non-region directive or disabled text starts the gap before a closing brace, a region directive ends it, and a blank line stands between them. Members that change: the class-body case above, the same in a method body (`a++;⏎#pragma …⏎⏎#endregion⏎}`), and `#if NEVER … #endif⏎⏎#endregion⏎}` in a method body. Members that hold (no difference): `a++;⏎#pragma …⏎⏎}` (FMT-BLANK-009 removes the blank line on both), the same gap before a collection expression's `]`, and `#if NEVER⏎#endif⏎⏎// c⏎}` in an initializer. This is a visible output change no requirement demands and no maintainer answer covers: an output question for the lead. The test asserts line endings; only its expected blank line is in question.

### D4 — FMT-BLANK-018 token set does not match today's output in two shapes (spec finding; implementation keeps today's output)

The spec names every initializer brace except the array initializer's and the complex element initializer's opening brace. Measured on e0ecaf5 (one fixture with a glued comment on every brace owner, with and without a blank line above it), two shapes differ from that list:

- **Opening brace of a complex element initializer that spans several lines** (`{ 1, 2 },⏎/* c1 */ {⏎3,⏎4⏎}`): today no blank line above the glued comment (the line-break phase lays such a brace out like an object initializer's and removed it). Pinned by `RH5303…AnalyzerTests.VerifyTwoComplexElementsMeasuredFromTheirLeadingCommentsAreDetectedAndFixed`.
- **Closing brace of a collection initializer assigned to a member** (`L = {⏎1⏎/* l */ }`): today a blank line above the glued comment (the line-break phase never normalized that brace). Pinned by `RH5303…FormatterTests.VerifyFormatterKeepsCommentPrefixedOrdinaryElementsStable`.

A third shape: a delimited documentation comment on the glued line (`public class TestClass⏎/* x */ /** Doc */ {`) keeps the blank line above `/* x */` today; pinned by `DelimitedDocumentationBeforeBlockStatementTests.KeepsOwnLineCommentAndDelimitedDocumentationGluedToTypeOpeningBrace`.

The spec's wording would change all three outputs; its `Why` and the change doc ("names exactly the tokens where today's output has none, so no glued comment … changes its output") and lead decision #3 say today's output is the intent. The implementation keeps today's output: a multi-line complex element `{` is in the set, an assigned collection initializer's `}` is not, and only ordinary comments on the glued line count. With that, the branch formats e0ecaf5's output of the fixture unchanged. The spec author should amend FMT-BLANK-018's token list (and through it FMT-BLANK-020's) and the "only ordinary comments" condition; no visible output is in question as long as today's output is kept.

### D5 — FMT-BLANK-011 above a comment glued to a block statement's brace (spec reading)

FMT-BLANK-011 exempts "a token FMT-BLANK-018 names", and FMT-BLANK-018 does not name the brace of a block statement that follows a statement ("FMT-BLANK-017 decides"). Read literally, FMT-BLANK-011 would insert a blank line above `A();⏎/* a */ {`; the test `KeepsCommentsGluedToBracesWithoutBlankLine` and FMT-BLANK-018's example ("after any other statement the author's blank line is kept") say it does not. The implementation follows the test and FMT-BLANK-017. The spec author may want FMT-BLANK-011's exemption to name that brace too.

### Scope

- Production paths named by the handovers: `TokenGapNormalizer`, `BracePlacer`, `LineBreakBlockRewriter`, `LineBreakInitializerRewriter`, `RootListPosition`, the blank-line subphases, RH5025. Added beyond them: one new public member in Core (`BlankLineSpacingPolicy.IsBlockStatementAfterStatement`, shared by the analyzer and the formatter so the exemption has one owner) and one new internal helper class (`BraceBlankLinePolicy`). `MemberLineStartRewriter` changed only at a call site. Not more than double the named paths.
- `BlankLineSpacingPolicy.IsBlockStatementAfterStatement` has no direct test in `Reihitsu.Core.Test`; it is covered through RH5025-005 and FMT-BLANK-017 tests. A direct test is the test implementer's call.
- `sdd/specs/formatter/blank-lines.md` lists the code owners of FMT-BLANK; `BraceBlankLinePolicy` is a new one (spec author).

## Architecture findings

_None._ The run's structural change is carried out: the line-break phase decides no blank line, and `RootListPosition` predicts nothing. `BraceBlankLinePolicy` reads `LineBreakInitializerRewriter.IsAssignedCollectionInitializer` and `LineBreakTriviaUtilities.SpansMultipleLines` for the D4 shapes; these are facts about the construct, read from their existing owners, not a prediction of the line-break phase's output. If the lead prefers the spec's literal token list (D4), that reuse goes away.

## Follow-up candidates

- `AGENTS.md`, *Formatter phase ownership*: the `BlankLinePhase` and `LineBreakPhase` rows do not name the new owner (`BraceBlankLinePolicy`) or the line-break phase's "keeps blank lines" rule (FMT-BREAK-030). Tooling-lane text, outside this run's lane.
- `src/Reihitsu.Formatter.Test/Regression/BlankLines/BlankLineBeforeIfDirectiveAfterStatementTests.cs:1160` still says in an XML comment that `TokenGapNormalizer`'s node-scoped calls cannot reach some anchors; the sentence is stale now (test code, not mine to edit).
