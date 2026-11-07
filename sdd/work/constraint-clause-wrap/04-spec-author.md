Role: spec author
Commit: 50903fd
Verdict: spec written

This is a repair after Q9 (`00-lead.md`, *Maintainer answers*: Q9 → A). The coordinator's message carried no intended fix or suspected cause.

## Changed requirements

- **RH5110-004 — dropped.** No input is left that it could cover. RH5110's fix formats through the formatter, and FMT-BREAK-037 never joins across a comment, a directive, or disabled text (FMT-BREAK-001). A constraint clause cannot hold a literal. So Q8's withholding has nothing constraint-specific to apply to, and SHARED-FIX-003 alone governs.
  - The ID was issued in c716d42 on this branch only; it was never on `main`. I removed its ledger line rather than retiring it, so the PR's ledger diff only adds lines (FMT-BREAK-037, FMT-INDENT-032).
  - Alternative: keep RH5110-004 as `Status: Retired`. Rejected, because it would leave a retired requirement that never shipped.
- **RH5110-003 — The fix puts each constraint clause on its own line — changed.**
  - The fix also lays out each clause's constraints (FMT-BREAK-037, FMT-INDENT-032).
  - Where a comment, a directive, or disabled text blocks a FMT-BREAK-037 join, the fix MUST still be offered. Its result is the formatter's fallback layout: the blocked line break stays, and the lines it leaves are placed by FMT-INDENT-032 (Q9).
  - Its `Why` states that the layout moves nothing, so SHARED-FIX-003 gives no reason to withhold.
  - New example: `where T : // c` followed by `class,` and `new()` at column 18, and `where T : class // c` followed by `, new()` at column 18. These are the outputs the maintainer was shown.
  - `Uses` now reads FMT-BREAK-001, FMT-BREAK-023, FMT-BREAK-037, FMT-INDENT-017, FMT-INDENT-032, SHARED-COMMENT-011, SHARED-FIX-003.
- **Change doc** `sdd/changes/2026-11/constraint-clause-wrap.md`:
  - `Affected requirements`: RH5110-004 (added) is replaced by RH5110-003 (changed).
  - The *What* bullet is rewritten.
  - Entry 13 now shows the fix result, with the before output from `apply-fix.sh` on c317972 and the after output above.
  - *Unchanged* now says the fix stays offered wherever it was offered before.
  - Q8's decision is rewritten: Q9 narrowed it, and no RH5110-specific withholding remains.
  - Q9 is added, with its rejected option.
  - The alternative "withhold where a join is blocked" is rejected by Q9.
  - `Status` stays `Accepted`; every entry has an answer.
- `SpecStructureTests` and `SpecConsistencyTests` pass (26/26). FMT-BREAK-037, FMT-INDENT-032, and RH5110-003 (new example) still need tests.

## Examples for the tests

- RH5110-003: the new pair, with a comment behind the colon and a comment in front of a comma. The fix is offered, and the result is aligned at column 18. The existing pair stays.
- FMT-BREAK-037 and FMT-INDENT-032: unchanged from `03-spec-author.md`.

## Output questions

_None._

## Internal choices

- I dropped RH5110-004 instead of retiring it, with the reasons given above.
- The Q9 behavior went into RH5110-003, not into a new requirement, because it is the same decision: the fix result is the formatter's result.

## Follow-up candidates

None new. The candidates in `00-lead.md` stand.
