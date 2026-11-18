# token-replacement-cost

| | |
|---|---|
| Date | 2026-11-18 |
| Run | issue #1862 |
| Pull request | #1872 |

## Questions to the maintainer

None. The change keeps formatter output byte-identical, so no output question arose.

## Cost

- No stops and no repair round; the audit passed with one comment-only finding, repaired by the cleaner and proven text-only, so no second audit.
- The test implementer was spawned twice: the implementer also converted the tab-replacement site, and the first test implementer had asked to be sent back for whole-file tests in that case (fresh agent, its context was large).
- The cleaner was spawned twice: once for the regular stage, once for the audit's comment finding.
- The expensive spawns were the skeptic (in-process measurement probe of every phase and sibling site) and the auditor (a perturbed-corpus comparison of `main` and the head CLI over all 1956 source files in six variants).

## Usage

Session `bb9660ba-31bc-5247-a71f-53020a1dc2ed`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 11 | 42 | 104 | 17.545 | 1.176 | 5.802.380 | 101.486 | 68.957 |
| skeptic | 1 | aa0c3bcbb5e50c823 | Skeptic premise check #1862 | large | high | 1 | 50 | 97 | 46.865 | 20.079 | 6.165.365 | 176.518 | 95.734 |
| test-implementer | 1 | ae4c9ab23a570b406 | Tests for #1862 | large | medium | 1 | 31 | 62 | 34.127 | 9.430 | 4.004.800 | 112.663 | 88.438 |
| implementer | 1 | a495c54f0369326b5 | Implement fix for #1862 | large | medium | 1 | 28 | 56 | 19.848 | 4.558 | 3.546.942 | 101.297 | 76.986 |
| test-implementer | 2 | a9c50ef2a5e46a749 | Tab whole-file tests #1862 | large | medium | 1 | 27 | 54 | 28.343 | 8.106 | 3.304.858 | 98.358 | 74.022 |
| cleaner | 1 | a03e11517eac064b1 | Cleaner for #1862 | medium | low | 1 | 4 | 8 | 1.164 | 0 | 252.085 | 88.575 | 11.877 |
| spec-checker | 1 | abcaf61bcda56183d | Spec check for #1862 | medium | medium | 1 | 6 | 12 | 2.357 | 188 | 434.668 | 94.081 | 17.859 |
| auditor | 1 | ad54f97ba8d5e3ecc | Audit #1862 | large | high | 1 | 33 | 68 | 24.734 | 11.261 | 4.494.725 | 123.239 | 98.910 |
| issue-manager | 1 | af26f187629048a11 | File follow-ups for #1862 | medium | medium | 2 | 11 | 26 | 7.859 | 1.041 | 1.097.283 | 132.736 | 56.533 |
| cleaner | 2 | aa218ee31914b2560 | Comment fix after audit #1862 | medium | low | 1 | 5 | 10 | 2.033 | 52 | 333.338 | 87.143 | 10.361 |
| validator | 1 | a3dc88ee0cb336350 | Validate #1862 | small | low | 2 | 8 | 16 | 5.043 | 1.122 | 182.557 | 70.541 | 17.069 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 211 | 441 | 171.462 | 54.610 | 27.319.070 | 713.561 |
| medium | 26 | 56 | 13.413 | 1.281 | 2.117.374 | 402.535 |
| small | 8 | 16 | 5.043 | 1.122 | 182.557 | 70.541 |
| **Total** | 245 | 513 | 189.918 | 57.013 | 29.619.001 | 1.186.637 |

## Decisions taken

Five. Worth the maintainer's eye: decision 1 narrowed the issue's goal — the indentation phase as a whole stays superlinear (about 3.2× per doubling) because `LayoutComputer.Compute` grows on its own; only the whole-tree token replacement was made linear, and the rest went to #1803. Decision 2 replaced a timing assertion with exact-output tests on inputs where the phases rewrite.

## Suggestions

- A performance issue has no natural reproduction test in the bug-fix lane. This run reproduced by a measurement probe kept outside the repository and passed between roles by a scratchpad path, which a later session cannot reuse. A short rule for performance runs — where the probe lives, and that its before/after table is the reproduction — would save the skeptic and the test implementer from deriving it.
