# issue-1873-context-summary

| | |
|---|---|
| Date | 2026-11-20 |
| Run | issue #1873 |
| Pull request | #1884 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|

None.

## Cost

No stops, no repair rounds; each role spawned once. Docs lane: spec author and spec checker ran; skeptic, test implementer, implementer, cleaner, and auditor skipped by the lane; validator skipped because the text-only proof passed (no compiled change).

## Usage

Session `a9766f55-4cf5-584e-a2ad-92acb3ac9436`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 1 | 12 | 29 | 4.758 | 273 | 1.387.678 | 70.945 | 38.994 |
| spec-author | 1 | a72a276a02568730a | Spec author for issue 1873 | large | medium | 1 | 13 | 27 | 6.231 | 1.267 | 1.153.709 | 46.176 | 22.274 |
| spec-checker | 1 | a3171febc009e587d | Spec checker for issue 1873 | medium | medium | 1 | 5 | 10 | 1.575 | 105 | 330.727 | 85.646 | 7.810 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 25 | 56 | 10.989 | 1.540 | 2.541.387 | 117.121 |
| medium | 5 | 10 | 1.575 | 105 | 330.727 | 85.646 |
| **Total** | 30 | 66 | 12.564 | 1.645 | 2.872.114 | 202.767 |

## Decisions taken

One: the spec author found the same stale "positional context" wording in the summary of the RH5111 code fix's `GetFormattingContextNode` and reworded it too, taken in as an adjacent fix (documentation comment only, lane unchanged).

## Suggestions

_None._
