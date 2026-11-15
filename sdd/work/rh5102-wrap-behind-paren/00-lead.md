# Lead — rh5102-wrap-behind-paren

- Issue: thoenissen/Reihitsu#1755 — `agent-filed` (goals are proposals). RH5102 reports an argument list wrapped only behind `(` whose arguments share one line; the formatter joins it.
- Branch: `claude/pensive-davinci-jjfo8i` (the session's designated push branch, used instead of `claude/issue-1755-…`). Draft PR: #1778.
- Lane: **behavior change** — the maintainer's answer removes a diagnostic RH5102-001 currently requires, so a visible output changes and a change doc is required.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist.

## Maintainer answers

- #1755, answered on the issue before the run (2026-11-15): **option A**. RH5102 does not report an argument list whose arguments all stand on one line, even when that line is not the line of the opening `(` — consistent with the RH5109 decision for parameter lists (#1736). Example `M(⏎    1, 2);` is not reported. The issue's "Done when": the answer is recorded in the RH5102 spec (change doc since visible output changes), and both shapes `M(⏎1, 2)` (not reported) and `M(1,⏎2)` have a covering test. Correction (spec author, 01): `M(1,⏎2)` is not reported, on `main` or after (each argument starts its own line); the reported counterpart is e.g. `M(⏎1, 2,⏎3)`.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 2 | Spec author | Change-doc entries 2–4 (`M(1, 2⏎)`, `M(1, 2 // c⏎)`, `M( // c⏎1, 2)` and `#pragma` variant) no longer reported: covered by answer A, or new output questions? | Covered by answer A; no question | Answer A is stated as "arguments all stand on one line" and "as decided for RH5109", whose decision covered these same shapes | The maintainer's answer covers them | afe2faf |
| 3 | Spec author | Accept change doc `sdd/changes/2026-11/rh5102-arguments-on-one-line.md` | Accept after the skeptic | Lead playbook section 4 | — | afe2faf |
| 4 | Skeptic | Issue showed option B's "before" wrongly (fix joins `M(⏎1, 2)`, does not split): re-ask the maintainer? | No; the chosen "after" output is unchanged, only the "before" description is corrected in the change doc | Answer A fixes the after-output; the corrected before-output does not alter it (skeptic 02) | Covered by the maintainer's answer | — |
| 5 | Skeptic | Change-doc "before" outputs and Why section wrong | Route corrections to spec author (continued agent) | Skeptic 02, must-corrections | Change-doc text is internal | — |
| 6 | Skeptic | Follow-up candidates: RH5102 reports `M(1, 2, /* a⏎b */ 3)` without fix; formatter drops space after `M( /* a⏎b */ 1, 2)` | Drop both | Only serve a rare input class (multi-line comment inside an argument list, which RH5038 reports) — lead.md, *Follow-ups* | Not taken in; nothing changes | — |
| 7 | Skeptic | Declare "comment/directive between `(` and first element" unsupported? | Not an option: deletes no special case and would refuse common code | Skeptic 02 | No declaration | — |
| 1 | Claim | Branch name | Use `claude/pensive-davinci-jjfo8i` instead of `claude/issue-1755-<slug>` | The session environment only permits pushing to that branch | Branch names are internal | c59c687 |

## Follow-up candidates

(none yet)

## State

- Last step: spec author (01, commits 854d92e, afe2faf).
- Last step: skeptic (02, f8430c5): proceed; change-doc corrections routed to spec author.
- Next: spec author amendment → accept change doc → test implementer (no output questions open).
