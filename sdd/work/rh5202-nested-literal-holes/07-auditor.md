Role: auditor
Commit: 685ab42
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit of head 685ab42 against base `origin/main` b4343557df67bc95a2278c405950476716fb9abf. The checkout is the head SHA, `origin/main` is the SHA in the prompt, and it is an ancestor of the head. The diff matches what the handovers describe: `RawStringLineUtilities` (new), `RawStringLineShifts` (predicates moved out, `CollectEdits` calls `MovesLine`), the RH5202 code fix rewritten to per-line `TextChange`s, 41 new RH5202 tests, and the RH5202 spec (RH5202-003 clarified, RH5202-004/005 added).

Evidence: `scripts/apply-fix.sh RH5202` over scratch fixtures, run at the head and at `origin/main` (temporary worktree), under LF and CRLF; `scripts/format.sh` over copies of the same fixtures. Fixtures and the worktree were deleted afterwards.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider.cs:63` (`currentClosingColumn = GetLeadingWhitespaceCount(...)`) against `src/Reihitsu.Analyzer/Rules/Layout/RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.cs:104` and `:133` (`TrimStart()`) | FC-001, FC-015, FC-007, FC-009 | – | PR-introduced | The rewrite measures the closing marker's column by counting only spaces and tabs. The analyzer still counts every whitespace character (`string.TrimStart`), and on `main` the fix did the same. When the whitespace in front of the closing quotes holds a different whitespace character, the fix shifts by the wrong amount. The analyzer keeps reporting, and on the next application the fix computes `delta == 0` and returns the document unchanged. The fix is offered, but it never clears the diagnostic. Counterexample (U+00A0 written as `·`): `var a = """⏎·   x⏎·   """;` inside `M()`. The analyzer measures the closing column as 4 and the opening as 16. The fix inserts 16 spaces where 12 are needed, so the analyzer now measures 20 and still reports, and the second application changes nothing. `apply-fix.sh`: head `no-progress (2 iterations)`, `main` `fixed (1 iteration)`. Class, enumerated: whitespace kind × position × direction × literal kind. **Failing at the head, fixed on `main`:** NBSP first, rightward, plain; U+000C first, rightward, plain; U+000B first, rightward, plain; U+2003 first, rightward, `u8`; NBSP first, rightward, interpolated `$"""`; NBSP behind 20 spaces, leftward, plain (the fix removes 4 where 5 are needed). **Failing on both, pre-existing:** NBSP first, leftward (see Hints). **Holding:** closing whitespace made only of spaces, or of spaces and tabs (existing tab tests), in every direction. Every cell was run under LF and CRLF with identical results. RH5202-003 is broken as well, because the closing quotes do not end up in the column of the opening quotes. | Measure the closing marker's width as the analyzer does, with one statement that the analyzer and the fix share instead of two copies (see *Architecture findings*), and keep the removal for a leftward shift consistent with that width. Add focused tests for the failing cells above, at least NBSP rightward, NBSP leftward behind spaces, and one interpolated cell, each with a CRLF variant or the existing CRLF pattern. |
| 2 | high | `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineUtilities.cs:27` (`line.Span.Length > 0`, applied by the fix at `RH5202...CodeFixProvider.cs:77`) against `sdd/specs/analyzer/RH5202.md` RH5202-003/RH5202-005 | FC-002, FC-001 | – | undecided output | `MovesLine` skips every empty line after the opening line, also an empty line inside an interpolation hole that is not inside nested text. The run's spec says the opposite. RH5202-005 is limited on purpose to content lines ("not a line of one of its interpolation holes"), and RH5202-003 says that every other line shifts by "adding spaces in front of each line". The code therefore breaks RH5202-003 for an empty hole line, and the fix's visible output changes from `main` with no test, no maintainer answer, and no unchanged requirement deciding the new output. Input: `var a = $"""⏎    Hello {name⏎⏎    }⏎    """;` inside `M(string name)`. On `main` the fix writes 13 spaces onto the empty hole line. At the head it leaves the line empty. Whole-document formatting removes the blank line altogether, so SHARED-FIX-007 decides neither output. **Members:** empty hole line, rightward: changed, no test. Empty hole line, leftward: unchanged (nothing to remove). Whitespace-only hole line: unchanged (2 → 15 spaces on both). Empty line inside nested text: covered by RH5202-004 and tested. Empty content line: covered by RH5202-005 and tested. | Either widen RH5202-005 (spec author) to every empty line after the literal's opening line, content or hole, and add a test for the empty hole line (LF and CRLF), or keep the spec and restrict the skip to content lines. The second option would bring back a private line policy in the fix and write trailing whitespace again. The lead decides whether the new output (the empty hole line stays empty) is covered or has to be asked as an output question. I recommend widening the spec: RH5202-005's own `Why` applies unchanged to a hole line. |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Line starts inside nested text (`StartsInsideNestedText`) | `FindTrivia`/`FindToken` at the line's start position; the previous token when a token starts the line with no leading trivia | Formatter constructor (`RawStringLineShifts` line 75) and `CollectEdits` (line 236) call the same method; FMT-RAW-009; RH5202-004 | Same method and same span. Traced cells: verbatim, `u8`, `$@`/`@$`, `/* */` behind and in front of a token, `/** */`, a nested regular `$"…"` with a multi-line hole, a nested raw string (moves), a nested raw string whose hole holds a verbatim string (both literals skip the verbatim line; `apply-fix` converges in 2 iterations at head and on `main`), the outer literal's own content, hole-opening and closing lines (move) | agree |
| Empty line not written (`line.Span.Length > 0`) | The line's span without its line break | RH5202-005 (content lines only); FMT-RAW-002 (formatter, empty lines stay empty) | Code covers all lines, the spec covers content lines only | disagree: finding #2 |
| Closing marker width (`GetLeadingWhitespaceCount`) | The prefix of the closing line made of `' '` and `'\t'` | The analyzer's `TrimStart()` (every `char.IsWhiteSpace`) at analyzer lines 104/133; on `main` the fix used `TrimStart()` too | Disagree for every other whitespace character | disagree: finding #1 |
| `delta == 0` → unchanged document | Result of the width above | The analyzer's report predicate `openingQuoteColumn != closingQuoteColumn` | Disagree on the same inputs, so an action is offered that changes nothing | disagree: finding #1 (FC-009) |
| Same-line literal → unchanged document | Opening and closing line numbers | The analyzer returns early for the same condition | Same | agree |
| Leftward removal amount | Leading `' '`/`'\t'` of each moved line, capped at `-delta` | Unchanged from `main` | — | holds apart from the pre-existing cell in *Hints* |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which lines of a raw string literal the move writes (nested text, empty line) | `rg -n "StartsInsideNestedText\|IsNonRawStringText\|MovesLine\(" src`: defined only in `RawStringLineUtilities.cs`; called from `RawStringLineShifts.cs:75`, `:236` and the RH5202 fix `:77`. `rg "InterpolatedVerbatimStringStartToken\|MultiLineCommentTrivia\)"` found no other copy that decides line moves | one owner, pass. The spec statement is narrower than the code: finding #2 |
| Width of the whitespace in front of the closing marker | `RH5202...Analyzer.cs:104`, `:133` (`TrimStart`); `RH5202...CodeFixProvider.GetLeadingWhitespaceCount` (spaces and tabs); `RawStringLineShifts.GetLeadingSpaceCount` (spaces only, after tab replacement) | three owners, and the diff made the fix's statement drift from the analyzer's: finding #1, *Architecture findings* |
| Opening-quote offset of an interpolated literal | `Reihitsu.Core` `RawStringLiteralUtilities.GetQuoteOffset`, used by the analyzer, the fix, and `RawStringLineShifts.GetQuoteOffset` | one owner, pass |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| 19 `VerifyFixLeaves…InHole…` / `…Comment…` tests (RH5202-004, LF and CRLF variants) | Lines that start inside nested text keep their exact text; every other line shifts; the diagnostic clears | Any changed column of a nested-text line, or a remaining diagnostic | `Verify`: exact fixed text, iterative single fix and Fix All, re-analysis | adequate |
| `VerifyFixShifts…Ended` / `…SingleLineComment…` / `…SingleLineVerbatim…` | The other side of each qualifier of RH5202-004 | A line after the nested text ended that stays unshifted | `Verify` | adequate (FC-003) |
| `VerifyFixLeavesNestedTextAsWrittenForSeveralLiteralsInOneDocument` | Fix All over two literals in opposite directions | A wrong merged text | `Verify` with 2 diagnostics | adequate (FC-008) |
| `VerifyFixShiftsLinesOfRawStringNestedInHoleAndNestedLiteralStaysReported` | One single fix shifts the inner raw string with the outer literal, and the inner literal stays reported | Different single-fix text, or the inner diagnostic gone | Analyzer `Verify` + `ApplyCodeFixAsync` exact text + `Verify` with 1 diagnostic | adequate. Fix All over the outer and inner literal is not pinned. Traced: the per-line changes of the two fixes meet at the same positions, so the batch either rejects the inner fix (the `main` outcome) or applies both, which aligns the inner literal correctly |
| 4 `VerifyFixMatchesFormatterFor…` | One fix application equals whole-document formatting and leaves no diagnostic | Any byte difference to `Format(...)` | `VerifyCodeFixMatchesFormatter` | adequate (FC-013, FC-007) |
| 7 RH5202-005 tests (empty content line, LF and CRLF, plain and interpolated, leftward, whitespace-only) | An empty content line stays empty; a whitespace-only line shifts | Spaces written onto the empty line, or an unshifted whitespace-only line | `Verify`, `ExpandWhitespaceMarkers` | adequate. The empty hole line has no test: finding #2 |
| Missing: closing whitespace with a non-space, non-tab character | The fix converges in one application (RH5202-003) | `no-progress` | — | missing: finding #1 |

## Failure classes dismissed

- FC-003: both sides are tested for every qualifier of RH5202-004, and for both directions. The missing cells belong to findings #1 and #2.
- FC-004, FC-005, FC-019, FC-020, FC-021: the fix only inserts or removes leading whitespace at line starts and joins no lines. A directive or disabled text in a hole is a parse error, so no diagnostic is reported. Comment lines are covered by RH5202-004.
- FC-006, FC-022: traced. Literal values are kept for every nested-literal kind. A regular string cannot hold a line start.
- FC-008: covered by the two-literal Fix All test and by the nested-raw trace above.
- FC-010, FC-011, FC-012: formatter behavior is unchanged. Traced: a nested-text line previously reached `CollectEdits` with a shift of 0, because the constructor excludes it from `_movingLiterals` and no `ApplyIndentation` places it, so `newSpaces == indentation`. Now the line is skipped one check earlier. The implementer's full formatter run (4517) was green.
- FC-013: the parity tests hold. The pre-existing formatter cells are in *Hints*.
- FC-014: no new rule.
- FC-016: every sibling shape of RH5202-004 is enumerated and tested.
- FC-017, FC-033, FC-034, FC-035: the helpers assert exact text, use CRLF variants, and reproduce the issue's own inputs. The expected values come from the spec, not from code output.
- FC-018, FC-023, FC-024, FC-025, FC-029, FC-036: no rename, identifiers, member kinds, or key/value layout are involved.
- FC-026: the edits are positional on line starts, not resolved by token text.
- FC-027: depth and enclosing construct are covered (argument, nested `$@`, nested regular `$"…"`, nested raw).
- FC-028: no detached or document-scoped formatting is involved; the fix is text-level.
- FC-030: the fix edits only line starts inside the literal's span, and the B7 test pins the unwrapped argument list.
- FC-031: the summaries of `ApplyCodeFixAsync`, `MovesLine`, `StartsInsideNestedText`, `CollectEdits`, and the class summaries describe the new code. `IsHoleOpeningLine`'s remark that only moving lines reach it stays true.
- FC-032: no project result was reused across the compiled change. The cleaner and the implementer ran after it.
- FC-037: no text-only claim.

## Architecture findings

The width of the whitespace in front of a multi-line raw string's closing marker is stated in three places:

- the RH5202 analyzer, as `TrimStart()` at `RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.cs:104` (interpolated) and `:133` (plain, `u8`), counting every whitespace character;
- the RH5202 code fix, as `GetLeadingWhitespaceCount`, counting spaces and tabs;
- the formatter, as `RawStringLineShifts.GetLeadingSpaceCount`, counting spaces after tab replacement.

The run rewrote the code fix's statement, and the rewrite drifted from the analyzer's, which is finding #1 (input: `var a = """⏎·   x⏎·   """;` with `·` = U+00A0). Decision 2 moved "which lines move" to one owner, but "by how much", the closing width, still has a private copy in the fix. Restoring `TrimStart()` in the fix would close #1 while keeping two copies of the analyzer's measurement. A shared helper that the analyzer and the fix both call would leave one statement.

## Hints

- Pre-existing, fails on `main` too: closing whitespace that starts with a non-space, non-tab character, shifted leftward (`var a = """⏎·                    x⏎·                    """;`). The fix removes only spaces and tabs from the front of the line, so it removes nothing, and `apply-fix` reports `no-progress` on `main` and at the head. Finding #1's repair closes it if the removal follows the shared width.
- Pre-existing, follow-up candidate (FC-013, same structural cause as above): whole-document formatting of `var a = """⏎·   x⏎·   """;` writes 16 spaces in front of the `·`. The formatter counts only spaces, so the RH5202 analyzer still reports its output (width 20 against column 16), and a second formatting pass changes nothing.
- Known and not the run's: on an empty line inside a hole, whole-document formatting removes the blank line, and on a re-alignable hole line it applies FMT-RAW-007. The fix does neither (RH5202-003 uniform shift; noted by the spec author).
