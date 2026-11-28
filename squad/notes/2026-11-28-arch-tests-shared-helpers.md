# arch-tests-shared-helpers

| | |
|---|---|
| Date | 2026-11-28 |
| Run | issue #1994 |
| Pull request | #1998 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|

None.

## Cost

No stops, no repair rounds, every role spawned once (tests lane: skeptic, test implementer, cleaner, spec checker, validator). The test implementer was the most expensive spawn: it rewrote a dozen test classes onto the shared helpers and wrote 26 new tests.

## Usage

Session `acdd460c-82a5-5681-803c-6e31018c38f4`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 6 | 25 | 59 | 9.434 | 715 | 2.410.488 | 88.269 | 55.239 |
| skeptic | 1 | ae3d85c2e05061a77 | Skeptic for #1994 | large | high | 1 | 35 | 70 | 26.264 | 8.988 | 2.055.000 | 91.746 | 67.844 |
| test-implementer | 1 | a18cdddb604839816 | Test implementer for #1994 | large | medium | 1 | 52 | 104 | 34.235 | 5.837 | 4.078.417 | 109.790 | 91.649 |
| cleaner | 1 | ad035dee54e76d9e3 | Cleaner for #1994 | medium | low | 1 | 10 | 20 | 2.490 | 177 | 280.077 | 34.648 | 10.749 |
| spec-checker | 1 | a5ce4f3cefedcde4d | Spec checker for #1994 | medium | medium | 1 | 5 | 10 | 1.634 | 46 | 115.929 | 34.024 | 10.670 |
| validator | 1 | adec7784c7fb55f15 | Validator for #1994 | small | low | 1 | 8 | 16 | 3.323 | 484 | 174.889 | 63.573 | 10.937 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 112 | 233 | 69.933 | 15.540 | 8.543.905 | 289.805 |
| medium | 15 | 30 | 4.124 | 223 | 396.006 | 68.672 |
| small | 8 | 16 | 3.323 | 484 | 174.889 | 63.573 |
| **Total** | 135 | 279 | 77.380 | 16.247 | 9.114.800 | 422.050 |

## Decisions taken

Seven. Worth a look: the shared build-output check decides only `bin`/`obj`, while each scan keeps its own dot-directory rule (one uniform rule would change which files two scans read); and the adjacent fix that makes the tracker-reference scan compare paths relative to its root, so a checkout below a `bin` or `obj` directory no longer passes with nothing checked.

## Suggestions

The issue comment recorded as a scope note was agent text under the maintainer's account; the lead first recorded it as a maintainer clarification, and the skeptic corrected that. A comment with the agent footer is a proposal, like the issue text.
