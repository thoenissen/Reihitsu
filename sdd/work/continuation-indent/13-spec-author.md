Role: spec author
Commit: 8c64649
Verdict: spec written

Amendment round for the lead's decision row 25 (`00-lead.md`), from `12-test-implementer.md`, *Spec findings* 1 and 2. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond that row.

## Changed requirements

- FMT-BLANK-018 — example corrected — clarified. The output of "Above a comment glued to an array initializer's brace…" now shows `/* b */ { 1, 2 };` in column 8, the continuation column FMT-INDENT-034 gives it; the blank-line decision it illustrates is unchanged. Checked with `scripts/format.sh` on the example's input: the head's output equals the corrected block. The example above it ("is left unchanged", the `Child =` / `/* c */ {` initializer) was run as well and is unchanged on the head, so it needs no edit.
- RH2001-009 — "The fix is withheld for a comment or directive behind a modifier, or a conditional directive in front of the type" (title changed) — changed:
  - The second paragraph no longer offers the fix for a directive between modifiers that keep their order. Any directive or disabled text between two modifiers or between the last modifier and the type withholds the fix, whatever its kind, whether or not the fix reorders the modifiers, and whether or not it adds `readonly`: the field holds the directive behind a modifier, which FMT-UNSUP-017 refuses (SHARED-FIX-010; maintainer answers 1d A and 3 A).
  - The third paragraph, on conditional directives, is narrowed to what the second does not already cover: a conditional directive in front of the type of a property without a modifier, also on the lines above it. Its RH2001-010 pointer is unchanged.
  - *Why* is rewritten to match: comments use FMT-UNSUP-004, directives FMT-UNSUP-017, and SHARED-FIX-003 is a second reason only for the inputs the fix reorders. The SHARED-FIX-009 build-dependency argument is kept for the case without a modifier.
  - The "directive that a reordered modifier would cross" example is replaced by three properties: `First`, crossed by reordering; `Second`, which keeps its order (the test implementer's input); and `Third`, a directive between the last modifier and the type with no `readonly` added. All three are reported without a fix. The sentence under the conditional-directive example now gives the directive behind a modifier as the reason for all five.
- RH2001-004 (cross-reference sentence only) — its pointer now reads "for a comment or directive between two modifiers or between the last modifier and the type". The requirement itself is unchanged.
- Change doc `sdd/changes/2026-11/continuation-join.md`: RH2001-009 is added to *Affected requirements*, and section 4 (*Code fixes*), RH2001 bullet, gains the visible change with input, `main`'s result, and the new result. On `origin/main`, checked in a temporary worktree with `scripts/apply-fix.sh RH2001` under LF and CRLF, the fix gives `private` / `#pragma …` / `static readonly int _value;`, and with a directive between the last modifier and the type it gives `int _value;`. On the head, both are `no-fix-offered`, and a property without a directive is still fixed.

No rule page changes: `documentation/rules/RH2001.md` does not mention directives among the modifiers. A newly withheld fix is a corner case that `general-notes.md` covers.

## Examples for the tests

- RH2001-009: the new example's `Third` (`private static` / `#pragma warning disable CS0169` / `int Third { get; set; }`, with the directive between the last modifier and the type, no reorder, and no `readonly`) has no test in `RH2001PrivateAutoPropertiesShouldNotBeUsedTriviaTests`. `First` and `Second` are covered at lines 1167 and 1262. The test implementer may add `Third` as a `VerifyNoCodeFixIsOffered` case, LF and CRLF. This is a gap in test coverage, not a lane issue.
- FMT-BLANK-018: no new test. `GluedCommentBlankLineTests.DecidesBlankLinesAboveGluedCommentAsDirectlyBeforeItsToken` already asserts the corrected column.

## Output questions

_None._ Both edits write down outputs the maintainer already decided: FMT-INDENT-034, and answers 1d A and 3 A for FMT-UNSUP-017.

## Internal choices

- RH2001-009 keeps a separate paragraph for a conditional directive in front of the type of a property without a modifier. FMT-UNSUP-017 does not cover that position in a build that excludes the branch, so the fix's own check is still needed there. The alternative was to fold all conditional-directive wording into the general directive paragraph, which would leave the case without a modifier unstated.
- The requirement keeps its ID and is changed in place rather than retired, because it still owns the same inputs: what the fix does for comments and directives among the modifiers.

## Follow-up candidates

- These are code comments for the cleaner, not spec issues. The XML documentation at `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider` (around line 427, "a comment or directive between them keeps its place, and the conversion is withheld when a reordered modifier would cross one") still describes the old RH2001-009, under which a directive between modifiers that keep their order is kept in the field. The fix now never builds such a field, because SHARED-FIX-010 withholds it. `ModifierOrderingUtilities.TryWriteModifiersInOrder`'s summary also cites RH2001-009 for trivia that keeps its place between modifiers; that utility's behavior may still be accurate for its other callers, but the RH2001-009 citation no longer describes an outcome RH2001 ships. Both are routed to the lead for the cleaner (14).
