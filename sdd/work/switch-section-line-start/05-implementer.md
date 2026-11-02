Role: implementer
Commit: e38c060
Verdict: test disputed

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Of the 40 handed-over tests, 39 pass; one handed-over test and one existing test are disputed below. No test assertion, input, or expected output was changed.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Utilities/TokenGapNormalizer.cs` | New `EnsureTokenStartsLine`: decides "starts its line" by SHARED-BLANK-008 (`TokenGapAnalysis.StartsLineAtLeadingTriviaIndex`) for the first content on the token's line (`TriviaListUtilities.FindFirstContentIndexOnLine`), so a comment glued to the token — a `/** */` on the same line, or a comment that starts its line above it — moves with it or keeps it in place; inserts one line break in front of that first content, never a blank line | FMT-BREAK-033, -034, -035, FMT-BREAK-001, FMT-BREAK-030 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakBlockRewriter.cs` | `VisitSwitchStatement` places sections 2..n (`EnsureSectionsStartOnSeparateLines`); `VisitSwitchSection` places labels 2..n and the first statement (`EnsureLabelsAndFirstStatementStartOnSeparateLines`), skipping an empty first statement and a block (FMT-BREAK-008 stays with `NormalizeContainedBlock`); class summary updated | FMT-BREAK-033, -034, -035 |
| `src/Reihitsu.Formatter/Pipeline/BlankLines/Utilities/BlankLineEditor.cs` | New `EnsureBlankLineBeforeSection`; `InsertBlankLineBeforeToken` made generic over the node so sections use the statement rules' line-break count (`RequiredLineBreakCountForBlankLine`: two line breaks when the gap does not end its line) | FMT-BLANK-004 |
| `src/Reihitsu.Formatter/Pipeline/BlankLines/Rewriter/BlankLineBreakSpacingRewriter.cs` | Private `EnsureBlankLineBeforeSection` (one line break at index 0) removed; both call sites use the editor's | FMT-BLANK-004 |
| `src/Reihitsu.Formatter/Pipeline/SwitchCaseBraces/Rewriter/SwitchCaseBraceRewriter.cs` | Narrow fix of lead decisions 5, 6, 8: `IsLastLineEndedByFollowingToken` — when the section's last token carries no line break and the token behind the section (next section or the switch's `}`) already starts with a line break in its leading trivia, `AddBraces` writes no extra line break behind the trailing `break` / the new `}`; the gap keeps exactly the line breaks earlier phases wrote | FMT-BLANK-004, FMT-PIPE-004 |

## Policy owners

- Line-start fact: `rg -n "StartsLineAtLeadingTriviaIndex|SyntaxTokenPositionUtilities.StartsLine" src/Reihitsu.Formatter` — `LineStartIndex`, `BraceBlankLinePolicy`, `BlankLineTokenCleanupRewriter`, `TokenGapNormalizer.TryPlaceTokenBehindLeadingDocumentationComment`, `BlankLineTriviaBoundaryRewriter`, `IndentationRewriter`, `CommentIndentationContributor`. All call the Core owner (`TokenGapAnalysis`); the new method calls it too, no private copy.
- First content on a line: `rg -n "FindFirstContentIndexOnLine" src/Reihitsu.Formatter` — owner `TriviaListUtilities`, reused.
- Moving a token onto its own line: `rg -n "NormalizeGapBefore|EnsureTokenStartsOwnLine|MoveTokenToNewLine" src/Reihitsu.Formatter/Pipeline/LineBreaks` — `TokenGapNormalizer` (owner; new method added there), `BracePlacer.EnsureTokenStartsOwnLine`, `LineBreakTriviaUtilities.MoveTokenToNewLine`. The existing `PlaceTokenOnOwnLine` decides by `HasLineBreakBetween` (any line break, including one inside a multi-line comment) and places a token behind a glued `/** */`; both differ from what FMT-BREAK-033 to -035 specify, so its callers were left unchanged.
- Blank-line line-break count: `rg -n "RequiredLineBreakCountForBlankLine" src` — `BlankLineEditor.InsertBlankLineBeforeToken` only; now shared by statements and sections.
- Line break ending a wrapped body's last line: `rg -n "GetCarriedTrailingTrivia|closeBraceTrailing" src/Reihitsu.Formatter` — `SwitchCaseBraceRewriter.AddBraces`, `ControlFlowBraceTransform` (which already inspects the next token's leading trivia, `StartsWithCommentOrDirective`).

## Tests run

Debug build (`scripts/build.sh --configuration Debug`), then:

- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~SwitchSectionLineStartTests|FullyQualifiedName~LineBreakPhaseSwitchSectionTests|FullyQualifiedName~ReihitsuFormatterNodeSwitchSectionTests"` — Failed: 1, Passed: 37, Total: 38 (`SectionBehindDirectiveKeepsItsLine`, disputed below)
- `scripts/test.sh --configuration Debug --project formatter` — Failed: 2, Passed: 4096, Total: 4098 (`SectionBehindDirectiveKeepsItsLine`, `KeepsDelimitedDocumentationWithStatementBehindCaseLabel`, both disputed below)
- `scripts/test.sh --configuration Debug --project analyzer` — Passed: 6396, Total: 6396 (includes both new RH5417/RH5403 tests and the RH5029/RH5103 fixed codes the skeptic named)
- `scripts/test.sh --configuration Debug --project cli` — Passed: 251
- `scripts/test.sh --configuration Debug --project architecture` — Passed: 92
- `scripts/test.sh --configuration Debug --project core` — Passed: 563
- `scripts/format.sh` on the five changed files — 0 of 5 reformatted.

## Disputed tests or scope

1. **`SwitchSectionLineStartTests.SectionBehindDirectiveKeepsItsLine`** (`src/Reihitsu.Formatter.Test/Regression/LineBreaks/SwitchSectionLineStartTests.cs:495`, new in this run). Its input wraps `case 2: return 2;` in `#if DEBUG` / `#endif`, and `AssertRuleResult` parses with the default options, which define no `DEBUG`. The section is therefore disabled text in the `}` token's leading trivia, not syntax. The test expects `case 2:` / `return 2;` on separate lines inside the disabled branch, i.e. a rewrite of disabled text, which `AGENTS.md` forbids (formatter transformations preserve disabled text); the formatter leaves it as written (`case 2: return 2;`), and every other line matches. `scripts/trace.sh` confirms that no phase touches that line. The test implementer's note "fails on main: `return 1;`/`return 2;` stay on label lines" did not notice that the second is disabled text. To exercise the directive arm, the test needs an active branch (for example `#if !DEBUG`, or parse options that define `DEBUG`), or an expected value that keeps the disabled line unchanged. Route to the test implementer; nothing visible is in question beyond what the spec already decides.
2. **`DelimitedDocumentationRelocationTests.KeepsDelimitedDocumentationWithStatementBehindCaseLabel`** (`src/Reihitsu.Formatter.Test/Regression/FullPipeline/DelimitedDocumentationRelocationTests.cs:673`, existing on `main`, no requirement tag). It asserts that `case 1: /** d */ M(2);` stays unchanged. Maintainer answer 6 and FMT-BREAK-035 decide the opposite: `case 1:` alone, then `/** d */ M(2);` on the next line, which is what the formatter now writes. The test characterizes superseded behavior and has to take the new expected output (or be tagged FMT-BREAK-035 with it). Route to the test implementer.

Scope: five production files, all in components the handovers name (`LineBreakPhase`, `BlankLinePhase`, `SwitchCaseBracePhase`) or their utilities. One new public member on an internal type each in `TokenGapNormalizer` (`EnsureTokenStartsLine`) and `BlankLineEditor` (`EnsureBlankLineBeforeSection`, which replaces a private copy in the rewriter). `InsertBlankLineBeforeToken` is now shared by statements and sections; it stays private to `BlankLineEditor`. No new dependency.

## Architecture findings

The skeptic's finding stands, fixed narrowly as decisions 5, 6, and 8 direct. `SwitchCaseBraceRewriter.AddBraces` still owns a line break behind a braced section, and it now reads the leading trivia of the token behind the section to avoid writing a second one (`IsLastLineEndedByFollowingToken`). That is the copy the skeptic predicted: two components (the braced-section rewrite on one side, `BlankLineEditor.EnsureBlankLineBeforeSection` / `TokenGapNormalizer.EnsureTokenStartsLine` / `BracePlacer.EnsureBraceOnOwnLine` on the other) still share the decision of how many line breaks stand behind a braced section. The narrow rule keeps the gap's own line-break count instead of adding to it. The structural change stays with thoenissen/Reihitsu#1518 and thoenissen/Reihitsu#1498. `ControlFlowBraceTransform` has the same kind of next-token check (`StartsWithCommentOrDirective(nextToken.LeadingTrivia)`).

## Follow-up candidates

- `TokenGapNormalizer.PlaceTokenOnOwnLine` (the FMT-BREAK-006 statement placement and others) decides by `HasLineBreakBetween`, which counts a line break inside a multi-line comment, so `A(); /* a⏎b */ B();` keeps `B();` behind the comment's last line. That is the same SHARED-BLANK-008 gap FMT-BREAK-033 closes for sections. It is outside this run's construct; this is a follow-up candidate, not reproduced here.
- The lead's existing follow-up candidates 1 to 6 are unchanged by this work. Candidate 6 (#1518 braced-section doubling): the `}` shape of decision 6 and both braced cells of decision 5 pass now (`BracedSectionEndingInBreakGetsOneBlankLineInOneRun`, `BracedSectionEndingInReturnGetsNoBlankLine`, `BracedSectionsOnLineOfClosingBraceGetNoBlankLineBeforeIt`), so the class is fixed narrowly but not structurally.
