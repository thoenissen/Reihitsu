# Zero-width elastic trivia on tokens a phase creates

| | |
|---|---|
| Date | 2026-10-31 |
| Escalation | 12 |
| Run | issue #1488 |

## Question

Tokens created through `SyntaxFactory` carry elastic markers, zero-width whitespace that the spacing phase collapses into a space, so a phase's tree can format differently from its own text (`get=> ""` versus `get=>""`), which FMT-PIPE-021's Why rules out and its structural comparison does not see. Options: A fix narrowly — the structural transforms create their tokens without elastic markers — and draft a follow-up for a pipeline-wide guarantee; B require in FMT-PIPE-021 that no phase output carries elastic trivia and fix every phase in this run; C stop the run.

## Answer

A — narrow fix in the structural transforms; the pipeline-wide guarantee becomes a follow-up candidate.

## Reason

None given beyond accepting the recommendation.
