# Lead — union-with-analyzer-rules

- Issue: thoenissen/Reihitsu#1767 — `agent-filed` (goals are proposals). Parent #1716; depends on #1761.
- Slug: `union-with-analyzer-rules`
- Branch: `claude/squad-1767-erhrdo` (session-mandated branch name, see Decisions #1)
- PR: #1787 (draft)
- Lane: behavior change — the issue asks analyzer rules (organization, design, layout, attributes, spacing) to start covering C# 15 union declarations and `with(...)` collection-expression elements, which changes which diagnostics are reported; it carries open output questions and likely needs a change doc.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-ups exist. None skipped.

## Maintainer clarifications

None yet.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use `claude/squad-1767-erhrdo` instead of `claude/issue-1767-<slug>` | The session only permits pushing to that branch | Branch names are internal | claim commit |
| 2 | Spec author | Widen existing kind lists vs union-specific requirements | Widen existing requirements; no new IDs | Fewer statements of one rule | Spec structure is internal; outputs asked as Q1-Q4 | 8d7105f |
| 3 | Spec author | One change doc for unions and `with(...)` | One change doc | Shared cause (C# 15 support) | Change-doc layout is internal | 8d7105f |
| 4 | Spec author | Formatter attribute split for unions | Bug against unchanged FMT-STRUCT-028 (read as covering any type) | Requirement already says any type's list is split | Pending Q2 answer anyway; asked as part of Q2 | 8d7105f |

## Follow-up candidates

- RH5108 does not report class/struct primary constructors with the first parameter on the next line (01-spec-author) — likely duplicate of #1158; issue manager checks.
- RH5110 does not report a `record struct` constraint clause (01-spec-author) — likely duplicate of #1154; issue manager checks.

## State

- Side task (maintainer request): moved #1786 from vNext to milestone 1.4.0; #1781/#1785 left in vNext.
- Last step: spec author 01 (change doc written; output questions Q1-Q4, Q3/Q4 are escalation 5).
- Next: skeptic (02), then batch of output questions before the test implementer.
