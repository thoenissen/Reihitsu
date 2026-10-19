Role: spec author
Commit: 5efd029
Verdict: spec written

Merge round. The lead's `git merge origin/main` stopped with a content conflict in `sdd/specs/requirement-ids.txt`. I resolved it and concluded the merge as `5efd029` (parents `328c205`, this branch, and `3b113db`, `origin/main`). My prompt carried no intended fix, suspected cause, or hoped-for verdict.

The conflict was one hunk after `SHARED-FIX-002`. This branch adds `SHARED-FIX-003`, `-004`, and `-005`. Main adds `SHARED-JOIN-001` and `-002`, from thoenissen/Reihitsu#1104. I kept both sides, with the FIX IDs first, because `FIX` sorts before `JOIN` in ordinal order. I changed no other line.

Check of the resolved ledger:

- `LC_ALL=C sort -c` passes, so the file is in ordinal sort order.
- No ID appears twice.
- Every ID in `origin/main`'s ledger is present, and so is every ID in this branch's pre-merge ledger.
- The file has 781 lines and ends with a newline.

Every other file merged cleanly. I did not edit any of them.

Test: `scripts/test.sh --project architecture` gives `Passed!  - Failed:     0, Passed:    72, Skipped:     0, Total:    72`.

## Changed requirements

_None._ Only the ledger was merged. No requirement text changed.

## Examples for the tests

_None new._

## Open questions to file

_None._
