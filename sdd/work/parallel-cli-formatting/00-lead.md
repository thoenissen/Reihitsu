# Lead — parallel-cli-formatting

## Run

- Issue: thoenissen/Reihitsu#1795 — "Improve reihitsu-format performance on larger code bases". Not `agent-filed` (filed by an external user, labels `enhancement`, `formatter`): its goals are binding.
- Slug: `parallel-cli-formatting`
- Branch: `claude/squad-1795-tllhl3` (session-designated)
- Draft PR: #1815
- Lane: **behavior change**. Reason: the maintainer's decision on the issue adds a new CLI option (visible CLI behavior) and parallel processing in the CLI; that needs a change doc and a spec diff for `sdd/specs/cli/reihitsu-format.md`.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publish if follow-up candidates exist. None skipped.

## Maintainer clarifications (from the issue comment of 2026-11-16, summarized)

- The CLI formats files in parallel.
- The degree of parallelism is configurable through a new CLI option.
- Output, exit codes, and the summary stay as today; results are printed in file order.
- The option's name, its default, and its documentation are decided in this run and asked as an output question.
- Lane: behavior change with a change doc.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

- Profile the per-file formatter pipeline and benchmark 1.2.0 vs 1.3.0 on a fixed corpus (issue's other two ideas; no visible output) — 01-spec-author.md

## State

- Last step: spec author 01 (change doc written, 78a5e30); 5 output questions drafted (Q5 = interrupted write on Ctrl+C, safety rail).
- Next: skeptic (02), then output-question batch.
