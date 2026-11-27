# using-block-directive-report

| | |
|---|---|
| Date | 2026-11-27 |
| Run | issue #1836 |
| Pull request | #1968 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | Report an out-of-order using block that holds a directive, with no fix (A), or stay silent (B)? | A | yes |
| 2 | output question | Every directive kind lifts the silence (A), or only conditional directives (B)? | A | yes |
| 3 | output question | Compare across the whole block (A), or only usings no directive separates (B)? | A | yes |
| 4 | 10 | Declare a using block that holds a directive unsupported? | A, not declared (the first reply "4" had no letter and was asked back) | yes |

## Cost

One output-question batch, plus one short ask-back. No repair round. The test implementer ran twice: the implementer disputed the setup of one Fix All test, because markup in the fixed state is ignored without `MarkupMode.Allow`. The spec author ran twice, once to draft and once to write in the answers. `origin/main` moved once before the audit and was merged cleanly. The auditor and the test implementer were the most expensive spawns.

## Usage

Session `3b25c682-0402-5e94-8825-6bf1a4a163da`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 10 | 51 | 125 | 19.020 | 1.469 | 7.324.723 | 110.461 | 80.124 |
| spec-author | 1 | a56d2bb26e8519bad | Spec author for #1836 | large | medium | 1 | 30 | 59 | 34.665 | 15.108 | 3.888.755 | 178.383 | 98.854 |
| skeptic | 1 | a5c14414266172d38 | Skeptic for #1836 | large | high | 1 | 40 | 78 | 43.941 | 26.300 | 6.156.290 | 150.912 | 126.016 |
| spec-author | 2 | a57c87e4ae61eda76 | Spec author follow-up #1836 | large | medium | 1 | 19 | 36 | 14.146 | 2.842 | 2.115.221 | 79.377 | 53.635 |
| test-implementer | 1 | a38b9dd23fd0e7651 | Test implementer #1836 | large | medium | 1 | 41 | 78 | 53.825 | 17.165 | 6.532.586 | 167.041 | 141.491 |
| implementer | 1 | ae57069308745c7d2 | Implementer #1836 | large | medium | 1 | 19 | 36 | 9.646 | 2.034 | 1.977.333 | 71.093 | 45.539 |
| test-implementer | 2 | ae589bc5a16bb585d | Test setup repair #1836 | large | medium | 1 | 6 | 12 | 4.039 | 462 | 507.619 | 46.016 | 20.390 |
| cleaner | 1 | a930483dcce3530e0 | Cleaner #1836 | medium | low | 1 | 6 | 12 | 1.565 | 0 | 432.274 | 89.262 | 9.883 |
| spec-checker | 1 | a47a27e4241fc96e0 | Spec checker #1836 | medium | medium | 1 | 9 | 17 | 3.891 | 780 | 816.747 | 118.156 | 39.223 |
| auditor | 1 | a8131637df228e309 | Auditor #1836 | large | high | 1 | 52 | 99 | 34.395 | 18.412 | 9.334.071 | 260.717 | 181.726 |
| validator | 1 | ae0cab3c394b01d8c | Validator #1836 | small | low | 1 | 9 | 18 | 3.375 | 340 | 211.357 | 68.267 | 11.793 |
| issue-manager | 1 | a7ca8b7d12ecabb4d | Issue manager #1836 | medium | medium | 1 | 8 | 17 | 4.479 | 457 | 690.851 | 112.484 | 33.547 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 258 | 523 | 213.677 | 83.792 | 37.836.598 | 1.064.000 |
| medium | 23 | 46 | 9.935 | 1.237 | 1.939.872 | 319.902 |
| small | 9 | 18 | 3.375 | 340 | 211.357 | 68.267 |
| **Total** | 290 | 587 | 226.987 | 85.369 | 39.987.827 | 1.452.169 |

## Decisions taken

Five. Decision 1, that RH7207 checks only the order and not the blank lines in a block that holds a directive, was taken by the lead under SHARED-DIAG-010 instead of being asked. The maintainer may want to see it.

## Suggestions

When an issue's own text is an output question, it could be asked before the spec author runs. Here the spec author and the skeptic mainly refined the question. A test implementer checklist item for `MarkupMode.Allow` on Fix All tests that expect a remaining diagnostic would have avoided the second test-implementer spawn.
