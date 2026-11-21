# rh5201-remaining-chain-layouts

| | |
|---|---|
| Date | 2026-11-21 |
| Run | issue #893 |
| Pull request | #1907 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | Report a first call behind a wrapped prefix member access (the issue's case) | A — report | yes |
| 2 | output question | Report a first call behind a first link a comment or directive keeps below the root | A — report | yes |
| 3 | output question | One more diagnostic on chains already reported for another link | A — yes | yes |
| 4 | output question | Report a first call behind a comment-held break inside a prefix operator | B — not reported | yes |
| 5 | output question | Rule page bullet and how-to-fix wording | A — update | yes |
| 6 | output question | Report a link behind an own-line `*/` in the anchor column (adjacent fix changed RH5201-001's MUST NOT) | A — report | yes |
| 7 | output question | A second comment between `*/` and the link | A — report | yes |
| 8 | output question | A multi-line comment starting behind another comment on its own line | A — report | yes |

## Cost

- Stops: four — the batch before the test implementer (Q1–Q5), then Q6, Q7 and Q8 one at a time. Q6 came from the spec author carrying out an adjacent fix the lead had judged "no output in question", which turned out to change an RH5201-001 MUST NOT. Q7 and Q8 were neighbours raised one after another by the implementer and then the spec author once a shared helper read wider than the spec wording.
- Repair rounds: two. Round 1 (auditor: two one-sided conditions without tests; one overstated code comment; plus a spec wording clarification) and round 2 (a missing `[Requirement]` tag on a round-1 test). Each round re-ran cleaner, spec checker and auditor.
- Roles spawned more than once: spec author four times (answers, Q7, Q8, repair wording), test implementer four times (initial, Q7/Q8 tests, two repairs), cleaner three times, spec checker three times, auditor three times.
- Most expensive: the initial test implementer, the first auditor, and the skeptic.

## Usage

## Usage

Session `d64fb6c0-923c-5480-aa28-a39f6f6225b7`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 28 | 85 | 214 | 36.568 | 4.213 | 14.444.615 | 154.579 | 126.645 |
| spec-author | 1 | a2974aa214f4f4372 | Spec author for #893 | large | medium | 1 | 43 | 83 | 38.735 | 18.650 | 5.430.664 | 169.178 | 87.786 |
| skeptic | 1 | a15af679fc59ee516 | Skeptic for #893 | large | high | 1 | 34 | 70 | 43.222 | 26.613 | 4.627.525 | 186.656 | 110.549 |
| spec-author | 2 | ab899e848d5069edb | Spec author follow-up #893 | large | medium | 1 | 22 | 44 | 18.335 | 6.543 | 2.320.854 | 128.537 | 51.626 |
| test-implementer | 1 | afccb9f8ef1f74ef4 | Test implementer for #893 | large | medium | 1 | 57 | 114 | 57.700 | 23.830 | 9.924.120 | 186.254 | 162.229 |
| implementer | 1 | a761e04960fe32698 | Implementer for #893 | large | medium | 1 | 23 | 46 | 22.132 | 8.453 | 2.956.477 | 107.163 | 82.383 |
| cleaner | 1 | a0e067940ec9dc24c | Cleaner for #893 | medium | low | 1 | 4 | 8 | 1.168 | 0 | 255.350 | 90.672 | 14.139 |
| spec-author | 3 | a7a873d1f9a683648 | Spec author Q7 pass #893 | large | medium | 1 | 20 | 40 | 19.390 | 7.571 | 2.081.204 | 71.200 | 46.983 |
| spec-author | 4 | a4c45d25e55c17bff | Spec author Q8 pass #893 | large | medium | 1 | 11 | 22 | 5.750 | 362 | 944.874 | 44.888 | 20.820 |
| test-implementer | 2 | a588023562d819c4f | Test implementer Q7/Q8 #893 | large | medium | 1 | 15 | 30 | 9.102 | 858 | 1.407.911 | 59.651 | 34.896 |
| cleaner | 2 | af6266a7412bc4cdd | Cleaner pass 2 #893 | medium | low | 1 | 4 | 8 | 1.220 | 0 | 245.725 | 85.782 | 9.235 |
| spec-checker | 1 | a4091f2bb19e48807 | Spec checker #893 | medium | medium | 1 | 7 | 14 | 3.641 | 759 | 599.209 | 56.564 | 31.934 |
| auditor | 1 | ac9120b2b2a11c5eb | Auditor for #893 | large | high | 1 | 43 | 88 | 42.907 | 27.484 | 7.346.089 | 241.324 | 165.167 |
| test-implementer | 3 | a3e0a548c6f6477a2 | Repair tests #893 | large | medium | 1 | 17 | 34 | 12.137 | 4.245 | 1.791.480 | 126.773 | 50.192 |
| cleaner | 3 | a6fd1c236886fca72 | Cleaner repair #893 | medium | low | 1 | 6 | 12 | 2.544 | 0 | 426.919 | 89.738 | 13.071 |
| spec-author | 5 | ab72e553b69f3b4d5 | Spec author repair #893 | large | medium | 1 | 12 | 24 | 6.088 | 1.359 | 1.042.078 | 46.548 | 21.810 |
| spec-checker | 2 | a388d339c59cf7c4c | Spec checker repair #893 | medium | medium | 1 | 5 | 10 | 2.115 | 127 | 338.110 | 94.246 | 18.105 |
| auditor | 2 | a6f52aa84f09f147e | Auditor repair #893 | large | high | 1 | 29 | 58 | 24.764 | 13.780 | 3.236.722 | 149.800 | 73.567 |
| test-implementer | 4 | a5f4a550c589bd8d7 | Repair 2 test tag #893 | large | medium | 1 | 7 | 14 | 3.377 | 95 | 577.704 | 42.505 | 17.905 |
| cleaner | 4 | ae683f9acc1c32752 | Cleaner repair 2 #893 | medium | low | 1 | 4 | 8 | 1.123 | 0 | 245.466 | 85.117 | 8.556 |
| spec-checker | 3 | a316aa54f84a74194 | Spec checker repair 2 #893 | medium | medium | 1 | 5 | 10 | 1.734 | 0 | 376.893 | 101.453 | 25.334 |
| auditor | 3 | a2b4cc07ea79b70a2 | Auditor repair 2 #893 | large | high | 1 | 8 | 16 | 4.117 | 544 | 631.147 | 100.710 | 24.532 |
| validator | 1 | a452e23e66dbf370e | Validator #893 | small | low | 2 | 8 | 16 | 2.618 | 225 | 172.649 | 65.188 | 10.860 |
| issue-manager | 1 | ae7a9ab9492307435 | Issue manager #893 | medium | medium | 1 | 7 | 16 | 2.661 | 115 | 604.983 | 50.771 | 26.139 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 426 | 897 | 344.324 | 144.600 | 58.763.464 | 1.815.766 |
| medium | 42 | 86 | 16.206 | 1.001 | 3.092.655 | 654.343 |
| small | 8 | 16 | 2.618 | 225 | 172.649 | 65.188 |
| **Total** | 476 | 999 | 363.148 | 145.826 | 62.028.768 | 2.535.297 |

## Decisions taken

Six: rule location in the shared chain spec with a new RH5201-006; dropping the issue's second case (no caller reaches it — the precheck refuses those inputs); the shared first-call decision moved into `Reihitsu.Core` `FluentChain` (architecture finding); taking in the anchor-column `*/` adjacent fix; change-doc listing and `Uses` row wording; stating the first-link exclusion in RH5201-002. The adjacent-fix decision is the one the maintainer would have wanted to see earlier: it surfaced as Q6.

## Suggestions

- When a spec-author handover proposes a neighbour question (Q7, Q8), the lead could ask the skeptic or spec author to enumerate the shared helper's whole accepted class at once, so siblings come in one batch instead of one stop each.
- A test added in a repair round could be checked for its `[Requirement]` tag by the spec checker of that round before the auditor, which would have saved repair round 2.
