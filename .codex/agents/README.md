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
| `reihitsu-rubber-duck` | Behavior Contract gate | `gpt-5.6-terra` | `high` | Produces a bounded contract and executable sweep before implementation; unclear or high-risk scope is escalated by the parent rather than paying frontier cost on every issue. |
| `reihitsu-preflight` | Standard preflight and every retry | `gpt-5.6-sol` | `high` | Performs the independent audit with a balanced frontier reasoning level; retries remain repair-delta aware. |
| `reihitsu-deep-preflight` | Deep preflight, at most once per issue | `gpt-5.6-sol` | `xhigh` | Reserved for public-API, semantic-rewrite, security-boundary, or destructive-behavior risk where deeper reasoning has a concrete target. |
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

## Preflight routing

The parent selects one preflight tier from the final diff before attempt 1:

- use `reihitsu-deep-preflight` when the diff changes public API, the semantic or trivia behavior of a
  rewrite, a security boundary, or destructive behavior;
- otherwise use `reihitsu-preflight`;
- use `reihitsu-preflight` for every repair retry, even when attempt 1 was deep.

Only one `reihitsu-deep-preflight` process may start per issue. A state-mismatch restart does not permit a
second deep process; fall back to the standard agent after reconciling state. This keeps `xhigh` targeted and
measurable rather than making it the default cost of every behavioral diff.

## Reasoning effort

`model_reasoning_effort` is set per agent so the cost profile is a standing, reviewable decision. Contract
analysis starts on Terra/high, standard review uses Sol/high, and Sol/xhigh is reserved for the explicit deep
trigger above. The validation agent omits judgment rather than checks, so it uses the lowest reasoning level.

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
