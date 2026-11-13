Role: auditor
Commit: 863d57d3c9e0f9733e71c76354bb113be9113f47
Verdict: pass, text cleanup

My prompt named the run, the base, the earlier audit, the decisions, and the handovers to read. It carried no intended fix, suspected cause, or hoped-for verdict beyond pointing at those files.

## Scope

Head `863d57d3c9e0f9733e71c76354bb113be9113f47` (checked out). Base `origin/main` = `4bdcd13186c2882e63985526458f63c4b359236d`, an ancestor of the head. This is a repair-delta audit of `0d587f2..863d57d` against `26-auditor.md`. The production delta is two files. In `RH7004UsingDeclarationsShouldNotBeUsedCodeFixProvider.TryCreateChanges`, one `HoldsOtherLineBreak` check over `FromBounds(usingDeclaration.SpanStart, movedSpan.End)` now runs before the paths split, and the narrower `movedSpan` check is removed. In `SyntaxIndentationUtilities.TryShiftMovedLines`, only the summary changes. The test delta is five `VerifyNoCodeFixIsOffered` tests in `RH7004UsingDeclarationsShouldNotBeUsedAnalyzerTests`. Everything else is handovers and lead notes. The diff matches handovers 27 and 28 and decisions 43 to 45. The scope did not grow: the repair touches only the RH7004 fix that #2 and #4 name and the summary that #5 names.

How I checked: I traced the changed guard against every `TextChange` the fix emits on both paths. I re-ran the audit-26 harness (one analyzer, its first code action, the output byte for byte, line endings not normalized), which builds against the head, on `n1`–`n6` and `h1` from `scratchpad/r2`. I also ran it on nine new boundary fixtures `m1`–`m9`, in `/tmp/claude-0/-home-user-Reihitsu/5c1cfd56-b4c6-5d2d-b57f-79173e97c80b/scratchpad/r3`. Every result below is measured.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 2 | closed | Every listed member is refused at the head (actions: 0). `n1`: LF behind the declaration in a CRLF document, statements follow. `n6`: the same, with the declaration as the last statement. `n5`: lone `\r`, the declaration as the last statement. Tests: `NoCodeFixWhenTheLineBreakBehindTheDeclarationIsOtherThanTheDocumentsLineEnding`, `…BehindTheLastStatementDeclaration…`, `NoCodeFixForALastStatementDeclarationInADocumentWithLoneCarriageReturnLineBreaks`. |
| 4 | closed | `n2` (a declaration that spans lines with an LF inside it, statements follow) and `n3` (the same on the empty-block path) are refused. Tests: `NoCodeFixWhenTheDeclarationHoldsALineBreakOtherThanTheDocumentsLineEnding`, `…AndTheBlockStaysEmpty`. |
| 5 | closed | The summary now says a blank line "holds only whitespace and does not start inside a string or a comment that spans lines or in disabled text". That matches the code: the `StartsInsideContent` branch comes before the blank-line branch. `h1` (RH7109, shift 0) still keeps the whitespace on the hole line, which the summary now describes. |
| Arch 26-1 (the span RH7004's line-ending refusal checks is chosen apart from its changes) | closed | The fix emits `(` at `Declaration.SpanStart`, which is at or after `usingDeclaration.SpanStart`. It also emits `;` → `)` (statements path) or `;` → `)⏎{⏎}` (empty-block path, `Semicolon.Span.End` ≤ `FullSpan.End` = `movedSpan.End`), and `movedSpan` itself. The checked span `[usingDeclaration.SpanStart, movedSpan.End)` contains every change on both paths, plus the declaration's trailing line break. `HoldsOtherLineBreak` tests each line break whose start lies in `[span.Start, span.End)`. `movedSpan.End` is a line start on every path that reaches a change, so no CRLF pair is split at the boundary. |

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 6 | low | `sdd/specs/analyzer/RH7004.md` RH7004-007, last paragraph ("is withheld where the code it moves holds a line break other than that line ending"); `sdd/changes/2026-11/moved-code-keeps-its-lines.md` header (SHARED-FIX-008 "…for a foreign line break in the moved code"), *Visible output* bullet "Foreign line breaks in moved code", the paragraph under "Rare-class refusals of repair round 2" ("where the moved code holds…"), and decision "A foreign line break in moved code withholds the move" ("the check of the moved span's line breaks") | FC-031 (text describes the old span) | – | PR-introduced (decision 43) | The code and SHARED-FIX-008 ("the code it moves **or rewrites**") now also withhold RH7004 when the foreign line break is in the rewritten declaration or directly behind it. That covers `n2`, `n3`, `n5`, and `n6`, where nothing or nothing affected is moved. The five new tests carry `[Requirement("RH7004-007")]`, but RH7004-007 and the change doc state the refusal only for moved code, so a reader of either would expect those inputs to keep the fix. No output depends on the text, and SHARED-FIX-008, which RH7004-007 points to, already states the behavior. | Say "the code it moves or rewrites, the declaration and its trailing line break included" in RH7004-007 and in the change doc's bullet, paragraph, and decision, citing decision 43. This is text only. |

Why it is text cleanup: spec and change-doc prose only. No compiled behavior, diagnostic, rule page, or public API documentation is involved, and the governing requirement (SHARED-FIX-008) already matches the code.

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Foreign line break withholds RH7004 | `HoldsOtherLineBreak(FromBounds(usingDeclaration.SpanStart, movedSpan.End))`, before the paths split | The fix's `TextChange`s (`(` at `Declaration.SpanStart`; the semicolon; `movedSpan`), and SHARED-FIX-008's "code it moves or rewrites, its trailing trivia included" | Yes, on both paths. Measured inside, refused: `n1`–`n3`, `n5`, `n6`, `m3` (LF in front of `;`), `m4` (LF behind a trailing `// c`), `m7` (LF behind the last moved statement). Measured outside, fix kept with the LF untouched and every created break CRLF: `m5` (LF on the `{` line in front of the declaration), `m6` (LF behind the enclosing block's `}`), `m1` (empty-block path, single-line declaration, LF at the end of the file). Controls: `n4`, `m8` (all CRLF, also a declaration that spans lines), `m9` (all LF, last statement) are offered with uniform endings | ok |
| Foreign line break, member/accessor move; RH3004 block body | Unchanged since audit 26 | — | Byte-identical code; audit 26 evidence reused | ok |
| Lone `\r`, empty-block path, single-line declaration with code behind it (`m2`) | The checked span holds no line break | SHARED-FIX-008: the fix creates `\n`; it is withheld only where the code it moves or rewrites holds a line break | Offered, writes `)\n{\n} }`, and keeps the `\r` outside. This is what the spec demands | ok (holds) |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Foreign line break withholds a moving fix | `LineEndingUtilities.HoldsOtherLineBreak` is the only predicate. Its callers: `OrderingMoveSafety.TryCreateWholeLineMove` (its `TextChange` span), `RH3004…WithholdsFixForLines` (`block.Span`), and `RH7004…TryCreateChanges` (now the span its changes cover, one call). The removed second call leaves no duplicate | one owner; each caller's span now agrees with its changes |
| Blank line in moved code is written empty | `SyntaxIndentationUtilities.TryShiftMovedLines` (unchanged code) | ok |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `NoCodeFixWhenTheLineBreakBehindTheDeclarationIsOtherThanTheDocumentsLineEnding` | SHARED-FIX-008: a foreign break behind the rewritten declaration withholds the fix (statements path) | Fix offered | `VerifyNoCodeFixIsOffered` | adequate. It failed before the change for the right reason (handover 28), so the LF reaches the fix unnormalized |
| `NoCodeFixWhenTheLineBreakBehindTheLastStatementDeclarationIsOtherThanTheDocumentsLineEnding` | Same, with nothing moved | Fix offered | same | adequate |
| `NoCodeFixForALastStatementDeclarationInADocumentWithLoneCarriageReturnLineBreaks` | SHARED-FIX-008 lone `\r`: a kept break in the changed code cannot be `\n` | Fix offered | same | adequate |
| `NoCodeFixWhenTheDeclarationHoldsALineBreakOtherThanTheDocumentsLineEnding` / `…AndTheBlockStaysEmpty` | A foreign break inside the rewritten declaration, on both paths | Fix offered | same | adequate. These are the audit's own shapes (FC-034). The other side (break outside the changed code keeps the fix, exact bytes) is pinned by the existing `LineBreakOtherThanTheDocumentsLineEndingOutsideTheChangedCodeKeepsTheFix` |

## Visible output is decided
Every expected value the repair adds is "no fix offered" for a document with mixed or lone-`\r` line endings. This is a rare-class refusal (`AGENTS.md`, *Code-fix coverage*: local and silent), demanded by SHARED-FIX-008's "moves or rewrites" as it stood at 0d587f2. The change doc records it under lead decisions 35 and 42, and 00-lead records it as decision 43. Decision 35 already named "the span they rewrite or move". The change doc's prose is narrower than that; see #6, which is text only. No `undecided output`. No rule page changed.

## Failure classes dismissed
- FC-001, FC-002, FC-016: the class of #2 and #4 is closed (*Guard scope*). I checked every member on the position axis: in front of, inside, and behind the declaration; among, and behind the last of, the moved statements; behind the enclosing `}`. I also checked it across the document-ending axis (CRLF with an LF, LF, CRLF, lone `\r`) and the path axis (statements, last statement, empty block with code behind it).
- FC-003: both sides of the new span boundary are measured (`m5`, `m6`, `m1` kept; `m3`, `m4`, `m7` refused) and tested (the existing outside-keeps-fix test).
- FC-004 to FC-015 and FC-018 to FC-030: the repair moves no trivia, string, directive, or layout decision. It only widens one refusal. The order of the checks changes nothing, because each branch returns `false`.
- FC-007, FC-008, FC-009, FC-043: registration and apply still both call `TryCreateChanges`, and Fix All is unchanged.
- FC-017, FC-033, FC-034: see *Assertion adequacy*.
- FC-031: #6 for the specs. The changed code summaries (`TryCreateChanges`, `TryShiftMovedLines`) and the inline comment describe the new code. No `TODO`, and no tracker numbers.
- FC-032, FC-035 to FC-042: no expected value comes from a stale run, no rename, no text-only claim, and no formatter phase or unsupported shape is touched.

## Architecture findings
_None._ The audit-26 architecture finding is closed. Every caller of `HoldsOtherLineBreak` now passes a span that contains all the changes it emits.

## Hints
- `m2`: in a lone-`\r` document, the empty-block path with a single-line declaration and code behind it is offered. It writes `\n` breaks into a `\r` document. This is exactly what SHARED-FIX-008 demands ("MUST be `\n`"; withheld only where the changed code holds a line break), and the formatter normalizes it. No action.
