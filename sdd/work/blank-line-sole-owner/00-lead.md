Role: lead
Commit: 741157c9ca6fd4e16b144da4cbd7be2f354aec8b
Verdict: lane claimed

# Run: blank-line-sole-owner

## Issue

- thoenissen/Reihitsu#1496 — "FMT-BLANK: the blank-line phase is the only owner of blank lines". Labeled `agent-filed`: its goals are proposals, except where the maintainer's revision comment (below) decided them.
- Package C3 of thoenissen/Reihitsu#1482. Blockers C1 (thoenissen/Reihitsu#1490, merged via #1535) and C2 (thoenissen/Reihitsu#1491, merged via #1532) are closed. The issue was written against `735d0dba`; every statement about today's code must be checked against current `main` (741157c) first, and a statement an earlier package made true or moot is noted and dropped.

## Slug

`blank-line-sole-owner`

## Lane

Behavior change. Reason: the issue requests changed formatter behavior (block statement after `}` gains the SHARED-BLANK-003 blank line) and changed analyzer behavior (RH5025 exemption for the `{` of a block statement), plus a rule-page change; the issue requires a change doc.

Branch: `claude/hopeful-gauss-tkf765` (the session's designated branch, used instead of `claude/issue-1496-<slug>`; see Decisions taken #1). Draft PR: thoenissen/Reihitsu#1541.

## Roles

All roles of the behavior-change lane run, in order: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. Issue manager before publishing if there are follow-up candidates.

## Maintainer clarifications (summarized)

The maintainer revised the issue on 2026-10-28 (issue comment):

- FMT-BLANK-008 and RH5025 do not apply to the `{` of **any** block statement, not only after `}`. After a statement ending in `}` the SHARED-BLANK-003 blank line remains required; after any other statement the author's blank line is kept (none is required) and RH5025 does not report it.
- RH5020 keeps reporting a comment glued to `{` (no exemption); such a comment is fixed by hand.
- The change doc names the superseded decisions: thoenissen/Reihitsu#1396 (RH5030 exempting a block statement) and the standalone-block case of thoenissen/Reihitsu#1081 item 1.

These decisions are binding for the run.

## Maintainer answers

_None yet._

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch `claude/hopeful-gauss-tkf765` instead of `claude/issue-1496-blank-line-sole-owner` | The session environment requires all pushes to go to this branch | Branch names are internal | claim commit |

## Follow-up candidates

_None yet._
