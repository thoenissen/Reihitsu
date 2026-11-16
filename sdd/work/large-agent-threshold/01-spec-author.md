Role: spec author
Commit: 2c6c668
Verdict: change doc written

Change doc: `sdd/changes/2026-11/squad-large-agent-by-context-growth.md` (`Status: Draft`, `Affected requirements: none — process change`, visible output changes: none).

## What the implementer builds against it

- `squad/lead.md`, section 4, size paragraph: an agent is large at a context growth of about 120 thousand tokens or more (`context_growth_tokens` from `scripts/usage.sh --json`); drop the 40-response signal, the per-response average, and "the current context is roughly twice that". Everything else of the paragraph stays. The table row "The agent is large, ..." needs no wording change.
- `scripts/usage.sh`: per spawn (not per tier), `first_context_tokens`, `last_context_tokens`, `context_growth_tokens`, where a response's context is `input_tokens + cache_read_input_tokens + cache_creation_input_tokens`, responses counted once per message id as today, first and last by timestamp over all prompts and tiers of the transcript. Markdown output: a *Context growth* column, the spawn's value on each of its tier rows. Update the header comment and `scripts/README.md`.
- Sanity point for the implementer: this spec author's own transcript (session `ac22de41-…`, agent `a91f2076248c84481`) starts at 56.329 read + 23.454 write = 79.785 context and was at about 100 thousand after orientation.

## Changed requirements

None — process change.

## Examples for the tests

None. `scripts/usage.sh` has no tests (lead decision: no test implementer).

## Output questions

_None._

## Internal choices

- Signal is growth since the first response, not the latest response's absolute context (alternative: absolute context; rejected because it carries the per-role baseline a fresh agent pays as well). Both ends are printed.
- The response-count signal is dropped, not kept beside growth (alternative: keep 40 responses as a second signal; the notes show it never decided anything, since every spawn with 20+ responses was already large by the average).
- Context sums read, write, and input, not the issue's suggested "cache read of the last response" (subagent caches are 5-minute caches, as the transcripts' `ephemeral_5m_input_tokens` show; a continuation after expiry reads little and rewrites everything, so cache read alone would call it small).
- The Markdown report gains the column too, not only `--json` (the issue asks only for `--json`; the notes paste the Markdown, and their lack of per-response figures is why this doc had to estimate).
- 120 thousand is an estimate from aggregates (two estimators, linear-average and cache-write based, which agree within about 30 thousand per role); stated as a starting point.

## Issue goals checked

- "The size signal separates an agent that only oriented itself from one whose context grew": met by growth; orientation-only roles (cleaner, spec checker) stay below about 95 thousand at their 90th percentile.
- "`scripts/usage.sh --json` prints the figure": met.
- "Starting points, as today": kept.
- The issue's premise holds in substance but is overstated in one point: not "nearly every agent" overall ends above 100 thousand per response (247 of 534 subagent spawns), but nearly every spawn with 20 or more responses does (213 of 219), and 153 of 247 spawns of the continuable roles.

## Follow-up candidates

- Continuing after cache expiry. Subagent caches live five minutes; a follow-up for the maintainer's answer (the table's first "Continue" row) usually arrives later, so the continued agent writes its whole context again at the write price before doing anything. The table assumes continuing is cheap for a small agent; for an expired cache that holds only if the context is near the orientation size. Worth measuring with the new `--json` figures (per prompt: the first response's cache write of a continuation) before changing the table. Out of this issue's scope (the issue is about the size signal only).
