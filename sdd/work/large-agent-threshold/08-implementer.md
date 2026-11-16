Role: implementer
Commit: (this commit)
Verdict: tests pass

Repair round 1 (lead decision 10): auditor finding #1 (FC-015) and the negative-growth sign hint. My prompt carried no intended fix beyond pointing at the auditor's suggestion, which I treated as a suggestion and re-derived; I took its first option because it leaves one owner and no field of any output changes.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `scripts/usage.sh` | The response filter (select of lines with an object `message`, an object `usage`, a model other than `<synthetic>`; once per `message.id`; earliest timestamp, raw model, first effort, usage of the line with the most output tokens) is now one jq function `responses`, held in `responses_def` and prepended to `turns_of`, `context_of`, and `summarize`; the three private copies are gone. `summarize` maps the raw model to its tier with `map(.model \|= tier)` after `responses`, so its grouping is unchanged. | auditor 07 finding #1 (FC-015) |
| `scripts/usage.sh` | The Markdown number formatter `n` prints a negative number with a leading `-` (it kept only digits before). Every other column is never negative, so only a negative *Context growth* cell changes. | lead decision 10 (07 hint) |

Comments: new comment on `responses_def`; the `summarize` comment drops "responses counted once per message id", which `responses_def` now states. The `turns_of`, `context_of`, and header comments are still true unchanged (FC-031).

## Policy owners
`rg -n 'select\(\(\.message \| type\)' scripts/` — one hit, `scripts/usage.sh:107` (`responses_def`). Before the repair: three (`turns_of`, `context_of`, `summarize`).

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: empty (no `src` change). No new type. Round delta: `scripts/usage.sh`, 18 insertions, 11 deletions.

## Deleted special cases
_N/A._

## Tests run
Old script = copy of `scripts/usage.sh` at de46002, run back to back with the repaired one (the session transcripts keep growing, so each pair is taken in one command).
- `bash -n scripts/usage.sh` — OK.
- `scripts/usage.sh --session ac22de41-b2e9-58d2-88fe-1db1aec6921b --json` vs old: `cmp` identical (9 spawns, 13256 bytes; efforts `high`, `low`, `medium` present, the lead with 5 prompts), so every field of every row, including models, efforts, turns, and the four context fields, is unchanged.
- Same session, Markdown vs old: `cmp` identical (all growth values positive).
- Constructed transcript (`--projects-dir <scratch>/proj --session neg`): two prompts, two tiers (large, medium), a duplicated content-block line for one message id (output 10 vs 30), a `<synthetic>` line, first context 2 + 56000 + 40000, last 2 + 20000 + 30000.
  - `--json` vs old: `cmp` identical; `first_context_tokens` 96002, `last_context_tokens` 50002, `context_growth_tokens` -46000, large 2 responses / 34 output / effort `high`, medium 1 response / effort `medium`, turns p1 = 1 and p2 = 2 responses.
  - Markdown diff vs old: only the two *Context growth* cells change, `46.000` -> `-46.000`.
- Constructed transcript with one user line and no response: the four context fields `null`, `models` and `turns` empty, exit 0.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None._
