Role: auditor
Commit: 4a00ace
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

This audit covers the repair delta against 685ab42, the head of the first audit (`07-auditor.md`). The head is 4a00ace and the base is `origin/main` b4343557df67bc95a2278c405950476716fb9abf. The checkout is 4a00ace. `git ls-remote origin refs/heads/main` returns the SHA from the prompt, and that SHA is an ancestor of the head. The working tree is clean. `git diff 685ab42 HEAD` (without `sdd/work/`) matches what handovers 08–18 describe:

- a shared `RawStringLiteralUtilities.GetLeadingWhitespaceLength` in Core, called twice by the analyzer and twice by the fix;
- the fix's private `GetLeadingWhitespaceCount` removed, and `IsEmptyTextLine` added (09) and removed again (16), so the fix calls `RawStringLineUtilities.MovesLine` exactly as it did at 685ab42;
- a summary-only change in `RawStringLineUtilities`;
- RH5202-004/005 amended (13, 14);
- new or retagged analyzer tests, two formatter characterization tests, and a Core `DataRow` test.

The last compiled change is 52e13fe. The cleaner's build and test results (381cbe7) came after it, and only `sdd/work/` changed after 381cbe7. Scope did not grow beyond RH5202, its Core helper, and their tests.

Evidence:
- `scripts/apply-fix.sh RH5202` over scratch fixtures, at the head and at `origin/main` (temporary worktree), under LF and CRLF.
- A scratch file-based app that calls `FixtureRunner.RunAsync` with a lone `\r` line ending, at the head and at `origin/main`.
- `scripts/format.sh` over a copy of the finding-#3 fixture.
- A one-line compiler probe of whitespace-only raw-string lines.

I deleted all fixtures and the worktree afterwards.

### Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | The analyzer and the fix now share one statement of the closing width (`GetLeadingWhitespaceLength`, `char.IsWhiteSpace`, the same result as `TrimStart()`). The leftward removal uses it too. Every failing cell of #1 is pinned by a `Verify` test: NBSP, U+000C, and U+000B first, rightward, plain; U+2003 first, rightward, `u8`; NBSP first, rightward, `$"""`; NBSP behind 20 spaces, leftward. The pre-existing NBSP-first leftward cell is pinned as well, with CRLF variants for NBSP rightward plain, NBSP rightward interpolated, and NBSP leftward behind spaces. |
| 2 | superseded | Decision 10 reverses decision 7. RH5202-005 now states that every empty line after the first stays empty, hole code included. The code keeps the 685ab42 behavior (`MovesLine`). Tests: `VerifyFixLeavesEmptyLineInsideHoleEmptyWhenShiftingRight` (and `…WithCrLf`), the nested `$"x{…}"` variant (and `…WithCrLf`), and `…WhenShiftingLeft`. Whether the output is decided is assessed under *Assertion adequacy*. |
| Architecture (three owners of the closing width) | closed for the analyzer and the fix | They now share one Core statement. The formatter's `RawStringLineShifts.GetLeadingSpaceCount` (spaces only) remains, as decision 6 settled, and is a follow-up candidate in `00-lead.md`. |

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 3 | high | `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider.cs:91` (`charactersToRemove = Math.Min(-delta, GetLeadingWhitespaceLength(line))`) against `sdd/specs/analyzer/RH5202.md` RH5202-004 ("they shift with the outer literal by the same amount", and its *Why*: "shifting its lines keeps its value") and SHARED-FIX-003 | FC-001, FC-006, FC-022, FC-027 | – | same requirement | **Defect.** When the outer literal shifts left by `d`, each line loses `min(d, its own leading whitespace)`. Consider a multi-line raw string nested in a hole whose closing line has `k < d` leading whitespace characters. That closing line loses only `k`, while its content lines lose `d`. The nested literal's stripped prefix therefore shrinks by `k` while its content shrinks by `d`, and its value changes. This is a literal value changed by the outer fix, which RH5202-004 says cannot happen and SHARED-FIX-003 forbids. **Counterexample**, inside `string M()`: `return $"""⏎` + 24 spaces + `Hello {"""⏎` + 34 spaces + `x⏎` + 4 spaces + `"""}⏎` + 24 spaces + `""";`. The opening quotes are in column 16, so `d = 8`. The outer fix writes `x` with 26 spaces and the nested closing quotes with 0, so the nested value goes from 30 spaces + `x` to 26 spaces + `x`. The nested literal's own fix then aligns it without restoring the lost 4 columns. `apply-fix` ends with `x` in column 49 and the closing quotes in column 23 (30 → 26). The same result appears under LF and CRLF, at the head and on `main`, so the defect is pre-existing; the run added the spec statement it breaks. Whole-document formatting of the same input keeps the value (`x` in column 53, closing quotes in 23, relative 30), so the formatter counterpart holds. **Class**, enumerated over nested raw kind × direction × nested closing whitespace `k` against the shift `d`. *Failing:* plain `"""` with `k = 4 < d = 8` (above); interpolated `$"""` nested with `k = 4` (`x{n}` relative 30 → 26); plain with `k = 0` (relative 34 → 26). *Holding:* plain with `k = d = 8` (relative 26 → 26, the boundary); every rightward shift (spaces are added uniformly); the outer literal's own content lines (the compiler requires them to start with the closing prefix, which is ≥ `d`); whitespace-only lines, whose value is `max(0, w − k)` (a compiler probe gave `[]` for a 2-space line and `....` for a 12-space line under an 8-space prefix), so the clamp leaves the value unchanged; hole code (no value). `u8` and `$$"""` nested literals go through the same per-line clamp. No test covers a leftward shift with `k < d`: `VerifyFixLeavesEmptyContentLineOfRawStringNestedInHoleEmptyWhenShiftingLeft` has `k = 29 ≥ d = 8`. | The fix's owner decides between two options. (a) Withhold the fix: do not register the action, under maintainer answer Q2 = A. The condition is a leftward shift for which a line of a multi-line raw string nested in a hole (its closing line, or any content line) has fewer leading whitespace characters than `-delta`. (b) Narrow RH5202-004's "same amount" claim and pick an output, which is an output question. Either way, add tests for `k < d` (plain, interpolated, `k = 0`), each with a CRLF variant, and for the boundary `k = d`. |
| 4 | low | `src/Reihitsu.Core.Test/RawStringLiteralUtilitiesTests.cs:25`, `:26`, `:27`, `:30` | FC-017 (reviewability) | – | PR-introduced | Rows 25, 26, 27, and 30 write U+00A0 and U+2003 as literal characters, so row 25 (`NBSP` + 3 spaces + `"""`, 4) displays identically to row 22 (4 spaces + `"""`, 4). A reader cannot see which character a row tests, and a later edit can turn the NBSP into a space with no visible diff. The analyzer tests avoid this on purpose (`¤` replaced at the call, `08-test-implementer.md`). | Write the non-ASCII whitespace as escapes (` `, ` `). |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Closing width (`GetLeadingWhitespaceLength`) | Fix: `sourceText.ToString(closingLine.Span)`, the `SourceText` line of the closing quotes | Analyzer `:104` (interpolated, after `endText.LastIndexOf('\n')`) and `:133` (plain/`u8`, after `Split(["\r\n", "\n"])`), same helper | Same width rule. The same closing line under LF and CRLF. Under a lone `\r` (or U+2028/U+2029/U+0085) line break the two extract different lines | agree for LF/CRLF; the lone-`\r` divergence is pre-existing (*Hints*) |
| `delta == 0`: unchanged document | Result of the width above | Analyzer report predicate `openingQuoteColumn != closingQuoteColumn` | Equivalent under LF/CRLF for every whitespace kind (the #1 cells) | agree |
| Leftward removal amount | Leading `char.IsWhiteSpace` characters of each moved line, capped at `-delta` | The nested raw literal's own stripped prefix (its closing line); RH5202-004 "same amount" | Disagree when a nested raw literal's closing line has fewer than `-delta` whitespace characters | disagree: finding #3 |
| Removed characters are trivia or prefix | `char.IsWhiteSpace` at a line start | C# whitespace (tab, VT, FF, Zs). Inside a line, `char.IsWhiteSpace` holds only for those, because CR, LF, U+0085, U+2028, and U+2029 end a `SourceText` line | Subset | agree |
| Which lines move (`MovesLine`) | `line.Span.Length > 0` and `StartsInsideNestedText` at the line start | `RawStringLineShifts.CollectEdits` `:236` (same call); RH5202-004/005 as amended | Same method and span; the spec now covers every empty line | agree |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Width of the whitespace in front of the closing quotes | `RawStringLiteralUtilities.GetLeadingWhitespaceLength` (Core `:35`), called from the RH5202 analyzer `:104`, `:133` and the RH5202 fix `:66`, `:91`. `RawStringLineShifts.GetLeadingSpaceCount` (formatter `:126`, spaces only after tab replacement) | Two owners. The difference is the recorded follow-up of decision 6 and is not PR-introduced. pass |
| Which lines of a raw string the move writes | `RawStringLineUtilities.MovesLine` (`:27`), called from the fix `:80` and `RawStringLineShifts.cs:236`; `StartsInsideNestedText` also from `RawStringLineShifts.cs:75`. `rg IsEmptyTextLine\|GetLeadingWhitespaceCount` finds nothing | one owner, pass |
| Which line is the closing line | Analyzer: `Split(["\r\n", "\n"])` / `LastIndexOf('\n')` on token text. Fix: `SourceText.Lines` | Two statements that diverge only for lone `\r` and Unicode line breaks. The defect is pre-existing (*Hints*) |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| 10 `VerifyFixShifts…ClosingWhitespace…` tests (NBSP/FF/VT/EmSpace, plain/`u8`/`$"""`, right and left, 3 CRLF variants) | One application puts the closing quotes in the opening column, for every whitespace kind | A shift by the spaces-only width, or a remaining diagnostic | `Verify` (exact text, iterative single fix, Fix All, re-analysis) | adequate |
| `VerifyFixLeavesEmptyLineInsideHoleEmptyWhenShiftingRight` (+CRLF), `…OfNestedInterpolatedString…` (+CRLF), `…WhenShiftingLeft` | An empty hole-code line stays empty, also one level deeper | Spaces written onto the line | `Verify` | adequate |
| 4 `VerifyFixLeavesEmptyContentLineOfRawStringNestedInHoleEmpty…` | An empty nested raw content line stays empty, and the inner literal stays reported | Spaces on the line, or a lost inner diagnostic | `Verify` + `ApplyCodeFixAsync` exact text + `Verify` (watched red under a mutation, `10-test-implementer.md`) | adequate; the leftward pair has `k ≥ d` only (finding #3) |
| `VerifyFixShiftsWhitespaceOnlyLineInsideHole`, `VerifyFixShiftsWhitespaceOnlyContentLine` | A whitespace-only line shifts (`main` behavior, decision 11) | An unshifted line | `Verify` | adequate |
| `RawStringInterpolationHoleTests.EmptyLineInsideHoleOfMisalignedLiteralIsRemoved`, `…LambdaInsideHole…StaysEmpty` | Formatter output is unchanged (characterization) | Any byte difference | `AssertRuleResult` (LF, CRLF, second pass, phase stability) | adequate. Untagged, which `sdd/README.md` allows. The outputs are those of FMT-BLANK-007 (blank line before the hole's `}` removed) and FMT-RAW-002/007 |
| `RawStringLiteralUtilitiesTests.GetLeadingWhitespaceLength…` (11 rows) | Every leading `char.IsWhiteSpace` character counts as one | A wrong length | `DataRow`, exact | adequate; readability is finding #4 |
| Missing: leftward outer shift with a nested raw literal whose closing whitespace is shorter than the shift | Nested value kept (RH5202-004) | Value change | — | missing: finding #3 |

**Visible output (check 10).** These expected values changed against `main`:

- **#1 cells.** RH5202-003 demands them (closing quotes in the opening column). The wording of that requirement is unchanged on this point.
- **Empty hole-code line stays empty** (`main` wrote 13 spaces). SHARED-FIX-006 and SHARED-FIX-007 together decide it, and neither is changed. Formatting reformats any whitespace written on an empty line in every member: the lambda member keeps the line empty, and the member before `}` removes it. SHARED-FIX-006 keeps the fix from changing hole-code layout that the diagnostic is not about, which leaves the empty line unchanged. The first audit reported this case as undecided. I now judge it decided, by the combination above, not by SHARED-FIX-007 alone.
- **Empty content line of a nested raw string stays empty.** SHARED-FIX-007 with FMT-RAW-002 decides it.

The NBSP-first leftward cell was `no-progress` on `main` and now converges, as RH5202-003 demands. Nothing else visible changed.

## Failure classes dismissed

- FC-002: the RH5202-005 qualifiers (empty versus whitespace-only, every kind of line) are explicit conditions (`line.Span.Length > 0`) and are tested on both sides.
- FC-003: tested on both sides: rightward and leftward for the closing width; NBSP behind spaces and NBSP first; empty versus whitespace-only lines. The missing `k < d` / `k = d` boundary belongs to finding #3.
- FC-004, FC-005, FC-019, FC-020, FC-021: the repair changes only how much leading whitespace is measured or removed. Lines inside comments are excluded by `StartsInsideNestedText`, and a directive in a hole is a parse error.
- FC-007, FC-009: the #1 cells converge in one application. The lone-`\r` cells are pre-existing (*Hints*).
- FC-008: Fix All is covered by `Verify` in every new test. The repair adds no overlapping edits.
- FC-010, FC-011, FC-012: the formatter has no code change in the delta (summary only).
- FC-013: the fix's output and the formatter's differ for an empty hole-code line by design: RH5202-005 cites SHARED-FIX-006, and the formatter removes the line.
- FC-014: no new rule.
- FC-015, FC-016: see *Policy ownership*. Every sibling cell of #1 is tested.
- FC-017: covered in the table above. The helpers assert exact text.
- FC-018, FC-023, FC-024, FC-025, FC-026, FC-029, FC-036: no rename, no identifiers, no member kinds, no token re-resolution, no key/value layout.
- FC-028: the fix is text-level, with no detached or document-scoped formatting.
- FC-030: edits are confined to line starts inside the literal's span.
- FC-031: the summaries of `ApplyCodeFixAsync` ("up to that many leading whitespace characters are removed"), `GetLeadingWhitespaceLength`, and the `RawStringLineUtilities` class describe the current code. The spec's "same amount" claim is finding #3.
- FC-032: the cleaner's build and test results (381cbe7) came after the last compiled change (52e13fe).
- FC-033: CRLF variants exist for the new failing cells.
- FC-034, FC-035: the tests reproduce the first audit's own inputs, and the expected values come from RH5202-003/005, not from code output.
- FC-037: no text-only claim.

## Architecture findings

_None._ Finding #3 has one owner (the fix's per-line removal). The formatter does not share that decision, because it aligns a nested raw literal itself (FMT-RAW-008).

## Hints

- **Pre-existing, follow-up candidate: lone `\r` line breaks (also U+2028, U+2029, and U+0085 inside a raw literal).** In a document whose line breaks are all a lone `\r`, which SHARED-FIX-008 names as a supported input, the RH5202 analyzer reports aligned multi-line raw strings, plain and interpolated alike. For plain and `u8` literals it splits the token text only on `\r\n`/`\n`, so it measures width 0. For interpolated literals it searches only for `\n`, so the leading `\r` counts as one more whitespace character. The fix is then offered and changes nothing. On `main`, all four cells (plain or interpolated, aligned or misaligned) end `no-progress`. At the head, the misaligned cells are aligned in one step (the fix reads `SourceText.Lines`), but the analyzer keeps reporting, so the run still ends `no-progress`. The structural cause is that the closing line is extracted in two ways, by the analyzer from the token text and by the fix from `SourceText`. Decision 6 shared the width but not the line.
- Decision 11's follow-up candidate (whitespace-only hole-code line) stands. RH5202-005 now leaves an empty hole-code line empty, while a whitespace-only one still gets 13 more spaces.
