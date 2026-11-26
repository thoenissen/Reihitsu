# rh2001-conditional-group-refusal

| | |
|---|---|
| Date | 2026-11-26 |
| Run | issue #1943 |
| Pull request | #1951 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | RH2001 fix for a conditional group of whole members next to the property: withhold (A) or keep with the excluded-branch parse (B) | A (answered in the issue comment before the run) | Yes |

## Cost

No stops during the run: the only output question was answered in the issue before the claim. One repair round (audit findings: two unpinned behind-the-property directive cases, two shapes missing from the change doc); the second audit passed. The spec author ran three times (initial, amendment after the skeptic for a shape the change doc missed, repair), the test implementer twice, the implementer was continued once for an adjacent cleanup. The skeptic and the first audit each found a visible shape the change doc did not list, all covered by the maintainer's answer.

## Usage

## Usage

Session `5f7b4f13-a45a-56f8-9376-1a44f7af3ec4`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 17 | 55 | 142 | 20.294 | 1.910 | 8.278.020 | 122.284 | 89.418 |
| spec-author | 1 | a40b0ef4e8cf6ace7 | Spec author for #1943 | large | medium | 1 | 35 | 69 | 28.319 | 8.454 | 4.696.291 | 168.766 | 86.561 |
| skeptic | 1 | a10ace4126342c79a | Skeptic for #1943 | large | high | 1 | 28 | 58 | 24.667 | 10.035 | 3.319.010 | 104.879 | 80.176 |
| spec-author | 2 | a0487334ee42dea65 | Spec author amendment #1943 | large | medium | 1 | 11 | 22 | 5.691 | 751 | 974.078 | 106.020 | 28.425 |
| test-implementer | 1 | a6246c42902fecf0c | Test implementer for #1943 | large | medium | 1 | 24 | 48 | 24.814 | 7.501 | 3.039.915 | 114.406 | 89.039 |
| implementer | 1 | aab87d016b0e02a93 | Implementer for #1943 | large | medium | 2 | 23 | 48 | 14.609 | 1.401 | 2.562.534 | 79.199 | 53.837 |
| cleaner | 1 | a5adf3785545e768c | Cleaner for #1943 | medium | low | 1 | 6 | 12 | 1.484 | 0 | 419.156 | 86.558 | 9.174 |
| spec-checker | 1 | a6e95c66f3708a626 | Spec checker for #1943 | medium | medium | 1 | 10 | 20 | 4.237 | 602 | 885.797 | 115.791 | 38.860 |
| auditor | 1 | a88e1637282b3ae58 | Auditor for #1943 | large | high | 1 | 39 | 78 | 35.179 | 19.776 | 6.544.517 | 226.685 | 149.694 |
| spec-author | 3 | acf592be5d63c3a72 | Spec author repair #1943 | large | medium | 1 | 12 | 24 | 5.902 | 654 | 1.096.869 | 50.830 | 25.419 |
| test-implementer | 2 | a570ce1d24f410141 | Test implementer repair #1943 | large | medium | 1 | 18 | 36 | 10.116 | 1.259 | 1.762.373 | 62.205 | 37.447 |
| cleaner | 2 | a87aa8ae0c7a23cfd | Cleaner repair #1943 | medium | low | 1 | 5 | 10 | 1.555 | 0 | 387.209 | 34.068 | 9.411 |
| spec-checker | 2 | af53efe8fc079785d | Spec checker repair #1943 | medium | medium | 1 | 5 | 10 | 2.037 | 0 | 396.496 | 43.232 | 18.517 |
| auditor | 2 | ad1bcd3fb3b062511 | Auditor repair round #1943 | large | high | 1 | 14 | 28 | 10.071 | 3.529 | 1.364.589 | 63.249 | 38.465 |
| validator | 1 | a7fdddaf909293287 | Validator for #1943 | small | low | 2 | 9 | 18 | 3.159 | 348 | 213.754 | 67.233 | 13.440 |
| issue-manager | 1 | a303fa367a81064f8 | Issue manager for #1943 | medium | medium | 1 | 7 | 16 | 4.083 | 290 | 618.177 | 52.459 | 27.128 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 259 | 553 | 179.662 | 55.270 | 33.638.196 | 1.098.523 |
| medium | 33 | 68 | 13.396 | 892 | 2.706.835 | 332.108 |
| small | 9 | 18 | 3.159 | 348 | 213.754 | 67.233 |
| **Total** | 301 | 639 | 196.217 | 56.510 | 36.558.785 | 1.497.864 |

## Decisions taken

Seven. Worth a look: decision 1 (a group that wraps the property itself is withheld, read as covered by answer A) and decision 7 (in one `#if` group of three private auto-properties, the first and last lose the fix while the middle one keeps it — a literal consequence of answer A). Decision 5 took a small adjacent cleanup: the RH2001 fix reuses the Core conditional-directive check, now public.

## Suggestions

The change doc's *Visible output changes* was incomplete twice (after the spec author and after the first audit), each time for a shape of the same boundary. A spec author enumerating the boundary along "which directive kind stands next to the property, on which side, in which branch" up front would have saved one amendment and one repair round.
