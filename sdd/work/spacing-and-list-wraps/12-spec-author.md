Role: spec author
Commit: 6710433
Verdict: spec written

Repair round 2 for `11-auditor.md` finding 1, on lead decision 18. My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond decision 18; I followed the decision and the finding.

- `sdd/specs/formatter/line-breaks.md` FMT-BREAK-012: a third bullet states that a first element a comment, a directive, or disabled text keeps off the line of the opening parenthesis or bracket (one of them in the gap, and a line break separating them, counted as SHARED-BLANK-008 counts) cannot join that line (FMT-BREAK-001), stays at the start of its own line, and makes the list multi-line, one element per line, also when all elements start on that one line. The qualifier has both sides: `Foo( /* a⏎ b */ 1, 2)` (line break only inside the comment's text) stays single-line, `Foo( // c⏎1, 2)` and `Foo(⏎/* c */⏎1, 2)` are held, `Foo(⏎1, 2)` holds nothing and is joined. The `Why:` gains the reason (FMT-BREAK-001; the other layout would leave an RH5102 report without a fix, SHARED-DIAG-010; rare class, RH5035/RH5038). A new example: a `//` comment behind `(` in a parameter list and an argument list, and a `#pragma` line in front of the first argument. Its "after" block is the CLI's output on the head (`scripts/format.sh`), which the auditor found identical to `main`.
- `Uses` gains SHARED-DIAG-010, at its sorted position, now referenced in the body.
- `sdd/changes/2026-11/list-wrap-by-elements.md`: the *What* bullet "The elements decide" and the decision *Elements decide, not line breaks at the delimiters* name the exception and that it is `main`'s output; *Alternatives considered* gains "keep the elements behind a held first element on its line", rejected. No visible-output entry (output as on `main`); `Status` stays `Accepted`.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"`: 51 passed.

No rule page changes (no report or fix changes for the user).

## Changed requirements
- FMT-BREAK-012 — A list is on one line or one element per line — clarified (held first element; output as on `main`)

## Examples for the tests
- FMT-BREAK-012: the new example "a first element that a comment or a directive keeps off the line of the opening parenthesis" (comment behind `(` in a parameter and an argument list; `#pragma` twin). Decision 18 asks for one test with a comment held behind `(` and a directive or disabled-text twin.

## Output questions
_None._

## Internal choices
- Wrote the case as its own bullet of FMT-BREAK-012 rather than a sentence inside the single-line bullet, so the qualifier "held" has its examples on both sides next to it. Alternative: a parenthetical exception in the single-line bullet.
- Left the multi-line block comment case (`Foo( /* a⏎ b */ 1, 2)`) out of the example block: on the head its output drops the space in front of `1` (`b */1, 2`), a spacing question I did not want pinned into a line-break example (see Follow-up candidates). It stays in the requirement text as the "not held" side.

## Follow-up candidates
- Seen while checking the example, not verified against `main` and not in this run's issues: `Foo(⏎    // c⏎    1, 2);` gets a blank line inserted between `Foo(` and `// c`. A blank line directly behind an opening parenthesis looks wrong; the blank-line phase owns it. The lead decides whether it is worth an `agent-filed` issue after checking `main` and FMT-BLANK.
- Also seen, not verified against `main`: `Foo( /* a⏎ b */ 1, 2);` becomes `Foo( /* a⏎ b */1, 2);` (the space behind the multi-line comment is removed, while the space in front of it is kept). Whether FMT-SPACE-001/FMT-SPACE-013 demand that (the single-line twin `Foo( /* a b */ 1, 2)` would show) is for the lead to check; it touches #1642's area only by subject.
