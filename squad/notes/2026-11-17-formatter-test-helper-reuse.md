# formatter-test-helper-reuse

| | |
|---|---|
| Date | 2026-11-17 |
| Run | issues #1847, #1848 |
| Pull request | #1853 |

## Questions to the maintainer

None.

## Cost

No stops, no repair rounds, every role spawned once. Tests lane: skeptic, test implementer, cleaner, spec checker, validator; spec author, implementer, and auditor skipped (no spec added, no production code, no auditor in the lane). The skeptic was the most expensive spawn: it ran the formatter suite in temporary worktrees to settle both premises and the design options before anything was written, which let the test implementer start on a decided design.

## Usage

Session `180d65f3-9dfc-57aa-bfbe-9bdbf766cb11`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 7 | 29 | 72 | 11.408 | 1.069 | 3.795.377 | 88.260 | 58.532 |
| skeptic | 1 | ad14b861f1785dbd2 | Skeptic for #1847 #1848 | large | high | 1 | 38 | 73 | 33.406 | 10.837 | 5.034.848 | 168.288 | 90.637 |
| test-implementer | 1 | a0ccc6ec3113e2489 | Test implementer for #1847 #1848 | large | medium | 2 | 35 | 74 | 25.599 | 5.172 | 4.277.948 | 207.373 | 80.779 |
| cleaner | 1 | ae613a0ae8571a7e1 | Cleaner for #1847 #1848 | medium | low | 1 | 6 | 12 | 1.588 | 0 | 403.501 | 83.479 | 8.735 |
| spec-checker | 1 | ae8953de34830a7df | Spec checker for #1847 #1848 | medium | medium | 1 | 5 | 10 | 1.983 | 97 | 322.138 | 85.535 | 11.252 |
| validator | 1 | af5805184e3810687 | Validator for #1847 #1848 | small | low | 1 | 7 | 14 | 2.530 | 394 | 130.145 | 57.468 | 7.450 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 102 | 219 | 70.413 | 17.078 | 13.108.173 | 463.921 |
| medium | 11 | 22 | 3.571 | 97 | 725.639 | 169.014 |
| small | 7 | 14 | 2.530 | 394 | 130.145 | 57.468 |
| **Total** | 120 | 255 | 76.514 | 17.569 | 13.963.957 | 690.403 |

## Decisions taken

Six, all internal (test code only): widen #1847's non-refusal check to both `ApplyRule` branches and `FormatWithPipeline`; demonstrate on a canonical refused input instead of the issue's fixed-buffer fixture and drop the goal of moving fixtures to `AssertUnsupportedFor`; for #1848 a public helper in the formatter test project whose signature names no internal type, instead of `InternalsVisibleTo`, a shared project, or linked sources; remove the dead `transformParseOptions` parameter; one owner carries the refusal check; leave `PhaseTreeRoundTripTests` building its own input. The maintainer may want to see the third: it decides the shape of the shared helper.

## Suggestions

_None._
