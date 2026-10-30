# Lead — preparation-phase

Role: Lead
Commit: 66fd1d7
Verdict: lane claimed

## Issue

- thoenissen/Reihitsu#1485 — a PreparationPhase (sub-phases TabReplacement, LineEndings) runs first in the formatter pipeline; package B1 of thoenissen/Reihitsu#1482.
- Draft PR (claim): thoenissen/Reihitsu#1508.
- Branch: `claude/ecstatic-dirac-kj42yo`. The session environment mandates this branch name, so the playbook's `claude/issue-1485-<slug>` name is not used.

## Lane

Behavior change. The issue requests changed pipeline behavior (phase order, a new phase-wide line-ending rule replacing FMT-PIPE-016, new observer/trace phase names) and states that a change doc under `sdd/changes/` is required and must be approved before tests are written.

Roles: spec author (change doc + spec diff + documentation) → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. No role skipped.

## Notes for the run

- The issue was written against `main` at `735d0dba`; `main` is now at `66fd1d7` (adds thoenissen/Reihitsu#1504 and thoenissen/Reihitsu#1506). The issue asks that every statement about today's code be checked against current `main` first; statements already made true or moot are noted in the handover and dropped. Target behavior and "Done when" stay binding.
- Maintainer clarifications: none so far (the issue has no comments).
