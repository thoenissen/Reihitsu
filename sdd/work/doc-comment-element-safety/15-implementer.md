Role: implementer
Commit: d68c5e5
Verdict: #1329: no change in this round; #1389: tests pass; #1241: no change in this round; #1344: tests pass; #1242: no change in this round

Repair round 2. The inputs were findings F1, F2 and F3 of `13-auditor.md` (audited `9161d75`) and the reproductions in "Repair round 2" of `05-test-implementer.md` (`d68c5e5`).

I re-derived the fix from FMT-CLEAN-009, FMT-PIPE-004 and answer 4 in `00-lead.md`. The auditor proposed two ways to fix F1. I picked one and give the reasons below. No test file was changed.

**Before the change.** On `d68c5e5`, the formatter filter in *Tests run* gave 8 red tests, exactly the ones `05-test-implementer.md` lists as red:
- the four `…IndentedFirstToken…`, `…IndentedComment…`, `…IndentedDocumentationComment…`, and `…IndentedUsingDirective…InOnePass` tests;
- the four `ByteOrderMarkCharacterOnBlankLineBefore{Else,Finally,OpeningBrace,ClosingBrace}IsRemoved` tests.

## thoenissen/Reihitsu#1329

No change in this round. F4 and F5 are not routed to the implementer.

## thoenissen/Reihitsu#1389

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/BlankLines/Rewriter/BlankLineTokenCleanupRewriter.cs` | **F2.** `VisitToken` reads `startsText = token.FullSpan.Start == 0` before the base visit, because a token that the base visit rewrites is detached and has lost its position. Only the call for a token with no preceding token passes it to `CollapseLeadingBlankLines`, as the new parameter `startsText` (default `false`). The BOM is kept only when `startsText` is true and `trivia[0]` starts with `U+FEFF`. Anywhere else, a `U+FEFF` is removed with the blank line, as on base. | FMT-CLEAN-009 |
| same | **F1.** When no line break is kept, the BOM and the indentation that followed the removed blank lines become one white-space trivia. That is the shape the parser gives the resulting text. When the documentation boundary line break is kept, the order is BOM, line break, indentation, so no two white-space trivia are adjacent. The method summary describes both rules (FC-031). | FMT-CLEAN-009, FMT-PIPE-004 |
| `src/Reihitsu.Formatter/Utilities/ReihitsuFormatterHelpers.cs` | New overload `CreateByteOrderMarkTrivia(IEnumerable<SyntaxTrivia>)`: the BOM joined with the white space that follows it. | FMT-CLEAN-009 |

**Why join, rather than emit the BOM alone and drop the old indentation.** The joined trivia is what the parser produces when the output is read again. So every later phase sees the same trivia in this run as in the next one, which is the property FMT-PIPE-004 needs. The Indentation phase already handles a line-start white space that starts with `U+FEFF`: it writes the BOM and then the wanted column. Dropping the indentation would also pass these tests. But BlankLines would then decide something that FMT-INDENT owns, the column of the first line.

**Scope.** The change stays inside answer 4A. In the BlankLines phase, only `CollapseLeadingBlankLines` and the one call in `VisitToken` changed, and both serve "a first line that holds only the BOM and white space". The other `CollapseLeadingBlankLines` calls (after `{`, and before braces and the chained keywords) keep the default `startsText: false`. They behave exactly as on base.

### Policy owners

- `rg -n "StartsWithByteOrderMark|CreateByteOrderMarkTrivia" src` finds the helpers in `ReihitsuFormatterHelpers` and two callers: `BlankLineTokenCleanupRewriter.CollapseLeadingBlankLines` and `CleanupPhase.CleanWhitespaceBeforeEndOfLine`.
- Both callers now restrict the BOM to the start of the text:
  - BlankLines: `startsText`, from `token.FullSpan.Start == 0`, together with no preceding token;
  - Cleanup: `startsText: original.FullSpan.Start == 0` and trivia index 0.
- `CleanupPhase` is unchanged. Its middle-of-text test `RemovesByteOrderMarkCharacterOnBlankLineInTheMiddleOfTheText` was already green.

### Tests run

- Command: `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~ByteOrderMarkTests|FullyQualifiedName~CleanupPhaseTests|FullyQualifiedName~DocumentationCommentWithoutOwnEndTagTests|FullyQualifiedName~BlankLine"`
- Result: `Passed! - Failed: 0, Passed: 445, Total: 445`. The 8 tests that were red are among them.

### Disputed tests or scope

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1241

No change in this round.

## thoenissen/Reihitsu#1344

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/DocumentationCommentUtilities.cs` | **F3.** The constant `ByteOrderMark` is now the escape `'﻿'` instead of the invisible character. A scan of every `src/**/*.cs` file, ignoring a leading encoding signature, finds no `U+FEFF` character left. | FMT-DOC-011, RH8304-004 (no behavior change) |

### Policy owners

_Unchanged since `07-implementer.md`._

### Tests run

- Core suite: see *Whole run*.

### Disputed tests or scope

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1242

No change in this round.

## Whole run

- `scripts/build.sh --configuration Debug`: 0 warnings, 0 errors.
- `scripts/format.sh` over the 3 changed files: `Formatted 0 of 3 file(s).`
- Full suites, Debug:
  - formatter: `Passed! - Failed: 0, Passed: 2736, Total: 2736`
  - core: `Passed! - Failed: 0, Passed: 533, Total: 533`
  - analyzer: `Passed! - Failed: 0, Passed: 6115, Total: 6115`
