Role: spec author
Commit: e67b580
Verdict: change doc written

Status follow-up to 03-spec-author.md, on the maintainer's answers recorded in 00-lead.md: question 1 (thoenissen/Reihitsu#1586) answer A, question 2 (thoenissen/Reihitsu#1728) answer A. The spec was written on A for both, so no spec text changed; the change doc `sdd/changes/2026-11/field-declaration-directives.md` records the answers and is `Status: Accepted`. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changed requirements

None in the specs. Change doc only:
- Status: Draft → Accepted.
- Decisions: "Declare the shape" now records question 2, answer A (declared in this change, the CLI reports it, the code fixes that format it are withheld); "The leading trivia of the first token is not part of the shape" is no longer open: the maintainer decided it (question 1, A); "Disabled text in the leading trivia keeps the declaration whole" is confirmed by the same answer (option D rejected).
- Special cases, *Checked and kept*: the disabled-text check's note now says the maintainer rejected declaring it unsupported.
- Alternatives considered: "Refuse the leading trivia too" (C), "Declare disabled text in the leading trivia unsupported" (D), "Keep the declaration whole where a directive leads it" (B) are marked rejected by the maintainer, not open.
- Checked: no requirement in `unsupported-syntax.md`, `structural-transforms.md`, `pipeline.md`, or `analyzer/RH7101.md` marks either question as open. No requirement-ID reference changed, so the architecture tests were not rerun.

## Examples for the tests

Unchanged from 03-spec-author.md.

## Output questions

_None._ Both are answered.

## Internal choices

_None._

## Follow-up candidates

Unchanged from 01-spec-author.md (event field / local declarator behind an `#endif` loses its continuation indent).
