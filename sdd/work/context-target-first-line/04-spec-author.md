Role: spec author
Commit: 90ef1f6
Verdict: change doc written

Round for the maintainer's answers, which are recorded in `00-lead.md` under *Maintainer answers*: A on questions 1–18.

## What changed

- Change doc `sdd/changes/2026-11/context-target-first-line.md`: added the subsection *The maintainer chose the listed outputs* at the top of *Decisions*. It records:
  - The "after" output of changes 1–18 is chosen.
  - Question 1 (A): the first line keeps its document column and the aligned lines follow it. This applies to changes 1, 3 and 7–10.
  - Question 4 (A): the parameter keeps the line it shares with another parameter, with no line break in front of it.
  - None of today's outputs are kept.
  - The maintainer's guidance on cost: a formatter change made only for code fixes must be worth its cost, and the maintainer sees no problem with this one.
- `Status` stays `Draft`. Under `sdd/README.md` the lead sets `Accepted` in a squad run, and every listed change now has the maintainer's answer.
- No spec text changed. Checks against option A:
  - Question 1: the FMT-PIPE-017 first-way example keeps `b` in column 14, with `2` under `1` in column 24. RH5111-004's declarator example gives the same output. Both match A.
  - Question 4: the FMT-PIPE-017 second-way example writes no line break, keeps `string` in column 18, and puts `+` under `"x"` in column 29. RH5111-004's parameter example gives the same output. Both match A. FMT-PIPE-024's line-break bullet ("neither breaking … nor joining") and SHARED-FIX-007 ("shares its line … also where formatting the result would move its first token … onto a line of its own") also match A.
  - Questions 2, 3 and 5–18: the spec examples and the change doc's "after" blocks are the A outputs.

## Changed requirements

None in this round. FMT-PIPE-017, FMT-PIPE-018, FMT-PIPE-024, SHARED-FIX-007 and RH5111-004 are as in `03-spec-author.md`.

## Examples for the tests

Unchanged from `03-spec-author.md`: the FMT-PIPE-017 examples, and changes 1–18 in the change doc.

## Output questions

_None._ All 18 are answered.

## Internal choices

- I left `Status: Draft` for the lead to set to `Accepted`, as `sdd/README.md` assigns it.

## Follow-up candidates

_None._
