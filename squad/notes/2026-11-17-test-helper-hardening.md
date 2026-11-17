# test-helper-hardening

| | |
|---|---|
| Date | 2026-11-17 |
| Run | issues #1804, #1838 |
| Pull request | #1842 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|

None: the change is test code only; no output question or escalation arose.

## Cost

No stops, no repair rounds, every role spawned once. Lane: tests (skeptic, test implementer, cleaner, spec checker, validator; spec author, implementer, and auditor skipped — no spec, production code, or audit in this lane). The skeptic and the test implementer were the expensive spawns: the skeptic ran four throw-away experiments in a worktree (full analyzer and formatter suites with the proposed assertions) that settled the scope before any test was written.

## Usage

Session `ecaa0f40-be1d-5137-b88f-1bcb9ffadca8`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 7 | 26 | 62 | 11.690 | 1.356 | 3.332.306 | 86.569 | 55.909 |
| skeptic | 1 | a3b9f626efb90d64e | Skeptic for #1804 #1838 | large | high | 1 | 49 | 95 | 37.979 | 13.796 | 7.285.702 | 144.607 | 121.081 |
| test-implementer | 1 | a7248c54a069685b3 | Test implementer #1804 #1838 | large | medium | 1 | 41 | 82 | 33.682 | 8.205 | 5.858.833 | 181.079 | 106.295 |
| cleaner | 1 | adbf096a7fd5354fa | Cleaner #1804 #1838 | medium | low | 1 | 5 | 10 | 1.756 | 0 | 331.753 | 86.965 | 12.263 |
| spec-checker | 1 | a996ef78ce474ae62 | Spec checker #1804 #1838 | medium | medium | 1 | 6 | 12 | 2.486 | 155 | 458.906 | 39.669 | 15.568 |
| validator | 1 | a835b255d38f6b323 | Validator #1804 #1838 | small | low | 1 | 7 | 14 | 2.563 | 180 | 143.827 | 45.808 | 7.793 |
| issue-manager | 1 | a40d3edfe33e3a39f | Issue manager #1804 #1838 | medium | medium | 1 | 6 | 14 | 3.966 | 37 | 513.359 | 54.548 | 31.135 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 116 | 239 | 83.351 | 23.357 | 16.476.841 | 412.255 |
| medium | 17 | 36 | 8.208 | 192 | 1.304.018 | 181.182 |
| small | 7 | 14 | 2.563 | 180 | 143.827 | 45.808 |
| **Total** | 140 | 289 | 94.122 | 23.729 | 17.924.686 | 639.245 |

## Decisions taken

Six, all on the skeptic's evidence: #1804 widened from four to eight copy classes plus two further `FormattingPipelineTests` tests; partial assertions kept; `ExecuteAlreadyFormattedCodeNoChanges` now asserts first pass == input as whole text; #1838's opt-out parameter dropped (no caller); the analyzer-test cross-project copy and the same refusal gap in Formatter.Test's `ApplyRule` filed as follow-ups #1848 and #1847. The maintainer may want to see the last one: the refusal check in `ApplyRule` was left out only to avoid colliding with the in-flight #1732 run.

## Suggestions

_None._
