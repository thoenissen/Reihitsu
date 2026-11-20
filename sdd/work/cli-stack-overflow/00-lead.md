# Lead — cli-stack-overflow

| | |
|---|---|
| Issue | thoenissen/Reihitsu#1887 (`agent-filed`: yes — goals are proposals) |
| Slug | cli-stack-overflow |
| Branch | claude/dazzling-clarke-n7m17j |
| Pull request | thoenissen/Reihitsu#1893 (draft claim) |
| Lane | Bug fix |

## Lane and reason

Bug fix: the issue reports an observed difference — a file whose expression nesting overflows the formatter's stack aborts the whole CLI process — against an unchanged requirement, CLI-016 (a failing file does not stop the run), `sdd/specs/cli/reihitsu-format.md`. The maintainer's triage comment on the issue states the expected behavior is the issue's option A, implemented as a bug fix against CLI-016, with no maintainer question open.

Roles run: skeptic → test implementer (reproduction first) → implementer → cleaner → spec checker → auditor → validator. Skipped: spec author (bug-fix lane, only for a clarification; none needed so far).

## Maintainer clarifications

- Triage (issue comment, 2026-11-20): critical bug for 1.5.0; expected behavior is option A — the file is reported as failed (`Error processing <path>: ...`), the other files are processed, the run ends with the error summary and exit code 2. Treated as already answered by CLI-016.

## Maintainer answers

_None yet._

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

_None yet._

## Worklist / next step

- Done: claim (0bd9f1b), draft PR #1893, `scripts/prepare.sh`.
- Next: skeptic.
