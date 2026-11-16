Role: spec author
Commit: 0903c13
Verdict: change doc written

Amendment of `sdd/changes/2026-11/squad-large-agent-by-context-growth.md` to lead decision 4 (skeptic option A plus evidence corrections). `Status` stays `Accepted` (decision 5). My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decisions.

## What the implementer builds against it

- `squad/lead.md`, section 4, size paragraph: large at context growth of about 50 thousand when the agent's last response is more than about 5 minutes old (cache cold), about 150 thousand when it is more recent (cache warm); judged when the follow-up is sent, and a follow-up that waits for the maintainer or another role counts as cold. Drop the 40-response signal, the per-response average, and "roughly twice". A continued agent that returns at or above the warm number is retired; between the two numbers it is continued only while warm. Everything else of the paragraph stays.
- `scripts/usage.sh --json`, per spawn (not per tier): `first_context_tokens`, `last_context_tokens`, `context_growth_tokens`, and `last_response_at` (latest response timestamp over all prompts and tiers). Context = input + cache read + cache write; responses once per message id; first and last by timestamp. Markdown: a *Context growth* column only (no time column). Header comment and `scripts/README.md` name the figures.
- Sanity points from this session's transcripts (first → last context, growth): spec author `a91f2076248c84481` 79.785 → 132.280, 52.495; skeptic `acb27825dcd397be6` 79.386 → 211.251, 131.865. Their last responses: 19:00:26.114Z and 19:07:43.900Z.

## Re-derived figures, and where mine differ from the skeptic's

All aggregates re-derived from the 23 notes' usage tables (557 rows, 534 subagent spawns, 0 multi-tier, 77 multi-prompt) and the three subagent transcripts of this session; prices from the `claude-api` reference (large tier: cache read 0.05, 5-minute write 1.25, output 5 times input).

- Response-count table, 247 / 213 of 219 / 153 / 98: reproduce. I replaced the doc's median ranges with the medians I get per bin: 72k, 93k, 121k, 170k.
- Small spawns (≤ 4 responses): 11, average context 75–85k; 9 of them wrote 76–88k on their own. Agrees with the skeptic.
- Growth per role (B = 80k): same as the skeptic's within interpolation; mine are in the doc (for example implementer upper quartile 80–120k where the skeptic has 83–127k). Difference in reading: the cache-write estimator is not a neutral alternative; on this run's transcripts it understates by about 56k, because both spawns read that much baseline from the cache, and the linear one overstates (60 vs 52k, 157 vs 132k). The doc states both biases.
- Continuable single-prompt spawns (195): 70–120 at ≥ 50k, 30–46 at ≥ 120k, 20–35 at ≥ 150k.
- Orientation (first seven responses): Δ = 21k / 38k / 26k and R + O = 44k / 41k / 47k for spec author, skeptic, and this agent. The skeptic used only the first.
- Cold break-even: 50 / 46 / 40k for k = 5 / 10 / 20 with the spec author's figures (agrees), up to about 61k at k = 10 with the skeptic's own transcript.
- Warm break-even: I keep only the skeptic's "prefix read" row (about 220k at k = 10, 121k at k = 20) and drop its "fresh agent writes its whole baseline" row (358k / 189k): while the continued agent's cache is warm, the shared 56k prefix is warm too, since the continued agent read it within the last 5 minutes. So the warm case is about 120–220k for k = 10–20, and about 150k is its middle.
- The 5–6 M run: the note's "spec author 24/26, test implementer 32/36" are decision numbers, not spawn numbers, so the follow-ups cannot be matched to spawns one by one. The skeptic names spec author 6 and test implementer 7 as the spawns "they followed"; I could not confirm that mapping. What holds: the note itself says they should have continued under the table, so the run shows a departure from it. The doc says that and names no note file.
- Cache lifetime counts from the start of the request (per the `claude-api` prompt-caching reference), so a last response just under 5 minutes old is already near expiry; the doc counts it as cold.

## Changed requirements

None — process change.

## Examples for the tests

None. The figures above are the sanity points for `usage.sh`.

## Output questions

_None._

## Internal choices

- Retirement uses the warm number (alternative: the cold number, which would retire agents a warm correction could still continue cheaply).
- The cache state is judged when the follow-up is sent, not when the agent returned (alternative: at return; wrong for follow-ups that wait for the maintainer or another role).
- `last_response_at` is in `--json` only, not a Markdown column (alternative: a column; a note read later cannot use it).
- The doc's growth table keeps three quantiles with both estimators as a range, rather than replacing it with a sentence (skeptic's alternative), because the ranges still place the two numbers.

## Follow-up candidates

- The lead's follow-up candidate (continuing for the maintainer's answer after cache expiry) is absorbed: that row is now judged cold, at about 50k. Nothing remains beyond revisiting the numbers on the measured growth of later runs, which the doc already states.
