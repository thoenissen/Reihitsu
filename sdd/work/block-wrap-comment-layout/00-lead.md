Role: lead
Commit: e796687
Verdict: lane claimed — bug fix

# Run: block-wrap-comment-layout

## Issues

- thoenissen/Reihitsu#1490 — "FMT-PIPE-013: wrapping statements in a block keeps their comments' layout". Labels: `bug`, `formatter`, `agent-filed`. Its premise, goals, "Done when" items, target behavior, and prescribed solution are proposals (`squad/README.md`, *Issues are hypotheses*). Its blocker, thoenissen/Reihitsu#1488 (B3), is closed (merged as PR #1513). The issue names code as of `735d0dba`; every statement about today's code is to be checked against current `main` (`fbf5b6f`).
- Draft PR: thoenissen/Reihitsu#1535. Branch: `claude/dazzling-planck-2e0bj3` (session-designated branch).

## Lane

**Bug fix.** The issue reports an observed-versus-expected difference against an existing requirement, FMT-PIPE-013 (`sdd/specs/formatter/`), in shipped formatter source (`ControlFlowBraceTransform`, `SwitchCaseBraceRewriter`).

Roles run: skeptic (premise check) → test implementer (reproduction, then tests) → implementer → cleaner → spec checker → auditor → validator.
Roles skipped: spec author — runs in the bug-fix lane only for a clarification; spawned only if the skeptic or a later role shows FMT-PIPE-013 or a neighbouring requirement needs one.

## Maintainer clarifications

None.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name: `squad/lead.md` asks for `claude/issue-1490-<slug>`, the session is bound to `claude/dazzling-planck-2e0bj3` | Use the session-designated branch | The session may push only to its designated branch | Branch name is internal | e796687 |
| 2 | Claim | `sdd/work/node-root-detached/` is left on `main` from merged PR #1520 | Delete it in this run's publish commit together with this run's `sdd/work/` | A published PR must not leave `sdd/work/`; the stale directory belongs to a merged run, and deleting it is a commit that only deletes `sdd/work/`, which the lead writes | Working files only | (publish commit) |

## Follow-up candidates

None yet.
