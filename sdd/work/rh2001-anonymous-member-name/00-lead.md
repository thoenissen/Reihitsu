# Lead — RH2001 anonymous-object member name

- Issue: thoenissen/Reihitsu#1990 — `agent-filed` (goals are proposals; the output decision below is the maintainer's and binding).
- Slug: `rh2001-anonymous-member-name`
- Branch: `claude/great-galileo-pf6zoo` (session-designated); draft PR thoenissen/Reihitsu#1997.
- Base: `origin/main` 13b0ed2.
- Lane: **behavior-change** — the run changes what the RH2001 code fix produces (a fix offered where it is withheld today), which needs a change doc.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager only if follow-up candidates exist.

## Maintainer clarifications (summarized)

The maintainer answered the issue's output question in an issue comment: option B. RH2001's fix is offered where a reference to the property gives an anonymous-object member its inferred name, and the member keeps its name explicitly (`new { Value }` → `new { Value = _value }`, property becomes `private int _value;`). The run also resolves the spec conflict that SHARED-CASING-015, read literally, covers RH2001. Tuple elements stay as RH2001-011 states them (issue text).

## Maintainer answers

1. Issue output question (A/B): **B** — given in the issue comment before the run.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | spec author | Fold the anonymous-member behavior into RH2001-011 or a new requirement? | New requirement RH2001-012; RH2001-011 keeps only tuples | RH2001-011's title is about withholding the fix | Spec structure only; output is the maintainer's answer B | 0070611 |
| 2 | spec author | Edit SHARED-CASING-015's scope sentence to name RH2001? | No; RH2001 lists SHARED-CASING-015 under Uses | Avoids an RH2001 row in the casing spec; the conflict is resolved by RH2001 following the scope sentence | Spec wording only | 0070611 |
| 3 | spec author | List `CasingAnonymousMemberUtilities` as a SHARED-CASING code owner? | Yes | It implements SHARED-CASING-015 and was unowned | Spec metadata only | 0070611 |
| 4 | skeptic | Architecture finding FC-015: RH2001's `GivesInferredMemberNameAsync` anonymous branch and `CasingAnonymousMemberUtilities.GetMemberNamedByReference` both decide which anonymous member takes its name from a reference | Change the structure in this run: RH2001 keeps no private copy of the anonymous-member decision and uses the casing helper; its tuple branch (RH2001-011) stays | The narrow fix would otherwise keep a second statement of the rule; within components the run changes | Same outputs as answer B | pending |
| 5 | skeptic | Reword RH2001-012 to defer to SHARED-CASING-015 for the written name? | Yes | One statement of the rule | Differs only for escaped/verbatim identifiers (rare) | 204f576 |
| 6 | skeptic | Issue's "Done when": Fix All test over the issue's input | Adjusted: single-fix test for the input plus Fix All over two properties (skeptic rows 17, 18) | The input has one diagnostic; Fix All needs two | Tests only | c808bf1 |
| 7 | skeptic | Fix All on `new { Value.Name }` (row 18) may lose an edit in BatchFixer | Test implementer pins the RH2001-012 result with Fix All; decide on failure | Unsettled by reading | Expected output follows RH2001-012 | see 8 |
| 8 | implementer | BatchFixer drops one of two RH2001 fixes (row 18 and the inherited Fix All scenario); options A sequential solution-scoped Fix All, B DocumentOrderFixAllProvider (loses partial-part renames), C two iterations + SHARED-FIX-002 exception | A. Prefer an opt-in solution-scoped mode of the existing `DocumentOrderFixAllProvider` (no new type, other users unchanged); else a private provider in RH2001 like the casing one. Over ~100 production lines: stop and report (escalation 9) | SHARED-FIX-002 (unchanged) demands one Fix All pass; B breaks partial parts; C needs a visible exception | Brings RH2001 Fix All to an unchanged requirement | 31c86f4 |
| 9 | implementer | Adjacent fix: `CasingAnonymousMemberUtilities.WriteMemberName` drops the elastic marker on `=` | Take it in (1 line, component the run changes, pinned by the RH2001 own-initializer test) | Without it the RH2001 result breaks layout | Casing tests unchanged and green | a13a255 |
| 10 | implementer | Solution mode re-analyzes a document an earlier fix changed and uses every diagnostic there, not only those offered on the starting code | Accept without guard | Only reached by interacting fixes in two parts of one partial type: rare class | Rare class; no output in question |  31c86f4 |
| 11 | auditor 1 | Repair round 1 worklist | Findings 1 (cross-document re-analysis branch untested) and 2 (duplicated `WithEndOfLine` helper) → test implementer; hints not routed | Both PR-introduced | Tests only | 0f63e34 |
| 12 | auditor 1 | Fix All action title now `RH2001Title` (as the casing fixes) | Keep | Matches the casing fixes; the IDE presents Fix All by scope | Same title as the single fix, like every casing fix | 31c86f4 |

## Follow-up candidates

- A casing-fix test pinning the layout of a multi-line anonymous creation whose member name the fix writes (04-implementer.md).
- Two owners compose Fix All sequentially across documents: `CasingCodeFixProviderBase<T>.CasingFixAllProvider` and the solution mode of `DocumentOrderFixAllProvider` (FC-015, 07-auditor.md).

## Audited SHA

- Audit 1: ae16c90 vs main 13b0ed2 — findings (repair round 1).

## Open worklist / next step

- Spec author done (01-spec-author.md, 0070611): no output questions.
- Skeptic done (02-skeptic.md, 6387aa5): proceed, no output questions.
- Spec author amendment done (204f576); decision 5 carried out there; change doc Accepted.
- Test implementer done (03-test-implementer.md, c808bf1): 23 red tests, intended.
- Implementer round 1 (04-implementer.md, a13a255/b84af93): blocked on Fix All merge; decided 8 = A.
- Implementer round 2 (31c86f4/83013bb): all tests green; opt-in solution mode of DocumentOrderFixAllProvider.
- Cleaner done (8f20d37): clean.
- Spec checker done (19371ac): consistent.
- Auditor 1 (4a7f19b): 2 findings.
- Repair 1 test implementer done (0f63e34).
- Repair 1 cleaner done (a16d4e5): clean.
- Repair 1 spec checker done: consistent.
- Next: auditor (repair only).
