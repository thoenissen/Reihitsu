# Lead — thoenissen/Reihitsu#1502

Role: lead
Commit: df4f828 (origin/main at claim)
Verdict: lane claimed

## Run

- Issue: thoenissen/Reihitsu#1502 — "FMT-PIPE: RH7101 and RH7201–RH7207 reach the formatter through a specified entry point". Label `agent-filed`: its goals, "Done when" items, target behavior and prescribed solution are proposals; a change to what an end user sees stays a maintainer question.
- Blocked-by issue thoenissen/Reihitsu#1501 (E1) is closed, merged with PR #1580, which is the current `origin/main` (df4f828).
- Slug: `fix-entry-point`
- Branch: `claude/zen-mayer-s173ql` (the session's assigned branch, instead of `claude/issue-1502-<slug>`; see Decisions taken).
- Draft PR: thoenissen/Reihitsu#1581
- Lane: **behavior change**. The issue asks for changed formatter structure and spec requirements (FMT-PIPE, FMT-USING, FMT-STRUCT, SHARED-USING-010) and requires a change doc; shipped source under `src/` with a change doc is the behavior-change lane.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; the issue manager before publishing when follow-up candidates exist. No role is skipped.

## Maintainer clarifications (summarized)

- The node-level formatting methods of `ReihitsuFormatter` are not a public API. A change to them only has to cover the cases the code fixes actually pass (the RH7101 and RH7201–RH7207 code fixes here, and the existing callers); inputs that only a direct call would produce need no handling, no requirement, and no test (`AGENTS.md`, *Supported inputs*).

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's assigned branch `claude/zen-mayer-s173ql` instead of `claude/issue-1502-<slug>` | The session environment permits pushes only to its assigned branch | Branch names are internal | claim commit |

## Follow-up candidates

None yet.
