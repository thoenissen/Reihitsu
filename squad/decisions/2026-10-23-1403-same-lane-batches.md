# Repair a third time, and do the issues of one run share a lane?

| | |
|---|---|
| Date | 2026-10-23 |
| Escalation | 7 |
| Run | issue #1403 |

## Question

The auditor blocked after repair round 2. Medium: tracking each issue's own lane inside a run of issues with different lanes has no reliable input — the roles that write before the implementer list no paths per issue, a change doc written for a docs issue triggers no widening, and a run-level lane-check mismatch is not tied to an issue — so a docs issue in a behavior-change run can silently get no skeptic, test implementer, or implementer. Low: a single issue reference inside prose (`keep it small, as in #99`) claims that issue without asking. Options: a third round in which all issues of a run must share one lane, removing the per-issue lanes; a third round that patches the per-issue lanes; publish with a follow-up issue; stop the run.

## Answer

A third repair round, with a changed design: every issue of a run must have the same lane. This replaces answer 3 of `2026-10-22-1403-multi-issue-run-design.md` and answer 1 of `2026-10-22-1403-batch-roles-and-change-doc.md`, and the per-issue lane widening of `2026-10-23-1403-second-repair-round.md`. The run claims that one lane, runs that lane's roles, and widens as a whole exactly like a single-issue run. When the issues of a prompt need different lanes, the lead asks the maintainer to split them before claiming. The low finding is fixed as well: a single issue reference inside other text is asked about too.

## Reason

none given
