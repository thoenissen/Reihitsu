# Lead — rename-check-one-owner

- Issue: thoenissen/Reihitsu#1771 — `agent-filed` (goals are proposals). Milestone 1.6.0, labels analyzer, maintenance.
- Branch: `claude/elegant-einstein-l9nk31` (session-designated). Draft PR: #1969.
- Slug: `rename-check-one-owner`
- Lane: `bug-fix` — the issue reports two components (the casing code fixes' generic base and the RH2001 code fix) deciding the same rename-safety policy (SHARED-CASING-009 / RH2001-011) differently, with concrete inputs where their verdicts disagree; an architecture finding against an existing spec. Widens to `behavior-change` if a change doc turns out to be required.
- Roles: skeptic (premise, reachability, architecture) → spec author only if the spec needs a clarification/ownership change → test implementer → implementer → cleaner → spec checker → auditor → validator. Issue manager before publishing if follow-up candidates exist.
- Maintainer clarifications: none.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | skeptic | Build the shared rename-safety component the issue proposes? | No — drop the goal | Binding comparison alone does not cover SHARED-CASING-009 (cast-argument and static-call rewrites keep every binding); RH2001-011 deliberately differs on inferred tuple names; component would exceed escalation-9 size for a rare class | Dropping an internal structure changes no output | ed3820a |
| 2 | skeptic | Architecture finding (two owners of the binding check) | Fix narrowly in `RewritesOtherCodeAsync` so the casing fix keeps the fix where the renamer only renamed and qualified, per unchanged SHARED-CASING-009; if no cheap check achieves it without un-withholding tested rewrite cases, keep the refusal and end with `Refs` | Rare class (SHARED-CASING-009 already decides it); narrowing is worth it only while cheap (AGENTS.md, Code-fix coverage) | Brings code back to an unchanged requirement | — |
| 3 | skeptic | Input 1 | Already agrees on main; covered by existing tests, no work | Skeptic reproduced it with apply-fix | No change | — |
| 4 | skeptic | Follow-up for shared ownership of the binding comparison | Not filed | Only serves a rare class; spec keeps the remaining checks separate by design | No output | — |
| 5 | auditor (round 1) | Finding #1: token-kind qualifier list withholds fixes `main` offers (`Base<int[]>`, `int[,]`, `int?` type qualifiers) — FC-044, the guard predicts the renamer's qualifier | Architecture finding decided as a structural repair within the changed component: the qualifier is read from the renamed tree's syntax (the left side of the member access / qualified name / alias-qualified name / cref whose name is the renamed identifier), replacing the token-kind list; regression tests first. Repair round 1 | Refusing would keep a PR-introduced regression against `main`; reading the renamer's syntax removes the prediction instead of enumerating more token kinds | Restores `main`'s offered fixes and SHARED-CASING-009, unchanged | — |

## Follow-up candidates

(none yet)

## Worklist / next step

- Claimed (empty commit pushed, draft PR #1969), toolchain prepared.
- Skeptic done (`01-skeptic.md`, proceed narrower; no output questions, no change doc, no escalation 10).
- Test implementer done (`02-test-implementer.md`, reproduced: 14 failing, 10 holding; no follow-ups).
- Implementer done (`03-implementer.md`, tests pass; token-by-token comparison replaces GetChanges in `RewritesOtherCodeAsync`; stricter for non-qualifier inserted tokens and nullable/tuple type-argument qualifiers, unreached by tests — auditor to judge).
- Cleaner done (`04-cleaner.md`, clean).
- Spec checker done (`05-spec-checker.md`, consistent).
- Auditor round 0 (`06-auditor.md`, audited head 123929e, base 2c77022): findings — #1 high. Repair round 1 open.
- Round 1 test implementer done (`07-test-implementer.md`, 11 failing rows, 4 holding).
- Next: implementer (`08-implementer.md`), cleaner, spec checker, auditor on the repair.
