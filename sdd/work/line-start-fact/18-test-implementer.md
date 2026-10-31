Role: test implementer
Commit: 596bb57
Verdict: tests written

This is the structural change after the second audit: `17-auditor.md` finding #4 and the decision `squad/decisions/2026-10-31-line-start-fact-audit-2-repeat.md`, option B. The prompt carried no intended fix.

## Enumeration

**Search.** Over `src/Reihitsu.Analyzer`, `src/Reihitsu.Analyzer.CodeFixes`, and `src/Reihitsu.Core`, excluding `obj/`:
1. `rg -n "SyntaxKind\.EndOfLineTrivia"`. This gives 28 hits. A search without the prefix found nothing more.
2. `rg -n "TrailingTrivia\.Count == 0|HasTrailingTrivia|TrailingTrivia\.Any\(|LeadingTrivia\.Count == 0|HasLeadingTrivia"`.
3. `rg -n "bool \w*(StartsLine|EndsLine|LineStart|OnOwnLine|OwnLine|FirstOnLine|LineBreak|NewLine|SameLine)\w*\("`.
4. A text search for `'\n'`, `"\n"`, `'\r'`, and `Environment.NewLine`.

A decision made from Roslyn's line table (`GetLineSpan`, `Lines.GetLineFromPosition`) is not a member: the line table counts the line break of a `///` comment.

**Probes.** For each candidate I built the `///` input and its `//` counterpart. I formatted both with `scripts/format.sh --force`, then ran the rule's code-fix surface with `scripts/apply-fix.sh <ID>` on the formatter output, under LF and CRLF. A member fails when the rule reports or rewrites formatter output while its `//` counterpart gets `no-diagnostic`.

### Members that fail: tested

| Site | Decision | Failing inputs (formatter output) | `//` counterpart |
|---|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Spacing/RH6001KeywordsMustBeSpacedCorrectlyAnalyzer.cs:66-68` | A line break or whitespace behind the keyword, judged from the keyword's trailing trivia (`WhitespaceTrivia` or `EndOfLineTrivia`) | `if`, `while`, `do … while`, `for`, `foreach`, `fixed`, `lock`, `switch`, `using`, each as `keyword/// x` / `(…)`. `apply-fix.sh RH6001`: fixed, 1 iteration, LF and CRLF, 18 of 18 | no-diagnostic, 18 of 18 |

### Holding members: no test needed

| Site | Why it holds or cannot be reached |
|---|---|
| RH6001 with `return`, `throw`, `catch` | The documentation-comment phase moves the comment onto its own line behind a blank line (`return` / blank / `/// x` / `(1);`), so the keyword is followed by an end-of-line trivia. Not reachable on formatter output |
| `src/Reihitsu.Core/SyntaxTriviaUtilities.cs:241` `HasCommentDirectlyAbove` | Returns true for a `///` written behind the previous code, because `EndsWithLineBreak` is true and no end-of-line trivia follows. The `//` counterpart lies in the previous token's trailing trivia and gives false. No caller in `src/` (`rg HasCommentDirectlyAbove`), so it cannot be reached |
| `src/Reihitsu.Core/SyntaxTriviaUtilities.cs:314` `ContainsBlankLine`, called from `Reihitsu.Analyzer/Base/StatementShouldBePrecededByABlankLineAnalyzerBase{TStatement}.cs:102` and `…FollowedBy…{TStatement}.cs:76` | Misses a single blank line behind a `///` that is written behind code, because the `///` resets the run. In formatter output a `///` in front of a statement is always moved onto its own line behind a blank line (characterized by `StrandedDocumentationCommentBeforeStatementIsStillRelocated`). The trivia then reads end-of-line, end-of-line, whitespace, `///`, so the blank line is found. Not reachable on formatter output |
| `src/Reihitsu.Core/SyntaxTriviaUtilities.cs:572` `GetLineIndentationTrivia`, called from `Reihitsu.Analyzer.CodeFixes/Base/TargetAttributeListShapeCodeFixProviderBase.cs:112` | Behind a `///` in the attribute list's leading trivia, it returns the whitespace in front of the comment instead of the whitespace behind it. In formatter output, a `///` in front of a member is the member's own documentation on its own line (or is moved there, per `StrandedDocumentationLineStartTests.StrayDocumentationCommentBefore…`), with the same indentation on both sides, so the value is the same. Not reachable on formatter output |
| `src/Reihitsu.Analyzer/Rules/Organization/RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.cs:54` `HasBlankLineBefore` | Counts end-of-line trivia only in front of the first significant trivia. For `using System; /// x` / blank / `using Foo.Bar;` the formatter moves the comment (`using System;` / blank / `/// x` / `using Foo.Bar;`), so the blank line stands in front of the `///`. `apply-fix.sh RH7207`: no-diagnostic, LF and CRLF |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Organization/RH7304RegionDirectivesMustUseConsistentIndentationCodeFixProvider.cs:128` `IsLineBoundary` | A `///` does not count as a line boundary. For `int A; /// x` / `#region R` the formatter moves the comment onto its own line behind a blank line, so a `///` is never in front of a `#region` on the same trivia line |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Organization/RH7501BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksCodeFixProvider.cs:127,134` | Its line-start state after a `///` misses the line break. A `///` in front of `break` is moved by the documentation-comment phase, so it cannot be reached on formatter output |
| `src/Reihitsu.Analyzer/Rules/Layout/RH5020CommentsShouldBePrecededByABlankLineAnalyzer.cs:83`; `src/Reihitsu.Analyzer/Rules/Documentation/RH8303ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzer.cs:65,93` | Skip whitespace and end-of-line trivia to find a preceding comment or directive. They decide adjacency of trivia, not whether a line break separates tokens. The `///` and `//` cases give the same answer |
| `src/Reihitsu.Core/AttributeTargetUtilities.cs:281` | An allowlist of the leading trivia a join may cross (whitespace, end-of-line). A `///` blocks the join, which is safe (FMT-PIPE-002). Not a line-break decision |
| `src/Reihitsu.Core/EmptyTypeDeclarationSemicolonAnalysisUtilities.cs:121` | "Meaningful trivia" guard. A `///` counts as meaningful and blocks the rewrite. Not a line-break decision |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5104CommentsMustBeOnTheirOwnLineCodeFixProvider.cs:118` | Withholds the fix when a non-whitespace trivia spans the line start. A multi-line `///` does span it, and withholding is correct. Not a line-break decision |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Documentation/RH8201InheritdocShouldBeUsedCodeFixProvider.cs:71` | Drops the end-of-line trivia behind a replaced documentation comment. Not a token line-break decision |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3002StatementMustNotUseUnnecessaryParenthesesCodeFixProvider.cs:116-129` | Inserts a space only when both sides are empty; a `///` makes the trivia non-empty. Conservative. Not a line-break decision |
| `src/Reihitsu.Analyzer/Rules/Layout/RH5604CodeMustNotContainMixedLineEndingsAnalyzer.cs:55`; `…CodeFixes/Rules/Layout/RH5604CodeMustNotContainMixedLineEndingsCodeFixProvider.cs:52` | Line-ending consistency of end-of-line trivia. A terminator question, not line separation. It belongs to the analyzer-side terminator follow-up (below) |
| `src/Reihitsu.Core/FluentChainLink.cs:85` `HasInnerLineBreak` | Uses `SyntaxTokenPositionUtilities.IsFirstOnLine`, the shared fact |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3103UseShorthandForNullableTypesCodeFixProvider.cs:115` `IsOwnLineComment` | Uses the line table |
| `src/Reihitsu.Core/AdjacentTokenSpacingUtilities.cs:33-34` (RH6021), `RH6005…Analyzer.cs:75` | Already routed through the shared fact in repair round 1 |
| `src/Reihitsu.Core/FormattingTextAnalysisUtilities.cs:383` | Returns false for every character other than a space or a tab (auditor, first audit) |

### Already-decided follow-ups: not tested here

- **Terminator sets:**
  - RH5202 (`RH5202…Analyzer.cs:100,131`, `RH5202…CodeFixProvider.cs:72,90`);
  - RH5401 (`RH5401…CodeFixProvider.cs:49`);
  - RH5603 (`RH5603…Analyzer.cs:56-57`);
  - RH8402 (`RH8402…CodeFixProvider.cs:68,74-75,99-102,132`);
  - the copyright-header helpers (`Reihitsu.Analyzer/Data/CopyrightHeaderTemplateResolver.cs:85-104`, `Data/ConfigurationManager.cs:366-372`).
  Decision 3A.
- **`AttributeTargetUtilities.StartsLine`** (`src/Reihitsu.Core/AttributeTargetUtilities.cs:350-355`). Decision 3A.
- **`ListElementInteriorJoinRewriter`'s tight-pair table.** Decision `…-join-spacing-pairs`.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Analyzer.Test/Formatting/RH6001…AnalyzerTests.Verify{If,While,DoWhile,For,ForEach,Lock,Switch,Using}KeywordFollowedByStrayDocumentationCommentIsNotReported` (8 tests; input is the formatter's output) | RH6001-002 (FC-013, FC-015) | `Verify(source)` | red, 1 unexpected diagnostic each |
| `…RH6001….VerifyFixedKeywordFollowedByStrayDocumentationCommentIsNotReported` | — (see *Finding*) | `Verify(source, AllowUnsafe)` | red |
| `…RH6001….Verify{…9 keywords…}KeywordFollowedBySingleLineCommentIsNotReported` (the `//` controls) | — | `Verify(source)` | green |
| `Analyzer.Test/Formatter/Formatting/RH6001…FormatterTests.VerifyFormatterOutputWithStrayDocumentationCommentBehindKeywordIsNotReported` (`if/// x` / `(c)`) | RH6001-002 (FC-013) | `VerifyFormatter(source)` | red |

Run: `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH6001"` gives `Failed: 10, Passed: 19`. The ten failures are the red rows above, each with "expected 0, actual 1" diagnostics.

## Finding

The RH6001 analyzer checks `fixed` (`RH6001…Analyzer.cs:57`, `SyntaxKind.FixedKeyword`), but RH6001-001 does not list `fixed`. Its list is `if`, `while`, `for`, `foreach`, `switch`, `catch`, `lock`, `using`, `return`, `throw`.
- The `fixed` test therefore carries no requirement tag.
- The spec and the code disagree on `fixed`. This predates the run. The spec author or the maintainer has to decide it.

## Not covered, and why

- **`if/* c *//// x` / `(c)`**: a `/* */` comment in the keyword's trailing trivia, then a `///`. RH6001-002 excludes it as well. I did not check whether the formatter keeps this shape, so no test was written. Candidate if the auditor wants an analyzer-only row.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH6001"
```
