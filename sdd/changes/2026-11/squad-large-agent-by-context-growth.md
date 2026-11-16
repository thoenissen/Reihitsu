# The lead measures a large agent by its context growth

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | none — process change |
| Depends on | [The lead chooses between continuing and spawning, and keeps its context small](squad-resume-or-fresh-and-lead-compaction.md) |

## What

The size signal of the continue-or-spawn table in the lead's playbook changes. An agent is large when its **context growth** is about 120 thousand tokens or more. The two earlier signals, about 40 responses and about 100 thousand cache-read tokens per response averaged over the spawn's lifetime, are dropped, and so is the remark that the current context is roughly twice that average.

- **A response's context** is the whole prompt that response was sent: its input tokens, cache-read tokens, and cache-write tokens added together.
- **Context growth** is the context of the spawn's latest response minus the context of its first response, over all its prompts (a continuation through `SendMessage` included) and all model tiers. The first response's context is the system prompt, the agent definition, the repository instructions, and the task prompt, before the agent has read anything; it is the same for a continued and a fresh agent, so it is subtracted.
- **`scripts/usage.sh --json`** prints, for every spawn (the lead's row included), `first_context_tokens`, `last_context_tokens`, and `context_growth_tokens`. The Markdown output gains a *Context growth* column, one value per spawn, repeated on each tier row of a spawn that used several tiers. `scripts/README.md` names the new figures.
- The playbook keeps everything else of the paragraph: the number is a starting point taken from past runs, not a gate; a continued agent that returns at or above it is retired; the lead reads the figure with `--json` and a filter; a departure is recorded under *Decisions taken*.

## Why

The table's "continue" rows are meant for an agent that is still small, but the old signal counted nearly every working agent as large. The usage tables of the squad's run notes (23 notes with a table, 534 subagent spawns) show why: the per-response average is almost a function of the response count, because every agent starts from the same baseline and grows from there.

| Responses of the spawn | Spawns | Median cache read per response | At or above 100 thousand |
|---|---|---|---|
| 1–9 | 187 | 60–72 thousand | 0 |
| 10–19 | 128 | 90–99 thousand | 34 |
| 20–39 | 124 | 115–129 thousand | 118 |
| 40 or more | 95 | 148 thousand and more | 95 |

- The 100-thousand average is crossed at about 20 responses, so the 40-response signal never decides anything: 213 of the 219 spawns with 20 or more responses were already large by the average.
- Of the 247 spawns of the roles the lead may continue (spec author, test implementer, implementer), 153 were large by the average; 98 of them only by the average, with fewer than 40 responses.
- The baseline is high before any work. A subagent's first response already carries about 80 thousand tokens of context, of which about 56 thousand are a prefix shared with other agents, and reading the role file, `squad/README.md`, `00-lead.md`, the handovers, and the specs brings it to about 100 thousand, measured from the per-response transcript of a spawn in the run that makes this change. A lifetime average over a context that starts at that baseline reaches 100 thousand after modest work; at that average the current context is about 130–140 thousand, not the roughly 200 thousand the playbook assumed when it called the current context "roughly twice" the average.
- One run spawned fresh agents for follow-ups the table's continue rows were meant for, and paid about 5–6 million cache-read tokens to re-orient them.

Context growth separates the two cases the table has to tell apart. The notes record no per-response figures, so growth was estimated per single-prompt spawn from its totals, in two ways: from the average context under linear growth, and from its cache writes less the first response's write. Both estimates agree on the picture.

| Role | Median growth | Upper quartile |
|---|---|---|
| Cleaner | 35–55 thousand | 40–60 thousand |
| Spec checker | 55–70 thousand | 65–80 thousand |
| Spec author | 75–85 thousand | about 125 thousand |
| Implementer | 70–105 thousand | 130–155 thousand |
| Test implementer | 85–115 thousand | 155–185 thousand |
| Skeptic, auditor | 160–180 thousand | 185–220 thousand |

The roles that only orient themselves and run one step, the cleaner and the spec checker, stay below about 95 thousand even at their 90th percentile; that is the orientation baseline. A threshold of about 120 thousand sits clear above it and marks the upper quarter or so of the roles the lead may continue: about 60–70 of their 195 single-prompt spawns, where the average counted 153 of their 247 spawns as large. The skeptic and the auditor, which are always fresh, are large by this measure too, which agrees with the table.

The figure has to add cache writes to cache reads. A subagent's cache lives five minutes, and a follow-up usually reaches a continued agent later than that, after the maintainer answered or another role ran. Its first response then reads little from the cache and writes its whole context again, so the cache read of the latest response alone would show a continued agent as small exactly when continuing it costs most.

## Visible output changes

None — no output an end user sees changes. The change is to the squad's playbook and to the report of a squad script.

## Decisions

### Growth since the first response, not the latest context

The context of the latest response would also work, but it includes the baseline of system prompt, agent definition, and repository instructions, which differs by role and tier and which a fresh agent pays as well. Growth subtracts it, so one number fits every role. `--json` prints both ends, so the lead can still read the absolute size.

### One signal, no response count

What a continued agent costs is its context on every response, not how many responses it took to build it: an agent that ran forty test commands is small, one that read three large files in ten responses is not. A second signal would only reintroduce a threshold that, like the old 40 responses, decides nothing on its own.

### 120 thousand as the starting point

The number is placed by the evidence above, between the orientation baseline (below about 95 thousand) and the growth of the larger working agents. It is a starting point, as before. A cost model of continuing against spawning fresh depends on the follow-up's length and on whether the agent's cache is still warm, so it brackets the break-even only loosely (about 100–150 thousand of context a fresh agent would not need); the notes of the runs after this change carry the measured growth, so the number can be revisited on measurements instead of estimates.

### The Markdown report carries the figure too

The run notes paste the Markdown output, and their missing per-response figures are why this change had to estimate growth. With a *Context growth* column the next evaluation measures it.

## Alternatives considered

- **Raise the per-response average** (for example to 130 thousand) — rejected: it still mixes the baseline with the growth and still tracks the response count, so it moves the problem instead of removing it.
- **The cache read of the latest response** — rejected: after a cache expiry it drops to the shared prefix, so it reports a continued agent as small at the moment its context is written again in full.
- **The response count alone** — rejected: the count is a proxy for context, and a poor one between roles that read much and roles that run commands.
- **Keep both old signals and add growth** — rejected: three numbers for one decision, two of which the evidence shows to be redundant or misleading.
