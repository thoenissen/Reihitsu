Role: spec author
Commit: c1301ff (statements derived from this head; spec text only)
Verdict: spec written

Decision 18 (`00-lead.md`), from audit 21 findings 4 and 5. My prompt carried the worklist restating decision 18 and no other intended fix or hoped-for verdict.

## Changed requirements

- SHARED-GETONLY-008 — The fix and a member that shares its line — changed (paragraph 2, `sdd/specs/shared/get-only-expression-bodies.md`).
  - Paragraph 2 now states the condition by its cause: no fix when the gap to a sibling of the same list (member's first token / token in front, or last token / token behind, the other token a sibling's) holds any trivia other than whitespace and no line break separates its two tokens as SHARED-BLANK-008 defines one (end-of-line trivia, or the line break ending a `///` comment, a directive, or disabled text; a line break inside a multi-line comment's text does not count, SHARED-BLANK-002). That is `SyntaxTriviaUtilities.AreSeparatedByEndOfLine` / `EndsLine`.
  - A new explanatory paragraph lists what can stand in such a gap (`/* c */`, `/** c */` in either documentation mode, a multi-line comment) as explanation only.
  - `Why:` gains one sentence: stated by cause, reads the line break as the Core definition, so no comment kind and no documentation mode is missed.
  - Two examples added, both directions each: `/** c */` in the gap, and `/* a` / `b */` in the gap — reported, no code fix offered.
- Spec header `Uses` gains SHARED-BLANK-002 and SHARED-BLANK-008; the intro's wording of the `BlankLineCodeFixUtilities` step says "across a gap that holds a comment".
- No change doc change: withholding a fix for a rare class puts nothing in question (decisions 14 and 18). No rule page change.

`scripts/test.sh --project architecture`: 198 passed.

## Examples for the tests

SHARED-GETONLY-008 paragraph 2, for RH3209 and the indexer counterparts for RH3210:
1. `/** c */` between the converted member and a sibling behind it, and in front of it: no fix offered. Today `scripts/apply-fix.sh RH3209` fixes both and keeps the members on one line (finding 4); the tests should run in `DocumentationMode.Parse` (and may add `Diagnose`).
2. `/* a` / `b */` between the converted member and a sibling behind it, and in front of it: no fix offered. Today the fix is offered and converts in place (finding 5's pinned outputs flip to withheld under decision 18).
3. Unchanged: `/* c */` both directions withheld (today `no-fix-offered`), and the "other side" example (comment then line break, sibling on the next line) fixed.

All example inputs compile and are reported (`scripts/apply-fix.sh RH3209` on fixtures in the scratchpad, LF and CRLF).

## Output questions

_None._

## Internal choices

- The kind list (`/* */`, `/** */`, multi-line) is kept as an explanatory paragraph marked "as explanation only", not as part of the normative condition (alternative: drop it; it helps readers see why directives and `//` never trigger the condition).
- No separate "other side" example for a multi-line comment followed by a line break; the existing comment-then-line-break example covers that side of the line-break condition (alternative: add a fourth example).

## Follow-up candidates

_None new._ The RH3202 family's documentation-comment gap is already in `00-lead.md`.
