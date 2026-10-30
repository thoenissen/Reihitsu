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

## Escalations answered

- Escalation 2 (change doc): approved; FMT-PIPE-020 covers every terminator FMT-EOL-001 lists; the doc stays under `2026-10`. Recorded in `squad/decisions/2026-10-30-preparation-phase-change-doc.md`.
- Escalation 12 (architecture finding of `02-skeptic.md`, the tab rule stated in `TabReplacementPhase` and in `ReihitsuFormatterHelpers.ComputeDocumentColumn`/`IsInOwnLeadingWhitespace`): option B — the structural change is part of this run. The column of a node-level target's first token (FMT-PIPE-017) is measured with the preparation rule, and the copy in `ComputeDocumentColumn` is removed; the existing FMT-PIPE-017 tests must keep passing unchanged. The accepted change doc already describes this end state, so no amendment. This does not use up a repair round. Recorded in `squad/decisions/2026-10-30-preparation-phase-tab-column-copy.md`.
- The maintainer asked for a follow-up issue about line breaks kept inside comments and disabled text that decide layout; the lead drafts it outside the run.
- Follow-ups for the PR's `Follow-up work`: thoenissen/Reihitsu#1509 (created after the maintainer approved the text: kept line breaks inside comments and disabled text decide a comment's column); thoenissen/Reihitsu#898 (existing: `scripts/trace.sh` lacks the executable bit).
