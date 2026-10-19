Role: spec author
Commit: 1b21a0d
Verdict: spec written

Merge round. The lead's `git merge origin/main` stopped with a content conflict in `sdd/specs/requirement-ids.txt`. Main had moved by thoenissen/Reihitsu#1101, #1102 and #1103. I resolved the conflict and concluded the merge as `1b21a0d`. Its parents are `206af00` (this branch) and `d65c782` (`origin/main`). My prompt carried no intended fix, suspected cause, or hoped-for verdict.

The conflict was one hunk after `SHARED-FIX-002`. This branch adds `SHARED-FIX-003`, `-004` and `-005`. Main adds `SHARED-GAP-001`, from thoenissen/Reihitsu#1103. I kept both sides, with the FIX IDs first, because `FIX` sorts before `GAP` in ordinal order. I changed no other line.

Check of the resolved ledger:

- `LC_ALL=C sort -c` passes.
- No ID appears twice.
- Every ID from both sides is present, and there are no extra IDs. The result has 941 lines: main's 938 plus this branch's 3.
- The file ends with a newline.

Every other file merged cleanly. I did not edit any of them.

## What main changed that this run depends on

- **No file changed on both sides** except the ledger.
- **`sdd/specs/shared/token-gap-fixes.md` (SHARED-GAP-001) is new.** It is the only new shared requirement about withholding a fix for a comment. It is a rule-family guard. It withholds the fix whenever a gap the fix rewrites holds a comment or a directive. SHARED-FIX-003 would count the whitespace next to such a comment as layout and would allow the fix. SHARED-FIX-003 only states when a fix MUST NOT be offered. It does not require a fix to be offered, so the stricter family guard is consistent with it. No requirement text needs a change. SHARED-ATTR-017 to -020 and the SHARED-OPSPACE requirements also withhold more, or keep comments in place. They are consistent with SHARED-FIX-003 in the same way.
- **No spec on main references a SHARED-FIX ID.** None restates SHARED-FIX-003 to -005.
- **Main added two helpers to the analyzer test bases:** `VerifyNoCodeFixIsOffered` in `AnalyzerTestsBase<TAnalyzer, TCodeFix>`, and `ExpandWhitespaceMarkers` in `AnalyzerTestsBase<TAnalyzer>`. No test file of this run declares a member with either name, so nothing hides them. The run's tests could use them in a later round, but they do not need to.
- **Main's new code-fix test classes derive from `BatchCodeFixTestsBase`.** These are the RH55xx list-shape and RH60xx spacing classes. They inherit its `SHARED-FIX-001`/`-002` tests. That is why the `SHARED-FIX-001` count rose to 234.

## Tests

- `scripts/test.sh --project architecture`: `Passed!  - Failed:     0, Passed:    72, Skipped:     0, Total:    72`. The whole solution built.
- `scripts/test.sh --project analyzer --filter "TestCategory=SHARED-FIX-001|TestCategory=SHARED-FIX-003|TestCategory=SHARED-FIX-004|TestCategory=SHARED-FIX-005"`: `Passed!  - Failed:     0, Passed:   253, Skipped:     0, Total:   253`.
  - Per category: 001: 234, 003: 17, 004: 1, 005: 1. All passed.

## Changed requirements

_None._ Only the ledger was merged. No requirement text changed.

## Examples for the tests

_None new._

## Open questions to file

_None._
