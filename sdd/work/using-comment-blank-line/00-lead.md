Role: lead
Commit: b27f329
Verdict: lane claimed — behavior change

# Run: thoenissen/Reihitsu#1497

- Issue: thoenissen/Reihitsu#1497 — "FMT-USING: the blank line above a comment that leads a using directive of the same group is kept". Labels `enhancement`, `formatter`, `agent-filed` (goals are proposals). Package C4 of thoenissen/Reihitsu#1482.
- Blocker thoenissen/Reihitsu#1496 (C3) is closed, merged with thoenissen/Reihitsu#1541; this run starts on `main` at `b27f329`, which contains it.
- Slug: `using-comment-blank-line`.
- Branch: `claude/great-pasteur-ur6nr8` (the session's designated branch, used instead of `claude/issue-1497-<slug>`). Draft PR: thoenissen/Reihitsu#1547.

## Lane

Behavior change. The issue requests changed behavior of shipped rules (SHARED-USING-008, RH7207 reporting and its code fix, the using phase) and requires a change doc under `sdd/changes/`.

Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. The issue manager runs before publishing when follow-up candidates exist. No role is skipped.

## Maintainer clarifications

- The issue was revised on 2026-10-28 by maintainer decision (issue comment): the originally filed form (no blank line above a comment that leads a same-group using directive) is reported by RH5020 and left no clean layout. The blank line is now kept: SHARED-USING-008 and RH7207 accept it, the using phase stops removing it, and the formatter's output does not change. This implements thoenissen/Reihitsu#965.
- The issue's "Before the run" section: every statement about today's code is checked against current `main`; a statement an earlier package made true or moot is noted and dropped. The target behavior and "Done when" stay binding; a difference that changes the target goes to the maintainer.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch `claude/great-pasteur-ur6nr8` instead of `claude/issue-1497-<slug>` | The session environment permits pushes only to that branch | Branch names are internal | claim commit |

## Follow-up candidates

None yet.
