Role: spec author
Commit: 91bab82
Verdict: change doc written

This answers audit finding #7 (`05-auditor.md`) with the maintainer's option A (`squad/decisions/2026-10-31-line-start-fact-fixed-spacing.md`). The maintainer accepted the change doc in advance.

## Changed requirements

- **FMT-SPACE-012** — One space after statement keywords, `new`, and `operator` — changed. The keyword list gains `fixed` (between `catch` and `using`), and a new example turns `fixed(int* p = items)` into `fixed (int* p = items)` inside an `unsafe` method.
- **Change doc** `sdd/changes/2026-10/fixed-keyword-spacing.md` — `Status: Accepted`, and it records that the acceptance was given in advance.
  - **What:** `fixed(…)` becomes `fixed (…)`, and `fixed/* c */(…)` becomes `fixed/* c */ (…)`.
  - **Why:** RH6001-001 and its *Why*, SHARED-DIAG-010.
  - **Decisions:** the formatter follows the rule; no exception for fixed-size buffers.
  - **Alternatives:** options B and C, rejected.

## RH6001-001's *Why* after the change

"formatted code raises no RH6001" holds again by the specs:
- `fixed(` is spaced (FMT-SPACE-012), like every other RH6001 keyword.
- With a comment, the formatter writes `fixed/* c */ (`, as it writes `if/* c */ (c)` (FMT-SPACE-013). RH6001-001 reports only a keyword with "no whitespace … anywhere between" it and the `(`, so it does not report that output.

It holds in the code only once `KeywordSpacingRule.IsSpacedKeyword` gains `FixedKeyword`.

Probed today with `scripts/format.sh --force`:
- `fixed(int* p = items)` is kept as written.
- `public fixed  int _buffer[4];` becomes `public fixed int _buffer[4];`, through FMT-SPACE-002's collapse. That is the output FMT-SPACE-012 now requires for the buffer too, so the change doc claims no output change there.

## Examples for the tests

- The new FMT-SPACE-012 `fixed` example, as a formatter regression test.
- A `VerifyFormatter` test for RH6001 on `fixed(` (finding #7).
- A test for the change doc's comment form, `fixed/* c */(…)` becoming `fixed/* c */ (…)`, tagged FMT-SPACE-012 and FMT-SPACE-013.

## Open questions for the maintainer

_None._
