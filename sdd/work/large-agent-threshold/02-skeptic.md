Role: skeptic
Commit: 0d04c23
Verdict: proceed (change the number before the implementer)

My prompt carried no intended fix or hoped-for verdict. The lead's acceptance of the change doc (decision 2, 9522a08) came before this skeptic ran. That was correct while the lane was tooling, but `lead.md` section 4 accepts a change doc only "once the skeptic has run and any amendment it caused is written". The acceptance should be made again after the amendment below.

## Premise

What I checked: the issue (thoenissen/Reihitsu#1792, `agent-filed`, no comments), `squad/lead.md` section 4, `scripts/usage.sh`, the usage tables of all 23 notes under `squad/notes/` (parsed: 557 rows, 534 subagent spawns, 0 spawns with more than one tier, 77 with more than one prompt), and this session's transcripts under `~/.claude/projects/-home-user-Reihitsu/ac22de41-…/` (lead, spec author `a91f2076248c84481`, this skeptic).

- **Reachability.** The usual caller check does not apply here, because no shipped surface is involved. The playbook paragraph is real and in force: `squad/lead.md` section 4 says "about 40 responses or more, or a cache read of about 100 thousand tokens per response or more (the lifetime average; the current context is roughly twice that)".
- **What reproduces.** The spec author's aggregate table reproduces exactly from the notes. By response count: 1–9 responses, 187 spawns, 0 at or above 100k average; 10–19, 128 spawns, 34; 20–39, 124 spawns, 118; 40 or more, 95 spawns, 95. Also: 213 of 219 spawns with 20 or more responses are large; 247 of 534 spawns overall; 153 of the 247 continuable spawns; 98 of those large only by the average. So the old per-response average does track the response count, and the 40-response signal decides nothing. The core claim is real: the old signal is a poor proxy.
- **The title is false as stated.** The title says the threshold is "below what every agent reaches by orientation alone", but an orientation-only spawn never reaches it: 0 of the 187 spawns with 1–9 responses averages 100k or more. Their median average is 60–72k. The issue's "80–90 thousand per response just by orientation" is the absolute context after orientation. The old threshold is a lifetime average, and that average starts lower.
- **The flagship evidence does not support the causal claim.** The issue's evidence is "one run spawned fresh agents … about 5–6 million cache-read tokens". The source note says those follow-ups "should have continued the previous agent under the continue-or-fresh table but were spawned fresh". The spawns they followed were small under the old rule:
  - spec author 6: 18 responses, 1.662.009 / 18 = 92k average;
  - test implementer 7: 12 responses, 1.145.230 / 12 = 95k average.

  So that run shows the lead departing from the table. It does not show a threshold that is too low.
- **Severity is overstated.** The 5–6 M is counted in cache reads, the cheapest token class. On the large tier those cost 0.05× base input: cache reads $0.20/MTok against $4 input for the current large-tier model, per the `claude-api` reference. That makes the 5–6 M worth about 0.25–0.3 M input-equivalents. It is also a gross figure, because continuing those agents would have cost something too. The per-decision stakes under the cost model below are on the order of ±0.1–0.2 M input-equivalents, about one small fresh spawn. The change is worth making because it is cheap, not because it is costly to leave.

## Mechanism and candidates

_N/A._ This is a process change, not a bug.

## Breaking inputs

These are the two measurement questions the change doc's signal and number depend on, settled on this session's transcripts.

1. **The signal can be measured.** `usage.sh` can compute it, and the definition (input + cache read + cache write per response, responses once per message id) gives the real prompt size whatever the cache state. Check on the spec author's transcript (jq over `agent-a91f2076248c84481.jsonl`, grouped by message id, sorted by timestamp):
   - first response: 2 + 56.329 + 23.454 = **79.785**;
   - last response (25th): **132.280**;
   - growth **52.495**.

   That transcript has 2 prompts: the status-only continuation arrived at 19:00:19, while the cache was still warm, and its 2 responses (19:00:22, 19:00:26) are the last ones, so first and last over all prompts work. The same agent averaged 2.673.171 / 25 = 107k cache read per response, so the old rule calls it large and the new rule calls it small. That is the intended separation.
2. **The 5-minute cache.** Subagent caches are 5-minute caches: `cache_creation.ephemeral_5m_input_tokens` holds every cache write of both subagent transcripts (143.464 and 223.170), with `ephemeral_1h` = 0. The lead's main session is the reverse, all 1-hour (`ephemeral_1h` 168.547, `ephemeral_5m` 0). The change doc's claim holds for subagents.

### The growth table in the change doc does not reproduce

The doc's own estimator does not reproduce from the notes. The notes give one row per spawn and no per-response figures, so growth can only be estimated, and the estimate depends on the baseline B, which the notes do not record.

- **The notes put the baseline at about 80k.** The 11 spawns with at most 4 responses have an average context of 75–85k. In 9 of them the cache write is close to the whole context (76–88k), which means their first response wrote the full baseline and read no shared prefix. The doc's "56 thousand shared prefix" is what this session saw, not what the notes show.
- **The 56k prefix is a poor baseline.** With B = 56k, the linear estimator gives 108k on the spec author's real transcript. With B = 80k it gives 60k. The real growth is 52k.

Recomputed per role over single-prompt spawns, with B = 80k:

| Role | Linear median / q75 / q90 | Cache-write − 80k median / q75 / q90 |
|---|---|---|
| Cleaner | 4k / 11k / 17k | 5k / 8k / 12k |
| Spec checker | 25k / 35k / 54k | 23k / 30k / 45k |
| Spec author | 44k / 93k / 156k | 33k / 76k / 120k |
| Implementer | 74k / 127k / 228k | 21k / 83k / 146k |
| Test implementer | 86k / 155k / 261k | 34k / 105k / 213k |
| Skeptic | 147k / 189k / 215k | 130k / 152k / 187k |
| Auditor | 140k / 181k / 267k | 109k / 136k / 226k |

The doc's figures sit 25–50k higher: cleaner 35–55k, spec author 75–85k. They fit neither B. Its two estimators "agree within about 30 thousand"; mine do not at either B (implementer at B = 56k: 122k against 45k). Consequences:

- **"Orientation baseline" mixes absolute context with growth.** The doc says orientation-only roles "stay below about 95 thousand … that is the orientation baseline". But orientation-only roles (cleaner, spec checker) grow by 5–55k, not 95k. A 120k threshold therefore sits far above orientation growth, and that baseline is not what places the number.
- **Share of continuable single-prompt spawns at 120k:** 30–46 of 195 at B = 80k, 41–87 at B = 56k. The doc says "about 60–70".

### The number: cost break-even depends on whether the cache is warm

This is the substantive challenge. The doc says a continuation usually arrives after the agent's 5-minute cache has expired. It is right, and structurally so: every "Continue" row's follow-up comes after the maintainer answered or after another role ran. In this run, any amendment for the spec author arrives after this skeptic, more than 10 minutes after its last response at 19:00:26. Yet the doc's break-even bracket, "about 100–150 thousand of context a fresh agent would not need", matches the warm-cache case.

Model inputs:

- **Prices** (large tier, current model, from the `claude-api` reference): cache read 0.05×, 5-minute cache write 1.25×, output 5×.
- **Fresh agent's orientation**, from the spec author's real transcript: reaches about 100k context at response 7 (Δ ≈ 20k over the 80k baseline). Its reads cost 590k × 0.05 ≈ 29.5k and its output 2.787 × 5 ≈ 14k, so R + O ≈ 44k input-equivalents.
- **Follow-up length:** k responses. Both sides pay the same for the follow-up's own new tokens. C = 80k + g is the continued agent's context.

| Cache at the follow-up | Continue − fresh | Break-even growth g |
|---|---|---|
| Cold (last response more than about 5 min ago) | (1.25 + 0.05k)(g − Δ) − (R+O) | k = 5: about 49k; k = 10: about 45k; k = 20: about 40k |
| Warm, fresh agent reads the 56k prefix from cache | 0.05k(g − Δ) − (1.25 × 44k + R+O) | k = 10: about 218k; k = 20: about 119k |
| Warm, fresh agent writes its whole baseline (the notes' usual case) | 0.05k(g − Δ) − (1.25 × 100k + R+O) | k = 10: about 358k; k = 20: about 189k |

At 120k, the doc's number is right for neither regime. For the usual cold continuation, continuing an agent with 50–120k growth costs more than a fresh one. For example, g = 120k, k = 10, cold: continuing costs about 1.75 × 100k − 44k ≈ 131k input-equivalents more than a fresh agent. The old rule, which called nearly every working agent large, was close to the cold optimum by accident. Raising the number moves cold continuations to the expensive side.

This is an estimate from one transcript plus list-price ratios. It argues for the regime split, not for exact numbers.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `squad/lead.md`, section 4, size paragraph | Must change. Signal: growth. Number: see Internal choices |
| `squad/lead.md`, continue-or-spawn table rows | Must stay. The row "The agent is large, …" needs no wording change |
| `scripts/usage.sh` | Must change: per-spawn first and last context and growth in `--json`, plus a *Context growth* column in the Markdown table. If choice 1 below is taken, also the spawn's last response time, which `--json` already prints today per tier as `models[].last_message` |
| `scripts/README.md`, usage section | Must change: name the new figures |
| `sdd/changes/2026-11/squad-large-agent-by-context-growth.md` | Must change: the growth table, the "orientation baseline" sentence, the 56k prefix sentence, the cost bracket, and the number (Internal choices) |
| `squad/notes/` | Must stay. Nothing may name a note (`SquadNoteReferenceTests`). The change doc names no note file, and must keep it that way |
| Shipped surfaces (analyzer, formatter, code fixes, CLI, rule docs) | Not involved |
| Tests | Not involved. No test runs `usage.sh`, and nothing outside notes parses its Markdown header (`rg 'Cache write'` finds only `scripts/usage.sh`) |

## Refuse option

**Do not make the change.** Keep the old signal. It costs nothing, and for cold follow-ups, which the doc says are the usual case, it already sends nearly every working agent fresh, close to the cost-optimal move. Against it: the old signal is badly defined (it tracks response count and mixes baseline with growth), and the notes cannot measure growth until `usage.sh` prints it.

The requested change is small: a few lines in `lead.md`, roughly ten lines of jq, and a README line. So the refuse option saves little.

**Simpler option:** keep the signal change and the `usage.sh` figures, and pick the number for the cold case, as below. No unsupported-shape option applies: there is no formatter or code-fix shape.

## Output questions

_None._ Nothing an end user sees changes.

## Internal choices

1. **The number.** All three options keep the growth signal and the cache-write-inclusive context definition, which are sound.
   - **A (recommended): a cache-aware size rule.** An agent whose last response is more than about 5 minutes old is large at about 50k growth. One whose last response is more recent is large at about 150k. The lead reads both figures from `--json`, and the 5-minute rule stays a starting point. Consequence: most cold continuations go fresh, as today; a correction sent right after an agent returns is continued even when the agent is larger. It needs the spawn's last response time, which `--json` already carries per tier.
   - **B: one number for the cold case,** about 50k. Simplest wording. It gives away the cheap warm continuations.
   - **C: the doc's 120k.** Continues cold agents with 50–120k growth, which the cost model says costs more than a fresh agent.

   Recommend A, because it is the only option consistent with the doc's own statement that follow-ups usually arrive after the cache expired. B is acceptable if the lead wants one number. Either way, the doc should say that the numbers come from one transcript plus list-price ratios and should be revisited with the new column.
2. **Correct the change doc's evidence.** The growth table should be either recomputed with B ≈ 80k, as above, or replaced by a sentence saying growth cannot be measured from the notes. The sentence "about 56 thousand are a prefix shared with other agents" should say this is the case only when another agent wrote that prefix within 5 minutes; in 9 of 11 small spawns in the notes, the first response wrote the whole baseline. The doc's premise statement should note that orientation-only spawns never reached the old average, and that the 5–6 M run was a departure from the table. No visible output either way.
3. **The lead's row in `--json`.** Growth means nothing for the session agent, which is never continued or spawned and runs on a 1-hour cache. Printing it is harmless. Recommend leaving the fields on every row, as the doc says, rather than adding a special case.

## Issue goals

- **Goal 1** (the size signal separates orientation from growth): keep. Growth meets it. Change its tail, "chosen from the usage tables of the remaining run notes". Those tables can justify the choice of signal but cannot measure growth or place the number, so the number has to be stated as an estimate. No visible output.
- **Goal 2** (`--json` prints the figure): keep. Under choice 1A, the last response time also counts as a figure the signal uses; it is already printed per tier. No visible output.
- **Goal 3** (starting points): keep.
- **Title and premise:** overstated. Orientation alone does not reach the old threshold, and the cited run departed from the table rather than being misled by it. The PR should say so. No visible output.

## Architecture findings

_None._

## Follow-up candidates

- The spec author's candidate (continuing after cache expiry) is mostly absorbed by Internal choice 1A, if the lead takes it. If not, it stays a follow-up, now with the cost model above as its starting evidence.
- `usage.sh --json` gives `first_message` and `last_message` per tier only, not per spawn. If choice 1A is taken, the lead needs the maximum over tiers. That is trivial today, because no spawn in the notes used two tiers, and it can be done in this run's `usage.sh` change instead of a follow-up.
