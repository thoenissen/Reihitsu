# Lead — rh5604-shared-line-endings

- Issue: thoenissen/Reihitsu#1874 — `agent-filed` (goals are proposals)
- Slug: `rh5604-shared-line-endings`
- Branch: `claude/squad-1874-5pbeux` (session-designated); draft PR #1885
- Lane: **bug fix** — the issue asks to remove a duplicated statement of one rule (architecture finding: one rule stated in two places) with unchanged output of the fix and the formatter; production paths only, no visible output change, so no change doc.
- Roles run: skeptic (premise), test implementer, implementer, cleaner, spec checker, auditor, validator. Spec author: only if a clarification of a spec is needed.
- Maintainer clarifications: none.

## Maintainer answers

1. Output question (auditor finding 1): RH5604 on a line ended by a lone CR, U+0085, U+2028 or U+2029 (in code, at a directive's end, inside a `///` comment) — keep today's behavior: reported, and the fix converts it to the file's line ending; the rule's spec states it for every line-break character, and the characterization tests stay. Answer: **A**.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Skeptic | Where does the single implementation live? | In `Reihitsu.Core` `LineEndingUtilities`, next to `DetectEndOfLine`, taking a root and the line ending; it applies no formatter guards and returns its input unchanged when nothing differs | Allowed by FMT-PIPE-009, no new type, no new formatter entry point; a formatter entry point would add the formatter's refusals and drop the fix on files the formatter refuses | Output of fix and formatter unchanged; existing tests stay unchanged | 936efd0 |
| 2 | Skeptic | Include `DetectEndOfLine` and the RH5604 analyzer (two more places counting the same line-break token kinds)? | Fix narrowly; file one shared definition of the line-break token kinds as a follow-up | All four places agree today; the issue is about the normalization only | Nothing changes in output | — |
| 3 | Skeptic | Issue's performance paragraph | Dropped from the run's goals and not filed: the CLI measurement shows no difference at 5000 lines and an unattributed one at 20000 | Unmeasured claim; `agent-filed` goal | Internal | — |
| 6 | Repair 1 | Extend RH5604-002 to files whose line breaks are all one other terminator (e.g. all lone CR)? | No; left as is | Not asked by the issue or the answer; the rule's non-goal covers it; no test pins it | Spec text unchanged; output unchanged | f88e1b9 |
| 7 | Repair 1 | Example and test for a file with more lone CR than CRLF and no LF (predominant is LF, every line reported)? | No | Follows from RH5604-001 and the unchanged FMT-PIPE-007 | Decided by an unchanged requirement; output unchanged | f88e1b9 |
| 4 | Skeptic | Spec `Code owners` lines (`line-endings.md`, `RH5604.md`) | Routed to the spec author after the implementer, before the spec checker, once the owner exists | A clarification of ownership only | Text only | 5239d8e |
| 5 | Implementer | Add a `Reihitsu.Core.Test` unit test for `LineEndingUtilities.NormalizeLineEndings`? | No | Both callers are pinned by the RH5604 fix tests and the phase tests | Internal | — |

## Follow-up candidates

- Shared definition of the line-break token kinds for `LineEndingUtilities.DetectEndOfLine`, the RH5604 analyzer, and the new shared normalization (01-skeptic, architecture findings).

## Audited SHA

909b83b (audit 1, 07-auditor, verdict: findings; head 1fd503e differs only in 00-lead.md)

## Open worklist / next step

- Done: skeptic (01, proceed).
- Done: test implementer (02, tests written, 51b10d0).
- Done: implementer (03, tests pass, 936efd0).
- Done: spec author (04, 5239d8e; intros clarified, Code owners lines unchanged), cleaner (05, clean).
- Done: spec checker (06, consistent, 909b83b). Build green on 909b83b; main at 31d2931 (no merge needed).
- Done: auditor (07, findings, 12d012c).
- Answered: output question 1 → A.
- Worklist after the answer (repair round 1): finding 1 per answer A: spec author states RH5604 for every line-break character, then test implementer tags the characterization tests with the requirement(s) [spec author done: 08, f88e1b9; test implementer done: 09, eeacf15 (4 tests tagged); next: cleaner, spec checker, auditor (repair only), validator]; finding 2 (text only): spec author drops implementation sentences from the intros of line-endings.md and RH5604.md, keeps the behavioral sentence; then spec checker, auditor on the repair, validator.
