# weekly-note-evaluation

| | |
|---|---|
| Date | 2026-11-27 |
| Run | Evaluation of the run notes from 2026-11-03 to 2026-11-27 (process topic, no issue) |
| Pull request | none (branch `weekly-note-evaluation`) |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | process | Implement the tool allow-list first and measure, or file all issues at once | A (allow-list first, then measure) | yes |
| 2 | process | Where the policy sections of `AGENTS.md` move | A (a doc under `sdd/`) | yes |
| 3 | process | One umbrella issue with sub-issues, or separate issues | B (separate issues) | no |

The maintainer asked to file only the allow-list issue for now, to delete no notes yet, and to measure about ten squad runs before continuing.

## Cost

### Cost per run

109 notes carry a usage table. Prices applied: large $4 / $20 per million input / output tokens, cache read $0.20; medium $2 / $10, cache read $0.10; small $0.10 / $0.50, cache read $0.01. Cache writes: the lead's session writes with the 1-hour lifetime (2× input), subagents with the 5-minute lifetime (1.25× input), both measured in this session.

| | |
|---|---|
| Mean per run | $26.50 |
| Median per run | $21.30 |
| Range | $2 to $145 |
| Total, 109 runs | about $2,900 |

Runs got cheaper: $45–80 in early November, $17–28 in the last days. Eight of the ten most expensive runs bundled two to six issues.

### Share per role, and spawns per run

| Role | Share | Spawns per run |
|---|---|---|
| test-implementer | 22.6 % | 2.2 |
| auditor | 17.6 % | 1.8 |
| spec-author | 17.6 % | 2.7 |
| implementer | 12.4 % | 1.6 |
| lead | 12.1 % | 1 |
| skeptic | 10.8 % | 0.9 |
| spec-checker, cleaner, issue-manager, validator | about 7 % together | |

These shares were computed with 2× cache writes throughout; with the measured lifetimes they shift only slightly.

### Where the tokens go

- Cache writes are the largest token cost. Their share was about 46 % under the first 2× assumption, and less with the measured 1.25× for subagents.
- Thinking is 2–13 % of a role's cost.
- Spawns after the first of the same role (amendments, repair rounds) are 28 % of the cost.
- Cache misses in subagents cost about $0.60 per run (about 2 %). They concentrate in the implementer and the test implementer, whose test runs take longer than the 5-minute cache. The estimate is cache write minus context growth minus startup context.

### Startup context, measured by probe spawns that read nothing

| | validator | implementer |
|---|---|---|
| First request | 23,166 tokens | 82,084 tokens |
| Of which `CLAUDE.md` + `AGENTS.md` | about 11k tokens (44k characters) | about 11k tokens |
| Tools | 6 (allow-list) | 56 (deny-list: every tool of the cloud session) |
| Cache lifetime | 5 minutes | 5 minutes |

The roughly 57k-token difference is tool definitions. About 30 of the 56 tools are `claude-code-remote` tools; the others include Artifact, Docs, and the plugin tools. Applied to the notes, those definitions cost about 26 % of a run; `AGENTS.md` costs about 5 %.

`CLAUDE.md` and `AGENTS.md` are injected into every subagent, the validator included, as an `instructions` attachment. There is no per-agent opt-out, so only a smaller `AGENTS.md` reduces that part.

### Size of the `AGENTS.md` sections (characters)

| Section | Characters | Proposed target (issue 2) |
|---|---|---|
| Formatter phase ownership table | 12.0k | A formatter architecture doc; `PhaseOwnershipDocumentationTests` reads the table from `AGENTS.md` and moves with it |
| High-level architecture | 2.8k | A formatter architecture doc |
| Code-fix coverage, Supported inputs, Issue intake | 10.6k | A policy doc under `sdd/`, required reading for the skeptic, spec author, test implementer, and auditor |
| Asking the maintainer, Output questions | 4.8k | A doc read before asking; the lead reads it at the start |
| Key conventions | 5.5k | Split by who needs it |
| Build/test, workflow, language, squad and SDD pointers | about 4.3k | Stays, together with an index of trigger-based reads |

## Usage

Session `7209ef3f-40a5-5ba8-be6e-4d08dd2b8e0d`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 3 | 30 | 61 | 30.355 | 14.554 | 5.283.734 | 157.924 | 126.567 |
| implementer | 1 | a11a0d007feb6f8f0 | Probe implementer startup context | large | medium | 1 | 1 | 2 | 52 | 0 | 0 | 82.084 | 0 |
| validator | 1 | a84ca5529eafa7516 | Probe validator startup context | small | low | 1 | 1 | 2 | 145 | 92 | 0 | 23.166 | 0 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 31 | 63 | 30.407 | 14.554 | 5.283.734 | 240.008 |
| small | 1 | 2 | 145 | 92 | 0 | 23.166 |
| **Total** | 32 | 65 | 30.552 | 14.646 | 5.283.734 | 263.174 |

## Decisions taken

- No note is deleted yet. The notes stay as the baseline for the comparison after the allow-list change.
- The Sonnet implementer trial waits until the startup changes have landed, because they change the baseline it is judged against.
- The light lane for diagnostic-text-only changes is already #1906 and is not filed again.

## Suggestions

These are the planned issues, in order. Only issue 1 is filed now.

1. **Give every squad role a tool allow-list.** Expected effect: about −25 % run cost. Measure it with `scripts/usage.sh --json` (`first_context_tokens`) over about ten runs.
2. **Shrink `AGENTS.md` to the rules that always apply plus an index of trigger-based reads.** Distribute the sections as in the table above; the policy sections go to a doc under `sdd/` (answer 2A).
   - Reads must be triggered by a condition ("before you change a formatter phase, read …"), never "when needed": an agent does not know which rule it is missing.
   - The policy sections stay required reading for the roles that judge (skeptic, spec author, test implementer, auditor).
   - Role files list their required reads; the wrappers only point to the role file and copy no text.
3. **Split `squad/README.md` (32k characters, read completely by every role) into a core for every role and parts only some roles need.**
4. **Make startup cost visible in the Markdown table of `scripts/usage.sh`:** first context per spawn, and cache writes split into 5-minute and 1-hour.
5. **Move the failure-class matrices proposed in about 20 notes into `failure-classes.md`.** Each audit found the next axis of a class the skeptic had enumerated only partly.
   - Proposed matrices: both sides of every token a fix removes; a trivia axis (none, `/* */`, `/** */`, directive); every parent of a Roslyn node type; documentation-comment ownership; tokens that touch once elastic trivia is removed; fixtures with at least three elements when a rule picks "the last" one.
   - Refuse a rare class as a whole at the first repeat.
6. **Ask principle questions and sibling questions before the first full spec draft** (about 12 notes).
7. **The spec author compiles and checks every example before handing over** (about 6 notes).
8. **Reduce sandbox friction.**
   - A chunked mode for `scripts/test.sh`: the test host runs out of memory on a whole analyzer project.
   - A `MarkupMode.Allow` hint in the test-implementer role.
   - The worktree recipe for a before-run in the role files: the no-stash rule exists, but roles broke it.
   - Lane-check classification of the run note.
9. **Try a medium-tier implementer** in single-issue bug-fix runs, after issues 1–3.
   - Saving: about 6 % of the cost ($1.90 per run).
   - Break-even: at most one extra repair round every three to four runs.
   - Measure per run the repair rounds the implementer caused, and the disputes it raised or missed.

### Evaluation script

Run from the repository root.

```python
import glob, statistics as st
P = {'large': (4, 20, 0.2), 'medium': (2, 10, 0.1), 'small': (0.1, 0.5, 0.01)}
P.update({'claude-opus-5-5': P['large'], 'claude-sonnet-5-5': P['medium'], 'claude-haiku-5-5': P['small']})
n = lambda s: int(s.replace('.', '') or 0)
runs = {}
for f in glob.glob('squad/notes/2026-*.md'):
    hdr, cost = None, 0.0
    for line in open(f):
        if not line.startswith('|'):
            continue
        c = [x.strip() for x in line.strip().strip('|').split('|')]
        if c[0] == 'Agent':
            hdr = c
            continue
        if hdr and len(c) == len(hdr):
            d = dict(zip(hdr, c))
            tier = d.get('Tier', d.get('Model'))
            if tier not in P:
                continue
            i, o, cr = P[tier]
            write = 2 if d['Agent'].startswith('lead') else 1.25
            cost += (n(d['Input']) * i + n(d['Output']) * o + n(d['Cache read']) * cr + n(d['Cache write']) * i * write) / 1e6
    if cost:
        runs[f] = cost
print(len(runs), round(st.mean(runs.values()), 2), round(st.median(runs.values()), 2))
```
