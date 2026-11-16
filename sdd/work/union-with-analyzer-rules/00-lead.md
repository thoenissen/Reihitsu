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
| 4 | Spec author | Formatter attribute split for unions | Bug against unchanged FMT-STRUCT-028 (read as covering any type) | Requirement already says any type's list is split | Pending Q2 answer anyway; asked as part of Q2 | 8d7105f || 5 | Skeptic | Change doc example 4 claims RH7104 skips `partial class N` in a union | Correct it: only top-level `partial union U` is new; RH7104-004 union part becomes a clarification | Skeptic showed today's fix already reports and fixes N | Corrects a "before", no output changes | pending amendment |
| 6 | Skeptic | RH7110-001 says "previous member" but code/test compare with every earlier member | Fix the spec wording in this run (spec-only cleanup) | Adjacent, small, spec matches the tested code | Wording follows existing behavior | pending amendment |
| 7 | Skeptic | Record exclusions for RH2103 and RH2005 | Add one-line non-goals; add RH7311-002 to change doc affected list | Meets the issue's "recorded decision" item | Non-goals record unchanged output | pending amendment |
| 8 | Skeptic | Architecture finding: type-declaration kind list duplicated in ~17 registrations, ParameterListParentPolicy, AttributeTargetUtilities, formatter visitors | Fix narrowly (add union kind to each list); file a shared kind set or architecture test as a follow-up | Narrow fix adds no new copy; structural change spans many components | Internal structure | — |
| 9 | Skeptic | Issue goals "RH2101-RH2107 cover unions" and "a test per RH55xx rule" | Drop to Q4 (nested union) and narrow to RH5507/RH5508 plus one formatter test | Other nested-type/RH55xx rules already see unions or are about other constructs | Remaining visible part is asked as Q2/Q4 | — |
| 10 | Skeptic | Formatter attribute split for unions | Treat as a bug fix against unchanged FMT-STRUCT-028; not asked | Requirement says "a type" | Unchanged requirement demands it | — |

## Follow-up candidates

- RH5108 does not report class/struct primary constructors with the first parameter on the next line (01-spec-author) — likely duplicate of #1158; issue manager checks.
- RH5110 does not report a `record struct` constraint clause (01-spec-author, 02-skeptic) — likely duplicate of #1154; issue manager checks.
- The type-declaration kind list is duplicated across ~17 analyzer registrations, `ParameterListParentPolicy`, `AttributeTargetUtilities`, and formatter visitors; one shared kind set or an architecture test (02-skeptic, architecture finding).

## State

- Side task (maintainer request): moved #1786 from vNext to milestone 1.4.0; #1781/#1785 left in vNext.
- Last step: skeptic 02 (output questions Q1-Q4 stand, with adjustments). Spec-author amendment 03 running (decisions 5-7). Output questions Q1 (unions join type rules), Q2 (RH5507/RH5508 on unions), Q3 (RH5418, escalation 5), Q4 (RH2110, escalation 5) asked to the maintainer; waiting.
- Next: record answers, route spec work for answers, then test implementer.
