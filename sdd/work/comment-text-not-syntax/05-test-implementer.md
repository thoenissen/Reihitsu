Role: test implementer
Commit: 50e689c
Verdict: #966: tests written (31 analyzer, 16 Core, and 8 formatter test cases fail on the issue's difference; the rest pass); #1289: tests written (all pass — no production change under the maintainer's answer A)

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec examples (SHARED-USING-009, RH2109-004) and, where a spec example does not spell out the layout, from the same input with the `#` of the comment line replaced by another letter, run on this head with `scripts/format.sh` and `scripts/apply-fix.sh RH7203|RH7201|RH7202|RH7204|RH7205|RH7206|RH7207` (LF and CRLF). SHARED-USING-009 makes the `#` line comment text, so the specified output equals that control's output with the `#` restored; no patched code was used.

## thoenissen/Reihitsu#966

### Tests

Core — `src/Reihitsu.Core.Test/UsingDirectiveOrderingSafetyTests.cs` (new), each `[DataRow("\n")]` and `[DataRow("\r\n")]`, calls `UsingDirectiveOrderingSafety.CanSafelyReorder` on the first block:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `HashLineInFileHeaderCommentIsSafe` (A1) | SHARED-USING-009 | direct call | fail (false, expected true) |
| `IndentedHashLineInCommentBetweenDirectivesIsSafe` (A2) | SHARED-USING-009 | direct call | fail |
| `HashLineInTrailingCommentIsSafe` (A3) | SHARED-USING-009 | direct call | fail |
| `HashLineInDelimitedDocumentationCommentIsSafe` (A4) | SHARED-USING-009 | direct call | fail |
| `HashLineInCommentBetweenTokensOfDirectiveIsSafe` (A5) | SHARED-USING-009 | direct call | fail |
| `HashLineInCommentOfBlockNamespaceIsSafe` (A6) | SHARED-USING-009 | direct call | fail |
| `HashLineInCommentOfFileScopedNamespaceIsSafe` (A7) | SHARED-USING-009 | direct call | fail |
| `CommentedOutConditionalDirectivesAreSafe` (A8) | SHARED-USING-009 | direct call | fail |
| `PragmaBetweenTokensOfDirectiveIsUnsafe` (B1) | SHARED-USING-009 | direct call | pass |
| `ConditionalGroupBetweenTokensOfDirectiveIsUnsafe` (B2) | SHARED-USING-009 | direct call | pass |
| `ConditionalGroupAcrossTokensOfTwoDirectivesIsUnsafe` (B3) | SHARED-USING-009 | direct call | pass |
| `ConditionalGroupBetweenDirectivesIsUnsafe` | SHARED-USING-009 | direct call | pass |
| `NullableDirectiveBeforeFirstDirectiveIsUnsafe` | SHARED-USING-009 | direct call | pass |
| `DirectiveInLeadingTriviaOfLaterDirectiveIsUnsafe` | SHARED-USING-009 | direct call | pass |
| `DirectiveAfterLastDirectiveIsSafe` | SHARED-USING-009 | direct call | pass |

Analyzer + code fix — `RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzerTests` (LF and CRLF via `DataRow` unless noted):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `HashLineInFileHeaderCommentDoesNotPreventReportAndFix` (spec example, A1, the issue's input) | SHARED-USING-009 | `VerifyCodeFixWithEndOfLine` | fail (0 diagnostics, 1 expected) |
| `HashLineInCommentBetweenDirectivesDoesNotPreventReportAndFix` (A2) | SHARED-USING-009 | `VerifyCodeFixWithEndOfLine` | fail |
| `HashLineInTrailingCommentDoesNotPreventReportAndFix` (A3) | SHARED-USING-009 | `VerifyCodeFixWithEndOfLine` | fail |
| `HashLineInDelimitedDocumentationCommentDoesNotPreventReportAndFix` (A4) | SHARED-USING-009 | `VerifyCodeFixWithEndOfLine` | fail |
| `HashLineInCommentBetweenTokensOfDirectiveDoesNotPreventReportAndFix` (A5) | SHARED-USING-009 | `VerifyCodeFixWithEndOfLine` | fail |
| `HashLineInCommentOfBlockNamespaceDoesNotPreventReport` (A6, diagnostic only — see *Not covered*) | SHARED-USING-009 | `Verify(source)` | fail |
| `HashLineInCommentOfFileScopedNamespaceDoesNotPreventReportAndFix` (A7) | SHARED-USING-009 | `VerifyCodeFixWithEndOfLine` | fail |
| `CommentedOutConditionalDirectivesDoNotPreventReportAndFix` (A8) | SHARED-USING-009 | `VerifyCodeFixWithEndOfLine` | fail |
| `HashLineInFileHeaderCommentDoesNotPreventFixAll` (two diagnostics, one-at-a-time and Fix All; LF) | SHARED-USING-009 | `Verify(source, fixedSource)` | fail (0 of 2) |
| `HashLineReadAsDirectiveByAnotherBuildIsReportedWithoutFix` (A9) | SHARED-USING-009, SHARED-FIX-012 | `VerifyNoCodeFixIsOffered` | fail (0 of 1) |
| `PragmaBetweenTokensOfDirectiveIsNotReported` (B1, spec example) | SHARED-USING-009 | `Verify(source)` | pass |
| `ConditionalGroupBetweenTokensOfDirectiveIsNotReported` (B2) | SHARED-USING-009 | `Verify(source)` | pass |
| `ConditionalGroupAcrossTokensOfTwoDirectivesIsNotReported` (B3) | SHARED-USING-009 | `Verify(source)` | pass |

One test per other analyzer, `HashLineInCommentDoesNotPreventReportAndFix` (LF and CRLF, `VerifyCodeFixWithEndOfLine`, tagged SHARED-USING-009), all fail now on 0 of 1 diagnostics: `RH7201SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectivesAnalyzerTests`, `RH7202UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesAnalyzerTests`, `RH7204UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameAnalyzerTests`, `RH7205UsingStaticDirectivesMustBePlacedAtCorrectPositionAnalyzerTests`, `RH7206UsingStaticDirectivesMustBeOrderedAlphabeticallyAnalyzerTests` (file header `/* # Header */`), `RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzerTests` (`/* # Alpha helpers */` between two groups; fix adds the blank line between groups).

Formatter — `src/Reihitsu.Formatter.Test/Regression/UsingDirectives/UsingDirectiveCommentTextTests.cs` (new). `AssertReordered` runs `AssertRuleResult` (pipeline, LF and CRLF, second pass, phase stability) and `ReihitsuFormatter.FormatSyntaxTree` (LF and CRLF); `AssertUnchangedThroughEntryPoint` runs `FormatSyntaxTree` (LF and CRLF):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `HashLineInFileHeaderCommentDoesNotPreventReordering` (spec example, A1) | SHARED-USING-009 | `AssertReordered` | fail (order kept) |
| `HashLineInCommentBetweenDirectivesDoesNotPreventReordering` (A2) | SHARED-USING-009 | `AssertReordered` | fail |
| `HashLineInTrailingCommentDoesNotPreventReordering` (A3) | SHARED-USING-009 | `AssertReordered` | fail |
| `HashLineInDelimitedDocumentationCommentDoesNotPreventReordering` (A4) | SHARED-USING-009 | `AssertReordered` | fail |
| `HashLineInCommentBetweenTokensOfDirectiveDoesNotPreventReordering` (A5) | SHARED-USING-009 | `AssertReordered` | fail |
| `HashLineInCommentOfBlockNamespaceDoesNotPreventReordering` (A6) | SHARED-USING-009 | `AssertReordered` | fail |
| `HashLineInCommentOfFileScopedNamespaceDoesNotPreventReordering` (A7) | SHARED-USING-009 | `AssertReordered` | fail |
| `CommentedOutConditionalDirectivesDoNotPreventReordering` (A8) | SHARED-USING-009 | `AssertReordered` | fail |
| `PragmaBetweenTokensOfDirectivePreventsReordering` (B1, spec example as amended) | SHARED-USING-009 | `AssertRuleResult(input)` + `AssertUnchangedThroughEntryPoint` | pass |
| `ConditionalGroupBetweenTokensOfDirectivePreventsReordering` (B2) | SHARED-USING-009 | same | pass |
| `ConditionalGroupAcrossTokensOfTwoDirectivesPreventsReordering` (B3) | SHARED-USING-009 | same | pass |
| `HashLineReadAsDirectiveByAnotherBuildLeavesTheDocumentUnchanged` (A9) | FMT-PIPE-033 | `AssertUnchangedThroughEntryPoint` | pass (and must keep passing) |

Every failing test was watched fail for the right reason: Core `IsTrue` got `false`; every analyzer failure is `expected "1" actual "0"` (one `expected "2" actual "0"` for Fix All); every formatter failure shows the input order kept (e.g. A3 actual `using System.Linq; /*…*/` then `using System;`).

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Issue's input (A1, `/* # Header */` before unordered usings) | LF, CRLF | RH7203 analyzer + fix | reproduced | no diagnostic vs RH7203 on `System` and reorder |
| A1 | LF, CRLF | formatter (`FormatSyntaxTree`, pipeline) | reproduced | order kept vs `using System;` first |
| A1 analogue per rule | LF, CRLF | RH7201, RH7202, RH7204, RH7205, RH7206, RH7207 | reproduced | no diagnostic vs one diagnostic and fix |
| A2–A8 (positions, `/** */`, block and file-scoped namespace, commented-out directives) | LF, CRLF | Core, RH7203, formatter | reproduced | as A1 |
| A9 (`#else` line in a comment that starts inside a group) | LF, CRLF | RH7203 | reproduced | no diagnostic vs diagnostic without fix |

### Not covered, and why

- **A6 code fix.** On this head, the RH7203 fix for an unordered block of a block-scoped namespace whose first using carries any leading comment (`// Header` and `/* Header */` controls too, no `#` involved) writes the moved directive at column 0: `scripts/apply-fix.sh RH7203` output `+using System.Linq;` under `    using System;`, LF and CRLF; without the comment the fix indents correctly. The formatter writes column 4. That is independent of this run's guard, so A6 asserts the diagnostic only on the fix surface; the formatter test asserts the full reorder. Follow-up candidate (observation only): RH7203 (likely every using-ordering fix through the shared utilities) misindents a moved directive in a block-scoped namespace whose first directive has a leading comment.
- **Analyzer↔formatter parity helper** (`FormatterTestsBase<TAnalyzer>.VerifyFormatter`): no RH7203 parity class exists; the fix tests assert exactly the texts the formatter tests assert for A1–A5, A7, A8, which pins the same output on both surfaces (FC-013).
- **One test per analyzer, not per matrix member.** `rg CanSafelyReorder src --type cs` shows RH7201–RH7207 analyzers, `UsingDirectiveCodeFixUtilities`, `UsingDirectiveOrderingCodeFixProviderBase`, the RH7207 provider, and `UsingDirectiveOrderingRewriter` all call `UsingDirectiveOrderingSafety.CanSafelyReorder` with the scope's `Usings`; the full matrix runs on Core, RH7203, and the formatter, one member on each other analyzer.
- **FC-031 (stale summaries of `_preprocessorDirectiveRegex`, `ContainsPreprocessorDirective`)**: production text, not a test concern; for the auditor.
- **Shebang `#!` / `#:` before the first using**: dropped by `03-skeptic.md` — a syntax error, which the formatter and fixes skip.
- **Comment and directive in a token gap**: the directive-in-gap cases are B1–B3, the comment-in-gap case A5.
- Other failure classes (naming, Renamer, FMT-UNSUP) do not reach a reorder-safety guard.

## thoenissen/Reihitsu#1289

### Tests

All in `RH2109RazorCodeBlocksShouldNotBeUsedAnalyzerTests` (`Verify` with a `.razor` additional file). All pass now; under answer A no production change is expected, so they lock today's output as the decided result.

Retagged characterization tests (summary's "pins today's behavior without stating it as intended" removed, `Requirement` added):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyCodeBlockLineInsideCSharpBlockCommentIsReportedWhileTextInMarkupCommentsIsNot` | RH2109-001, RH2109-004 | `Verify` | pass |
| `VerifyCodeBlockLinesInCSharpCommentAndRawStringAreReported` (the issue's own shape, line 7) | RH2109-004 | `Verify` | pass |
| `VerifyCodeBlockLineInsideDocumentationBlockCommentIsReported` | RH2109-004 | `Verify` | pass |
| `VerifyCodeBlockLineInsideVerbatimStringIsReported` | RH2109-004 | `Verify` | pass |
| `VerifyCodeBlockLineInsideInterpolatedVerbatimStringIsReported` | RH2109-004 | `Verify` | pass |
| `VerifyCodeBlockLineInsideInterpolatedRawStringIsReported` | RH2109-004 | `Verify` | pass |
| `VerifyCodeBlockLineInsideBlockCommentOfFunctionsBlockIsReported` | RH2109-004 | `Verify` | pass |
| `VerifyCodeBlockLineInsideBlockCommentAfterClosingBracesInLiteralsIsReported` | RH2109-004 | `Verify` | pass |
| `VerifyCodeBlockLineInsideBlockCommentAfterNestedBracesIsReported` | RH2109-004 | `Verify` | pass |

New:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifySpecExampleCodeBlockLinesInsideCSharpCommentAndRawStringAreReported` (example 1: lines 1, 3, 7) | RH2109-004 | `Verify` | pass |
| `VerifySpecExampleCodeBlockLinesInsideCSharpCommentAndRawStringWithCarriageReturnLineFeedAreReported` | RH2109-004 | `Verify`, CRLF | pass |
| `VerifySpecExampleRazorCommentOpenerLineInsideCSharpCommentHidesTheRestOfTheFile` (example 2) | RH2109-004 | `Verify` | pass |
| `VerifySpecExampleRazorCommentOpenerLineInsideCSharpCommentWithCarriageReturnLineFeedHidesTheRestOfTheFile` | RH2109-004 | `Verify`, CRLF | pass |
| `VerifySpecExampleRazorCommentOpenerLineInsideCSharpCommentIsNotClosedByHtmlCommentCloser` (example 3) | RH2109-004 | `Verify` | pass |
| `VerifyRazorCommentOpenerLineInsideCSharpCommentEndsAtRazorCommentCloser` (other side: `*@` follows, line 7 reported) | RH2109-004 | `Verify` | pass |
| `VerifyHtmlCommentOpenerLineInsideCSharpCommentHidesTheRestOfTheFile` (`<!--` converse) | RH2109-004 | `Verify` | pass |
| `VerifyHtmlCommentOpenerLineInsideCSharpCommentIsNotClosedByRazorCommentCloser` (pairing, `<!--` side) | RH2109-004 | `Verify` | pass |
| `VerifyHtmlCommentOpenerLineInsideCSharpCommentEndsAtHtmlCommentCloser` (other side: `-->` follows) | RH2109-004 | `Verify` | pass |
| `VerifyRazorCommentOpenerLineInsideVerbatimStringHidesTheRestOfTheFile` (converse inside a literal) | RH2109-004 | `Verify` | pass |
| `VerifyCodeBlockLineInsideBlockCommentOfStatementBlockIsReported` (host `@{ }`) | RH2109-004 | `Verify` | pass |
| `VerifyCodeBlockLineInsideBlockCommentOfControlFlowBlockIsReported` (host `@if`) | RH2109-004 | `Verify` | pass |
| `VerifyCodeBlockLineWithBraceOnNextLineInsideBlockCommentIsReported` (brace on next line) | RH2109-004 | `Verify` | pass |
| `VerifyCodeBlockTextInsideSingleLineCommentIsNotReported` (`// @code {`, `/* @code { */`: the single-line sentence) | RH2109-004 | `Verify` | pass |

### Scenarios (bug fix)

_Not applicable_ — under answer A the issue's input keeps its reported output; `VerifyCodeBlockLinesInCSharpCommentAndRawStringAreReported` is its regression test with the decided result (FC-034 shape: `@code {` starting its own line inside the raw string).

### Not covered, and why

- `VerifyFunctionsBlockIsNotReported` keeps its characterization wording: it is thoenissen/Reihitsu#1452's case, not this run's.
- Tab indentation inside a C# extent: the scanner's line-start check is the one RH2109-001 already tests with a tab (`VerifyCodeBlockIndentedWithTabIsReported`); no separate branch for C# extents exists, since RH2109-004 states nothing is lexed.
- Code fix, formatter, CLI: RH2109 has no fix and `.razor` is never formatted.

## Whole run

- `scripts/format.sh` over every changed test file: two new files reformatted (final newline only).
- Architecture tests: `UsesListsExactlyTheReferencedRequirements` fails — `shared/diagnostics.md: RH2109-004 is referenced but missing from Uses`. It fails identically on 50e689c without my changes (checked with my tracked edits stashed): a spec finding for the spec author, not caused by these tests. Every other architecture test, including requirement coverage, passes.

## Commands

```shell
scripts/test.sh --project core --filter "FullyQualifiedName~Reihitsu.Core.Test.UsingDirectiveOrderingSafetyTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.UsingDirectives.UsingDirectiveCommentTextTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH720"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH2109RazorCodeBlocksShouldNotBeUsedAnalyzerTests"
scripts/test.sh --project architecture
```
