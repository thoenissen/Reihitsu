Role: spec author
Commit: f88a099
Verdict: spec written

Repair round 2: carries out decision 6 in `00-lead.md` (auditor finding #1 in `11-auditor.md`). The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond pointing at decision 6.

## Changed requirements

- RH5419-004 — The fix is withheld where the indexer cannot be put on one line — changed. The one-line check now measures the indexer's code from the first token behind its attribute lists through its `;` (the span RH5401's fix measures), not from the `=>`. Stated by cause: a comment, a directive, or disabled text in front of the `[` that keeps a line break between two tokens of the declaration withholds the fix (a line comment behind `this`, a comment on its own line in front of the `[`, a directive between `this` and `[`). Both sides of the boundary are stated (FC-002): the first token's leading trivia (documentation comment) and the attribute lists are not measured; a block comment on one line in front of the `[` (`this /* c */ [`) and a bare line break between `this` and `[` (joined by the formatter, FMT-BREAK-011) keep the fix. `Uses` gains FMT-BREAK-011 (the spec-structure test requires it).
- Withheld example gains four indexers: auditor members (a) line comment behind `this` with a wrapped list (`sbyte`), (b) own-line block comment in front of `[` (`ushort`), (c) `#if DEBUG`/`#endif` between `this` and `[` (`float`), (d) line comment behind `this`, one-line list, wrapped arrow (`double`).
- Offered example gains a documentation comment in front of a wrapped indexer (`short`), the counterpart of the "leading trivia is not measured" side.
- Change doc `sdd/changes/2026-11/rh5419-indexer-arrow-single-line.md`: the *What* summary states the new withheld class; new decision "A comment or a directive in front of the `[` that keeps a line break withholds the fix" with an input/after block; the offering alternative is listed as rejected. Status stays Accepted (decision 6 withholds a rare class, nothing visible in question).

Checks: the RH5419-004 example blocks run through `scripts/apply-fix.sh RH5419` (LF and CRLF): the offered example gives exactly the spec's output block; the four new withheld indexers, each in its own fixture, parse and are reported; today's code still offers the two-line fix for them (the behavior not built yet), the rest of the output agrees with the auditor's observations. `scripts/test.sh --project architecture` passes (198/198). Rule page unchanged: a withheld fix for a corner case.

## Examples for the tests

RH5419-004 (both examples). The tests `VerifyFixIsOfferedForALineCommentInFrontOfTheOpeningBracket` and `VerifyFixIsOfferedForACommentInTheLeadingTriviaOfTheOpeningBracket` flip to withheld; add the directive case (c), the one-line-list case (d), and the documentation-comment counterpart.

## Output questions

_None._

## Internal choices

- Replaced my earlier choice in `03-spec-author.md` (*Withhold condition by cause*: measure from `=>`) with the first-token span, per decision 6. Alternative was keeping it and asking an output question; the lead decided.
- Stated the measured span as "from the first token behind its attribute lists", so modifiers (`public // c` + line break) are measured too; such input is rare and withheld consistently with RH5401.

## Follow-up candidates

_None._
