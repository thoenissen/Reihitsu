# Lead — rh5604-shared-line-endings

- Issue: thoenissen/Reihitsu#1874 — `agent-filed` (goals are proposals)
- Slug: `rh5604-shared-line-endings`
- Branch: `claude/squad-1874-5pbeux` (session-designated); draft PR #1885
- Lane: **bug fix** — the issue asks to remove a duplicated statement of one rule (architecture finding: one rule stated in two places) with unchanged output of the fix and the formatter; production paths only, no visible output change, so no change doc.
- Roles run: skeptic (premise), test implementer, implementer, cleaner, spec checker, auditor, validator. Spec author: only if a clarification of a spec is needed.
- Maintainer clarifications: none.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Skeptic | Where does the single implementation live? | In `Reihitsu.Core` `LineEndingUtilities`, next to `DetectEndOfLine`, taking a root and the line ending; it applies no formatter guards and returns its input unchanged when nothing differs | Allowed by FMT-PIPE-009, no new type, no new formatter entry point; a formatter entry point would add the formatter's refusals and drop the fix on files the formatter refuses | Output of fix and formatter unchanged; existing tests stay unchanged | — |
| 2 | Skeptic | Include `DetectEndOfLine` and the RH5604 analyzer (two more places counting the same line-break token kinds)? | Fix narrowly; file one shared definition of the line-break token kinds as a follow-up | All four places agree today; the issue is about the normalization only | Nothing changes in output | — |
| 3 | Skeptic | Issue's performance paragraph | Dropped from the run's goals and not filed: the CLI measurement shows no difference at 5000 lines and an unattributed one at 20000 | Unmeasured claim; `agent-filed` goal | Internal | — |
| 4 | Skeptic | Spec `Code owners` lines (`line-endings.md`, `RH5604.md`) | Routed to the spec author after the implementer, before the spec checker, once the owner exists | A clarification of ownership only | Text only | — |

## Follow-up candidates

- Shared definition of the line-break token kinds for `LineEndingUtilities.DetectEndOfLine`, the RH5604 analyzer, and the new shared normalization (01-skeptic, architecture findings).

## Audited SHA

—

## Open worklist / next step

- Done: skeptic (01, proceed).
- Next: test implementer — pin the U+2028 / lone CR fix cases and the fix on a file the formatter refuses (01-skeptic); then implementer.
