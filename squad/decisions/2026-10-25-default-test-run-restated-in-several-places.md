# The default test run's coverage is restated in several text files

| | |
|---|---|
| Date | 2026-10-25 |
| Escalation | 12 |
| Run | issue #1467 |

## Question

The auditor reported an architecture finding: what the default `scripts/test.sh` run covers (and therefore what the validator runs) is defined in the script's `all)` branch but restated in eight text places. The run updated five to say the self-hosting scans are excluded; `CONTRIBUTING.md`, `squad/README.md`, and `.claude/agents/validator.md` still said "all tests". Options: A — correct the three lines in this run and draft a follow-up issue to state the rule in one place; B — consolidate all eight places in this run first; C — stop the run.

## Answer

A: correct the three stale lines narrowly in this run; the consolidation becomes a follow-up issue.

## Reason

None given.
