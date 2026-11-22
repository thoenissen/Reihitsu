# ordering-move-directives

| | |
|---|---|
| Date | 2026-11-22 |
| Run | issues #970, #1668 |
| Pull request | #1940 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 970-1 | output question | A file-level `#pragma warning` for another warning in front of the move: withhold the fix, or compare IDs | A (withhold) | yes |
| 970-2 | output question | Include RH7309 in the pair rule | A (include) | yes |
| 970-3 | output question | A closed pair in front of the move withholds the fix? | B (no) | yes |
| 970-4 | output question | RH7309: pairs in a block that keeps its place | B (accepted like moved blocks) | yes |
| 1668-1 | output question | Any `#if` block in moved or crossed code withholds RH7102/RH7103/RH7109/RH7309 fixes | B (refuse all) | yes |
| 1668-2 | output question | `#if` block in front of the moved instance fields or of the class withholds an instance-field reorder | A (withhold, whole-type wrap exception) | yes |

## Cost

- One batch of output questions (six questions), answered in one reply. No escalations.
- One repair round (audit 1, finding on RH7309's layout clause with two separately reordered runs); the repair audit passed with comment/spec-text findings, proven text-only afterwards.
- Roles spawned more than once: the spec author seven times (answers, an example that contradicted its requirement, a RH7309 clarification, a merge conflict, repair wording, a text fix), the test implementer three times, the implementer three times (one to delete a helper the run left without callers). Several of those were small follow-ups that a single spec author pass with a self-check of examples against the requirement would have avoided.
- The expensive spawn was the first test implementer (~34M cache read): #1668 rewrote or deleted the tests of about 200 `SHARED-ORDER-007`/`-008` cases across seven files.

## Usage

Session `b9f461a1-fd2e-5fc9-aab6-75cafb8191ca`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 22 | 93 | 246 | 37.653 | 3.982 | 17.076.687 | 175.321 | 142.789 |
| spec-author | 1 | aab8584d048dbedbc | Spec author for #970 #1668 | large | medium | 1 | 56 | 108 | 65.860 | 33.653 | 9.102.773 | 226.563 | 147.299 |
| skeptic | 1 | ad7690f660f242779 | Skeptic for #970 #1668 | large | high | 1 | 38 | 74 | 72.518 | 48.821 | 6.426.926 | 254.437 | 175.593 |
| spec-author | 2 | a36e74b076867ad1e | Spec author: apply answers | large | medium | 1 | 50 | 95 | 57.554 | 20.737 | 8.581.245 | 230.418 | 150.850 |
| test-implementer | 1 | a1ba41453115a8047 | Test implementer #970 #1668 | large | medium | 1 | 121 | 232 | 137.772 | 38.916 | 34.286.103 | 400.283 | 374.719 |
| spec-author | 3 | a75a3ff2075dec7d9 | Spec author: fix example | large | medium | 1 | 11 | 21 | 5.225 | 487 | 1.070.334 | 57.909 | 32.367 |
| implementer | 1 | a645b65bbbdefce50 | Implementer #970 #1668 | large | medium | 1 | 45 | 85 | 55.683 | 17.570 | 8.093.071 | 435.495 | 171.909 |
| spec-author | 4 | aefbde7e7bf1f0375 | Spec author: RH7309 crossed block | large | medium | 1 | 13 | 26 | 11.484 | 4.218 | 1.169.815 | 114.810 | 39.191 |
| test-implementer | 2 | a1a5a0b2d3d4cd82d | Retarget Core directive tests | large | medium | 3 | 29 | 60 | 18.496 | 2.889 | 2.915.637 | 125.470 | 49.828 |
| implementer | 2 | a46a2c9c160db8098 | Implementer: delete dead method | large | medium | 1 | 13 | 25 | 6.274 | 573 | 1.194.227 | 107.839 | 28.611 |
| cleaner | 1 | a622377506ee98d1e | Cleaner #970 #1668 | medium | low | 1 | 6 | 12 | 1.963 | 49 | 432.001 | 90.387 | 11.254 |
| spec-checker | 1 | af3165873c39d867c | Spec checker #970 #1668 | medium | medium | 1 | 16 | 31 | 8.992 | 1.901 | 1.761.732 | 141.941 | 63.225 |
| spec-author | 5 | a093ae4bbe1d9cad9 | Resolve merge conflict code-fixes.md | large | medium | 1 | 9 | 18 | 3.072 | 337 | 759.132 | 39.940 | 14.397 |
| auditor | 1 | a5fef753ea23d7b53 | Auditor #970 #1668 | large | high | 1 | 47 | 89 | 56.374 | 36.450 | 9.549.790 | 296.381 | 217.616 |
| test-implementer | 3 | a086be2ffe9084265 | Repair 1: no-fix test | large | medium | 2 | 26 | 54 | 18.169 | 3.787 | 2.810.184 | 140.017 | 64.483 |
| spec-author | 6 | afe022adf3a06fa32 | Repair 1: spec wording | large | medium | 3 | 24 | 48 | 20.632 | 10.018 | 2.691.275 | 142.265 | 66.689 |
| implementer | 3 | a3a5074ba708dc375 | Repair 1: implementer | large | medium | 1 | 11 | 21 | 5.742 | 685 | 882.368 | 218.021 | 31.012 |
| cleaner | 2 | a1476fa5e17ec5768 | Repair 1: cleaner | medium | low | 1 | 5 | 10 | 1.285 | 0 | 326.910 | 83.441 | 4.299 |
| spec-checker | 2 | ad3b18e43dae4c3fe | Repair 1: spec checker | medium | medium | 1 | 9 | 17 | 3.881 | 678 | 752.020 | 109.291 | 30.572 |
| auditor | 2 | a9d94bc88240988ed | Repair 1: auditor | large | high | 1 | 27 | 52 | 21.142 | 10.723 | 3.216.662 | 152.732 | 73.953 |
| cleaner | 3 | a701a067da4ad918a | Fix stale comments (#2) | medium | low | 3 | 10 | 22 | 4.677 | 120 | 766.028 | 90.485 | 14.914 |
| spec-author | 7 | a0e0610de60a87719 | Fix RH7309 example reason (#3) | large | medium | 1 | 7 | 14 | 2.365 | 114 | 508.863 | 90.697 | 15.209 |
| validator | 1 | a041251c51a786591 | Validator final tree | small | low | 1 | 7 | 14 | 3.096 | 284 | 152.798 | 67.805 | 12.206 |
| issue-manager | 1 | acf1d94c4c51d4bb8 | Issue manager follow-ups | medium | medium | 1 | 10 | 21 | 5.029 | 810 | 948.262 | 123.602 | 44.894 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 620 | 1.268 | 596.015 | 233.960 | 110.335.092 | 3.208.598 |
| medium | 56 | 113 | 25.827 | 3.558 | 4.986.953 | 639.147 |
| small | 7 | 14 | 3.096 | 284 | 152.798 | 67.805 |
| **Total** | 683 | 1.395 | 624.938 | 237.802 | 115.474.843 | 3.915.550 |

## Decisions taken

13. Worth the maintainer's look: decision 13 (the layout anchor reads "in front of the moved fields" as the last instance field of the spans, narrower than the auditor's suggestion), and decision 4 (the "in front of the class" place is the leading trivia of each declaration's first token, as on `main`).

## Suggestions

- The spec author's examples should be checked against their own requirement's span definitions before hand-off: one RH7103 example placed a group in the target's leading trivia, which the requirement refuses, and it cost a test-implementer finding and a spec-author spawn.
