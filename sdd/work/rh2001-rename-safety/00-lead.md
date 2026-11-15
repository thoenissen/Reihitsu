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
| 3 | Skeptic | FC-015 architecture finding: reuse the casing fixes' private `RenameWithoutConflictAsync` vs. a narrow refusal in RH2001 | Fix narrowly in RH2001 with a cheap refusal as RH2001-011 states it (see decision 7); file unifying the rename-safety owner as a follow-up | Extraction from the 1485-line generic base is a structural change likely past ~100 lines/new type; the class is rare except f05/t4, which a cheap refusal also covers | Withholding for the run-time class is demanded by unchanged SHARED-FIX-003; the rewrite class waits on output question 1 | — |
| 4 | Skeptic | Follow-up for RH2001 Fix All on `Value` + `value` deriving one field name | Dropped | Rare input: the second name already breaks RH4111 (`AGENTS.md`, Code-fix coverage) | Nothing changes | — |
| 5 | Answer 1 | Lane | Widen bug fix → behavior change: spec author writes a change doc and the RH2001 spec statement for the withheld rewrite class | Answer B adds a visible output change that no unchanged requirement demands | Decided by the maintainer's answer B | — |
| 6 | Answer 1 | Re-run the skeptic on the spec diff | Skip; spec checker and auditor check the spec diff later | The spec diff states exactly the skeptic's own recommended option, now answered by the maintainer | Nothing visible beyond answer B | — |
| 7 | Spec author | Spec shape | Accept change doc `sdd/changes/2026-11/rh2001-rename-binding.md` and new RH2001-011 (only SHARED-CASING-009's binding paragraphs; `this.`-qualified inner-scope cases f01, f03, f04, s02, s05, t3, s10 stay offered, as on `main`) | Matches answer B without widening into interface-member or inner-scope refusals nobody was asked about; if a cheap check cannot keep the inner-scope cases offered, that is escalated, not decided in code | Keeps `main` output for every case outside answer B and SHARED-FIX-003 | 454da9a |
| 8 | Implementer | 5 disputed tests (f09, s01, s12, f09 in another part, f07): on the test inputs `main`'s fix keeps every binding, contrary to 01-skeptic's table | Route to a fresh test implementer: confirm on `main` with `scripts/apply-fix.sh`; if an input of the same class that misbinds or rewrites exists, use it; otherwise turn the test into a characterization of `main`'s offered fix and report the spec/change-doc text to correct | Whether the fix is offered on these inputs is decided by unchanged SHARED-FIX-003 and answer B on the real output; a correct fix on `main` stays offered | Either outcome keeps `main`'s output or follows answer B | — |
| 9 | Implementer | FC-015 extended: RH2001 gets its own binding comparison (~134 net lines, one file, no new type) | Accept under decision 3; follow-up already listed | Reuse of casing helpers falsely refused a correct case and missed a partial-document rebinding | Internal structure | 37e364a |
| 10 | Test implementer (05) | Outcome of decision 8 | Accept 29d3add: withheld tests now use the misbinding shapes (lambda in a block body; delegate-typed property for the extension rewrite); the original inputs become offered-fix characterizations matching `main` | Measured on `main` 962d0c4 | Offered cases keep `main` output; withheld cases follow SHARED-FIX-003 / answer B | 29d3add |
| 11 | Test implementer (05) | Untested block-body siblings (local declaration, assignment, constructor, explicitly typed lambda in a block, query range variable) | Add tests (continue the 05 agent once) | FC-016 sibling shapes; cheap | Withheld per SHARED-FIX-003 | — |
| 12 | Test implementer (05) | Spec/change-doc text no longer matches the measured class | Route corrections to the spec author (continue once) | Text must match tests and `main` measurements | Spec wording is internal; outputs unchanged from decision 10 | — |

## Follow-up candidates

- Unify the rename-safety decision of `CasingCodeFixProviderBase<T>.RenameWithoutConflictAsync` and RH2001's own guards into one owner (01-skeptic, Architecture findings, FC-015; 04-implementer adds: `RewritesOtherCodeAsync` falsely refuses `Value + _value + other` → `this._value + _value + other` through tree-diff misalignment).

## Worklist / next step

- Skeptic done (01-skeptic.md, commit 994a2e2).
- Answer 1 = B. Spec author done (02, 454da9a). Test implementer done (03, f95f956): reproduced, 19 red. Implementer done (04, 37e364a): 5 disputed tests. Test implementer 05 done (29d3add), 207/207 RH2001 green. Next: in parallel, spec author correction (06) and test implementer sibling tests (07); then cleaner.
