# Agent definitions

The `gh-*` workflow skills spawn their gates as subagents. Without a definition every gate inherits the
session model and the session effort level, so a purely mechanical stage costs exactly as much per token as
the adversarial audit next to it. These files are the **single owner** of that decision: model tier, effort
level, and the tool restrictions that make a stage's "read-only" or "reports only" promise mechanical rather
than a sentence in a prompt.

The skills reference an agent by its `name` through the `subagent_type` parameter and pass no model
argument, so re-tuning a stage is a one-line frontmatter change here — not an edit spread across
`gh-implement`, `gh-apply-review`, and `gh-preflight`.

## Current assignment

| Agent | Stage | Model | Effort | Why this tier |
|---|---|---|---|---|
| `reihitsu-reproduction` | Reproduction gate (`gh-implement`) | `sonnet` | `high` | Writes one test against a helper named in a fixed table and returns a rigid schema. Its cheap verdict is falsified by the next three stages; its terminal verdict is escalated, see below. |
| `reihitsu-rubber-duck` | Behavior Contract gate | `opus` | `xhigh` | Produces the guard-delta and predicate-boundary tables the workflow treats as load-bearing. A missed defect class here is paid for by a full implement/preflight/review cycle. |
| `reihitsu-preflight` | Official preflight, attempt 1 and retry | `opus` | `xhigh` | Adversarial audit against ~50 rules; it is the gate that catches a boundary test which cannot falsify the change it guards. |
| `reihitsu-validate` | Full validation | `haiku` | `low` | Runs two repository scripts and reports pass/fail plus the failing assertions. No judgement, and thousands of output lines that would otherwise land in the orchestrator's context. |

The orchestrator itself is the session agent, not a subagent, so it is not configured here. It stays on the
session model and effort — it owns the `Never` rules, the scope ledger, and the 1 + 1 preflight budget, and a
weaker model there silently skips a gate rather than failing loudly.

## Asymmetric escalation of the reproduction gate

The reproduction gate runs at the cheaper tier because its verdicts are not equally reversible:

- `REPRODUCED` is falsified immediately — the contract, the implementation, and the audit all run against
  that test.
- `BLOCKED` costs one restart.
- `NOT REPRODUCED` ends the run, downgrades the PR link to `Refs #<N>`, and hands the user a question. A
  wrong one is expensive and quiet.

So the terminal negative is ratified before it is acted on: the parent re-runs **the same**
`reihitsu-reproduction` agent type once with a per-invocation `model: opus` override, handing it the first
run's report as evidence. The cheap tier does the work; the expensive tier only confirms the verdict that
cannot be taken back. Exactly one escalation — a disagreement is decided by the escalated run.

If the environment offers no per-invocation model override, the parent performs that confirmation itself
rather than acting on an unconfirmed `NOT REPRODUCED`.

## Effort levels

`effort` is set per agent instead of being left to whatever the operator chose that day, so a stage's cost
profile is a standing, reviewable decision. On Opus it is the primary cost lever and, unlike a tier
downgrade, it preserves instruction-following against a large rule set — which is why the two audit agents
keep `opus` and are tuned through `effort` rather than through their model.

`reihitsu-validate` omits nothing but judgement, so it takes the lowest level. Available levels depend on the
model; if a level is rejected for a model, drop it from that file rather than moving the stage to another
tier.

## Tool restrictions

`reihitsu-validate` denies `Edit`, `Write`, and `NotebookEdit`: it exists to run `scripts/build.sh` and
`scripts/test.sh` and report, and the parent stays the only writer.

`reihitsu-rubber-duck` and `reihitsu-preflight` are read-only towards the **repository**, but both legitimately
create disposable fixtures in a temporary directory outside it to execute a sweep or check a corpus. Their
restriction therefore stays where it can express that difference — in the skill's own read-only guarantees —
rather than in a `disallowedTools` list that would also block the fixtures.

## Deliberately not configured

- **PR title and body rewrite.** It runs in the orchestrator, which already holds the diff. Spawning a
  cheaper agent for it would re-read the diff into a second context, so the tier saving is smaller than the
  duplication it buys.
- **The delegated implementation commands.** They also run in the orchestrator; moving them into a subagent
  is a restructuring of the workflow, not a configuration change.
