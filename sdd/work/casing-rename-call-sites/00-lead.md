# Lead — casing-rename-call-sites

- Issue: thoenissen/Reihitsu#1676 — casing fixes rewrite call sites to keep their binding when renaming (labels: bug, analyzer, `agent-filed`; goals are proposals).
- Slug: `casing-rename-call-sites`
- Branch: `claude/adoring-hawking-l4994s` (the session's designated branch; see Decision 1)
- PR: thoenissen/Reihitsu#1751 (draft claim)
- Lane: **bug fix** — the issue reports an observed difference between the casing fixes and an unchanged requirement (SHARED-FIX-006). Option B of the issue would instead add a spec exception; if the maintainer picks it, the lane is re-evaluated.
- Roles: skeptic → (output question) → test implementer → implementer → cleaner → spec checker → auditor → validator. Spec author: only for a clarification (e.g. recording the withheld class in the casing spec).

## Maintainer clarifications

None yet. The issue itself carries an output question (withhold the casing fix when the rename would capture a call, or keep rewriting call sites); it is asked at the batch point before the test implementer.

## Maintainer answers

- Output question 1 (RH4103, rename that would capture an existing reference to another symbol, e.g. `toList` → `values.ToList()` rewritten to `Enumerable.ToList(values)`): **A** — withhold the fix; the diagnostic stays. Applies to every casing rule. No casing fix may change code other than the rename cascade's declarations and references.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch `claude/adoring-hawking-l4994s` instead of `claude/issue-1676-<slug>` | The session environment permits pushes only to that branch | Branch naming is internal | bb8fc78 |
| 2 | Skeptic (01) | Take R2 (rename to an inherited member's name that silently rebinds member accesses; f06, f20, f22, f23, f26) into the run | Yes, withhold the fix for R2 | Same decision (does the rename change an existing reference's binding) and same method region as the issue's class; detection must cover it anyway | SHARED-FIX-003 (unchanged) allows only withholding | — |
| 3 | Skeptic (01) | Issue goals | Replace the stale `select`/`add` examples with reproducing members (f03, f04, …); widen "rewrites a call site" to "changes code other than the rename cascade's declarations and references"; keep the ambiguity-path test (f12); drop the SHARED-FIX-003/006 conflict framing | Examples are withheld on main by SHARED-CASING-011; class reaches type refs, fields, nameof, enum initializers | Decided by the output question's answer; agent-filed goals | — |
| 4 | Skeptic (01) | Where to record the withheld class | Extend SHARED-CASING-009 (spec author clarification) | One requirement for "new name meets an existing binding" | Spec wording is internal | — |

## Follow-up candidates

(none yet)

## Worklist / next step

- Skeptic done: `01-skeptic.md` (verdict: output question; R1 reproduces with names outside SHARED-CASING-011; new R2).
- Maintainer answered A.
- Next after answer: spec author (clarification of SHARED-CASING-009) → test implementer.
