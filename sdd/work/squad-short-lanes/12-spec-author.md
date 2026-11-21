Role: spec author
Commit: e0ea3c5
Verdict: text written

Repair round 1, auditor findings 2 and 3 (`10-auditor.md`), as decision 23 states. My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond pointing at decision 23, which is the lead's recorded decision.

## Changed requirements
None — process change; no spec under `sdd/specs/` touched.

Text changes:
- Finding 2 / architecture finding — the restatement of the widening outcome is removed:
  - `squad/lead.md` §4, row "Spec author: needs a test (wording lane)": widen to the lane the lane-check rules give the diff once the named tests are in it, pointing to `scripts/README.md`, *The lane check*, as the one statement.
  - Change doc `sdd/changes/2026-11/squad-wording-lane-and-ci-only-watch.md`, *No test path in the wording lane*: the "`tests` without a change doc, `behavior-change` with one" sentence now points to the lane-check rules and notes that a changed test fails the proof. *The spec author writes the resource texts in every lane*: "widens to `tests` or `behavior-change`" (a second restatement, same defect) now reads "widens to the lane the lane check names".
  - `scripts/README.md`, *The lane check*, the sentence after rule 5 (the one statement, not a restatement): states the consequence the auditor's counterexample shows — a changed test fails the proof, so a C# file outside the test projects counts as production, giving `bug-fix` / `behavior-change` by rule 2, and rule 4's `tests` / `behavior-change` only when no such file is in the diff.
- Finding 3: `squad/roles/spec-author.md`, *Wording* bullet: quote the `TEXT-ONLY PROOF` and `LANE CHECK` lines that `scripts/lane-check.sh wording --head worktree` prints, and says why `scripts/verify-text-only.sh` alone does not serve.

Other places checked and kept: `squad/README.md` *Lanes* (wording bullet already says "which the lane check names"; the tests bullet and the claim paragraph state rule outcomes that are correct), change doc *What* bullet (records rule 4 itself, which is correct since production is decided first), `squad/roles/test-implementer.md` (no lane named).

## Examples for the tests
None.

## Output questions
_None._

## Internal choices
- Added the comment-edit consequence to `scripts/README.md` (the one statement) rather than to the lead row; alternative: leave README unchanged and point only. Chosen so the lead's pointer lands on a sentence that answers the counterexample.

## Follow-up candidates
_None._
