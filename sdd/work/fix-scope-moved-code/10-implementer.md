Role: implementer
Commit: e75064e (base); production commit is the commit that adds this file
Verdict: tests pass

How the statements below were established: `00-lead.md` (maintainer answers, escalation 9 = A, decisions 19–25), `07-implementer.md`, `08-spec-author.md`, `09-test-implementer.md`, and the spec diff of 840e68b read. The 8 failing rows of 09 reproduced on e75064e before any change (`Failed: 8, Passed: 676`). No test file was touched. Ignored input: my prompt carried the lead's decisions and process only; no intended fix, suspected cause, or hoped-for verdict.

## Whole run

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/SyntaxIndentationUtilities.cs` | `TryShiftMovedLines`: the per-line loop now refuses a left shift that would remove more characters than the whitespace at the start of a moving line (previously it cut the line off at column 0); the span's last line is clipped at the span's end, so a caller can pass a span that ends inside a line (RH3004). New public `KeepsItsColumn(root, text, line, end)`: the one statement of the line rule — blank line, comment line (only comments and whitespace, first comment in column 0, `///` lines on their own marker), non-region directive line; a line that holds code moves even with a column-0 comment in front of it (`root.FindToken(position).SpanStart >= lineEnd`). Private `AppendShiftedLines` and the old private `KeepsItsColumn(trivia, indentation)` replaced; small private `GetIndentationEnd` added | SHARED-FIX-007 (*Moved code*), SHARED-ORDER-010, RH3004-006 |
| `src/Reihitsu.Core/OrderingMoveSafety.cs` | `TryCreateWholeLineMove`: refuses when the moved node's leading trivia does not start its line (`IsLineStart(nodeToMove.FullSpan.Start)`), the counterpart of the existing target check; the branch that carried a `///` behind code onto its own line is deleted (decision 22). Deleted `MoveNodeBeforePreservingSeparators`, `GetLeadingGap`, `GetLeadingPayload` (decision 20); unused `using System.Linq` removed. Summary updated | SHARED-ORDER-009, SHARED-DOC-004, SHARED-ORDER-010 |
| `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` | Deleted `MoveMemberBefore` (decision 20) | — |
| `src/Reihitsu.Core/AccessorOrderingUtilities.cs` | Deleted `MoveAccessorBefore` (decision 20) | — |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3004UseLambdaSyntaxCodeFixProvider.cs` | Decision 19: the later lines are written by `SyntaxIndentationUtilities.TryShiftMovedLines`; `GetFirstStatementLinePosition` and `GetMovingLineColumns` decide moving lines with `SyntaxIndentationUtilities.KeepsItsColumn`. Deleted the private copy: `AppendLaterLines`, `ShiftIndentation`, `IsMovingLine`, `GetLineElements`, `AddTriviaElements`, `IsCode`. `GetFirstStatementLinePosition` takes the first statement's line directly (the first code at or after the content start is that statement's first token). `GetColumn` counts each character as one column, as the shared shifter does, so the shift RH3004 computes and the shift the helper applies agree. Summaries updated | RH3004-006, SHARED-FIX-007 |

### Policy owners

- `rg -n "KeepsItsColumn|TryShiftMovedLines|IsMovingLine|ShiftIndentation" src --glob '!**/obj/**' --glob '!*Test*/**'`: one owner, `SyntaxIndentationUtilities.KeepsItsColumn` / `TryShiftMovedLines`. Callers: `OrderingMoveSafety.TryCreateWholeLineMove` (RH7102/7103/7107/7108/7109), `RH7004UsingDeclarationsShouldNotBeUsedCodeFixProvider`, `RH3004UseLambdaSyntaxCodeFixProvider`. No private copy left.
- Separator rule (SHARED-ORDER-002): `rg -n "MoveNodeBeforePreservingSeparators|MoveMemberBefore|MoveAccessorBefore" src sdd documentation` finds nothing outside `sdd/work`; `TryCreateWholeLineMove` is the one owner.

### Production diff size

- `git diff --shortstat origin/main -- src ':(exclude,glob)src/*Test*/**'` (working tree of this round): 15 files changed, 681 insertions(+), 545 deletions(-) — net +136 (07 was +700/-242, net +458).
- Delta of this round against e75064e: 5 files changed, 131 insertions(+), 453 deletions(-).
- New types: none. New public member: `SyntaxIndentationUtilities.KeepsItsColumn` (needed by RH3004's shift measurement; see below).

### Deleted special cases

_N/A._ (No shape declared unsupported.) Deleted as decisions 19/20/22 ask: RH3004's private line shifter (six methods), the three old move helpers and their two private helpers, and the `///`-behind-code carry branch of `TryCreateWholeLineMove`.

### Tests run

```text
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH7004|FullyQualifiedName~RH7103|FullyQualifiedName~RH7109|FullyQualifiedName~RH7303|FullyQualifiedName~RH7310|FullyQualifiedName~RH3004|FullyQualifiedName~RH7102|FullyQualifiedName~RH7107|FullyQualifiedName~RH7108"
  Passed!  - Failed: 0, Passed: 788
scripts/test.sh --configuration Debug --project analyzer
  Passed!  - Failed: 0, Passed: 8451
scripts/test.sh --configuration Debug --project core
  Passed!  - Failed: 0, Passed: 651
scripts/test.sh --configuration Debug --project architecture
  Passed!  - Failed: 0, Passed: 117
scripts/format.sh <the five changed files>
  Formatted 0 of 5 file(s).
```

### Disputed tests or scope

1. **RH3004 with tab indentation — visible change (decision 19 consequence).** RH3004's private shifter counted a tab as four columns, both when measuring and when shifting left; the shared helper counts a tab as one column (SHARED-FIX-007, as the rules count it, FMT-PIPE-017). To keep one rule and keep the measurement and the shift consistent (otherwise a left shift can be refused or land in the wrong column), RH3004 now counts each character as one column. Measured with `scripts/apply-fix.sh RH3004` on a block whose statements are indented with three tabs under a lambda anchored at column 24: main writes 16 spaces + the tabs (tabs read as 4 wide, visually in the statement column); now 25 spaces + the tabs. Input class: tab indentation inside the moved block, which RH5601 reports — rare. RH3004-006 is silent on tabs, and the spec author scoped SHARED-FIX-007's tab sentence to member/accessor moves and RH7004 (08, *Internal choices* 4). No existing RH3004 test covers tabs. Options for the lead: A — accept (rare class, one rule; the spec author extends the tab sentence of SHARED-FIX-007 to RH3004-006); B — give the shared helper a tab-width parameter so RH3004 keeps four columns (a second counting rule in the one owner). Recommendation: A.
2. **RH3004 whitespace-only blank line inside the moved block — visible change.** The old private shifter wrote a whitespace-only line as an empty line (it copied from the first non-whitespace position); the shared helper keeps a blank line as written (SHARED-FIX-007: blank lines keep their column). Measured with `apply-fix.sh`: a line holding eight spaces between two statements now stays `        ` instead of becoming empty. Trailing whitespace on a blank line is reported by the spacing rules and removed by the formatter — rare; no RH3004 test covers it. Recommendation: accept.
3. **New public member** `SyntaxIndentationUtilities.KeepsItsColumn` (Core): RH3004 needs the line rule itself to find the leftmost moving line and its measured line, not only the shifted text.
4. **Release build fails on a test file**, not production: `src/Reihitsu.Analyzer.Test/Ordering/RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests.cs(21,14): error RH7305: Types should be organized with regions` (present at e75064e; I touched no test file). For the cleaner or the test implementer.

### Architecture findings

_None._ Decision 19 removed the second statement of the moved-line rule.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#969

- RH7103 `NoCodeFixWhenADocumentationCommentBehindCodeDocumentsTheMovedDeclaration` (Parse, Diagnose) and `NoCodeFixWhenADocumentationCommentBehindCodeAndAConditionalPairPrecedeTheMovedDeclaration` (Parse, Diagnose): pass. `TryCreateWholeLineMove` refuses a moved node whose full span does not start its line (decision 22, SHARED-ORDER-009/SHARED-DOC-004).
- RH7109 `NoCodeFixWhenALeftShiftWouldCutAMovingLineOffAtColumnZero` and `NoCodeFixWhenALeftShiftRemovesMoreColumnsThanATabAtTheStartOfAMovingLine`: pass. `TryShiftMovedLines` refuses a left shift larger than a moving line's leading whitespace (SHARED-ORDER-010); `LeftShiftMovesALineWithJustEnoughWhitespaceIntoColumnZero` and `RightShiftWritesSpacesInFrontOfATabAtTheStartOfAMovingLine` keep passing.
- Decision 20 carried out (helpers deleted).

## thoenissen/Reihitsu#1335

- RH7004 `CodeLineWithAColumnZeroCommentInFrontOfItMovesWithTheStatements` (LF, CRLF): passes; `KeepsItsColumn` keeps a column-0 comment line only when no token starts on that line. `LineOfOnlyCommentsStartingWithADelimitedCommentInColumnZeroKeepsItsColumn` keeps passing.

## thoenissen/Reihitsu#1263

No production change in this round; the RH7303/RH7310 tests of 09 pass.

## thoenissen/Reihitsu#1151

No change in this round.

## thoenissen/Reihitsu#1604

Decision 19 carried out: RH3004 uses the shared line rule and shifter; its private copy is deleted. Every existing RH3004 test passes unchanged, and `FixMovesALaterStatementLineThatStartsWithAColumnZeroCommentByTheShift` passes. RH3004-007/008/009 withholding is unchanged. Two untested visible differences for rare inputs (tabs, whitespace-only blank lines) are listed under *Disputed tests or scope* 1 and 2.
