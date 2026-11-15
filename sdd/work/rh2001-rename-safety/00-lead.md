# Lead — rh2001-rename-safety

- Issue: thoenissen/Reihitsu#1757 — RH2001 fix rename may change code beyond the rename (SHARED-FIX-003). Labels: bug, analyzer, `agent-filed` (goals are proposals).
- Slug: `rh2001-rename-safety`
- Branch: `claude/squad-1757-pm9s4n` (session-designated branch; see decision 1). Draft PR: #1770.
- Lane: **behavior change** (widened from bug fix, decision 5) — the issue reports a suspected observed-vs-expected difference of an existing code fix against unchanged requirements (SHARED-FIX-003, RH2001-001, SHARED-CASING-009).
- Roles run: skeptic → test implementer (reproduction) → implementer → cleaner → spec checker → auditor → validator. Spec author: skipped unless a clarification is needed.
- Maintainer clarifications: none.

## Maintainer answers

- Output question 1 (RH2001, derived field name already used by an inherited member, fix today rewrites the read to `base._value`): **B** — withhold the fix; the diagnostic stays. The same answer covers the rare rewrite variants (extension method `_value` → `Extensions._value(this)`, static import → `Holder._value`, enclosing-type member → `Outer._value`): withhold.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name `claude/issue-1757-<slug>` vs. the session's designated branch | Use `claude/squad-1757-pm9s4n` | The session may push only to its designated branch | Branch naming is internal | claim commit |
| 2 | Skeptic | Issue's example as reproduction | Keep it as a characterization test; reproductions are the run-time-changing rows of 01-skeptic (f05, t4, e2, f08, f09, s01, s12, s06, t1, s13) | Example yields `this._value + _value`, which SHARED-CASING-009 allows | No output change; goal of an agent-filed issue adjusted | — |
| 3 | Skeptic | FC-015 architecture finding: reuse the casing fixes' private `RenameWithoutConflictAsync` vs. a narrow refusal in RH2001 | Fix narrowly in RH2001 with a cheap refusal (withhold when the derived field name already binds to another symbol); file unifying the rename-safety owner as a follow-up | Extraction from the 1485-line generic base is a structural change likely past ~100 lines/new type; the class is rare except f05/t4, which a cheap refusal also covers | Withholding for the run-time class is demanded by unchanged SHARED-FIX-003; the rewrite class waits on output question 1 | — |
| 4 | Skeptic | Follow-up for RH2001 Fix All on `Value` + `value` deriving one field name | Dropped | Rare input: the second name already breaks RH4111 (`AGENTS.md`, Code-fix coverage) | Nothing changes | — |
| 5 | Answer 1 | Lane | Widen bug fix → behavior change: spec author writes a change doc and the RH2001 spec statement for the withheld rewrite class | Answer B adds a visible output change that no unchanged requirement demands | Decided by the maintainer's answer B | — |
| 6 | Answer 1 | Re-run the skeptic on the spec diff | Skip; spec checker and auditor check the spec diff later | The spec diff states exactly the skeptic's own recommended option, now answered by the maintainer | Nothing visible beyond answer B | — |

## Follow-up candidates

- Unify the rename-safety decision of `CasingCodeFixProviderBase<T>.RenameWithoutConflictAsync` and RH2001's own guards into one owner (01-skeptic, Architecture findings, FC-015).

## Worklist / next step

- Skeptic done (01-skeptic.md, commit 994a2e2).
- Answer 1 = B. Next: spec author (02), then test implementer.
