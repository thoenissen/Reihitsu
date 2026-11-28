# resource-text-literal-check

| | |
|---|---|
| Date | 2026-11-28 |
| Run | issue #1970 |
| Pull request | #1992 |

## Questions to the maintainer

None.

## Cost

No stops and no repair rounds; every role ran once (skeptic, test implementer, cleaner, spec checker, validator, issue manager). Spec author, implementer, and auditor were skipped: no spec covers architecture tests, and the tests lane runs neither implementer nor auditor. The skeptic and the test implementer carried most of the cost — the skeptic by scanning the tree before and after the earlier literal clean-up, the test implementer by mutating the scan to prove each test fails for its reason.

## Usage

Session `d4cf7f09-29b6-563b-89f1-c0623e627fe4`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 4 | 23 | 58 | 9.523 | 829 | 2.187.693 | 116.731 | 53.595 |
| skeptic | 1 | a5c90388f25e6ab6f | Skeptic for issue 1970 | large | high | 1 | 37 | 74 | 27.679 | 10.360 | 2.364.226 | 96.386 | 72.417 |
| test-implementer | 1 | a4d6bca2efd9cb0d7 | Test implementer for issue 1970 | large | medium | 1 | 24 | 48 | 31.007 | 6.055 | 1.625.249 | 89.295 | 71.781 |
| cleaner | 1 | aeb2ca5b154612432 | Cleaner for issue 1970 | medium | low | 1 | 5 | 10 | 1.177 | 0 | 118.053 | 32.155 | 8.319 |
| spec-checker | 1 | a8bda9c5c6ec12b6c | Spec checker for issue 1970 | medium | medium | 1 | 4 | 8 | 1.566 | 60 | 96.491 | 37.439 | 14.057 |
| validator | 1 | aef55c4f7bdebd1df | Validator for issue 1970 | small | low | 1 | 7 | 14 | 3.067 | 225 | 154.121 | 68.345 | 12.322 |
| issue-manager | 1 | a91df15f76c89e8fd | Issue manager for issue 1970 | medium | medium | 1 | 6 | 12 | 2.676 | 84 | 195.642 | 48.493 | 21.225 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 84 | 180 | 68.209 | 17.244 | 6.177.168 | 302.412 |
| medium | 15 | 30 | 5.419 | 144 | 410.186 | 118.087 |
| small | 7 | 14 | 3.067 | 225 | 154.121 | 68.345 |
| **Total** | 106 | 224 | 76.695 | 17.613 | 6.741.475 | 488.844 |

## Decisions taken

Four: skip the spec author; narrow "passes after the literals are replaced" to "passes as it stands", because no test file quotes a resource text any more; match by containment rather than equality; no minimum length and no exempt file. One follow-up filed (#1994).

## Suggestions

_None._
