Role: auditor
Commit: 0d587f2d75a4e0b6b1e6d5781a9e7918dd1b3803
Verdict: #969: pass; #1335: findings; #1263: pass; #1151: pass; #1604: findings

My prompt named the run, the base, the earlier audit, and the handovers to read. It carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Head `0d587f2d75a4e0b6b1e6d5781a9e7918dd1b3803` (checked out, clean). Base `origin/main` = `4bdcd13186c2882e63985526458f63c4b359236d` (`git ls-remote` agrees), an ancestor of the head. This is a repair-delta audit of `2ee125d..0d587f2` against `18-auditor.md`. Production changes are in `SyntaxNodeUtilities`, `SyntaxIndentationUtilities`, `LineEndingUtilities`, `OrderingMoveSafety`, `AccessorOrderingUtilities`, and the RH3004 and RH7004 fixes. Test changes are in RH3004, RH7004, RH7107, RH7108, and RH7109. Spec changes are in SHARED-FIX-007/008, SHARED-ORDER-009/010, RH7004-007, RH3004-008, and the change doc. The diff matches handovers 19 to 24. The scope did not grow: every file the repair touches belongs to a component the run already changed, except `AccessorOrderingUtilities` and `LineEndingUtilities`, which are the counterparts that findings #2 and #3 name.

How I checked: static tracing of every changed guard and its counterparts. I also re-ran the scratch harness from audit 18 (one analyzer, its first code action, the output byte for byte, line endings not normalized) against the head, on every earlier counterexample and on new fixtures. Every counterexample below is a measured output.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | Every listed member is now refused: `interp`, `f2`, `f3c`, `f4` (RH7004), `f5b` (RH7109), and the right-shifted accessor (new test). The members that held still hold: `f3`, `f3b`, left shift, shift 0. One node-based owner, `SyntaxNodeUtilities.GetStringsAndCommentsSpanningLines`, is now used by `TryShiftMovedLines` (RH7004 and every member and accessor move) and by RH3004. RH3004's private `WithholdsFixForSpanningStringOrComment`/`IsString` are deleted. I traced parity with the deleted copy: same string kinds, and comments within `Span` (now *overlaps*, with no difference outside the token span). |
| 2 | open | Most listed members are closed: `f8`, `f9` (RH7109), `f10`, `f12` (RH7004, refused because of the moved statements), `f13` (RH7107), and `f14` (RH3004, hint 18). The listed RH7004 member "writes `using (…)⏎`" still fails: the line break behind the using declaration is kept as written, see #4, inputs `n1`, `n6`, `n5`. |
| 3 | closed | `f6`, `f11`, and `f7` (decision 40) are refused. A comment inside an accessor, or behind `}`, still swaps (tests). |
| Arch 18-1 (two owners of "a spanning string withholds") | closed | One owner, see #1. |
| Arch 18-2 (private line-ending choice) | closed for the written ending | `RH7004…GetEndOfLine` is deleted, and every created break is `LineEndingUtilities.DetectEndOfLine(root)`. Which span the refusal checks is a new architecture note (below). |
| Hint 18: RH3004 mixed endings | closed | `f14` refused (`WithholdsFixForLines` → `HoldsOtherLineBreak` over `block.Span`). |
| Hint 18: RH3004 fallback | closed | Now returns `originalDocument`, with a comment that says why. |
| Hint 18: RH7004 tab (`f15`) | closed | Refused (decision 37). |

## thoenissen/Reihitsu#969

### Findings
_None._ The member-move refusal span `FromBounds(target.FullSpan.Start, moved.FullSpan.End)` is the span of the `TextChange`, so the SHARED-FIX-008 refusal covers exactly the replaced code (`f8`, `f9` refused; a foreign line break in front of the target or behind the type is kept, which the tests show). The interpolated-string refusal holds (see #1).

## thoenissen/Reihitsu#1335

### Findings
#2 (open, RH7004), #4 (RH7004), and #5 (low, text). RH7107 and RH7108 are clean: the one-line swap refusal `HoldsCommentOutsideEveryAccessor` inspects `OpenBrace.Span.End`…`CloseBrace.SpanStart` and excludes every `accessor.Span`, which is exactly SHARED-ORDER-009's "in front of the first, between two, behind the last". The multi-line accessor move goes through the member-move check (`f13` refused).

## thoenissen/Reihitsu#1263

### Findings
_None._ The repair does not touch RH7303/RH7310 or `RegionDirectiveBlankLineUtilities`. Audit 18 still holds.

## thoenissen/Reihitsu#1151

### Findings
_None._ The repair does not touch RH7302. Audit 18 still holds.

## thoenissen/Reihitsu#1604

### Findings
#2 (open) and #4, through RH7004. RH3004's block-body path is clean: the line breaks of the header that the formatter lays out are normalized (`r1`: `delegate (int x)⏎{` with an LF in a CRLF document gives CRLF). The block is checked by `HoldsOtherLineBreak(block.Span)`. The tab check moved to the shared `HoldsTabInFrontOfMovingLine` over the same lines as the deleted private copy (lines after `{` through the `}` line, a line that starts with `}` excluded, blank lines excluded). The only extra exclusion is lines that `KeepsItsColumn`, and RH3004-009 already withholds the directive lines among them.

## Whole run

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 2 | high (open) | `src/Reihitsu.Analyzer.CodeFixes/Rules/Organization/RH7004UsingDeclarationsShouldNotBeUsedCodeFixProvider.cs` `TryCreateChanges`, `HoldsOtherLineBreak(text, movedSpan, endOfLine)` | FC-001, FC-016 | – | PR-introduced (SHARED-FIX-008) | The listed member "`using (…)⏎` keeps a foreign line break" still fails. `movedSpan` starts at `usingDeclaration.FullSpan.End`, so the refusal never sees the line break behind the declaration. The fix rewrites the declaration (`(` inserted, `;` → `)`), so by SHARED-FIX-008 that break is "code it changes … its trailing trivia included". Measured: **CRLF document, LF only behind the declaration** (`n1`): the result has `using (var s = new MemoryStream())\n` followed by `{\r\n`, `s.Flush();\r\n` …, so the fix is offered and keeps the LF. **The declaration as the last statement, LF behind it** (`n6`): `using (…)\n` then `{\r\n }\r\n`. **All lone `\r`, declaration as the last statement** (`n5`): `using (…)\r` followed by `{\n`, `}\n`. The fix mixes `\r` and `\n` in a lone-`\r` document, which SHARED-FIX-008 forbids ("MUST be `\n`"; "withheld wherever the code it moves holds a line break"). **Holds:** a foreign line break among the moved statements (`f12`, `f10`, tests), and a foreign line break outside the changed code (`using System.IO;⏎`, test). No test puts the foreign line break behind the declaration. Handover 20's reason for leaving it out ("the declaration's own line break is inside the rewritten span") does not match the code. | Refuse the class: run `HoldsOtherLineBreak` over the code the fix changes, `FromBounds(usingDeclaration.SpanStart, movedSpan.End)`, on both paths (this also covers #4). Add tests: an LF behind the declaration in a CRLF document, with statements following it and with the declaration as the last statement, and the last-statement case in a lone-`\r` document. |
| 4 | high | Same location; the empty-block path (`movedSpan` not at a line start) has no line-ending check at all | FC-001, FC-016 | #2 | PR-introduced (SHARED-FIX-008) | A member #2 did not name: a line break **inside** a using declaration that spans lines. The fix rewrites the declaration and keeps the line break as written. Measured: CRLF document with `using var s =\n            new MemoryStream();` and a statement behind it (`n2`): the result has `using (var s =\n` and the fix is offered. The same declaration on the empty-block path, `… new MemoryStream(); }` (`n3`): the result has `using (var s =\n`, then `{\r\n } }`. **Holds:** a single-line declaration on the empty-block path (no line break in the rewritten code). This is the same decision as #2, failing between the same components (which line breaks RH7004's refusal covers vs which code it changes), so it is a repeat. | Same refusal as #2. Its span starts at the declaration, so it covers the empty-block path too. Add a test for a declaration that spans lines with a foreign line break inside it, on both paths. |
| 5 | low | `src/Reihitsu.Core/SyntaxIndentationUtilities.cs` `TryShiftMovedLines` summary | FC-031 | – | PR-introduced | The summary still says "A blank line, which holds only whitespace outside a token, a comment, and disabled text, is written empty". The code now decides on the string *node*. A whitespace-only line inside a multi-line interpolation hole is outside every token, yet it is copied as written (measured `h1`, RH7109 at shift 0: `$"{nameof(C)⏎        ⏎        + 1}"` keeps `        ` on the middle line; at 2ee125d it was written empty). The behavior is harmless (the hole's whitespace is not part of the value) and is rare. Only the summary is stale. | Say "outside a string, a comment, and disabled text" (`GetStringsAndCommentsSpanningLines`). This is text only. |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A line inside a string or comment that spans lines cannot shift | `GetStringsAndCommentsSpanningLines(root, span)`: string literal nodes (all kinds), `InterpolatedStringExpressionSyntax`, `/* */` and `/** */` trivia, overlapping the shifted span and not on one line; a line counts when it starts strictly inside one | RH7004-007, SHARED-ORDER-010, RH3004-008, SHARED-FIX-007 ("also where it starts at a hole or at the closing quote") | Yes. Every multi-line token the old token test caught (verbatim, raw, interpolated text) lies inside one of these nodes. Directive strings are single-line | ok (#1 closed) |
| RH3004 spanning strings and comments | Same helper over `anon.Span`; `keepsBlockBody` or `ContainsLineStartingWithHash` | Deleted private `WithholdsFixForSpanningStringOrComment` | Same set (contains vs overlaps differs only outside `Span`) | ok |
| Foreign line break, member/accessor whole-line move | `HoldsOtherLineBreak(FromBounds(target.FullSpan.Start, moved.FullSpan.End))` | The `TextChange` span of the same method | Identical | ok |
| Foreign line break, RH7004 | `HoldsOtherLineBreak(movedSpan)`, only on the moved-statements path | SHARED-FIX-008: the code the fix changes, the rewritten declaration and its trailing trivia included; the fix's `TextChange`s start at `Declaration.SpanStart` | No: the declaration and its line break are left out, and the empty-block path is not checked | #2, #4 |
| Foreign line break, RH3004 block body | `HoldsOtherLineBreak(block.Span)` | Lines that `TryShiftMovedLines` copies (laterLine … contentEnd ⊂ block); header laid out by the formatter | Yes (`r1`) | ok |
| One-line swap with a comment | Trivia between `{`.End and `}`.Start, minus every `accessor.Span` | SHARED-ORDER-009 (in front of the first, between two, behind the last; inside an accessor travels) | Yes; comment behind `}` excluded, comment in an accessor's leading trivia refused (decision 40) | ok |
| Tab in front of a moving line | Lines that start in the span, hold text before the span end, and do not `KeepsItsColumn` | RH3004-006, RH7004-007; deleted RH3004 private copy | Yes (traced the `}`-line and blank-line edges) | ok |
| RH7004 declaration-line tab | Leading `' '`/`'\t'` of the declaration line, both paths | RH7004-007 "a tab in front of the declaration's line" | Yes (decision 41) | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| A string or comment that spans lines withholds a moving fix | `SyntaxNodeUtilities.GetStringsAndCommentsSpanningLines` (used by `SyntaxIndentationUtilities.TryShiftMovedLines` and `RH3004…WithholdsFixForLines`); `IsString` and `WithholdsFixForSpanningStringOrComment` are gone (`rg -n "IsString\b" src` finds nothing in fixes or Core). The other hits (`FormattingTextAnalysisUtilities`, `TokenSeparationUtilities`, `StringInterpolationUtilities`) answer other questions | one owner, ok |
| Line ending a moving fix writes | `LineEndingUtilities.DetectEndOfLine` in `OrderingMoveSafety`, RH7004, RH3004; no private copy left (`rg -n "GetEndOfLine\(" src`) | ok |
| Foreign line break withholds a moving fix | `LineEndingUtilities.HoldsOtherLineBreak`, with the span chosen by each of three callers | one predicate, but each caller picks its own span; RH7004's span disagrees with its changes (#2, #4) |
| Tab in front of a moving line | `SyntaxIndentationUtilities.HoldsTabInFrontOfMovingLine` (RH3004, RH7004); RH7004's extra declaration-line check covers a line that is not moved | ok |
| One-line accessor swap | `AccessorOrderingUtilities.TryCreateMove` → `HoldsCommentOutsideEveryAccessor` → `OrderingMoveSafety.TryCreateSwapWithinLine` (only caller) | ok |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH7004 `NoCodeFixForAnInterpolated…` (hole, closing quote, `@$"`, hole spanning lines), RH7109/RH7107 right-shift interpolated | RH7004-007 / SHARED-ORDER-010 refusal | Fix offered | `VerifyNoCodeFixIsOffered` | adequate; these are the audit's own shapes (FC-034) |
| RH7004 `NoCodeFixForAnInterpolatedRawString…`, RH7109 left-shift, `FieldIndentedAsItsSlotMoves…` (LF/CRLF) | Other side of each condition | Refusal where none is due, or a byte difference | exact | adequate (FC-003) |
| RH7004/RH7109/RH7107/RH3004 mixed and lone-`\r` refusals | SHARED-FIX-008 refusal for foreign line breaks in the moved code | Fix offered | `VerifyNoCodeFixIsOffered` | adequate for the moved code; no RH7004 test puts the foreign line break behind or inside the declaration (#2, #4) |
| `LineBreakOtherThanTheDocumentsLineEndingOutsideTheChangedCodeKeepsTheFix` (RH7004, RH7109, RH3004) | A foreign line break outside the changed span is kept, the rest uses the detected ending | Any byte difference | `ApplyCodeFixAsync` + `Assert.AreEqual` (no normalization) | strong |
| RH7107/RH7108 comment refusals (in front of, between, behind) and `CommentInside…Travels…`, `CommentBehindTheClosingBrace…` | SHARED-ORDER-009 boundary of "in the list" | Comment handed to another accessor; refusal of an inside comment | `VerifyNoCodeFixIsOffered` / exact LF+CRLF | adequate |
| RH7107 `AccessorsOnOneLineSwapInADocumentWhoseLineBreaksAreLoneCarriageReturns` | Swap moves no line break; every `\r` outside it kept | Any byte difference | `ApplyCodeFixAsync` exact | strong |
| RH7004 tab tests (declaration line, moving line, between tokens, in a string, on a blank line, in front of a directive) | RH7004-007 tab refusal and its other side | Refusal of a harmless tab; tab copied onto a new line | exact / no-fix | adequate; empty-block arm of the declaration-line tab not pinned (decision 41 accepted it) |
| RH3004 `FixWithheldFor…InterpolatedString…`, mixed and lone-`\r` | RH3004-008, SHARED-FIX-008 after the move to Core | Fix offered | `VerifyNoCodeFixIsOffered` | adequate |

### Visible output is decided
Every expected value the repair adds or changes is either "no fix offered" or the other side of a refusal, and each is covered. The refusals are rare-class refusals recorded as lead decisions 34–37 and 40–42 in the change doc's *Decisions* and its "Rare-class refusals of repair round 2" section (`AGENTS.md`, *Code-fix coverage*: local and silent). The other sides are demanded by requirements the repair does not change: SHARED-FIX-007's token spacing and directive-line column (tab kept between tokens and in front of a directive line), decision 27 (a blank line written empty), SHARED-FIX-008 first paragraph (a line break outside the changed span is kept), and Q7 B (a swap with a comment behind `}` or inside an accessor). No rule page changed. No `undecided output`.

### Failure classes dismissed
- FC-002, FC-003: the new qualifiers ("outside every accessor", "in front of a moving line", "spans lines", "other than the detected") are explicit conditions, and both sides are tested, except for #2 and #4.
- FC-004, FC-019, FC-021: comments in the one-line swap are now refused or travel inside the accessor (#3 closed). The whole-line move is unchanged apart from the line-ending refusal.
- FC-005, FC-020: the directive handling is unchanged. A one-line list cannot hold a directive. A `\t#pragma` line among the RH7004 statements keeps its column and its tab (test).
- FC-006, FC-022: string values are now protected by the node-based check, which I traced for every `$` form, verbatim, raw, and UTF-8. Interpolated holes that span lines are refused at any non-zero shift.
- FC-007, FC-008, FC-009, FC-043: register and apply still call the same `TryCreate*`, and Fix All is unchanged. The refusals only remove offers.
- FC-010 to FC-014, FC-028, FC-038 to FC-042: no formatter phase or unsupported shape changed. RH3004's formatter use is unchanged.
- FC-015: one owner per policy (see *Policy ownership*).
- FC-016: covered by #2 and #4. I checked every sibling caller of `HoldsOtherLineBreak`.
- FC-017, FC-033: the mixed-ending tests assert exact bytes through `ApplyCodeFixAsync`, which does not normalize.
- FC-018, FC-023, FC-036: no rename.
- FC-024 to FC-027, FC-029: no construct-specific rewrite is added. The nested strings and lambdas inside holes are covered by the whole-node decision.
- FC-031: #5. The other changed summaries (`OrderingMoveSafety.TryCreateWholeLineMove`, `AccessorOrderingUtilities.TryCreateMove`, `RH7004…TryCreateChanges`, `RH3004…WithholdsFixForLines`, `StartsInsideContent`, `HoldsOtherLineBreak`, `HoldsTabInFrontOfMovingLine`, `GetStringsAndCommentsSpanningLines`) describe the new code. No `TODO`, and no tracker numbers.
- FC-032, FC-035: the expected values come from the spec examples of 0669c17 (handover 20). I took my measurements fresh on the head.
- FC-034: the refusal tests use the audit's own shapes.
- FC-037: no text-only claim.

### Architecture findings
- **The span a line-ending refusal checks is chosen apart from the changes a fix emits** (#2, #4; a repeat of #2's class). `LineEndingUtilities.HoldsOtherLineBreak` is one predicate, but each caller passes its own span. `OrderingMoveSafety.TryCreateWholeLineMove` passes the span of its `TextChange`, so it agrees. RH7004 passes `movedSpan`, while its `TextChange`s start at `usingDeclaration.Declaration.SpanStart`, so the rewritten declaration and the line break behind it are never checked (`n1`, `n2`, `n3`, `n5`, `n6`). Under `README.md`, *Architecture findings* ("a repeated class"), the remedy is to refuse the class. Deriving RH7004's checked span from the changes it emits (from the declaration's start to `movedSpan.End`) is the cheap check that does that.

### Hints
- SHARED-FIX-007's blank-line definition says "outside a string literal, a comment, and disabled text", while the moved-code bullet now treats a line in an interpolation hole as inside the string. These are consistent with the code (#5), but "string literal" there could read "string" to match. Text only; no output depends on it beyond `h1`.
- `TryCreateWholeLineMove` now calls `DetectEndOfLine(root)`, which walks the whole document, once per registration of each ordering diagnostic. That is cheaper than the formatter call it replaced. No action.
