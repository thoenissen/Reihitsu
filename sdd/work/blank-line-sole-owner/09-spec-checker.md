Role: spec checker
Commit: bdff4c7 (checked tree; this handover is committed on top)
Verdict: findings

The prompt carried no intended fix or hoped-for verdict; none was used.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 1, 6 | `sdd/specs/formatter/blank-lines.md` FMT-BLANK-009 (the "MUST NOT insert a blank line in front of the first non-region directive ... when a region directive ends the gap" sentence, whose gap covers brace and, via FMT-BLANK-019, `]` and `)`); `sdd/changes/2026-11/blank-line-sole-owner.md` visible output change 11 and its `Decisions` entry | The maintainer's answer (00-lead.md, implementer-stage question) names a closing brace only. The spec sentence is written for "a gap before a closing brace or bracket", so it also decides the collection-expression `]` case (and `)`), which is the open output question in 00-lead.md Decisions taken row 16. The change doc is `Accepted` and says "Decided with the output question asked during the implementer stage", while the `]` extension has no recorded answer. Not a defect in routing (row 16 keeps it open for the pre-auditor batch, and no test pins it), but the change doc may not stay `Accepted` with an unanswered visible change: either the lead's batch records the answer under `Maintainer answers` before the PR is marked ready, or the spec and change doc are narrowed to the brace until it is. | spec author (after the lead's batch) |

No other findings.

## Checked
1. Change doc: required (FMT-BLANK-008/009/011 changed, FMT-BLANK-017..020, FMT-BREAK-030, RH5025-005 added, FMT-PIPE-024 changed); present, no task list, has `Visible output changes` (11 entries), `Status: Accepted`, names the superseded #1396 and #1081 item 1. Each of the 11 entries has its answer in `Decisions`, except the `]` extension (finding 1).
2. Specs match the diff: FMT-BLANK-008/009/011/017/018/019/020, FMT-BREAK-030, FMT-PIPE-024, RH5025-005 each carry the normative sentence, `Why:`, and examples (RH5025-005 uses `{|RH5025:...|}` markup); the FMT-BLANK-008 text carries the new exemption; the FMT-BREAK-030 non-goal wording is updated. Otherwise ok, except finding 1.
3. IDs: FMT-BLANK-017..020, FMT-BREAK-030, RH5025-005 are the next free numbers, inserted at their sorted positions in `requirement-ids.txt`; nothing reused or removed.
4. Code owners: `BraceBlankLinePolicy` added to the FMT-BLANK header and to `sdd/specs/README.md` (rows match). `BlankLineSpacingPolicy.IsBlockStatementAfterStatement` stays under SHARED-BLANK (lead row 15). `scripts/test.sh --project architecture` (includes SpecConsistencyTests): `Passed!  - Failed: 0, Passed: 92, Skipped: 0, Total: 92`.
5. User documentation: no `documentation/` file changed; the RH5025 page stays unchanged by decision 5 (the exemption is a corner case, per AGENTS.md); `RH5025.md` exists; ID range fits. RH5030 and RH5020 pages need no change (their behavior is unchanged).
6. Open questions: the seven earlier output questions and the implementer-stage D3 question are answered under `Maintainer answers`; the `]` case is open in row 16 (finding 1); follow-up candidates are listed under `Follow-up candidates`; no question is written into the spec as open or recorded only in the PR.
7. Coverage: architecture result above; every new active requirement (FMT-BLANK-017, -018, -019, -020, FMT-BREAK-030, RH5025-005) has `[Requirement]` tests in the diff (added tag counts: 017 x39, 018 x25, 019 x5, 020 x8, BREAK-030 x8, RH5025-005 x14, 009 x10, 011 x3, PIPE-024 x6). Changed FMT-BLANK-008 is covered by the existing tagged tests in `BlankLineRequirementTests.cs`.
8. Tags: a scripted check of every test in the `Tests` tables of 04 and 07 found each listed requirement present as `[Requirement]` on that method; the requirement set of the tags the diff adds equals the tables' (plus FMT-BLANK-013, FMT-EOL-002, FMT-SWITCH-001/002 named in the tables for changed existing tests). Every requirement under `Examples for the tests` (FMT-BLANK-009, -011, -018) is tagged on an added or newly tagged test.
9. `sdd/work/` holds only this run's directory.
