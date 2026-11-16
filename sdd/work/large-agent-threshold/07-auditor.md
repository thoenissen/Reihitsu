Role: auditor
Commit: 5ce6f54e96d6d578f9c1cd240deaf81969fbf050
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit of head `5ce6f54` against base `origin/main` = `372daeee7a757fe7a3026896a594ece4df03fa8a` (matches `git ls-remote origin refs/heads/main`; merged: the base is an ancestor of the head). Checkout is the head SHA. The diff outside `sdd/work/` is four files: `scripts/usage.sh`, `scripts/README.md`, `squad/lead.md`, `sdd/changes/2026-11/squad-large-agent-by-context-growth.md`, which is what handovers 03 to 06 describe. `scripts/lane-check.sh behavior-change`: confirmed, and it reports that the diff needs only the tooling lane (see Hints).

Issue thoenissen/Reihitsu#1792 (`agent-filed`) goals: the size signal now separates orientation from growth (context growth, cache-aware numbers); `usage.sh --json` prints `first_context_tokens`, `last_context_tokens`, `context_growth_tokens`, and `last_response_at`; `squad/lead.md` keeps "starting points, not a gate". All three goals are addressed. The goal's tail "chosen from the usage tables of the remaining run notes" was adjusted by decision 4 (the number comes from a cost model, which the change doc states). That is a process text change, so no visible output is involved.

Re-run on this session (`scripts/usage.sh --session ac22de41-… --json`): every spawn carries the four fields. Spec author 1 is 79785 → 132280 (52495), skeptic 79386 → 211251 (131865), matching the sanity points in 03. The Markdown row ends with the spawn's growth, and the tier and total tables are unchanged.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | medium | `scripts/usage.sh:121-131` (`context_of`) | FC-015 | – | PR-introduced | "A response is a transcript line whose `message` is an object with a `usage` object and a model other than `<synthetic>`, counted once per `message.id`, with the usage of the line that has the most output tokens and the earliest timestamp" now has three verbatim owners: `summarize`, `turns_of`, and the new `context_of`, which copies the first two lines of `turns_of` without its `model` field. The three agree today. But if one filter changes (for example, another message kind is excluded, or the dedupe key changes), the new context figures and the response counts in the same row are computed over different response sets, and nothing flags it, because the script has no tests. The diff gives no justification for a third copy. | Define the response filter once (a shell variable such as `responses='…'` holding the select, group_by, and per-message map, with `model` and `effort` included) and prepend it to `summarize`, `turns_of`, and `context_of`. Alternatively, derive the context fields from the per-response list that `turns_of` already builds. |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which transcript lines are responses | `context_of`: lines with an object `message`, an object `usage`, and a model other than `<synthetic>`; grouped by `message.id` | `summarize` and `turns_of`, same select and group_by | Identical today (three copies, finding #1) | ok in behavior, finding #1 for ownership |
| First and last response | `context_of`: earliest block timestamp per message id, sorted by timestamp over all prompts and tiers | `summarize.first_message`/`last_message` (min and max of the same per-message timestamp, per tier) | Same timestamp per response, so `last_response_at` equals the maximum of `models[].last_message` | ok |
| A spawn without a response | `context_of`: `length == 0` → four `null`s | `summarize` → empty `models`, so no Markdown row; Markdown prints `-` for null growth | Agree: a null-growth spawn has no Markdown row anyway, and `--json` shows the nulls | ok |
| Cold or warm cache (`squad/lead.md` §4) | "last response more than about 5 minutes old", judged when the follow-up is sent; a follow-up that waits for the maintainer or another role counts as cold, and so does a last response just under 5 minutes old | Change doc *What*, bullet "The cache state"; retirement at the warm number, continued between the numbers only while warm | Wording agrees in both places | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| The size rule for continue or fresh (numbers and signal) | `squad/lead.md:50` only. `rg "40 responses|roughly twice|100 thousand|per response"` outside `squad/notes/` and `sdd/work/` finds no other live statement. `sdd/changes/2026-11/squad-resume-or-fresh-and-lead-compaction.md:12,32` keeps the old numbers as a historical, accepted decision, and the new doc names it under `Depends on`. `sdd/README.md` has no supersede convention | one owner, ok |
| Response dedupe per message id | `scripts/usage.sh` `summarize`, `turns_of`, `context_of` | finding #1 |
| Definition of context and growth | `squad/lead.md:50` (parenthetical), `scripts/usage.sh` header and `context_of` comment, `scripts/README.md:133`, change doc *What*. All state the same definition (input + cache read + cache write; latest minus first over all prompts and tiers) | The definition is stated in four places, but they are consistent descriptions of one implementation (`context_of`), so no drift is possible in behavior; ok |

## Assertion adequacy
_N/A — the run adds and changes no test (lead decision 7: `scripts/usage.sh` has no test project). I checked the script's fields on this session's transcripts and on a constructed transcript (below)._

## Failure classes dismissed
- FC-001, FC-002, FC-003, FC-005, FC-016, FC-044: no formatter or analyzer guard. The script's response filter is covered under Guard scope and finding #1.
- FC-004, FC-019 to FC-021: no edit of user code, trivia, or directives.
- FC-006, FC-022 to FC-027, FC-029, FC-036: no rewrite of C#.
- FC-007 to FC-009, FC-018, FC-028, FC-030, FC-043: no code fix.
- FC-010 to FC-014: no formatter phase or analyzer.
- FC-038 to FC-042: no unsupported shape declared or touched.
- FC-017, FC-033 to FC-035: no test added or changed.
- FC-031: the changed usage.sh header, the new `context_of` comment, and the README bullets describe the new code. The unchanged `summarize` comment is still true.
- FC-032: no compiled change. The cleaner's Release build (0 warnings) covers the unchanged tree.
- FC-037: no C# file changed, so no text-only proof is needed.
- No `TODO`, no tracker number in the changed text (the `#123` examples in `squad/lead.md:11` are pre-existing). Scope is limited to the four files the issue and change doc name.
- Visible output (check 10): no visible surface changes. No test, rule page, formatter output, diagnostic, code fix, or CLI behavior is involved.
- Special cases (check 11): not applicable.

## Architecture findings
_None._ Finding #1 is a local duplicate inside one script. The other copy (`summarize` and `turns_of`) predates the run.

## Hints
- **Negative growth loses its sign in the Markdown table.** `n` (`scripts/usage.sh:212`) keeps only digits, so a negative `context_growth_tokens` prints as positive. A constructed transcript (first response 2 + 56000 + 40000, last 2 + 20000 + 30000) gives `--json` `-46000` and Markdown `46.000`. Whether a real transcript gets there is uncertain: only a transcript whose latest response has a smaller context than its first, such as the lead's main session right after a compaction, could. Claude Code re-attaches recently read files after a compaction, so the post-compaction context probably stays above the first. That is why this is a hint, not a finding. If it matters, print a sign for negative values.
- **The lane widening rests on a premise the lane check contradicts.** Decision 3 widened tooling → behavior change because "a change doc under `sdd/changes/` makes the diff need behavior change". `scripts/lane-check.sh behavior-change` reports that the diff needs only tooling. A wider claim is admitted, so nothing breaks. The lead may want to correct decision 3's reason, and decision 7's skip of the test implementer that the widened lane required.
- `last_response_at` is the timestamp of the latest response's first content block, which comes after the start of the request that refreshed the cache. The lead's clause "just under 5 minutes counts as cold" already absorbs that margin.

## Follow-up candidates
_None._
