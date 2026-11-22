Role: spec author
Commit: 87e7b5e
Verdict: spec written

Merge of `origin/main` (7975188) into `claude/squad-970-1668-3bsm54`, concluded as 87e7b5e.

The one conflict was the `Uses` row of `sdd/specs/shared/code-fixes.md`. The resolution keeps both sides:
- main added SHARED-CASING-014, which the casing change cites in this spec's RH4103 example (line 290), so it stays;
- this run dropped SHARED-ORDER-007, which `code-fixes.md` no longer cites (see `03-spec-author.md`), so it stays dropped.

Consistency checks after the merge:
- `sdd/specs/requirement-ids.txt` merged without a conflict. Compared with main, its only difference is this run's SHARED-ORDER-011. The file is sorted and has no duplicates.
- The only spec file that both sides changed is `code-fixes.md`. Main's casing changes cite none of the ordering IDs this run changed.
- `scripts/test.sh --project architecture`: 198 of 198 pass.

## Changed requirements
_None._ The merge resolution changes no requirement text; only the `Uses` row is merged.

## Examples for the tests
_None._

## Output questions
_None._

## Internal choices
_None._

## Follow-up candidates
_None._
