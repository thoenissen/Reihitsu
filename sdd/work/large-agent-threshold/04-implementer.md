Role: implementer
Commit: (this commit)
Verdict: tests pass

No handed-over tests (lead decision 7). Verified against the sanity points of `03-spec-author.md` on this session's transcripts; both reproduce exactly. My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the accepted change doc.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `scripts/usage.sh` | New jq filter `context_of` per transcript: responses once per message id, context = input + cache read + cache write, sorted by timestamp over all prompts and tiers; adds `first_context_tokens`, `last_context_tokens`, `context_growth_tokens`, `last_response_at` to every spawn (lead row included), `null` when a transcript has no response. Markdown spawn table gains a *Context growth* column (`-` when null), repeated on each tier row. Header comment names the figures. | change doc, *What* (usage.sh bullet) |
| `scripts/README.md` | Usage section: a *Context growth* bullet and the four new `--json` fields | change doc, *What* |
| `squad/lead.md` | Section 4 size paragraph: cache-aware growth rule (~50k cold / ~150k warm, judged when the follow-up is sent, waiting follow-ups and a last response just under 5 min count as cold); old 40-response and per-response-average signals and "roughly twice" dropped; retirement at the warm number, continued between the numbers only while warm. Rest of the paragraph unchanged; the table row "The agent is large, ..." unchanged. | change doc, *What* |

## Policy owners
`rg -n "usage\.sh|Cache write|roughly twice" --glob '!squad/notes/**' --glob '!sdd/work/**' --glob '!sdd/changes/**' .` — only `scripts/usage.sh`, `scripts/README.md`, and `squad/lead.md` (sections 4 and the note step in section 7, which just runs the script). The size rule has one owner, `squad/lead.md` section 4.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: empty (no `src` changes). No new type. Working diff: 3 files (script, README, playbook).

## Deleted special cases
_N/A._

## Tests run
- `bash -n scripts/usage.sh` — OK. `shellcheck` is not installed in the sandbox.
- `scripts/usage.sh --session ac22de41-b2e9-58d2-88fe-1db1aec6921b --json | jq -c '.spawns[] | {agent, agent_id, first_context_tokens, last_context_tokens, context_growth_tokens, last_response_at}'`:
  - spec-author `a91f2076248c84481`: 79785 → 132280, growth 52495, last 2026-11-16T19:00:26.114Z — matches the sanity point.
  - skeptic `acb27825dcd397be6`: 79386 → 211251, growth 131865, last 2026-11-16T19:07:43.900Z — matches.
  - also: lead 96047 → 150745 (54698); spec-author 2 `aa6ac2701d94b860a` 79794 → 182288 (102494).
- `scripts/usage.sh --session ac22de41-…` (Markdown): header ends `| Cache write | Context growth |`, each spawn row ends with its growth (`52.495`, `131.865`, …); the per-tier and total table is unchanged.
- Edge: a transcript with one user line and no response (`--projects-dir <scratch> --session s`): fields are `null`, exit 0, no rows.
- No multi-tier spawn exists in this session; the column is the spawn's value by construction (`$s.context_growth_tokens`), repeated on each `$s.models[]` row.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None._
