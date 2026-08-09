# Agent definitions

The `gh-*` workflow skills spawn their gates as custom Codex agents. Without a definition every gate inherits
the session model and reasoning effort, so a mechanical validation stage can cost as much per token as the
adversarial audit next to it. These files are the **single owner** of each gate's model, reasoning effort, and
tool restrictions.

The skills reference an agent by its `name` and pass no model or reasoning override, so re-tuning a normal
gate is a one-line TOML change here rather than an edit spread across `gh-implement`, `gh-apply-review`, and
`gh-preflight`.

## Current assignment

| Agent | Stage | Model | Reasoning effort | Why this tier |
|---|---|---|---|---|
| `reihitsu-reproduction` | Reproduction gate (`gh-implement`) | `gpt-5.6-terra` | `high` | Writes one test against a helper named in a fixed table and returns a rigid schema. Its cheap verdict is falsified by later stages; its terminal verdict is escalated. |
| `reihitsu-rubber-duck` | Behavior Contract gate | `gpt-5.6-sol` | `xhigh` | Produces the guard-delta and predicate-boundary tables the workflow treats as load-bearing. A missed defect class here costs a full implementation and review cycle. |
| `reihitsu-preflight` | Official preflight, attempt 1 and retry | `gpt-5.6-sol` | `xhigh` | Performs the adversarial audit and checks whether boundary tests can falsify the predicates they guard. |
| `reihitsu-validate` | Full validation | `gpt-5.6-terra` | `low` | Runs three repository scripts and reports pass/fail plus failing assertions. It makes no judgment and keeps thousands of log lines out of the orchestrator context. Terra is the lowest-cost GPT-5.6 tier currently exposed to this Codex subagent runtime. |

The orchestrator is the session agent and is not configured here. It owns the `Never` rules, scope ledger,
and 1 + 1 preflight budget, so it deliberately keeps the session model and reasoning effort.

## Asymmetric escalation of the reproduction gate

The reproduction verdicts are not equally reversible:

- `REPRODUCED` is checked by the contract, implementation, and audit that follow.
- `BLOCKED` costs one restart.
- `NOT REPRODUCED` ends the run, changes the PR link to `Refs #<N>`, and asks the user what to do next. A
  wrong negative is expensive and quiet.

Before the parent acts on `NOT REPRODUCED`, it therefore starts one fresh `reihitsu-reproduction` agent with
no inherited turns, the first report as evidence, and explicit `gpt-5.6-sol` / `xhigh` overrides. The
escalated verdict decides. Exactly one escalation is allowed. If the environment cannot override a custom
agent's model and effort for one spawn, the parent performs the confirmation itself.

## Reasoning effort

`model_reasoning_effort` is set per agent so the cost profile is a standing, reviewable decision. The two
analysis gates remain on Sol and are tuned through reasoning effort because they must follow a large rule set
and challenge plausible but incomplete conclusions. The validation agent omits judgment rather than checks,
so it uses the lowest reasoning level.

Available effort levels depend on the model and Codex deployment. If a configured level is unavailable, drop
the setting and record the effective default rather than silently moving the stage to a different model.

## Tool restrictions

The validator needs workspace write access because `dotnet build` writes `bin/` and `obj/`. Its agent file
therefore uses `workspace-write` and a `PreToolUse` hook that rejects `apply_patch` (`Edit`/`Write`) calls. It
may run only the canonical validation scripts and report their results; the parent remains the only source
editor and repair owner.

The Rubber Duck and preflight agents are read-only towards the **repository**, but both legitimately create
disposable fixtures in a temporary directory and may run focused commands that write build artifacts. Their
restriction remains in their developer instructions, where it can express that distinction.

## Deliberately not configured

- **PR title and body rewrite.** It runs in the orchestrator, which already holds the diff. A cheaper agent
  would have to re-read the same material into another context.
- **Delegated implementation commands.** They also stay in the orchestrator; moving them would restructure
  the workflow rather than configure an existing gate.
