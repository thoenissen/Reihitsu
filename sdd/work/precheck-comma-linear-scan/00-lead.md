# Lead — precheck-comma-linear-scan

- Issues: thoenissen/Reihitsu#1802 (`agent-filed`: goals are proposals)
- Slug: precheck-comma-linear-scan
- Branch: claude/sweet-ptolemy-igxteg (session-designated); draft PR #1840
- Lane: bug fix — the issue reports an observed cost (quadratic time in the precheck's comma check) against expected linear behavior; no visible output is meant to change, so no change doc is required.
- Roles run: skeptic (premise and reachability), test implementer (reproduction), implementer, cleaner, spec checker, auditor, validator.
- Roles skipped: spec author — the bug-fix lane uses it only for a clarification; none is needed unless the skeptic finds one.
- Maintainer clarifications: none.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Skeptic | Proceed with #1802? | Proceed: reproduced in the CLI and in code-fix registration; reachable | 01-skeptic.md measurements | Time only; verdicts, reasons, locations unchanged | — |
| 2 | Skeptic | Keep the issue's prescribed single-walk solution as a requirement? | Dropped as a requirement; the implementer chooses the mechanism | Proposal of an `agent-filed` issue | Internal mechanism | — |
| 3 | Skeptic | Keep "verdicts identical on the repository's own sources" as a done-criterion? | Dropped; the existing precheck tests passing unchanged is the proof | Formatted sources all answer "nothing unsupported", so the sweep proves little | No visible output changes | — |
| 4 | Skeptic | How to test a cost? | One regression test with a generous wall-clock budget on a long single list; the test implementer confirms how a timeout fails since the precheck does not observe cancellation | Linearity cannot be asserted robustly | Test only | — |

## Follow-up candidates

- Whole-document formatting is quadratic in the length of one list (no comment: 2.9 s at 2 500, 9.9 s at 5 000, 40.5 s at 10 000 elements); the precheck is ~5 % of it. Performance, no output question. Source: 01-skeptic.md.

## Audited SHA

None yet.

## Open worklist / next step

- Done: skeptic (proceed).
- Next: test implementer (reproduction).
