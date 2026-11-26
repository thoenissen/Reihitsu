Role: spec author
Commit: 4132a2d
Verdict: spec written

Follow-up round: record the maintainer's answers `1A, 2A, 3A, 4A, 5A` (00-lead.md, Maintainer answers rows 2–6), carry out lead decision 2, accept the change doc.

## Changed requirements
- SHARED-EMPTY-003 — The fix writes the semicolon declaration — clarified: the sentence about a comment behind the closing brace now applies only when no semicolon follows the brace; otherwise SHARED-EMPTY-005 decides. This removes the overlap with SHARED-EMPTY-005's withheld case `} // note⏎;` that 02-skeptic.md found. No output changes.

Change doc `sdd/changes/2026-11/empty-type-stray-semicolon.md`:
- `Status: Accepted`.
- *Decisions* gains "The maintainer approved every visible output change", with one bullet per answer (Q1 base forms and fix, Q2 RH5418, Q3 comment or directive between `}` and `;` reported without a fix, Q4 formatter, Q5 nested conversion through RH5404's fix).
- *Why* now cites the earlier decision as "#1169, question 2, decided under #912" instead of "#912, question 2 of the empty-type spec questions".

The specs already agreed with every answer: SHARED-EMPTY-001, -004, and -005, FMT-STRUCT-017 and -018, and the RH5412–RH5416 and RH5418 specs from 9af7865. No other spec text changed. Rule pages stay unchanged.

## Examples for the tests
SHARED-EMPTY-001, SHARED-EMPTY-003, SHARED-EMPTY-004, SHARED-EMPTY-005, FMT-STRUCT-017, FMT-STRUCT-018. Use the change doc's *Visible output changes* for RH5418 and for the RH5404 nested case.

`scripts/test.sh --project architecture`: 197 passed, 1 failed. The failure is `EveryRequirementOfAnEnforcedSpecIsCovered`: SHARED-EMPTY-005 has no `[Requirement]` test yet. That is expected before the test stage, and the test implementer must cover it.

## Output questions
_None._

## Internal choices
_None._ Both changes are lead decision 2.

## Follow-up candidates
_None_ new. The ones in 00-lead.md stand.
