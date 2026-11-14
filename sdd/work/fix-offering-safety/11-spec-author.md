Role: spec author
Commit: 0f42032
Verdict: change doc written

Repair round 1 for the spec checker's Finding 1 (10-spec-checker.md). My prompt named the sources for the two entries (the maintainer's 2026-11-06 decision in #1150 for SHARED-FIX-011; SHARED-FIX-005 bug fix for #1673 for RH5308-006); both match `00-lead.md` (clarifications, decisions 4 and 9), so I used them. It carried no intended fix or hoped-for verdict otherwise.

- thoenissen/Reihitsu#1600 — no change in this round.
- thoenissen/Reihitsu#1150 — SHARED-FIX-011 now recorded in the change doc (What, Visible output changes #6, Decisions).
- thoenissen/Reihitsu#1274 — no change in this round.
- thoenissen/Reihitsu#1143 — no change in this round.
- thoenissen/Reihitsu#1673 — RH5308-006 now recorded in the change doc (What, Visible output changes #7, Decisions).
- thoenissen/Reihitsu#1678 — no change in this round.

## What I changed

Only `sdd/changes/2026-11/fix-offering-safety.md`; `Status: Accepted` kept; no spec file touched.
- *Affected requirements*: + SHARED-FIX-011, + RH5308-006.
- *What*: a paragraph "Fixes the formatter cannot lay out" naming both as bug fixes against SHARED-FIX-005/007, with their sources.
- *Visible output changes*: #6 (RH5308 fix in a file with a syntax error, from the SHARED-FIX-011 example; RH3004 and RH6023 notes) and #7 (RH5308 fix in an `[assembly:]` attribute argument, from the RH5308-006 example), each with input, before, after.
- *Decisions*: "A fix the formatter cannot lay out is not offered" (maintainer's 2026-11-06 decision, #1150) and "The RH5308 fix is withheld where it changes nothing" (#1673), each stating it was not an output question because it is a bug fix.

## Changed requirements
_None._ (change doc only)

## Examples for the tests
_None new._ #6 and #7 restate the SHARED-FIX-011 and RH5308-006 examples, which are already tagged.

## Output questions
_None._

## Internal choices
- Added the entries under *Visible output changes* with a Decisions entry each, rather than the checker's alternative of one sentence saying no entry is needed: the doc is the run's one record of what end users see.

## Follow-up candidates
_None._
