# Lead — report-across-directives

- Issue: thoenissen/Reihitsu#1759 — SHARED-DIAG-008: analyzers judge the code as the preprocessor symbols of the current build are set. Label `agent-filed` (goals are proposals). The principle is stated in the issue as a maintainer decision of 2026-11-15 taken under #1756; the issue's comment (also agent-filed) adds the empty-type rules RH5412–RH5416, RH5418 to the affected list.
- Slug: `report-across-directives`
- Branch: `claude/inspiring-meitner-761ukw` (session-designated)
- PR: #1832 (draft)
- Lane: behavior change — the issue requests changed behavior (rules report where they are silent today) and reverses a shared spec requirement, so a change doc is required.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing when there are follow-up candidates. None skipped.

## Maintainer answers

Batch 1 (before test implementer), answered 1A 2A 3A 4A 5A 6A:
1. Rules that start reporting across a conditional directive (RH7105/7106, RH7102/7103, RH7107/7108, RH5102/5109, RH7101, attribute-list shape rules SHARED-ATTR-019/020): A — reported, no fix offered.
2. RH5409/5410/5411 trailing comma in front of an `#if` block: A — reported (in both builds, as shown), never with a fix; formatter keeps the comma.
3. RH5111 conditional directive around `=`: A — reported, no fix; extends RH5111's exception to SHARED-DIAG-010.
4. RH7311: A — the compiled branch's region is compared with regions outside the group; branches never compared with each other.
5. RH8201 implicit implementation in a file with `#if`: A — reported, no fix.
6. Escalation 10, conditional directive between last comma and closing brace: A — not declared unsupported.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Spec author | Bound the fix half by the part of the span a fix rewrites, not every fix when a directive is anywhere in the span | Accept the spec author's bound | Blanket withholding removes RH2005's safe fix and contradicts SHARED-ORDER-006 | Keeps today's offered fixes; the newly reported cases are asked as output questions | 3d85973 |
| 2 | Spec author | Change SHARED-TRAIL-002, SHARED-ATTR-019/020, RH8201-003 in place vs. retire | In place | References keep resolving | Spec structure only | 3d85973 |
| 3 | Skeptic | Issue's RH5412 example and "nine specs" count are wrong (RH5412 already reports, no fix) | Drop the example and count as goals; empty-type rules get spec clarification only, no new tests (their behavior is already covered) | Observed with apply-fix at 3d85973 | No output changes | — |
| 4 | Skeptic | "Code fixes keep withholding as today" is false for trailing commas and RH8201 | Fix providers gain the analyzer's existing guard (spec already requires the fix withheld) | Otherwise the fixes become unsafe (SHARED-FIX-003) | Spec requirement already demands the withheld fix | — |
| 5 | Skeptic | general-notes.md moved sentence lost its antecedent | Route to spec author: keep the #region sentence at the end of the first paragraph | Wording | Text placement only | — |
| 6 | Skeptic | Test scope | "Reported, no fix" tests only where the output changes, with the skeptic's shape matrix and Fix All contrast | Avoid redundant tests | Tests only | — |
| 7 | Implementer | 19 Fix All / fixed-state tests mark a remaining diagnostic without allowing markup in the fixed state | Test defect; route to test implementer to fix so the marked diagnostic is checked (no assertion meaning changes) | Implementer showed the marked diagnostic is the one found | Test harness only | — |
| 8 | Implementer | RH7102 example (change doc, RH7102-001, test) does not compile in the DEBUG build (CS8983) | Spec author makes the example compile in both builds; test implementer follows the spec | Example must be valid in every build it is run in | Reported output for the example unchanged (RH7102 reported, no fix) | — |

## Follow-up candidates

- SHARED-USING-009 keeps RH7201–RH7207 silent on a using block holding any directive; not a build-configuration reason, outside #1759 (01-spec-author.md).
- Narrower trailing-comma fix guard: offer the fix when no `#if` stands between the comma and the brace, with the formatter following (02-skeptic.md, optional, low value).
- RH2001 spec example (`#if !RELEASE` around `Value`, used outside the group) fails to compile (CS0103) in a RELEASE build (06-spec-author.md).

## State

- Audited SHA: —
- Last step: spec author done (3d85973), lane check confirmed behavior-change.
- Skeptic done (00547ae, verdict output question).
- Batch 1 answered.
- Spec author follow-up done (f3b78d5), change doc Accepted.
- Test implementer done (dcbc177): 185 failing tests, all expected under-reporting.
- Implementer done (233dfda, handover 4702026): verdict test disputed, 21 failing tests.
- Spec author repair done (1c988a4).
- Test implementer repair done (b06bebd): affected classes green.
- Cleaner done (01c4749): clean.
- Spec checker consistent (55a42ef). Merged origin/main fe1d122 (131a7c3), conflicts in RH2001/RH7101 Uses rows resolved by spec author; cleaner clean after merge (dea1e49).
- Next step: auditor. (was: batch 1: Q1 the newly reported rules (change doc), Q2 trailing commas, Q3 RH5111, Q4 RH7311, Q5 RH8201, Q6 escalation 10 (trailing-comma shape, recommend not declaring). Then route answers + decision 5 to spec author (Status Accepted), then test implementer.
