# The lead measures a large agent by its context growth

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | none — process change |
| Depends on | [The lead chooses between continuing and spawning, and keeps its context small](squad-resume-or-fresh-and-lead-compaction.md) |

## What

The size signal of the continue-or-spawn table in the lead's playbook changes. An agent is large when its **context growth** reaches the number for the state of its cache at the follow-up:

- **Cache cold** — its last response is more than about 5 minutes old: large at a context growth of about **50 thousand** tokens or more.
- **Cache warm** — its last response is more recent: large at about **150 thousand** or more.

The two earlier signals, about 40 responses and about 100 thousand cache-read tokens per response averaged over the spawn's lifetime, are dropped, and so is the remark that the current context is roughly twice that average.

- **A response's context** is the whole prompt that response was sent: its input tokens, cache-read tokens, and cache-write tokens added together.
- **Context growth** is the context of the spawn's latest response minus the context of its first response, over all its prompts (a continuation through `SendMessage` included) and all model tiers. The first response's context is the system prompt, the agent definition, the repository instructions, and the task prompt, before the agent has read anything; it is the same for a continued and a fresh agent, so it is subtracted.
- **The cache state** is judged when the lead sends the follow-up, not when the agent returned: a follow-up that waits for the maintainer or for another role reaches a cold cache. A subagent's cache lives five minutes, counted from the start of the request that last read or wrote it, so a last response just under 5 minutes old counts as cold.
- **`scripts/usage.sh --json`** prints, for every spawn (the lead's row included), `first_context_tokens`, `last_context_tokens`, `context_growth_tokens`, and `last_response_at`, the timestamp of the spawn's latest response over all its prompts and tiers. The Markdown output gains a *Context growth* column, one value per spawn, repeated on each tier row of a spawn that used several tiers. `scripts/README.md` names the new figures.
- A continued agent that returns at or above the warm number is retired: its next follow-up goes to a fresh agent. Between the two numbers it is continued only while its cache is warm.
- The playbook keeps everything else of the paragraph: the numbers are starting points, not a gate; the lead reads the figures with `--json` and a filter; a departure is recorded under *Decisions taken*.

## Why

The table's "continue" rows are meant for an agent that is still small, but the old signal counted nearly every working agent as large. The usage tables of the squad's run notes (23 notes with a table, 534 subagent spawns, none of them on more than one tier) show why: the per-response average is almost a function of the response count, because every agent starts from the same baseline and grows from there.

| Responses of the spawn | Spawns | Median cache read per response | At or above 100 thousand |
|---|---|---|---|
| 1–9 | 187 | 72 thousand | 0 |
| 10–19 | 128 | 93 thousand | 34 |
| 20–39 | 124 | 121 thousand | 118 |
| 40 or more | 95 | 170 thousand | 95 |

- The 100-thousand average is crossed at about 20 responses, so the 40-response signal never decides anything: 213 of the 219 spawns with 20 or more responses were already large by the average.
- Of the 247 spawns of the roles the lead may continue (spec author, test implementer, implementer), 153 were large by the average; 98 of them only by the average, with fewer than 40 responses.
- Orientation alone does not reach the old threshold: none of the 187 spawns with fewer than 10 responses averaged 100 thousand. What it reaches is a high absolute context. A subagent's first response already carries about 80 thousand tokens of context; reading the role file, `squad/README.md`, `00-lead.md`, the handovers, and the specs brings it to about 100–120 thousand within its first seven responses, measured on the three subagent transcripts of the run that makes this change. A lifetime average over a context that starts at 80 thousand reaches 100 thousand after modest work; at that average the current context is about 130 thousand, not the roughly 200 thousand the playbook assumed when it called the current context "roughly twice" the average.
- Part of the baseline can come from the cache of another agent. In the three transcripts of this run, the first response read about 56 thousand from the cache, written by an agent that had run within the last 5 minutes, and wrote the rest. In the notes, 9 of the 11 spawns with at most 4 responses wrote 76–88 thousand on their own, that is, their whole baseline: no other agent had kept that prefix warm.
- One run's note records follow-ups that should have continued the previous agent under the table but were spawned fresh, at about 5–6 million cache-read tokens. That is a departure from the table, not evidence that its threshold was too low, and at the large tier's cache-read price (0.05 of the input price) it is worth about 0.25–0.3 million input tokens before subtracting what continuing would have cost. The change is worth making because the old signal is badly defined and the new one is cheap to print, not because the old one was costly.

### Growth by role

Context growth separates the two cases the table has to tell apart. The notes record no per-response figures, so growth can only be estimated from each single-prompt spawn's totals, with the first response's context taken as 80 thousand, in two ways:

- **linear** — twice the average context less 80 thousand, which assumes the context grows at a steady rate;
- **cache write** — the spawn's cache writes less 80 thousand, which assumes the first response wrote the whole baseline and no cache expired during the spawn.

Neither is exact. On this run's transcripts the linear estimate overstated the measured growth (60 against 52 thousand for the spec author, 157 against 132 thousand for the skeptic), and the cache-write estimate understated it by about 56 thousand, because both spawns read that much of their baseline from the cache. The table gives the two estimates as a range, cache write first.

| Role | Median growth | Upper quartile | 90th percentile |
|---|---|---|---|
| Cleaner | 4–5 thousand | 8–11 thousand | 12–17 thousand |
| Spec checker | 23–25 thousand | 30–35 thousand | 44–51 thousand |
| Spec author | 33–44 thousand | 74–91 thousand | 119–156 thousand |
| Implementer | 21–74 thousand | 80–120 thousand | 145–225 thousand |
| Test implementer | 34–86 thousand | 105–155 thousand | 211–259 thousand |
| Skeptic | 130–147 thousand | 152–187 thousand | 186–211 thousand |
| Auditor | 109–140 thousand | 134–179 thousand | 221–261 thousand |

The roles that only orient themselves and run one step, the cleaner and the spec checker, grow by about 5–50 thousand. The roles the lead may continue spread widely. Of their 195 single-prompt spawns, 70–120 reach 50 thousand and 20–35 reach 150 thousand. The skeptic and the auditor, which are always fresh, are large by either number, which agrees with the table.

The figure has to add cache writes to cache reads. A follow-up that reaches an agent after its cache expired makes the first response read little and write the whole context again, so the cache read of the latest response alone would show a continued agent as small exactly when continuing it costs most.

## Visible output changes

None — no output an end user sees changes. The change is to the squad's playbook and to the report of a squad script.

## Decisions

### Growth since the first response, not the latest context

The context of the latest response would also work, but it includes the baseline of system prompt, agent definition, and repository instructions, which differs by role and tier and which a fresh agent pays as well. Growth subtracts it, so one pair of numbers fits every role. `--json` prints both ends, so the lead can still read the absolute size.

### One signal, no response count

What a continued agent costs is its context on every response, not how many responses it took to build it: an agent that ran forty test commands is small, one that read three large files in ten responses is not. A second signal would only reintroduce a threshold that, like the old 40 responses, decides nothing on its own.

### Two numbers, by the state of the cache

Whether continuing is cheaper than a fresh agent depends on the agent's cache. The cost model, in multiples of the large tier's input price (cache read 0.05, five-minute cache write 1.25, output 5), compares an agent with growth `g` against a fresh agent that orients itself and then handles the same follow-up of `k` responses:

- **The fresh agent's orientation**, measured over the first seven responses of the three subagent transcripts of this run: it grows the context by `Δ` = 21–38 thousand, and its cache reads and output cost `R + O` = 41–47 thousand.
- **Cache cold.** The continued agent writes its whole context again; the fresh agent writes its baseline and its orientation. Either may read a prefix another agent keeps warm, which cancels out. Continuing costs `(1.25 + 0.05k)(g − Δ) − (R + O)` more. With the spec author's figures (`Δ` = 21, `R + O` = 44 thousand) that breaks even at about 50 thousand of growth for `k` = 5, 46 thousand for `k` = 10, and 40 thousand for `k` = 20; with the other two transcripts' figures, at up to about 60 thousand for `k` = 10.
- **Cache warm.** The continued agent only reads its context; the fresh agent writes the part of its baseline the cache does not hold, its orientation, and pays `R + O`. While the continued agent's cache is warm, the shared prefix of about 56 thousand is warm as well, so the fresh agent writes about 24 thousand of its baseline. Continuing costs `0.05k(g − Δ) − (1.25(24 thousand + Δ) + R + O)` more, which breaks even at about 220 thousand of growth for `k` = 10 and 120 thousand for `k` = 20.

Every "continue" row of the table follows the maintainer's answer or another role's work, so a follow-up usually reaches a cold cache, and for a cold cache the break-even is about 50 thousand. A correction sent right after the agent returns reaches a warm cache, where continuing stays cheaper up to about 150 thousand for a follow-up of typical length. One number cannot serve both: 50 thousand would send cheap warm continuations fresh, and a number in between would continue cold agents that cost more than a fresh one. The numbers rest on three transcripts and list-price ratios; they place the threshold to within tens of thousands, not exactly. They are starting points, and the notes of the runs after this change carry the measured growth, so they can be revisited on measurements.

### The Markdown report carries the figure too

The run notes paste the Markdown output, and their missing per-response figures are why this change had to estimate growth. With a *Context growth* column the next evaluation measures it. The last response's time stays in `--json` only: the lead needs it at the moment of a follow-up, and a note read later cannot use it.

## Alternatives considered

- **Raise the per-response average** (for example to 130 thousand) — rejected: it still mixes the baseline with the growth and still tracks the response count, so it moves the problem instead of removing it.
- **The cache read of the latest response** — rejected: after a cache expiry it drops to the shared prefix, so it reports a continued agent as small at the moment its context is written again in full.
- **The response count alone** — rejected: the count is a proxy for context, and a poor one between roles that read much and roles that run commands.
- **Keep both old signals and add growth** — rejected: three numbers for one decision, two of which the evidence shows to be redundant or misleading.
- **One number for the cold case, about 50 thousand** — rejected: the simplest wording, but it sends fresh the warm continuations that the cost model shows to be cheaper.
- **One number between the two, about 120 thousand** — rejected: it continues agents with a cold cache and 50–120 thousand of growth, which costs more than a fresh agent; at 120 thousand of growth and a ten-response follow-up, about 130 thousand input tokens more.
- **Keep the old signal** — rejected: for a cold cache it is near the right decision by accident, since it calls nearly every working agent large, but it measures the response count instead of the context, and the notes cannot measure growth until `usage.sh` prints it.
