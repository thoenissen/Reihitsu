Role: implementer
Commit: 6367111 (changes made on top of it; see the commit carrying this file)
Verdict: #1695: tests pass; #1696: tests pass; #1707: tests pass (RH3003 changed, RH3005 unchanged); #1721: nothing to implement (Decision 6)

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Before any change, the handed-over tests failed as `04-test-implementer.md` states: formatter class 16 failed / 3 passed; the six analyzer classes 23 failed. After the change all pass.

## thoenissen/Reihitsu#1695

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/ReihitsuFormatter.cs` | New private `EndTargetLineAsFormatted(SyntaxTriviaList, string endOfLine)`: when the trailing trivia ends in `EndOfLineTrivia`, that line break is replaced by `SyntaxFactory.EndOfLine(endOfLine)` unless its text already is the detected ending, and the whitespace trivia directly in front of it is removed with the existing `TriviaListUtilities.RemoveTrailingWhitespace`; a trivia without a line break is returned unchanged. Called at the three write-back sites: `WriteBackTrailingTrivia` (now takes the `endOfLine` that `FormatNodeInDocumentAsync` already computed), the general path of `FormatNodeInDocumentWithContextAsync` (`DetectEndOfLine(root)`), and the non-shared-line branch of `FormatSplitFieldInContext` (the `lineBreak` it already builds from `DetectEndOfLine(root)`). XML docs of both public entry points (remarks), `WriteBackTrailingTrivia`, and `FormatSplitFieldInContext` updated to say the line ending the trailing trivia ends is ended as the phases end every other line (FC-031). | FMT-PIPE-010, SHARED-FIX-008 |

Whitespace in front of a comment and the text of a comment are not touched: only `WhitespaceTrivia` that directly precedes the final `EndOfLineTrivia` is removed (a `//` comment's trailing spaces are part of its trivia text). Comments kept on an inner token by `WriteBackTrailingTrivia` (`RemoveLeadingComments`) are removed first, then the line is ended, so `} /* c */···` loses the spaces in both orders of events.

### Policy owners

- `rg -n "RemoveTrailingWhitespace\(" src --type cs -g '!*Test*'`: owner `TriviaListUtilities.RemoveTrailingWhitespace` (`Pipeline/Core/Utilities/TriviaListUtilities.cs:82`), with the forwarder `LineBreakTriviaUtilities.RemoveTrailingWhitespace`; `FormatSplitFieldInContext` already used the owner for the same end-of-line purpose on its shared-line branch. Reused, no new copy. `CleanupPhase.CleanWhitespaceBeforeEndOfLine` (private) is the phase's own statement of FMT-CLEAN-001; the entry point does not call into the phase (the write-back runs after the last phase, Decision 3).
- Line ending: `ReihitsuFormatterHelpers.DetectEndOfLine` → `LineEndingUtilities.DetectEndOfLine` (Core), the one detection; reused. `SyntaxFactory.EndOfLine(...)` appears in `ReihitsuFormatter.cs` only in the new helper and the split path's existing `lineBreak`.

### Production diff size

See *Whole run*. No new type, no new public member.

### Deleted special cases

_N/A._ No shape is declared unsupported.

### Tests run

- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~ReihitsuFormatterNodeTrailingLineEndTests"` → `Passed! - Failed: 0, Passed: 19`
- RH3202, RH5111, RH7101 classes: in the combined analyzer run under *Whole run*.

### Disputed tests or scope

_None._

### Architecture findings

_None_ beyond the skeptic's, which the lead decided (Decision 3, narrow fix at the three write-back sites). The narrow fix keeps the known cost that the end of the target's last line is decided after the last phase, by the entry point, with the shared helpers named above.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1696

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Data/CopyrightHeaderTemplateResolver.cs` | New overload `NormalizeLineEndings(string header, string lineEnding)` that writes every line break of the header as the given ending; the existing `NormalizeLineEndings(string, SourceText)` (the analyzer's comparison in the file's first line break, `\n` when none) now delegates to it and its summary says so. Analyzer call unchanged. | RH8402-002 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Documentation/RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderCodeFixProvider.cs` | The fix detects the line ending with `LineEndingUtilities.DetectEndOfLine` on the document's syntax root (`Environment.NewLine` when the tree is missing, and also from the detection itself for a file without line breaks) and normalizes the header to it; `EnsureHeaderIsSeparatedFromCode` takes that ending for the separator line break. The private `GetPreferredLineEnding` copy is deleted. `using Reihitsu.Core;` added. | RH8402-005, SHARED-FIX-008, FMT-PIPE-007 |

The detection runs on the document's own root (as the formatter and other fixes do), not on `GetRootWithParsedDocumentation`'s root, which stays the input of `GetLeadingHeaderSpan` only.

### Policy owners

- `rg -n "Lines\[0\]|SpanIncludingLineBreak" src/Reihitsu.Analyzer src/Reihitsu.Analyzer.CodeFixes --type cs`: only `CopyrightHeaderTemplateResolver.cs:92`, the analyzer's first-line comparison (RH8402-002, intended to stay). The fix's private first-line copy (`GetPreferredLineEnding`) is gone (FC-015).
- `rg -n "LineEndingUtilities.DetectEndOfLine" src/Reihitsu.Analyzer.CodeFixes`: the shared detection other fixes use (`StatementBracesCodeFixProviderBase`, RH7309, RH7004, RH3004); RH8402 now uses it too.

### Production diff size

See *Whole run*.

### Deleted special cases

_N/A._

### Tests run

- `RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzerTests`: in the combined analyzer run under *Whole run*, all pass. `FixInsertsHeaderInFrontOfCode` and `FixWithHeaderEndingInLineBreakWritesTheMachineLineEndingInAOneLineFile` cannot distinguish `\n` from `Environment.NewLine` on this Linux host (FC-017, as the test handover says).

### Disputed tests or scope

_None._

### Architecture findings

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1707

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3003UseStringEmptyForEmptyStringsCodeFixProvider.cs` | `string.Empty` is built from tokens with explicit trivia: `string` keyword carries the literal's leading trivia, the `.` token and the `Empty` identifier's leading trivia are empty lists, and `Empty` carries the literal's trailing trivia; previously `SyntaxFactory.Token(kind)`/`IdentifierName(...)` left elastic trivia that `WithTriviaFrom` did not replace inside the expression, so the code-action clean-up re-formatted the gap. Same pattern as RH3001/RH3006. Comment at the construction states why. | SHARED-FIX-006, RH3003-004 |

RH3005: no production change (characterization tests pass unchanged, Decision 7).

### Policy owners

`rg -n "TriviaList\(\)" src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3006*.cs src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3001*.cs`: the explicit-trivia token construction those fixes use; no shared helper exists for it, and none was added.

### Production diff size

See *Whole run*.

### Deleted special cases

_N/A._

### Tests run

- `RH3003UseStringEmptyForEmptyStringsAnalyzerTests`, `RH3005UseReadableConditionsAnalyzerTests`: in the combined analyzer run under *Whole run*, all pass, the existing Fix All scenario included.

### Disputed tests or scope

_None._

### Architecture findings

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1721

Nothing to implement: Decision 6 keeps the specified behavior and existing tests; no test was handed over for it.

## Whole run

### Production diff size

`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` (with this change): `4 files changed, 89 insertions(+), 36 deletions(-)`. No new type; one new public overload on the `public static` class `CopyrightHeaderTemplateResolver` in `Reihitsu.Analyzer` (internal to the repository by *Supported inputs*; one new caller, the RH8402 fix).

### Tests run

- `scripts/build.sh --configuration Debug` → `0 Warning(s) 0 Error(s)`
- `scripts/format.sh <the four changed files>` → `Formatted 0 of 4 file(s).`
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests|FullyQualifiedName~RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests|FullyQualifiedName~RH7101DoNotCombineFieldsAnalyzerTests|FullyQualifiedName~RH8402|FullyQualifiedName~RH3003|FullyQualifiedName~RH3005"` → `Passed! - Failed: 0, Passed: 483` (23 failed before the change)
- Full projects, Debug: formatter `Passed: 5023`, analyzer `Passed: 9389`, core `Passed: 652`, cli `Passed: 279`, architecture `Passed: 118`, each `Failed: 0`. Self-hosting scans not run (CI).
