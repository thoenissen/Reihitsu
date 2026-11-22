Role: spec author
Commit: af00142
Verdict: spec written

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's decision rows 11 and 12, which this round carries out.

## Changed requirements

- RH5419-003 — The fix puts the indexer on one line — changed: the in-place formatting joins a line break behind each modifier (FMT-BREAK-041); new example `public⏎virtual int this[int a,⏎int b] => a;` → `public virtual int this[int a, int b] => a;`. `Uses` gains FMT-BREAK-041 and FMT-UNSUP-017.
- RH5419-004 — The fix is withheld where the indexer cannot be put on one line — changed (decision row 11): a line break between a modifier and the type no longer withholds the fix; the kept breaks in front of `this` are now only those between the type, an explicit interface name, and `this`. A comment, directive, or disabled text behind a modifier, also on one line (`public /* c */ int this[`), withholds the fix because the formatter refuses the indexer (FMT-UNSUP-017, SHARED-FIX-010); new negative example `public /* note */ int this[nint a,⏎nint b] => 1;`.
- RH5401-003, RH5401-004 — changed the same way for properties (see Internal choices): modifier line break joined by the in-place formatting; a comment behind a modifier withholds the fix; new negative example `public /* note */ int Commented⏎=> 1;`. `Uses` gains FMT-BREAK-041 and FMT-UNSUP-017.
- FMT-UNSUP-017 — clarified (decision row 12): the modifier row names a function-pointer parameter, example `delegate*<ref // c⏎int, void>`; the overlap paragraph states that a comment behind a function-pointer parameter's modifier inside the type of a field or a local gives FMT-UNSUP-004's reason.
- FMT-BREAK-041 — clarified: the gaps behind the modifiers of a function-pointer parameter, like a parameter's, are joined by FMT-BREAK-015, not by this requirement.
- FMT-BREAK-015 — clarified: a function-pointer parameter list is one of its lists (existing behavior: `ListElementInteriorUtilities` already treats `FunctionPointerParameterSyntax` as an element, and `ListElementInteriorJoinTests.FunctionPointerParameterWrappedAfterRefModifierIsJoined` pins it).
- Change doc `sdd/changes/2026-11/continuation-join.md`: affected requirements gain FMT-BREAK-015, RH5401-003/004, RH5419-003/004; section 3 lists the refused function-pointer modifier comment (with the CLI line, location 3,19); section 4 gains RH5419 and RH5401 bullets with before/after; Decisions gain the function-pointer sentence and "RH5419's and RH5401's fixes follow the modifier join".

## Examples for the tests

Tests whose expected values change:

- `RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedAnalyzerTests.VerifyFixIsWithheldForALineBreakBetweenAModifierAndTheType` (LF, CRLF): withheld → fix offered; expected `public int this[int a, int b] => a;` (RH5419-003, RH5419-004). Its summary ("which the formatter keeps") must change.
- `ParameterContinuationLineAlignmentTests.FunctionPointerParameterTypeAfterModifierAndLineCommentStaysAligned`: formatted unchanged → refused under FMT-UNSUP-017 with reason `a comment or directive behind a modifier` (the input is a property's type, so FMT-UNSUP-004 does not apply); rename and tag FMT-UNSUP-017.

Tests whose expected value stays but whose stated reason changes:

- `RH5419…AnalyzerTests.VerifyFixIsWithheldForALineCommentBehindAModifier`: still no fix, now because the formatter refuses the input (FMT-UNSUP-017), not because the line break is kept; its summary should say so.

New tests the examples seed:

- RH5419-003 modifier-join example; RH5419-004 `public /* note */ int this[nint a,⏎nint b] => 1;` (no fix, on one line).
- RH5401: `public⏎int Q⏎=> 2;` → `public int Q => 2;`; `public /* note */ int Commented⏎=> 1;` → no fix (both reported). No RH5401 test pins either case today.
- FMT-UNSUP-017 modifier row: `delegate*<ref // c⏎int, void>` and `delegate*<ref /* c */ int, void>` in a property type and in a method parameter (this requirement's reason), and in a field or local type (FMT-UNSUP-004's reason); the other side `delegate*<ref int, void>` joined from `ref⏎int` (already pinned).

Probes on the current head (`main`'s code): `scripts/apply-fix.sh RH5419` — `public⏎virtual int this[…` no fix; `public /* c */ int this[short a,⏎short b] => a;` fixed to one line. `scripts/apply-fix.sh RH5401` — `public /* c */ int P⏎=> 1;` fixed; `public⏎int Q⏎=> 2;` no fix. `scripts/format.sh` leaves the change doc's function-pointer input unchanged apart from the final newline. `scripts/test.sh --project architecture`: 198 passed.

## Output questions

_None._ Rows 11 and 12 are the lead's decisions under the standing instruction; the RH5401 extension follows them.

## Internal choices

- Extended decision row 11 to RH5401 (properties): its fix is withheld on `main` for a modifier line break and offered for a comment behind a modifier, exactly as RH5419's; FMT-BREAK-041 and FMT-UNSUP-017 flip both, so the change doc has to list it as a visible change either way. Alternative: leave RH5401's spec and change doc untouched, which would leave a visible code-fix change unlisted. The lead should record it next to row 11 and in the PR Review notes.
- Stated the function-pointer parameter in FMT-BREAK-015's list instead of only in FMT-BREAK-041, because FMT-BREAK-015 owns the join and the code already does it. Alternative: a sentence in FMT-BREAK-041 only.

## Follow-up candidates

_None._
