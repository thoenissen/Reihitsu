Role: auditor
Commit: cd7ef2399c2a3f28030e0d3078070dcd9b26d882
Verdict: state mismatch

## Scope

Repair round 1, to be audited as the delta `5dde996..cd7ef23` (excluding `sdd/work`). Not audited: the state check failed first.

- Head: `cd7ef23` — the checkout matches.
- Base in the prompt: `8a26419`, which the branch does contain.
- `git ls-remote origin refs/heads/main` returns `81eabebef044033483214fc0f01e63cf06067bb1` (#1733, "Lay out wrapped argument lists one element per line with trailing commas"). It was committed at 2026-11-15T07:40Z, before the repair commits at 08:01Z. It is not merged into the branch, so the prompt's base was already stale when this audit was spawned (`squad/lead.md` step 4).
- `git merge-tree --write-tree HEAD 81eabeb` reports **a content conflict** in `src/Reihitsu.Analyzer.Test/Formatting/RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzerTests.cs`. As it stands, the PR does not merge into `main`.

What the new `main` commit overlaps with:

- **Textual overlap.** Only the RH5102 test file above. Both conflict hunks sit at the end of the class, where both sides add tests:
  - The PR adds `VerifyConditionalDirectiveInAGapKeepsTheListFromBeingReportedButNotOneInsideAnArgument` and `VerifyConditionalDirectiveWithAlternativesInFrontOfTheFirstArgumentKeepsTheListFromBeingReported` (RH5102-004).
  - `main` adds `FixMovesLeadingCommaBehindPreviousArgument` and its siblings (RH5102-003, FMT-BREAK-012).
  - These are independent entries, so the resolution keeps both sides. The test implementer owns the file.
- **Behavioral overlap.** `main` changes `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakListRewriter.cs`, the layout the RH5101/RH5102 code fixes delegate to. The PR changes the RH5102 analyzer's directive handling (RH5102-004, #1337).
  - `main`'s new RH5102 fix tests will run against the PR's analyzer.
  - The PR's RH5102/RH5101 fix tests will run against `main`'s new list layout.
  - Neither side was tested against the other. This is code that both sides change, not only independent ledger or test entries.
- `main` also changes `sdd/specs/formatter/line-breaks.md`, which the PR does not touch, and adds `sdd/changes/2026-11/list-wrap-by-elements.md`.

Needed before the repair audit:

1. Merge `origin/main` (`81eabeb`).
2. Have the test implementer resolve the RH5102 test-file conflict by keeping both sides.
3. Run the cleaner, and the focused RH5101/RH5102 analyzer tests on the merged tree.
4. Spawn the auditor again with the merged head and the new `ls-remote` SHA.

A state mismatch does not use up the repair round.

## Findings
_N/A — state mismatch; not audited._

## Guard scope
_N/A — state mismatch; not audited._

## Policy ownership
_N/A — state mismatch; not audited._

## Assertion adequacy
_N/A — state mismatch; not audited._

## Failure classes dismissed
_N/A — state mismatch; not audited._

## Architecture findings
_None._

## Hints
_None._
