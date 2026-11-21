# spec-wording-indent-diag

| | |
|---|---|
| Date | 2026-11-21 |
| Run | issue #1901 |
| Pull request | #1904 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|

None.

## Cost

Docs lane: spec author and spec checker, each spawned once. No stops, no repair rounds. The spec author was the expensive spawn; it verified both claims by running `scripts/format.sh` and `scripts/apply-fix.sh` and the architecture tests.

## Usage

Session `eca6b4fa-011b-5100-b656-aed9ec233bb5`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 2 | 15 | 37 | 5.271 | 284 | 1.740.583 | 68.393 | 36.438 |
| spec-author | 1 | aacefb9e668dc55bc | Spec author for #1901 | large | medium | 1 | 28 | 55 | 12.294 | 2.609 | 3.089.532 | 75.676 | 51.738 |
| spec-checker | 1 | aed2a76f36935675b | Spec checker for #1901 | medium | medium | 1 | 5 | 10 | 2.089 | 173 | 340.325 | 89.767 | 11.869 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 43 | 92 | 17.565 | 2.893 | 4.830.115 | 144.069 |
| medium | 5 | 10 | 2.089 | 173 | 340.325 | 89.767 |
| **Total** | 48 | 102 | 19.654 | 3.066 | 5.170.440 | 233.836 |

## Decisions taken

Two. The issue's general wording for FMT-INDENT-012 ("every bracketed argument list") was narrowed to the three parents that align, with the indexer-initializer key named as the FMT-INDENT-014 exception, because the general wording would contradict FMT-INDENT-014. Tagging existing tests with FMT-INDENT-012 was left out of the docs lane.

## Suggestions

_None._
