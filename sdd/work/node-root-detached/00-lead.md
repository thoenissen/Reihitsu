Role: lead
Commit: f9d475c
Verdict: lane claimed

# Run: thoenissen/Reihitsu#1489

- Issue: thoenissen/Reihitsu#1489 — FMT-PIPE: node-level roots are detached, and their input facts are specified plain values (package B4 of thoenissen/Reihitsu#1482).
- Slug: `node-root-detached`
- Branch: `claude/sleepy-goldberg-pli2uv` (the session's designated branch; the harness allows pushes only there, so it stands in for `claude/issue-1489-node-root-detached`).
- Draft PR: thoenissen/Reihitsu#1520
- Base: `origin/main` at `e7853b6`. Blocker thoenissen/Reihitsu#1488 (B3) is merged (#1513).

## Lane

Behavior change. The issue requests changed behavior of the node-level entry points (detached root, specified input facts, a different base-depth rule) and states that a change doc under `sdd/changes/` is required.

Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. None skipped.

## Maintainer clarifications

None yet. The issue asks the run to first check every statement about today's code against current `main` and to drop statements an earlier package already made true or moot, recording them in the handover; target behavior and "Done when" stay binding, and a difference that changes the target is an open question for the maintainer.
