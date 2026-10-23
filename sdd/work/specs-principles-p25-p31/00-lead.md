Role: Lead
Commit: f52df45
Verdict: lane claimed

# Run: thoenissen/Reihitsu#1331

- Issue: thoenissen/Reihitsu#1331 — record principles P25–P31 and the P18/P22 clarifications in the shared specs. Read the issue body and both of its comments.
- Slug: `specs-principles-p25-p31`
- Branch: `claude/specs-principles-p25-p31-x5tof1` (session-designated), draft PR thoenissen/Reihitsu#1404, base `main`.
- Lane: **Specs and tests for existing behavior** (`tests`). Reason: the run writes requirements into enforced shared specs; each new or amended requirement needs covering tagged tests in the same PR. The one `sdd/README.md` sentence is docs text, which the tests lane admits.
- Roles run: spec author → skeptic → test implementer → cleaner → spec checker → validator.
- Roles skipped: implementer (no production code may change), auditor (the tests lane does not run it).

## Maintainer clarifications (summarized)

- The principle texts are decided in thoenissen/Reihitsu#912; write them as stated in the issue, do not reopen them.
- Build on what the merged blockers wrote: thoenissen/Reihitsu#1359 (P15–P22: SHARED-DIAG-008, SHARED-USING-007), thoenissen/Reihitsu#1357 (SHARED-FIX-003..008), thoenissen/Reihitsu#1360 (P11 section in `sdd/README.md`). For P18 and P22, check whether the clarifications are already present and write only what is missing.
- ID plan: only two new IDs — SHARED-DIAG-009 (P25) and SHARED-DIAG-010 (P31, placed in `shared/diagnostics.md` as the mirror of P11). Everything else amends existing requirements: SHARED-DIAG-004 (P28), SHARED-FIX-007 narrowed for moved code (P26), SHARED-ORDER-004 (P27), SHARED-DOC-004 (P29), SHARED-DOC-008 (P30). If an amendment does not fit, report it in the handover instead of minting another ID — the lead escalates.
- Wording-only reconciliation: SHARED-FIX-003's "reports the construct it names for every input" is narrowed so it says a missing fix never removes the report, pointing at SHARED-DIAG-008, SHARED-JOIN-001 and P31 (SHARED-DIAG-010) for the inputs that are not reported (issue comment 2, item 1).
- `sdd/README.md`: narrow the closing sentence of the P11 section — a rule that does not report a shape the formatter deliberately keeps is P31, not a P11 gap (issue comment 2, item 2; PR thoenissen/Reihitsu#1360 review notes).
- Where a principle states behavior the code does not yet meet, the requirement still states the principle; its covering tests pin only shapes that already conform. Non-conforming shapes are tracked by the follow-ups named in the issue comment (thoenissen/Reihitsu#1334, #1335, #1337, #1339, #1345, #1346, #1347, and the notes on #1266/#1287/#1338) — no open question, and no characterization test that contradicts the requirement. A requirement with no conforming shape to test at all is reported in the handover; the lead escalates before it is written.
- Update the `Uses` lines of the affected rule specs. Register new IDs in `sdd/specs/requirement-ids.txt`.
- Concurrent runs: thoenissen/Reihitsu#1380 edits SHARED-FIX-008 in `shared/code-fixes.md` and several formatter specs — touch only FIX-003 and FIX-007 in that file. Do not write P32–P37. Do not edit `AGENTS.md`.
- Must not: change production code or behavior, fix a bug, write an open question into a spec, retire/move/renumber an existing ID. A new instance of a principle found in passing goes to the spec's "SDD open questions" issue marked "Decided by principle Pn (#912)" — list it in the handover; the lead files it.

## Escalation answers after the skeptic (summarized)

- P26 reach (`squad/decisions/2026-10-22-1331-p26-reach.md`): SHARED-FIX-007's moved-code rule covers statements a fix wraps into a new block, including the brace fixes RH5405–RH5407 (SHARED-BRACE-002 is a P26 instance; the lead files it). It does not cover the expression-body-to-block conversion of RH3202/RH3203 (the expression becomes a new return statement, the replaced node of P10) or the using-block organizers RH7201–RH7207 (their group separators are the rules' subject). SHARED-FIX-007 states this scope.
- P30 reach (`squad/decisions/2026-10-22-1331-p30-reach.md`): RH8301–RH8303 stay gated. The gate covers every rule about documentation comments; only rules about ordinary comments or file text (RH8401, RH8402, RH8501) check in every documentation mode. SHARED-DOC-008 is worded accordingly.
- Spec examples that show decided but not yet implemented behavior stay, by the precedent `squad/decisions/2026-10-21-1260-razor-content-no-conforming-shape.md`; covering tests pin conforming shapes only.
- Existing untagged tests that pin the opposite of a new requirement keep their assertions; by the precedent `squad/decisions/2026-10-21-1260-contradicting-characterization-tests.md`, their summaries are reworded (comment-only) to say they pin current behavior and the decided behavior is tracked by the named follow-up issue.

## Lane widened

- The escalation answers are recorded under `squad/decisions/`, a tooling path, so the lane check reported a mismatch. Before any implementer started, the lane is widened tests → **tooling** (precedent: thoenissen/Reihitsu#1359). The only tooling paths are the decision entries.
- The widened lane adds the implementer and the auditor. The auditor runs. The implementer is skipped: the run carries no production or tooling code.

## Filed by the lead

- thoenissen/Reihitsu#1407 — `SDD open questions: sdd/specs/shared/element-documentation.md`, Q1: RH8305/RH8307 fixes move a behind-code `///` comment (decided by P29).
- thoenissen/Reihitsu#1408 — `SDD open questions: sdd/specs/shared/code-fixes.md`, Q1: the brace fixes RH5405–RH5407 reformat the wrapped statement (decided by P26).

## Second repair round (escalation 7, `squad/decisions/2026-10-23-1331-second-repair-round.md`)

The maintainer approved a second repair round on all seven auditor findings in `11-auditor.md`. Routing: findings 1, 2, 3, 5, 6 to the spec author; findings 4 and 7 to the test implementer. Afterwards: cleaner, spec checker, auditor (repair only), validator.

## Third repair round (escalation 7, `squad/decisions/2026-10-23-1331-third-repair-round.md`)

The maintainer approved a third repair round on findings 8 and 9 of `16-auditor.md`, both routed to the spec author. The lead files the not-covered SHARED-DOC-004 shapes (documentation mode `None`; a directive between comment and declaration) as a question on thoenissen/Reihitsu#1407. Afterwards: test implementer if tags or tests are affected, cleaner, spec checker, auditor (repair only), validator.
