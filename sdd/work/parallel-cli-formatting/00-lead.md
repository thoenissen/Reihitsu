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

Batch 1 (before test implementer), answer "1a 2a 3a 4a 5a 6a":
- Q1 name: A — `--max-parallelism <count>`.
- Q2 default: A — number of logical processors.
- Q3 docs: A — help lines plus full README row and example (01-spec-author.md Q3 A text).
- Q4 Ctrl+C with several in progress: A — every finished file named in selection order, then `Operation canceled.`, exit 2, no summary.
- Q5 aliases in write mode (skeptic's Q6): A — carve out of CLI-022; which alias paths are named Formatted/Unchanged and the count may vary; content correct; --check/--dry-run unchanged.
- Q6 accepted values (skeptic's Q7): A — digits only, 1..int.MaxValue, no upper bound, last value wins, `--lang-version`-style messages.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | skeptic | Drafted Q5 (write truncated by Ctrl+C) — output question? | Not asked; repaired in this run as an adjacent fix: a started write completes (no cancellation token passed into the write), CLI-022/CLI-017 cancellation text says a file whose write has started is finished and named | Pre-existing on `main`, reproduced by skeptic (02), widened by the change; small, in `FormatCommandHandler`/`DefaultFileSystem`, testable; option B would violate non-destructiveness | Unchanged non-destructiveness requirement demands it; no alternative output is admissible | 0a1bd37 (spec); code pending |
| 2 | skeptic | Reword CLI-006 "selected more than once" to "the same full path" | Accept | States today's behavior precisely | Changes no output | 0a1bd37 |
| 3 | spec author 03 | Leading zeros in the count (`007`) | Accept as 7 | Follows from answer 6A (digits only, value 1..int.MaxValue) | Covered by answer 6A | 0a1bd37 |
| 4 | spec author 03 | README sentence "same for every count" not literally true for aliases in write mode | Keep as answered (3A) | Maintainer chose the text; the alias carve-out (5A) is recorded in spec and change doc | Text was answered verbatim | 0a1bd37 |

## Follow-up candidates

- Profile the per-file formatter pipeline and benchmark 1.2.0 vs 1.3.0 on a fixed corpus (issue's other two ideas; no visible output) — 01-spec-author.md, 02-skeptic.md
- LcsComputer (n+1)(m+1) memory per changed file in --dry-run, multiplied by parallel degree — 02-skeptic.md
- Confirm Windows behavior of concurrent writes to one physical file via aliases — 02-skeptic.md (depends on Q5 answer)
- Dedupe one physical file reached by several paths — 02-skeptic.md (Q5 answered A)

## State

- Last step: skeptic 02 (verdict: output question). Batch 1 asked: Q1 name, Q2 default, Q3 docs, Q4 Ctrl+C several in progress, Q5 aliases (skeptic's Q6), Q6 accepted values (skeptic's Q7).
- Spec author 03 done (0a1bd37, change doc Accepted). Lane check confirmed behavior-change. Next: test implementer 04.
