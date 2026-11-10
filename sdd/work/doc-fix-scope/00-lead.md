# Lead — doc-fix-scope

- Issues (prompt order): thoenissen/Reihitsu#1269, #1342, #1343, #1459 — all four carry `agent-filed` (goals are proposals).
- Slug: `doc-fix-scope`
- Branch: `claude/squad-tickets-1269-1342-gxh523` (the session's designated branch; overrides the playbook's `claude/batch-1269-...` name)
- Draft PR: #1679
- Base at claim: `8ac5705` (origin/main)

## Lane

**Bug fix**, for every issue. Each reports an observed-versus-expected difference in documentation code fixes against existing requirements:

- #1269 — RH8201/RH8202/RH8204 fixes change more than the reported markup (layout, line endings, re-serialized content) or need two applications (SHARED-FIX-001/006/008).
- #1342 — RH8305/RH8307/RH8309/RH8402 fixes reformat beyond the reported comment, move a behind-code `///`, drop a blank line (SHARED-FIX-006/007, SHARED-DOC-004).
- #1343 — RH8308/RH8309/RH8401 fixes need two applications (SHARED-FIX-001/005); RH8401 rule page text to be aligned with behavior (docs path, admitted by the bug-fix lane).
- #1459 — RH8305 fix makes no progress on the first type of a file without a namespace (RH8305-004). Same instance also commented on #1343.

Roles: skeptic → spec author (only for clarifications and the RH8401 rule page) → test implementer → implementer → cleaner → spec checker → auditor → validator. Issue manager before publishing if follow-up candidates exist.

## Maintainer clarifications

None in the prompt.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch `claude/squad-tickets-1269-1342-gxh523` | Harness requires it | Internal | a200b98 |
| 2 | Lane | Spec author before skeptic? | Skeptic first; spec author only for clarifications and the RH8401 rule page | Bug-fix lane runs the spec author only for a clarification | Internal | — |

## Follow-up candidates

(none yet)

## Worklist / next step

- Next: skeptic (01-skeptic.md).
