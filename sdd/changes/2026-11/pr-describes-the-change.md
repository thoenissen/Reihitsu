# The squad's pull request describes the change, not the run

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | none — process change |

## What

- **The repository's pull request template is used as it stands.** The lead fills `.github/PULL_REQUEST_TEMPLATE.md` and adds no sections or tables.
- **The body describes the change.** `Summary` states the behavior that changes for a user or caller; `Why` the motivation; `Review notes` only what a reviewer needs; `Follow-up work` the filed issues. A section with nothing to say is deleted.
- **The run stays out of the body.** Lane, `LANE CHECK:` line, roles run or skipped, the `Decisions taken` table, and repair rounds live in the run's note and in `00-lead.md`. The PR keeps a safety-rail follow-up, answers that shape the change, a decision a reviewer could reasonably overrule, the `Refs` result for an unreachable case, and checks that did not run locally.
- **The note is never linked** from the PR; notes are temporary.

## Why

The run report made `Review notes` a large table that hid the change. A reviewer needs to know what changed and why, not which roles ran.
