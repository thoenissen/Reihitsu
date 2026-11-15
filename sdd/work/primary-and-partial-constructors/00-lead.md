# Lead — primary-and-partial-constructors

- Issues: thoenissen/Reihitsu#1680 (`agent-filed`, bug, analyzer), thoenissen/Reihitsu#1742 (`agent-filed`, bug, analyzer). Goals of both are proposals.
- Slug: `primary-and-partial-constructors`
- Branch: `claude/beautiful-shannon-h7hcav` (the session's designated branch; the session forbids pushing to any other, so the playbook's `claude/issues-1680-1742-<slug>` name is not used)
- PR: thoenissen/Reihitsu#1744 (draft claim)
- Lane: **bug fix**, for both.
  - #1680: reported observed-versus-expected difference — documentation analyzers report a positional record / primary-constructor type twice (SHARED-DIAG one diagnostic per element).
  - #1742: reported observed-versus-expected difference — syntax-node rules on constructors miss the defining part of a partial constructor.
- Roles run: skeptic (premise check), test implementer (reproduction), implementer, cleaner, spec checker, auditor, validator. Spec author: only for a clarification, skipped unless a role asks for one.
- Maintainer clarifications: none.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch `claude/beautiful-shannon-h7hcav` | The session environment permits pushing only to this branch | Branch name is internal | 5a879f2 |

## Follow-up candidates

None yet.

## State

- Last step: claim, draft PR, prepare.sh green, 00-lead written.
- Next: skeptic.
