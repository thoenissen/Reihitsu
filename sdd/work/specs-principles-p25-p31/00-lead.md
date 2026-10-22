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
