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
| 5 | test impl 04 | Tests require real concurrency above degree 1 although CLI-022 says MAY | Accept | Issue goal (binding) and maintainer decision: files are formatted in parallel | Internal test strictness; output identical | d872a51 |
| 6 | test impl 04 | Replace `ExecuteAsyncPassesCancellationTokenToFileWrite` by a behavior test | Accept | Contradicts decision 1 / CLI-022 started-write rule | Carries out decision 1 | d872a51 |
| 7 | implementer 05 | Ctrl+C after the last file but before the summary now ends with `Operation canceled.`, no summary (main printed the summary in that window) | Accept | CLI-022 "MUST NOT write the summary" on cancellation; answer 4A shows no summary on cancel | Covered by answer 4A and CLI-017/022 | a1d2458 |
| 8 | implementer 05 | New type `BufferedConsoleOutput` (62 lines) | Accept, not escalation 9 | Part of the issue's own fix | Internal | a1d2458 |
| 9 | cleaner 06 | Debug-only S1125 on unchanged lines of FormatCommandHandler.cs | Drop as follow-up | Pre-existing, Release build is warning-free, not reached by the run | Internal | — |
| 10 | spec checker 07 | Repair round 1: spec index row misses `BufferedConsoleOutput`; cleaner skipped for this round | Route to spec author; then spec checker again | Markdown-only repair, nothing for the cleaner to format or build | Internal | b0ae134 |

## Follow-up candidates

- Profile the per-file formatter pipeline and benchmark 1.2.0 vs 1.3.0 on a fixed corpus (issue's other two ideas; no visible output) — 01-spec-author.md, 02-skeptic.md
- LcsComputer (n+1)(m+1) memory per changed file in --dry-run, multiplied by parallel degree — 02-skeptic.md
- Confirm Windows behavior of concurrent writes to one physical file via aliases — 02-skeptic.md (depends on Q5 answer)
- Dedupe one physical file reached by several paths — 02-skeptic.md (Q5 answered A)

## State

- Last step: skeptic 02 (verdict: output question). Batch 1 asked: Q1 name, Q2 default, Q3 docs, Q4 Ctrl+C several in progress, Q5 aliases (skeptic's Q6), Q6 accepted values (skeptic's Q7).
- Implementer 05 done (a1d2458), tests pass. Lane check confirmed. Cleaner 06 clean (df039b2). Spec checker 07: 1 finding (sdd/specs/README.md:399 index row). Repair round 1 open. Spec author 08 repaired (b0ae134). Next: spec checker 09, then merge main/build/auditor 10.
