Role: spec author
Commit: 9ce0f90b3cf9a128345a943e2afab4c3bbc941a7
Verdict: text written

## Scope

Resolved the text conflicts of the lead's `git merge origin/main`. Merge parents: this branch at `58e27f87e99163dd483134cfce3a5ff1741ed9fc` and `origin/main` at `d1f4aee2b8c89337a3780ec086292a0bd4956ee0`, which brings in thoenissen/Reihitsu#1163 and thoenissen/Reihitsu#1164. No other file was touched; every other path merged without conflict.

## What conflicted and how it was resolved

- `sdd/specs/README.md`, the `shared/` rows of the index. This branch added the `shared/struct-equality.md` (SHARED-EQUAL) row; main added the `shared/statement-blocks.md` (SHARED-BLOCK) and `shared/statement-braces.md` (SHARED-BRACE) rows at the same spot. All three rows are kept, in the index's alphabetical path order: `statement-blank-lines`, `statement-blocks`, `statement-braces`, `struct-equality`, `token-gap-fixes`.
- `sdd/specs/requirement-ids.txt`. This branch added `SHARED-EQUAL-001`–`002`; main added `SHARED-EMPTY-001`–`004` at the same spot. All six IDs are kept, `SHARED-EMPTY-*` before `SHARED-EQUAL-*`.

How it was checked:

- `git diff --numstat` of the resolution against each parent shows only added lines for both files (README: +21 against main, +30 against this branch; ledger: +60 against main, +84 against this branch), so every row and every ID of both sides is present.
- The ledger is sorted under `LC_ALL=C sort -c` and has no duplicate IDs.
- No conflict markers remain anywhere in the working tree (`rg '^(<<<<<<<|>>>>>>>) '` returns nothing).

## Test result

`scripts/test.sh --project architecture` on the resolved tree passed 72 of 72 tests, with no failures or skips.

The merge was committed as `9ce0f90b3cf9a128345a943e2afab4c3bbc941a7` ("Merge origin/main into claude/quirky-lamport-mb0j4d [skip ci]"). It was not pushed.

## Changed requirements

_None._ No requirement was added, changed, or retired. Only ledger lines and index rows from the two parents were combined.

## Examples for the tests

_None._

## Open questions to file

_None._
