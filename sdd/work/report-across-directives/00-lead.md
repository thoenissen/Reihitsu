# Lead — report-across-directives

- Issue: thoenissen/Reihitsu#1759 — SHARED-DIAG-008: analyzers judge the code as the preprocessor symbols of the current build are set. Label `agent-filed` (goals are proposals). The principle is stated in the issue as a maintainer decision of 2026-11-15 taken under #1756; the issue's comment (also agent-filed) adds the empty-type rules RH5412–RH5416, RH5418 to the affected list.
- Slug: `report-across-directives`
- Branch: `claude/inspiring-meitner-761ukw` (session-designated)
- PR: #1832 (draft)
- Lane: behavior change — the issue requests changed behavior (rules report where they are silent today) and reverses a shared spec requirement, so a change doc is required.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing when there are follow-up candidates. None skipped.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Spec author | Bound the fix half by the part of the span a fix rewrites, not every fix when a directive is anywhere in the span | Accept the spec author's bound | Blanket withholding removes RH2005's safe fix and contradicts SHARED-ORDER-006 | Keeps today's offered fixes; the newly reported cases are asked as output questions | 3d85973 |
| 2 | Spec author | Change SHARED-TRAIL-002, SHARED-ATTR-019/020, RH8201-003 in place vs. retire | In place | References keep resolving | Spec structure only | 3d85973 |
| 3 | Skeptic | Issue's RH5412 example and "nine specs" count are wrong (RH5412 already reports, no fix) | Drop the example and count as goals; empty-type rules get spec clarification only, no new tests (their behavior is already covered) | Observed with apply-fix at 3d85973 | No output changes | — |
| 4 | Skeptic | "Code fixes keep withholding as today" is false for trailing commas and RH8201 | Fix providers gain the analyzer's existing guard (spec already requires the fix withheld) | Otherwise the fixes become unsafe (SHARED-FIX-003) | Spec requirement already demands the withheld fix | — |
| 5 | Skeptic | general-notes.md moved sentence lost its antecedent | Route to spec author: keep the #region sentence at the end of the first paragraph | Wording | Text placement only | — |
| 6 | Skeptic | Test scope | "Reported, no fix" tests only where the output changes, with the skeptic's shape matrix and Fix All contrast | Avoid redundant tests | Tests only | — |

## Follow-up candidates

- SHARED-USING-009 keeps RH7201–RH7207 silent on a using block holding any directive; not a build-configuration reason, outside #1759 (01-spec-author.md).

## State

- Audited SHA: —
- Last step: spec author done (3d85973), lane check confirmed behavior-change.
- Skeptic done (00547ae, verdict output question).
- Next step: WAITING on maintainer for batch 1: Q1 the newly reported rules (change doc), Q2 trailing commas, Q3 RH5111, Q4 RH7311, Q5 RH8201, Q6 escalation 10 (trailing-comma shape, recommend not declaring). Then route answers + decision 5 to spec author (Status Accepted), then test implementer.
