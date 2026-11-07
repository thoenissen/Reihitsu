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
| 3 | Lane | Spec author in bug-fix lane | Run it | Each issue needs maintainer-decided requirements recorded (clarifications) | Decided by maintainer answers quoted in the issues | fd5393f |
| 4 | Skeptic | #967 case 1 premise | Narrow to declarations with attribute lists on their own line (incl. comment/directive lines between attributes and declaration, both RH2004 and RH2005); the inserted keyword takes over the first token's leading trivia. The `#pragma` → unparsable output is a safety rail repaired in scope | `static public` without attributes is already correct; RH2004-006/RH2005-003 ("column of the first token") decide the output | An unchanged requirement decides the output | — |
| 5 | Skeptic | Architecture finding: excluded-`#if` check private in `CasingCodeFixProviderBase<T>.HasExcludedCodeOccurrenceAsync` | Change the structure in this run: share the check between the casing fixes and the RH2001 fix instead of copying it | A narrow fix would add a second copy (FC-015); both components are changed by the run or are its counterpart | Internal structure; casing-fix outputs unchanged | — |
| 6 | Skeptic | RH2005-004 wording | "any field the declaration declares" | Fix changes every declarator at once; only one safe reading | Wording only | — |
| 7 | Skeptic | RH2001-007 scope | Widen to comments or directives between the name and the end of the declaration, except inside the initializer | SHARED-FIX-003 already withholds these; one place states the class | Both readings withhold the fix | — |
| 8 | Skeptic | #1287 goal "RH2004-002 and its tests are updated" | Replace by RH2004-001 coverage and the characterization test near `RH2004AccessModifierMustBeDeclaredAnalyzerTests.cs:881` | RH2004-002 covers partial types/methods, which stay unreported | Goal wording only | — |
| 9 | Skeptic | Spec-author follow-up candidates 1 and 2 | Collapse into one spec-wording follow-up for SHARED-DIAG-004; candidate 2 folds into Q1's answer | SHARED-NESTED-002 already decides candidate 1 | Wording follow-up only | — |

## Follow-up candidates

1. SHARED-DIAG-004 wording: exclusion list should name SHARED-NESTED-002's per-part reports; second paragraph should not seem to cover RH8306 summaries (replaces spec-author candidates 1+2) — 02-skeptic.md.

## Pending output questions (batch before test implementer)

- Q1 (blocks #1287 rest): location of the single diagnostic of a partial member (RH2002, RH2004, RH8306, RH7401) — A first reporting part in document order (rec.), B first part, C implementing part.
- Q2: RH2001 mutable-struct boundary — re-asked with target-framework consequence; options A, B, C, C′ (rec. C′).
- Q3: RH2005 references outside the type body that compile after the fix (nameof in own attribute, cref) — A offer fix (rec.), B withhold.
- ASKED in chat; waiting.

## State

- Audited SHA: —
- Last step: spec author 01 (fd5393f).
- Last step: skeptic 02 (7e38ec4). Output questions Q1–Q3 asked.
- Next: on answers → spec author (amend per answers + decisions 4–8), then test implementer.
