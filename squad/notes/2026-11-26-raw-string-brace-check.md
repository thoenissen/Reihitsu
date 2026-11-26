# raw-string-brace-check

| | |
|---|---|
| Date | 2026-11-26 |
| Run | issue #1722 |
| Pull request | #1952 |

## Questions to the maintainer

None.

## Cost

No stops, no repair rounds, each role spawned once. The skeptic was the most expensive spawn: it disproved the issue's premise at the named site by trying the proposed swap in a throwaway worktree (one pinned test failed) and found the real second statement of the check in the horizontal-spacing rule. The test implementer proved by mutation that existing tests pin both places, so no test was added.

## Usage

Session `a593c4f4-0126-50ec-ab45-c0f0c528aebb`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 6 | 35 | 88 | 11.564 | 643 | 4.747.812 | 94.078 | 60.256 |
| skeptic | 1 | add54d6fdf6678ba8 | Skeptic for issue #1722 | large | high | 1 | 32 | 65 | 23.377 | 10.200 | 3.991.634 | 162.349 | 80.665 |
| test-implementer | 1 | a6785d06c8d546ec3 | Test implementer for #1722 | large | medium | 1 | 14 | 27 | 8.203 | 1.563 | 1.433.866 | 61.732 | 36.192 |
| implementer | 1 | a166ce083fba7c086 | Implementer for #1722 | large | medium | 1 | 11 | 21 | 5.355 | 161 | 1.028.178 | 46.724 | 21.272 |
| cleaner | 1 | a3c4540d90929d8b3 | Cleaner for #1722 | medium | low | 1 | 4 | 8 | 1.191 | 0 | 255.546 | 87.595 | 5.438 |
| spec-checker | 1 | ad65ba23762320d59 | Spec checker for #1722 | medium | medium | 1 | 5 | 10 | 1.888 | 185 | 350.943 | 92.395 | 10.710 |
| auditor | 1 | a9d68bd0ca4159719 | Auditor for #1722 | large | high | 1 | 18 | 37 | 10.872 | 3.595 | 1.900.385 | 71.448 | 46.021 |
| validator | 1 | a44a604d229245f11 | Validator for #1722 | small | low | 2 | 7 | 14 | 2.875 | 331 | 144.658 | 64.008 | 10.612 |
| issue-manager | 1 | a724bed9ffffbd399 | Issue manager for #1722 | medium | medium | 1 | 7 | 16 | 2.870 | 122 | 565.352 | 108.654 | 31.783 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 110 | 238 | 59.371 | 16.162 | 13.101.875 | 436.331 |
| medium | 16 | 34 | 5.949 | 307 | 1.171.841 | 288.644 |
| small | 7 | 14 | 2.875 | 331 | 144.658 | 64.008 |
| **Total** | 133 | 286 | 68.195 | 16.800 | 14.418.374 | 788.983 |

## Decisions taken

Two: keep `RawStringLineShifts.IsHoleOpeningLine` with a doc comment that states why it is not the Core check (the issue's own fallback), and take in the adjacent fix that switches `SingleSpaceSpacingRule.RequiresSpaceBeforeBrace` to `FormattingTextAnalysisUtilities.IsInterpolationBrace`. The maintainer may want to see the first, since it departs from the issue's primary goal.

## Suggestions

_None._
