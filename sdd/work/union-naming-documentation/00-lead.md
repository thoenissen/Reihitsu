# Lead — union naming and documentation

- Issue: thoenissen/Reihitsu#1766 — `agent-filed` (goals are proposals). Parent #1716; dependency #1761 is closed (merged in #1779, Roslyn 5.12, C# 15).
- Slug: `union-naming-documentation`
- Branch: `claude/squad-1766-qkoyls` (the session's designated branch; see decision 1)
- Draft PR: #1784
- Lane: **behavior change / new rule** — the issue requests new analyzer behavior for union declarations (naming and documentation diagnostics), probably new rule IDs; that is visible output and needs a change doc.
- Roles: spec author → skeptic → (output questions) → test implementer → implementer → cleaner → spec checker → auditor → validator. None skipped.
- Maintainer clarifications: none beyond the issue.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use `claude/squad-1766-qkoyls` instead of `claude/issue-1766-<slug>` | The session environment permits pushing only to this branch | Branch name is internal | 81a7af9 |

## Follow-up candidates

- Stale comment in RH5404 analyzer (unions parse only under preview) — belongs to #1762 (union layout), not a new issue (01-spec-author.md).

## Worklist / state

- Spec author done (01-spec-author.md, commit 2a43412): change doc `sdd/changes/2026-11/union-naming-documentation.md`, new RH4012/RH8035/RH8036, 5 output questions. Lane check confirmed behavior-change.
- Next: skeptic (running), then output-question batch 1 before the test implenter.
