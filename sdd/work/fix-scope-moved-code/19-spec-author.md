Role: spec author
Commit: dfb829a
Verdict: spec written

Repair round 2 (lead decisions 34–37, auditor 18 findings 1–3). My prompt named the decisions to record; it carried no suspected cause or hoped-for verdict beyond them.

## Changed requirements
- RH7004-007 — The fix is withheld for lines it cannot change as whole lines — changed: interpolated strings in every `$` form count as a whole (a hole spanning lines, a later line starting at `{` or the closing quote); new bullet: a tab in front of the declaration's line or a moving line withholds the fix; points at SHARED-FIX-008 for line endings. Uses gains RH3004-006, SHARED-FIX-008.
- SHARED-ORDER-009 — A move moves whole lines — changed: the one-line swap is withheld when a comment stands in the accessor list outside every accessor (in front of the first, between two, behind the last); a comment inside an accessor travels with it.
- SHARED-ORDER-010 — A shifted declaration holds no line that cannot move — changed: decided on the string or comment node that spans lines, not on where a line starts; interpolated example added. The fixed-output block of the "indented as its slot" example was misplaced behind the comment-line example; moved to its own example.
- SHARED-FIX-008 — A fix keeps the document's line ending — changed: the member/accessor moves, RH7004, and RH3004's block-body path write created line breaks as the detected ending and are withheld when the moved or rewritten code holds another line break (mixed, all lone `\r`); the one-line swap stays offered.
- SHARED-FIX-007 — *Moved code* — changed: tab counting applies to member/accessor moves only (RH7004 now withholds, like RH3004); string sentence covers interpolated strings and points at SHARED-FIX-008. Uses gains RH3004-008.
- RH3004-008 — clarified: interpolated strings in every `$` form, a hole spanning lines, later lines starting at `{` or the closing quote.
- Change doc `sdd/changes/2026-11/moved-code-keeps-its-lines.md` (Status Accepted kept): header, What, a "Rare-class refusals of repair round 2" visible-output section (inputs, After: no fix; Before not measured byte by byte on `main`), four decisions.

## Examples for the tests
RH7004-007 (interpolated `$@"` hole line, hole spanning lines, tab in front of a moved line), SHARED-ORDER-009 (comment between / behind / in front of one-line accessors withheld; comment inside an accessor swapped), SHARED-ORDER-010 (right-shifted field with `$@"a⏎{nameof(C)}"`), SHARED-FIX-008 (RH7109 field with an LF break in a CRLF document; RH7004 alike; all lone `\r`).

## Output questions
_None._

## Internal choices
- Decision 36's "inside the accessor list" is written as "in the list outside every accessor", including in front of the first accessor (alternative: only between/behind accessors, as the auditor suggested; the comment behind `{` reads as describing the first accessor, so it is refused too). A comment inside an accessor does not withhold.
- Decision 39: RH3004's block-body path is named in SHARED-FIX-008's refusal, so its pre-existing mixed-ending gap is covered in this run rather than a follow-up (alternative: leave RH3004 out and file a follow-up).
- The line-ending refusal lives in SHARED-FIX-008 only; the per-rule refusal lists (RH7102-006 etc.) are not extended, because SHARED-FIX-008 applies to every fix.
- Rule pages unchanged: every change is a withheld fix.

## Follow-up candidates
_None._
