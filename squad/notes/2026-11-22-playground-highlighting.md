# playground-highlighting

| | |
|---|---|
| Date | 2026-11-22 |
| Run | issue #1941 |
| Pull request | #1942 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | Error marks after the input is edited: vanish until the next Format, stay stale, or recompute live | Vanish until the next Format | yes |
| 2 | output question | Colour of `var`, `nameof`, `dynamic`, `value`: follow the parser, or keyword by word | Follow the parser | yes |
| 3 | output question | Mark of an empty error span at a line break: behind the line's last character, on it, or none | Behind the line's last character | yes |
| 4 | output question | Doc comments and directives: comment/directive colour as a whole, or XML and keywords inside coloured | As a whole, decided by trivia kind | yes |
| 5 | 9 | Keep incremental highlighting (+534 production lines, two new types) for typing speed, or a 60-line partial fix | Keep it ("if it works, we use it") | no (lead recommended the partial fix) |

The plan itself (Roslyn colours, live input colouring, error list, no wrap, alert box) was chosen in the chat before the issue was filed.

## Cost

- Stops: one output-question batch, one escalation 9.
- Repair rounds: 3 of 3 (audit 1: six findings; validation: panes grew to their content and large input was slow; audit 4: NUL turned into U+FFFD by the new markup rendering). One architecture finding (repeat of the accessible-name class) handled outside the rounds.
- Expensive: the speed repair (implementer spawn 4) and the first test implementer; browser checks ran only at validation, so the pane-width and speed defects surfaced after three audits.

## Usage

Session `7ccecb21-b72f-5c00-b88a-569dde938365`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 37 | 123 | 327 | 55.860 | 7.703 | 24.475.425 | 224.348 | 190.617 |
| spec-author | 1 | a4993b4127f16df66 | Spec author for #1941 | large | medium | 1 | 20 | 40 | 27.865 | 12.099 | 2.374.901 | 153.687 | 74.493 |
| skeptic | 1 | a6a7e7d76ab5c3f41 | Skeptic for #1941 | large | high | 1 | 24 | 50 | 36.394 | 19.758 | 2.777.453 | 163.330 | 88.255 |
| spec-author | 2 | ae00ea89241560d3f | Spec author amend #1941 | large | medium | 1 | 12 | 24 | 13.487 | 2.418 | 1.096.097 | 112.632 | 36.843 |
| test-implementer | 1 | a481512dda319763b | Test implementer for #1941 | large | medium | 1 | 63 | 126 | 96.436 | 31.360 | 11.191.520 | 198.777 | 173.454 |
| implementer | 1 | a79b437ff1a373912 | Implementer for #1941 | large | medium | 1 | 28 | 56 | 28.817 | 10.690 | 3.799.432 | 115.919 | 90.596 |
| spec-author | 3 | a8197845cd9ec6e2f | Code owners fix #1941 | large | medium | 2 | 9 | 18 | 3.597 | 515 | 726.648 | 40.233 | 14.858 |
| cleaner | 1 | a7890da2ba094f932 | Cleaner for #1941 | medium | low | 1 | 5 | 10 | 1.155 | 0 | 312.981 | 79.985 | 4.492 |
| spec-checker | 1 | a9da80ba6ebce675a | Spec checker for #1941 | medium | medium | 1 | 7 | 14 | 3.148 | 425 | 530.832 | 102.704 | 27.706 |
| auditor | 1 | a0cb4cdbf4ee4bdb8 | Auditor for #1941 | large | high | 1 | 38 | 76 | 45.164 | 28.773 | 5.978.341 | 170.289 | 144.997 |
| test-implementer | 2 | a75b61c6be1561b31 | Repair tests for #1941 | large | medium | 1 | 29 | 58 | 26.850 | 5.990 | 3.556.359 | 153.700 | 78.043 |
| implementer | 2 | ad867d1e80e274511 | Repair implementer for #1941 | large | medium | 1 | 14 | 28 | 8.871 | 515 | 1.394.823 | 70.252 | 45.421 |
| cleaner | 2 | a2527c904d6d7873d | Repair cleaner for #1941 | medium | low | 1 | 5 | 10 | 1.291 | 0 | 313.482 | 80.412 | 4.940 |
| spec-checker | 2 | a16ed2b801db61006 | Repair spec check #1941 | medium | medium | 1 | 7 | 14 | 3.877 | 612 | 504.630 | 92.579 | 17.496 |
| spec-author | 4 | afd049ab3cceaa85a | PLAY-012 sentence #1941 | large | medium | 1 | 9 | 18 | 3.484 | 178 | 718.991 | 38.417 | 13.116 |
| auditor | 2 | a37c83dd56aa23bcc | Repair audit #1941 | large | high | 2 | 24 | 48 | 24.450 | 12.142 | 2.859.555 | 161.665 | 86.479 |
| spec-checker | 3 | a310b55a2ac26f41c | Spec recheck #1941 | medium | medium | 1 | 4 | 8 | 1.611 | 0 | 292.288 | 34.681 | 10.083 |
| test-implementer | 3 | acfcc65f4cabbe908 | Accessible-name test #1941 | large | medium | 1 | 11 | 22 | 6.180 | 595 | 978.003 | 49.930 | 24.639 |
| implementer | 3 | a8206aa10c85fb734 | Accessible-name fix #1941 | large | medium | 1 | 8 | 16 | 3.341 | 141 | 646.865 | 40.243 | 15.595 |
| cleaner | 3 | af156cb65c9d6ce21 | Cleaner after name fix #1941 | medium | low | 1 | 5 | 10 | 985 | 0 | 328.006 | 83.709 | 4.585 |
| auditor | 3 | ac042d5574732487a | Audit name fix #1941 | large | high | 1 | 10 | 20 | 8.212 | 2.920 | 884.142 | 53.588 | 28.938 |
| validator | 1 | a5e70875187c76d38 | Validator for #1941 | small | low | 1 | 22 | 44 | 19.267 | 5.589 | 1.031.940 | 113.578 | 45.242 |
| test-implementer | 4 | af8492de61d82540a | Repair 2 tests #1941 | large | medium | 1 | 30 | 60 | 32.682 | 11.226 | 3.565.016 | 94.122 | 69.433 |
| implementer | 4 | ae46c87ed5f66cebb | Repair 2 implementer #1941 | large | medium | 1 | 93 | 188 | 112.267 | 49.240 | 16.417.326 | 338.642 | 179.195 |
| spec-author | 5 | af13e8c214b7fde7a | Code owners round 2 #1941 | large | medium | 2 | 14 | 32 | 3.895 | 475 | 1.179.638 | 98.992 | 23.412 |
| test-implementer | 5 | aab46f805014b75cd | Equivalence test #1941 | large | medium | 3 | 24 | 48 | 25.293 | 8.730 | 2.617.961 | 136.837 | 61.247 |
| cleaner | 4 | a20ff3492b9772495 | Repair 2 cleaner #1941 | medium | low | 1 | 5 | 10 | 1.094 | 0 | 328.600 | 83.903 | 4.762 |
| auditor | 4 | ac86693ae54a59bbb | Repair 2 audit #1941 | large | high | 2 | 37 | 74 | 36.727 | 20.819 | 4.914.438 | 173.032 | 97.892 |
| spec-checker | 4 | ad024350c137fbb3b | Repair 2 spec check #1941 | medium | medium | 1 | 6 | 12 | 2.612 | 178 | 420.764 | 98.272 | 23.218 |
| test-implementer | 6 | a9a779aa80c388d46 | NUL test #1941 | large | medium | 1 | 41 | 82 | 37.165 | 13.587 | 4.783.020 | 155.032 | 79.437 |
| implementer | 5 | afaab3ea14af561a7 | Repair 3 implementer #1941 | large | medium | 2 | 21 | 42 | 17.449 | 6.992 | 2.196.977 | 72.468 | 47.096 |
| test-implementer | 7 | ac0ea916688978e66 | Remove BL0006 suppression #1941 | large | medium | 1 | 18 | 36 | 12.561 | 2.572 | 1.728.906 | 60.853 | 36.078 |
| cleaner | 5 | a7228d595f5ef861e | Repair 3 cleaner #1941 | medium | low | 1 | 5 | 10 | 997 | 0 | 330.215 | 84.332 | 5.191 |
| auditor | 5 | a3a636c86564f92c4 | Repair 3 audit #1941 | large | high | 2 | 25 | 50 | 20.400 | 8.854 | 2.839.269 | 146.655 | 71.549 |
| spec-checker | 5 | abadf0f28277299d1 | Repair 3 spec check #1941 | medium | medium | 1 | 5 | 10 | 2.387 | 195 | 337.023 | 91.926 | 16.878 |
| validator | 2 | a06cb7b4b82be19e5 | Final validator #1941 | small | low | 1 | 23 | 46 | 13.706 | 3.899 | 1.032.297 | 101.149 | 41.335 |
| issue-manager | 1 | a05067f07fdf3ef6a | Follow-up issues #1941 | medium | medium | 1 | 9 | 21 | 5.175 | 369 | 784.746 | 109.687 | 31.034 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 725 | 1.539 | 687.447 | 258.292 | 103.701.106 | 3.023.643 |
| medium | 63 | 129 | 24.332 | 1.779 | 4.483.567 | 942.190 |
| small | 45 | 90 | 32.973 | 9.488 | 2.064.237 | 214.727 |
| **Total** | 833 | 1.758 | 744.752 | 269.559 | 110.248.910 | 4.180.560 |

## Decisions taken

12, recorded in the run's lead file. Worth the maintainer's eye: Decision 9 (the speed bar the lead set: paste ≤ ~2 s, keystroke < ~100 ms at 1000 lines) and Decision 12 (accepting it as met on the median: 93 ms median, 126 ms max, 251 ms first key after Format).

## Suggestions

- Browser-only behaviour (layout, speed, HTML-parser effects) was invisible to bUnit and the stylesheet tests until the validator served the page; for a Playground run, a browser smoke check at the first audit would have found the growing panes and the slowness two rounds earlier.
- A new rendering path (markup strings) needs its own input-class enumeration (control characters, NUL) at the time it is introduced; the equivalence tests compared it only with itself.
