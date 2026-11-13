Role: spec author
Commit: ff07175
Verdict: change doc written

Repair round 1, from 15-spec-checker.md findings 1–4 and lead decisions 31–33. My prompt carried no intended fix or hoped-for verdict beyond the lead's decisions.

## Changed requirements
_None in the specs._ The change doc `sdd/changes/2026-11/moved-code-keeps-its-lines.md` now lists:
- RH7303-004 and SHARED-FIX-007 *Removed directive line* (#1263): header row, a *What* bullet (edges of any braces), a visible output change (initializer; accessor list and switch alike), decision (maintainer Q5 A), rejected alternative (Q5 B).
- RH7302-005 (#1151): header row, a *What* bullet (trailing whitespace behind the `#region` description), a visible output change, decision (maintainer Q6 A), rejected alternative (Q6 B).
- *Why* names #1263 and #1151; the substring renames and the RH7303 doubled/edge blank lines in bodies are bugs against SHARED-FIX-003/007 and are not listed.
- Status set to `Accepted` (decision 33).

`documentation/rules/RH7004.md`: the withheld-fix paragraph (local function, labeled statement) removed from *How to fix it*; the page is otherwise unchanged (decision 32).

`scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd|FullyQualifiedName~Documentation"`: 53 passed, 0 failed.

## Examples for the tests
_None new._ The before/after outputs are the existing RH7303-004 and RH7302-005 examples.

## Output questions
_None._

## Internal choices
- The "Before" outputs of the two new entries are the ones the skeptic (02, Q4) and spec author (01, Q3) measured on `main` with `scripts/apply-fix.sh`, marked "issue run"; not re-measured. Alternative: re-measure on `origin/main` — not done; the auditor or validator can re-measure if the base moved.

## Follow-up candidates
_None._
