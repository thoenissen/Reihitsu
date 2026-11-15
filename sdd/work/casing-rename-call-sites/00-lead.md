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
| 2 | Skeptic (01) | Take R2 (rename to an inherited member's name that silently rebinds member accesses; f06, f20, f22, f23, f26) into the run | Yes, withhold the fix for R2 | Same decision (does the rename change an existing reference's binding) and same method region as the issue's class; detection must cover it anyway | SHARED-FIX-003 (unchanged) allows only withholding | 4e56156 |
| 3 | Skeptic (01) | Issue goals | Replace the stale `select`/`add` examples with reproducing members (f03, f04, …); widen "rewrites a call site" to "changes code other than the rename cascade's declarations and references"; keep the ambiguity-path test (f12); drop the SHARED-FIX-003/006 conflict framing | Examples are withheld on main by SHARED-CASING-011; class reaches type refs, fields, nameof, enum initializers | Decided by the output question's answer; agent-filed goals | — |
| 4 | Skeptic (01) | Where to record the withheld class | Extend SHARED-CASING-009 (spec author clarification) | One requirement for "new name meets an existing binding" | Spec wording is internal | 9e7419d |
| 5 | Spec checker (06) | Finding 1: handover table row for f26 omits the SHARED-FIX-003 tag the test carries | No repair round; handovers are deleted before publishing and the test's tags are correct | The defect is only in a temporary handover | Handover text is internal | — |

| 6 | Auditor (07) | F1 (architecture finding): R1 guard's private `oldNames` set differs from the base's `GetOldNames` policy, letting rewritten references of other symbols through (c4, c6, c7, d1) | Change the structure in this run: the guard reuses the base's single old-name policy; tests for c4 and d1 (and c6/c7 as cheap variants) | The narrow fix would keep a second statement of the rule; stays within the one file the run changes | Brings the code to SHARED-CASING-009 as answered (withhold) | 8c7e443 |
| 7 | Auditor (07) | F2: interface re-implementation rebinding (a3) passes the R2 count | Refuse the class: withhold when the rename changes which member implements an interface member; test for a3 | SHARED-FIX-003 forbids the run-time change; rare class (a camelCase member that re-implements via hiding) | Withholding for a rare class under an unchanged requirement | 8c7e443 |
| 8 | Auditor (07) | F3: PR-introduced withholding when the renamer rewrites a cascade reference beyond its name (a1 `Extensions.ToList(values)`, a2 `box.Write((object)1)`) | Keep the withholding; spec author records the class in SHARED-CASING-009; tests for a1 and a2 | Rewriting the call's shape is an edit beyond the rename, in line with the maintainer's answer A; rare class (lower-case extension/overload colliding with an existing binding) | Withholding for a rare class; covered by answer A's intent | b58ee29 |
| 9 | Implementer (10) | Run's production diff is +114/−5 in one file (about 30 lines XML docs/comments), slightly over the ~100-line mark | No escalation 9: about 85 code lines, no new type, one file, every line serves the answered class | Within the spirit of the threshold; no new mechanism or type | Internal size judgment | 8c7e443 |
| 10 | Auditor (13) | F4 repeats F2's class (interface re-implementation via a derived type that lists the interface: b1, b2, b4, b5, b6) | Architecture finding: refuse the class wider instead of counting more precisely. Withhold the fix for a renamed method, property or event whenever its new name equals the name of a member of any interface implemented by its containing type or by any type derived from it. The spec's counter-statement ("does not list the interface → not withheld") is dropped; controls that asserted a fix there now assert none. Tests for b1, b2 at least | Repeated class → refuse, not more reasoning (README, Architecture findings); the class is rare (a wrongly cased member whose correct name meets an interface member in its hierarchy) | Withholding for a rare class; SHARED-FIX-003 unchanged | 11d3dd8 |
| 11 | Implementer (16) | Run's production diff grew to +138/−4 (one file, no new type, ~40 lines XML docs/comments); decision 10's part is +45/−20 | No escalation 9: the structural changes of the run (decisions 6 and 10) add far fewer than 100 lines; the rest is the bug fix's two guards; keep the interface-as-declaring-type branch (~4 lines) because the spec and two tests cover it | Escalation 9 is about structural changes; none here adds a type or ~100 lines | Internal size judgment | 11d3dd8 |
| 12 | Auditor (19) | F5 repeats F4/F2: the interface-name check covers only the reported symbol, not every cascade member (e1, e3, p1, p3, v1) | Architecture finding: refuse over the whole cascade — run the same name check for every method, property and event of the cascade; spec states it per cascade member; tests for e1, e3 and one property/event variant | Repeated class → refuse wider; keeps one region rule (the cascade) for every check of `RenameWithoutConflictAsync` | Withholding for a rare class; SHARED-FIX-003 unchanged | — |
| 13 | Auditor (19) | F6 (PR-introduced): local functions lose their fix under the interface-name check (l1, l2) | Repair: exclude local functions from the check; the spec names them as excluded; RH4104 test asserting the fix for l2 (and l1) | A local function cannot implement an interface member; restores `main`'s output | Restores main's output | — |
## Follow-up candidates

- RH2001 code fix (`RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs:100`) also calls `Renamer.RenameSymbolAsync`; unchecked whether it rewrites or rebinds references to other symbols (SHARED-FIX-003/006). Source: `04-implementer.md`.

## Worklist / next step

- Skeptic done: `01-skeptic.md` (verdict: output question; R1 reproduces with names outside SHARED-CASING-011; new R2).
- Maintainer answered A.
- Spec author done: `02-spec-author.md` (SHARED-CASING-009 extended, 5 examples; SHARED-CASING-012 Why adjusted).
- Test implementer done: `03-test-implementer.md` (reproduced; 40 tests, 36 failing on 9fc76b7, commit b66f38a).
- Implementer done: `04-implementer.md` (tests pass; +87/−2 in `CasingCodeFixProviderBase{T}.cs`, no new type; commit 4e56156).
- Cleaner done: `05-cleaner.md` (clean, no changes; Release 0 warnings).
- Spec checker done: `06-spec-checker.md` (1 minor handover-only finding, decision 5).
- Auditor 1 on 62b92c4 (base 6ab3d75): `07-auditor.md`, findings F1–F3 (decisions 6–8).
- Repair round 1 worklist: spec author (F3 class in SHARED-CASING-009) → test implementer (c4, c6, c7, d1, a3, a1, a2) → implementer (F1, F2) → cleaner → spec checker → auditor (repair only) → validator.
- Spec author round 1 done: `08-spec-author.md` (b58ee29).
- Test implementer round 1 done: `09-test-implementer.md` (c8650bb; 12 tests, 7 failing as intended). Decision 5 premise was a misreading; f26 row was already correct.
- Implementer round 1 done: `10-implementer.md` (8c7e443, e30fcd7; all Naming tests pass, Release 0 warnings).
- Cleaner round 1 done: `11-cleaner.md` (clean).
- Spec checker round 1 done: `12-spec-checker.md` (consistent).
- Auditor round 1 on df00d3b (base 3b3639e): `13-auditor.md` — #1–#3 closed; F4 repeats F2 → architecture finding, decision 10 (does not use up a repair round; repair rounds used: 1).
- Worklist: spec author (narrow SHARED-CASING-009 per decision 10) → test implementer (b1, b2, b4–b6; flip the not-re-listed controls) → implementer → cleaner → spec checker → auditor (delta since df00d3b) → validator.
- Spec author (decision 10) done: `14-spec-author.md` (141b218). Accepted its choices: name match ignores member kind; interface as declaring type included; fields excluded.
- Test implementer (decision 10) done: `15-test-implementer.md` (e48cb14; 9 failing as intended).
- Implementer (decision 10) done: `16-implementer.md` (11d3dd8, cd22e38; Naming 750 pass; analyzer 9598 pass; Release 0 warnings).
- Cleaner (decision 10) done: `17-cleaner.md` (clean; Naming 750/750).
- Spec checker (decision 10) done: `18-spec-checker.md` (consistent).
- Auditor on 07b4f2f: `19-auditor.md` — #4 closed; F5 (architecture, decision 12), F6 (repair round 2, decision 13). Repair rounds used: 2.
- Worklist: spec author (decisions 12, 13) → test implementer → implementer → cleaner → spec checker → auditor (delta since 07b4f2f) → validator.
- Spec author (decisions 12, 13) done: `20-spec-author.md` (8938f16).
- Next: test implementer.
