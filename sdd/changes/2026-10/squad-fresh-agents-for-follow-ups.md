# The squad gives follow-up work to fresh agents

| | |
|---|---|
| Status | Accepted |
| Date | 2026-10 |
| Affected requirements | none — process change |

## What

The lead continues a role's agent only while that agent's own reasoning is the input: the maintainer's answer it waits on, or evidence for a question it raised and has not yet answered. Every other follow-up — a repair round, an amendment, a status or wording change, a small correction — goes to a fresh agent of the same role. Its prompt names the handovers to read, the role's own earlier handover among them, the commit, and the change asked for.

## Why

The lead's rule was to continue the same agent with any follow-up evidence. Each continuation carries the agent's whole context, and that context only grows. In the run of issue #1489, the spec author was continued nine times and the implementer three times, partly for status changes and small text corrections, and these two agents dominated the run's cost.

A continued context is not what keeps a run coherent; the handovers are. Every role writes its result to a file so that the next role needs nothing else from the conversation, and a fresh agent of the same role can rely on the same files. Where a fresh agent cannot finish from the handovers, the handover was too thin, which is better found than hidden by a large context.

The one case where the context is the point stays: an agent that raised a question and waits on its answer, or on evidence for it, keeps the reasoning that led to the question, and re-deriving it would cost more than continuing.

## Decisions

- **No list of task types.** One test — is the agent's own unanswered question the input? — decides every case, so the rule needs no table of which work is small enough for a fresh agent.
- **The auditor is unchanged.** It already runs as a fresh agent for each audit of a run.
