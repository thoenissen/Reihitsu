# Lead — line-count-rules

- Issues: thoenissen/Reihitsu#353 (agent-filed) — report methods whose line count exceeds a limit; thoenissen/Reihitsu#354 (agent-filed) — report types whose line count exceeds a limit. Both carry `agent-filed`: goals, target behavior, and prescribed solutions are proposals.
- Slug: `line-count-rules`
- Branch: `claude/squad-353-354-pq7nz1` (designated by the session)
- PR: thoenissen/Reihitsu#1860 (draft claim)
- Lane: behavior change / new rule — both issues request a new analyzer rule (new diagnostic, new configuration option, new rule page); no existing behavior is reported as wrong. They share a mechanism (counting the lines of a declaration's body against a configured limit) and both sit in the Design (`RH2###`) range, so one run.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing when follow-up candidates exist. None skipped.
- Maintainer clarifications: none beyond the prompt (`/squad #353 #354`).

## Maintainer answers

- Batch 1 (Q1–Q8 of 01-spec-author/02-skeptic): 1A (limits in `reihitsu.json` `design` section, validated by RH0001; RH8402 no longer silenced by a valid `design` section), **2B** (built-in default limits apply when no limit is configured — not inactive), 3A (nested code counts toward the container), 4A (RH2008 / RH2111, titles and messages as drafted, operator token / accessor keyword location), 5A (unions judged, enums not), 6A (each accessor on its own), 7A (initializer lambdas not judged), 8A (strict whole-number validation 1..2147483647, unknown setting reported).
- Batch 2: **9B** (defaults 60 lines per method-like member, 500 per type), **10A** (every line of a multi-line string literal counts), **11A amended**: both rules are turned off for this repository's own build (`Release.ruleset`, `Debug.ruleset`) **and must also be ignored by the self-hosting scans (`Reihitsu.SelfHosting.Test`)**; a follow-up issue decides the repository's own limits or refactors.
- New maintainer requirement (with batch 2): the rules must distinguish production code from test code and have separate limits for each. Pending: the spec author designs it and drafts output questions (how test code is recognized, default test limits, configuration keys, RH0001 validation); then batch 3.
- Earlier: the maintainer asked which default values to use: what limits would work in this repository. Pending: measure the method and type line counts of this repository under the spec's counting definition and propose defaults as a follow-up output question.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Skeptic | State that the line count follows the text as laid out (formatting can raise it) | Accept; spec author adds one sentence to SHARED-LINES-001 `Why:` | Prevents later roles from raising FC-013/FC-014 disagreements | Wording only | pending |
| 2 | Skeptic | Define a line as the compiler's line split (CRLF/LF/CR count alike, FC-033) | Accept; spec author states it in SHARED-LINES-001 | Removes an ambiguity | Same count for all line endings is the only sensible reading; no output changes beyond it | pending |
| 3 | Skeptic | One owner for the counting (FC-015) | Accept; implementer builds one shared counter that both rules use, and it becomes the code owner of `shared/line-count.md` | Avoids private copies | Internal structure | pending |

## Follow-up candidates

- This repository's own limits for RH2008/RH2111 (or refactors of its outliers), with both rules turned off meanwhile (maintainer answer 11A).
- A setting written twice in `reihitsu.json` is not reported by RH0001; the later value wins (02-skeptic.md, found by reading code, low value).

## Audited SHA

(none yet)

## Open worklist / next step

- Done: 01-spec-author (eaa17de), 02-skeptic (b7f1ce2).
- Measurement done (scratchpad linecount/linecounts.csv): prod methods p95 25 / p99 42 / max 132, over 50: 34, over 60: 19, over 75: 7; prod types p99 437 / max 812, over 400: 10, over 500: 5, over 750: 1; test code far larger (raw-string inputs count). Batch 2 posted: Q9 default limits, Q10 raw-string lines, Q11 this repository's own build. Waiting.
- Earlier: measuring the repository's method/type line-count distribution for the default limits (2B).
- Next: route answers to the spec author (amend specs, add decisions 1–2, set change doc Accepted), then the test implementer.
