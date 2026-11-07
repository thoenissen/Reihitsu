# rh2-fix-safety

| | |
|---|---|
| Date | 2026-11-07 |
| Run | issues #967, #1286, #1287, #1288 |
| Pull request | #1609 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | Which part of a partial member carries its single diagnostic (RH2002, RH2004, RH8306, RH74xx) | A — first qualifying non-generated part, defining before implementing | yes |
| 2 | output question | RH2001: which struct types withhold the fix | C — every struct not declared `readonly`; `Nullable<T>` of enum/readonly struct counts as readonly (not the recommended C′ with built-in types exempt) | no |
| 3 | output question | RH2005: references outside the type body that still compile (`nameof` in own attribute, `cref`) | B — withhold | no |
| 4 | output question | RH2001: property typed by an unconstrained type parameter | A — withhold | yes |
| 5 | output question | RH2005: `public` on its own line behind another modifier | A — line stays in place | yes |
| 6 | output question | RH2005/RH2001: comment, directive or `#if` line in the rewritten gap | A — withhold | yes |
| 7 | output question | RH2005: later `public` ends its line | A — lines stay | yes |
| 8 | output question | RH2005: comment, directive or `#if` behind a later `public` | A — withhold | yes |
| 9 | output question | RH2005: gap widths around a later `public` in the middle of a line | A — `public` goes with the gap behind it | yes |

Maintainer guidance given with answers 1–3: "keep it simple".

## Cost

- Stops: five output-question batches (Q1–Q3 before the test implementer; Q4 after the first spec check; Q5–Q6, Q7–Q8 and Q9 inside repair rounds 1–3).
- Repair rounds: three, the maximum. Round 1: four audit findings (comment loss in the modifier helper, RH2001 guard positions, a copied trivia predicate, SHARED-DIAG-012 picking a part the driver never analyses) with two structural decisions. Round 2: the modifier-helper defect repeated its class (trailing trivia of a later `public`) and was handled as an architecture finding. Round 3: gap widths (an output question) and dead overloads.
- Roles spawned more than once: spec author 6×, test implementer 6×, implementer 4×, auditor 4×, cleaner 4×, spec checker 4× — once per round each, plus small harness/code-owner corrections.
- Expensive: the first test implementer and implementer spawns (four issues, ~15 production files) and the first auditor. The RH2005 modifier-trivia class cost three audits because the skeptic's first matrix enumerated only the trivia in front of the first token after the attribute lists, not both sides of a later accessibility keyword.

## Usage

Session `11377a95-24cc-5692-825a-5a87f974a5d7`. Responses counted once per message id. Prices are not applied; compute a cost from the tokens and the model.

| Agent | Spawn | Agent ID | Description | Model | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | claude-opus-5-5 | medium | 26 | 120 | 297 | 68.411 | 10.453 | 24.235.103 | 218.078 |
| spec-author | 1 | ab0be072b1225c5f9 | Spec author for rh2-fix-safety | claude-opus-5-5 | medium | 1 | 51 | 99 | 59.447 | 35.765 | 7.825.312 | 219.253 |
| skeptic | 1 | aafad39a0a66aa980 | Skeptic for rh2-fix-safety | claude-opus-5-5 | high | 1 | 52 | 106 | 57.091 | 30.082 | 7.773.274 | 214.257 |
| spec-author | 2 | abd9d7aaac48c19ed | Spec author amendment round | claude-opus-5-5 | medium | 2 | 40 | 84 | 44.432 | 18.549 | 5.756.767 | 185.905 |
| test-implementer | 1 | a4d61b2ae94ab881c | Test implementer for rh2-fix-safety | claude-opus-5-5 | medium | 1 | 75 | 150 | 91.637 | 21.876 | 14.619.911 | 243.294 |
| implementer | 1 | a91b96c38b9f00d29 | Implementer for rh2-fix-safety | claude-opus-5-5 | medium | 1 | 90 | 180 | 98.762 | 35.348 | 19.843.079 | 263.106 |
| test-implementer | 2 | a0c6dfc3e33b7879c | Fix three disputed test harness settings | claude-opus-5-5 | medium | 1 | 10 | 19 | 6.153 | 226 | 899.974 | 50.044 |
| spec-author | 3 | a3c560493a77186e9 | Spec author: name new code owners | claude-opus-5-5 | medium | 1 | 9 | 18 | 5.608 | 1.185 | 825.638 | 51.769 |
| cleaner | 1 | acc229af7f6543fa9 | Cleaner for rh2-fix-safety | claude-sonnet-5-5 | low | 1 | 6 | 12 | 1.492 | 94 | 403.143 | 82.665 |
| spec-checker | 1 | a8949556f1aaec508 | Spec checker for rh2-fix-safety | claude-sonnet-5-5 | medium | 1 | 14 | 27 | 8.030 | 2.233 | 1.407.734 | 128.598 |
| auditor | 1 | a66ef5ece85107615 | Auditor for rh2-fix-safety | claude-opus-5-5 | high | 1 | 61 | 122 | 76.024 | 44.742 | 13.779.603 | 323.110 |
| test-implementer | 3 | ab07ab3e137dadf37 | Repair round 1 regression tests | claude-opus-5-5 | medium | 1 | 50 | 95 | 67.868 | 30.927 | 8.358.585 | 238.227 |
| spec-author | 4 | ad3a83bd53a54fbdd | Spec author records Q5/Q6 | claude-opus-5-5 | medium | 1 | 20 | 38 | 16.285 | 6.521 | 2.069.000 | 139.092 |
| test-implementer | 4 | ae14206e146f2ddd6 | Narrow tests to answered outputs | claude-opus-5-5 | medium | 1 | 24 | 46 | 20.511 | 4.887 | 2.851.823 | 95.959 |
| implementer | 2 | aa9b9f9613ad6bf2c | Implementer repair round 1 | claude-opus-5-5 | medium | 1 | 48 | 96 | 62.598 | 34.990 | 8.832.014 | 253.477 |
| cleaner | 2 | ae75191ba60191ac5 | Cleaner repair round 1 | claude-sonnet-5-5 | low | 1 | 7 | 14 | 1.764 | 64 | 542.061 | 94.690 |
| spec-checker | 2 | ab550b09783f6cbf2 | Spec checker repair round 1 | claude-sonnet-5-5 | medium | 1 | 8 | 15 | 5.925 | 1.402 | 689.907 | 115.184 |
| auditor | 2 | a2d1078036a188d32 | Auditor repair round 1 | claude-opus-5-5 | high | 1 | 36 | 72 | 51.729 | 33.931 | 5.698.579 | 235.426 |
| spec-author | 5 | a830b133ffe54f6ae | Spec author records Q7/Q8 | claude-opus-5-5 | medium | 1 | 13 | 25 | 10.819 | 3.967 | 1.203.479 | 119.238 |
| test-implementer | 5 | aac823eea1cd041e1 | Test implementer repair round 2 | claude-opus-5-5 | medium | 1 | 31 | 59 | 27.722 | 9.184 | 3.916.573 | 110.754 |
| implementer | 3 | adba00bd7dd14eb65 | Implementer repair round 2 | claude-opus-5-5 | medium | 1 | 22 | 44 | 24.086 | 11.016 | 2.627.388 | 157.963 |
| cleaner | 3 | accd5a063bce5839d | Cleaner repair round 2 | claude-sonnet-5-5 | low | 1 | 6 | 12 | 1.667 | 46 | 408.092 | 84.141 |
| spec-checker | 3 | ae549cd475c4a9fd4 | Spec checker repair round 2 | claude-sonnet-5-5 | medium | 1 | 8 | 16 | 3.828 | 466 | 675.780 | 111.255 |
| auditor | 3 | adc17c5ff5a25c689 | Auditor repair round 2 | claude-opus-5-5 | high | 1 | 38 | 76 | 50.039 | 31.018 | 6.001.468 | 216.245 |
| spec-author | 6 | a8e77d4c3b865a082 | Spec author records Q9 | claude-opus-5-5 | medium | 1 | 10 | 19 | 6.284 | 1.594 | 849.409 | 107.120 |
| test-implementer | 6 | a937b61e37641229b | Test implementer repair round 3 | claude-opus-5-5 | medium | 1 | 16 | 31 | 10.181 | 1.539 | 1.626.615 | 70.150 |
| implementer | 4 | ad167bc541b3da71f | Implementer repair round 3 | claude-opus-5-5 | medium | 1 | 15 | 30 | 14.103 | 2.220 | 1.384.864 | 117.731 |
| cleaner | 4 | adfbbd3c460e37d6f | Cleaner repair round 3 | claude-sonnet-5-5 | low | 1 | 5 | 10 | 1.547 | 50 | 347.244 | 91.135 |
| spec-checker | 4 | a8a66be15adf57d0c | Spec checker repair round 3 | claude-sonnet-5-5 | medium | 1 | 9 | 18 | 3.609 | 343 | 748.457 | 109.295 |
| auditor | 4 | a40fb4144bcdfa08e | Auditor repair round 3 | claude-opus-5-5 | high | 1 | 26 | 52 | 25.213 | 13.013 | 3.257.725 | 165.771 |
| validator | 1 | a2d67d693adc24630 | Validator for rh2-fix-safety | claude-haiku-5-5 | low | 1 | 9 | 18 | 3.345 | 121 | 674.646 | 88.539 |
| issue-manager | 1 | a2231641e504c2933 | Issue manager for rh2-fix-safety | claude-sonnet-5-5 | medium | 1 | 9 | 17 | 8.742 | 897 | 885.788 | 137.026 |

| Model | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| claude-haiku-5-5 | 9 | 18 | 3.345 | 121 | 674.646 | 88.539 |
| claude-opus-5-5 | 857 | 1.758 | 895.003 | 383.033 | 144.236.183 | 3.795.969 |
| claude-sonnet-5-5 | 72 | 141 | 36.604 | 5.595 | 6.108.206 | 953.989 |
| **Total** | 938 | 1.917 | 934.952 | 388.749 | 151.019.035 | 4.838.497 |

## Decisions taken

30 decisions. Ones the maintainer may want to see: decision 10 (type-parameter half later confirmed as Q4); decision 16/24 (only RH2004 moved to a per-type symbol action; RH2002, RH8306 and the RH74 bases still report from each part's node analysis with the part chosen from all parts); decision 21 (RH2001 name on its own line keeps the formatted layout); decision 23 (RH7409–RH7412 stand-in rule keeps `main`'s only-some-parts output pending #1334).

## Suggestions

- The skeptic's failure-class matrix for a modifier rewrite should enumerate both sides of every token the fix removes or replaces (leading and trailing trivia, start/end of line, gap widths), not only the shape the issue quotes. In this run each audit found the next axis of the same class.
- Test helpers that accept several outcomes (`VerifyCodeFixIsWithheldOrProducesOneOf`) hid undecided visible output until the lead read the handover; a test implementer that needs one should raise it as an output question in its verdict line.
