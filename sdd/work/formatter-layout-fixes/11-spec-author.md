Role: spec author
Commit: 737847a
Verdict: change doc written

Repair round 1, lead decision 18 (spec checker 10, finding 1). My prompt carried no intended fix beyond that recorded decision. Only `sdd/changes/2026-11/raw-string-tab-one-column.md` changed; no spec, requirement ID, or rule page.

## Changed requirements

None in the specs. Change doc repaired:
- `Affected requirements` lists FMT-RAW-002 (changed).
- *What*: the tab is stated as the only extension into the non-goal and the only behavior change. A new bullet states that `main` counted only U+0020, that U+00A0, U+2003, vertical tab, and form feed counted as no column and could not be removed on a leftward shift (four U+00A0 left the closing quotes four columns right of the opening quotes), and that counting and removing them is the #1611 bug fix against RH5202-003 and SHARED-FIX-007, which already demanded it (02-skeptic, #1611 *Premise*).
- *Why*: names the non-tab part as a defect fixed as a bug.
- *Visible output changes*: a lead sentence says only the tab and the single-type base list are listed, and why the non-tab output is not.

## Examples for the tests

Unchanged (FMT-RAW-001, FMT-RAW-002 examples already in the spec).

## Output questions

_None._

## Internal choices

- The non-tab output gets no `Visible output changes` entry, only a sentence saying why; the alternative, a full before/after entry, would present a bug fix as a decided change.

## Follow-up candidates

_None._
