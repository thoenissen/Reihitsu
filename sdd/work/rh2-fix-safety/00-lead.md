# Lead — rh2-fix-safety

- Issues (prompt order): thoenissen/Reihitsu#967, #1286, #1287, #1288 — all `agent-filed` (goals are proposals).
- Branch: `claude/eager-ride-45bd9o` (designated by the session; replaces the playbook's `claude/batch-967-...` name). Draft PR: thoenissen/Reihitsu#1609.
- Slug: `rh2-fix-safety`. Lane: **bug fix**.

## Lane reasons

- #967 — RH2005 fix: observed wrong output (`static public` loses indentation; also RH2004 `[Obsolete]` + no-modifier instance in the comment) and a fix offered that breaks the build (references outside the type). Maintainer decided both as bugs (in #930). Bug fix.
- #1286 — RH2001 fix: observed unsafe/invalid output for four inputs (comments dropped; inactive `#if` references; mutable struct property semantics; `readonly readonly`). Decided by P1 / SHARED-FIX-001/-003. Bug fix. A comment adds facts: any inactive branch and inactive declaration of the new name behave the same; the casing fixes' shared check `CasingCodeFixProviderBase<T>.HasExcludedCodeOccurrenceAsync` exists.
- #1287 — partial declarations judged as one declaration (P15, SHARED-DIAG-004, recorded by #1260, closed): RH2002, RH2004, RH2101, RH7002, RH7401–RH7412, RH8306; RH2004 fix on one part of a partial withheld. Triage comment: bug against SHARED-DIAG-004, bug-fix lane, no change doc; the rule's coverage sentence (RH2004-002, which reports both parts) is widened as a clarification. RH7104/RH7105/RH7106 are explicitly NOT in scope (P28).
- #1288 — RH2004 on explicit interface operators/conversions: maintainer round 7b chose option A — not reported (RH2004-003 exemption extends to operators and conversions); the fix-withholding (Q3) is then moot. Ordinary-operator `public` part moved to #1435 (not in this run). Bug fix.

## Roles

Bug-fix lane: spec author (clarifications only: the maintainer-decided requirements for RH2005, RH2001, RH2004, partial-declaration coverage sentences) → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. Issue manager before publishing if follow-up candidates exist.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use session-designated `claude/eager-ride-45bd9o` | The session environment requires pushing only to this branch | Branch names are internal | 560af96 |
| 2 | Lane | Change doc for #1287 despite its body's "behavior change (change doc)" | No change doc; bug-fix lane | Later triage comment on #1287 classifies it as a bug against SHARED-DIAG-004 with a coverage clarification | Behavior is fixed by SHARED-DIAG-004 and maintainer decisions P15 | — |
| 3 | Lane | Spec author in bug-fix lane | Run it | Each issue needs maintainer-decided requirements recorded (clarifications) | Decided by maintainer answers quoted in the issues | — |

## Follow-up candidates

(none yet)

## State

- Audited SHA: —
- Last step: claim, prepare.sh OK.
- Next: spec author (01).
