Role: Lead
Commit: f52df45
Verdict: lane claimed — tests

# Run: thoenissen/Reihitsu#1380

- Issue: thoenissen/Reihitsu#1380 — record decided principles P32–P37 and the P12 clarification (question round 9 of thoenissen/Reihitsu#912) in the specs.
- Slug: `principles-p32-p37`
- Branch: `claude/principles-p32-p37-p12-p7beal` (session-assigned), draft PR thoenissen/Reihitsu#1405, base `main`.
- Lane: **Specs and tests for existing behavior** (`tests`). Reason: the issue says docs/spec, but `formatter/` and `shared/` specs are enforced (thoenissen/Reihitsu#939), so every new requirement needs covering tests in the same PR. No production code changes, no change doc.
- Roles run: spec author → skeptic → test implementer → cleaner → spec checker → validator.
- Roles skipped: implementer (no production change in this lane), auditor (not part of the tests lane).

## Maintainer's clarifications (summarized)

1. Write P32–P37 and the P12 clarification where the issue body places them:
   - P32 → `formatter/pipeline.md` (next to FMT-PIPE-002), referenced from FMT-SWITCH-007, FMT-STRUCT-002, FMT-STRUCT-006.
   - P33 → `shared/comment-placement.md` (generalizing SHARED-BLANK-002).
   - P34 → `formatter/pipeline.md` (or the formatter specs' common section).
   - P35 → amends SHARED-FIX-008 in `shared/code-fixes.md`; no new FIX ID; touch nothing else in that file (thoenissen/Reihitsu#1331 edits FIX-003 and FIX-007 in parallel).
   - P36 → `formatter/structural-transforms.md` (next to FMT-PIPE-003).
   - P37 → `shared/statement-blank-lines.md` (next to SHARED-STMT-004) and `formatter/blank-lines.md` (next to FMT-BLANK-006).
   - P12 clarification → amends SHARED-COMMENT-011 in `shared/comment-placement.md`.
2. ID plan: mint new FMT-PIPE / FMT-STRUCT / FMT-BLANK / SHARED-COMMENT / SHARED-STMT IDs as needed and register them in `sdd/specs/requirement-ids.txt`. Never mint SHARED-DIAG IDs (SHARED-DIAG-009/010 belong to thoenissen/Reihitsu#1331).
3. The texts are decided — write them as the issue states, do not reopen them.
4. Where a principle states behavior the code does not yet meet (P32, P33, P34, P36, P37), the requirement still states the principle; its covering tests pin only shapes that already conform. The non-conforming shapes are tracked by thoenissen/Reihitsu#1381–#1385 — they are not open questions.
5. Existing tests that pin today's contradicting behavior (e.g. the two formatter tests P33 names) stay unchanged and are not tagged with the new requirement. If such a test is already tagged with a requirement whose amended wording it now contradicts, stop and report — the lead escalates before anything changes.
6. If a new requirement has no conforming shape to test at all, stop and report — the lead escalates before it is written.
7. Update the `Uses` lines of affected specs.
8. Must not: change production code or behavior; fix a bug; write an open question into a spec; retire, move or renumber an existing requirement ID; write P25–P31 (thoenissen/Reihitsu#1331); edit AGENTS.md.
9. A further instance of an existing follow-up issue is reported in the handover (the lead comments on that issue). A new instance of a decided principle is a follow-up candidate filed in the spec's open-questions issue, marked "Decided by principle Pn (#912)".
10. Follow-ups named on the issue: #1381 (P32), #1382 (P33), #1383 (P34), #1384 (P36), #1385 (P37), #1377 (SHARED-ATTR-008/-017 may cite SHARED-FIX-008), #1147 (P12 test note), #963.

## Escalations

- Escalation 1 (P37 shared side without a conforming shape): answered (b) — P37 is recorded only as FMT-BLANK-016; no SHARED-STMT requirement is written in this run; thoenissen/Reihitsu#1385 adds it. Recorded in `squad/decisions/2026-10-22-1380-p37-shared-side-deferred.md`.
- Lead commented further instances on thoenissen/Reihitsu#1147 (RH5110 example), #1382 (RH5203 fix without progress), and the deferral on #1385.

## Lane widened

`scripts/lane-check.sh tests --head worktree` → `LANE CHECK: mismatch — claimed tests, diff needs tooling`, caused only by the lead's decisions entry under `squad/decisions/` (known classification, thoenissen/Reihitsu#1054). Widened before any implementer stage: tests → tooling, as in thoenissen/Reihitsu#1035 and #1036. Roles now: spec author → skeptic → test implementer → cleaner → spec checker → auditor → validator. Implementer: skipped — no production or tooling code to change. The run remains a specs-and-tests run in substance: no production code, no change doc.
- Escalation 3, Q1 (skeptic): SHARED-COMMENT-011 against SHARED-CHAIN-004 — answered (b): a rule's own comment exemption in that gap still applies; SHARED-COMMENT-011 says so. `squad/decisions/2026-10-23-1380-p12-chain-exemption.md`.
- Escalation 3, Q2 (skeptic): P34 scope — answered (b): P34 governs the members its sources named within the layout each construct's own requirement chooses; FMT-PIPE-014 keeps the per-construct decisions (FMT-BREAK-014, -021, -022, -026, the switch-arm non-goal). `squad/decisions/2026-10-23-1380-p34-family-scope.md`.
