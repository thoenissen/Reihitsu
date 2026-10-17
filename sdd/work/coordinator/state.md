# SDD coordinator state

Parent: #899. Branch: `claude/sdd-coordinator` (never merged; `sdd/work/` stays out of PRs to main).

## Phase

1 — decision round for open questions (#912, sub-issues #913–#934). No code.

## Order for phase 1

1. Program meaning: #919 Q1, #930 Q2
2. Shared code-fix bases (family questions): #926 Q2, #928 Q1, #929 Q1
3. Idempotency: #919 Q3, #922 Q1+Q2 (with #921 Q3, same cause)
4. Phases undo each other: #918 Q3
5. Analyzer/formatter disagree: #920 Q1, #916 Q1
6. Groups: continuation lines (#921 Q1, #918 Q5, #913 Q2+Q4); trailing whitespace in comments (#923 Q1, #915 Q1)
7. Everything else, per issue

## Decisions

| Question | Decision | Comment | Follow-up |
|---|---|---|---|

## Running

- nothing

## Waiting for maintainer

- #919 Q1 decision

## Collected for later phases

- Clarifications per spec batch: —
- Bug issues to draft (phase 2): —
- #946 (RH5005/RH5006 `await using`/`await foreach` fix never converges) → phase 2; blocks RH5005/RH5006 specs.
- #939 split → phase 3.
- #942 → phase 5.
