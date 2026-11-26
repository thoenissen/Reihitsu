# rh5202-tab-aligned-raw-strings

| | |
|---|---|
| Date | 2026-11-26 |
| Run | issue #1925 |
| Pull request | #1953 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | RH5202 on a smart-tab literal (tabs, then spaces aligning the closing quotes): report and fix to spaces, or not | A (report, fix writes spaces) | yes |
| 2 | output question | RH5202 rule page: add a sentence that the indentation is written with spaces, not tabs | B (add it) | yes |

The issue's own output question (report and fix a tab-aligned literal) was answered A on the issue before the run.

## Cost

One stop for one output-question batch, after the skeptic. Two repair rounds: round 1 was a one-line spec wording finding from the spec checker (cleaner skipped, spec text only); round 2 came from the first audit, whose high finding was a pre-existing lookup defect in the code fix (`FindNode` returning a wrapper node with the literal's span, so no fix was registered for a literal used as an argument, tuple or collection element, constant pattern, or attribute argument), newly exposed because tab-aligned literals in those positions are now reported; it was taken in as an adjacent fix. The skeptic, the first audit, and the first test implementer were the expensive spawns.

## Usage

Session `8454a2c2-597e-59f6-a041-7ba5648efd58`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 18 | 59 | 147 | 24.498 | 2.630 | 8.817.115 | 124.245 | 92.750 |
| spec-author | 1 | aecd7e2b32772eed7 | Spec author for #1925 | large | medium | 1 | 23 | 45 | 21.636 | 7.437 | 2.884.905 | 103.824 | 78.962 |
| skeptic | 1 | a2b18b6d763daabda | Skeptic for #1925 | large | high | 1 | 32 | 66 | 39.033 | 25.121 | 4.410.466 | 190.710 | 113.674 |
| spec-author | 2 | af70e54df31c51677 | Spec author amendment #1925 | large | medium | 1 | 15 | 30 | 8.933 | 1.473 | 1.445.746 | 57.841 | 32.950 |
| test-implementer | 1 | afdec05207ddc07ca | Test implementer #1925 | large | medium | 1 | 47 | 94 | 46.024 | 13.810 | 6.981.553 | 146.994 | 122.221 |
| implementer | 1 | a561841cf7e80b38c | Implementer #1925 | large | medium | 1 | 19 | 38 | 7.960 | 1.059 | 2.166.274 | 80.819 | 56.074 |
| cleaner | 1 | a68d26680b928bd57 | Cleaner #1925 | medium | low | 1 | 4 | 8 | 1.068 | 0 | 245.512 | 84.964 | 7.532 |
| spec-checker | 1 | a16eea7ca5b5383a5 | Spec checker #1925 | medium | medium | 1 | 6 | 12 | 3.150 | 531 | 468.728 | 106.460 | 29.469 |
| spec-author | 3 | aebabe0fae8a7e4a6 | Spec author repair #1925 | large | medium | 1 | 9 | 18 | 4.200 | 903 | 761.199 | 42.662 | 17.939 |
| spec-checker | 2 | a4879bb702f9816b1 | Spec checker re-check #1925 | medium | medium | 1 | 4 | 8 | 1.213 | 0 | 299.834 | 34.287 | 9.560 |
| auditor | 1 | a0708c96e8f89bf2e | Auditor #1925 | large | high | 1 | 41 | 84 | 42.426 | 24.615 | 6.854.184 | 179.310 | 154.548 |
| test-implementer | 2 | a556e9594e98baec3 | Test implementer repair #1925 | large | medium | 1 | 16 | 32 | 18.787 | 4.273 | 1.713.208 | 133.388 | 55.831 |
| implementer | 2 | a555bac73ae7faad1 | Implementer repair #1925 | large | medium | 1 | 15 | 30 | 8.704 | 2.078 | 1.349.363 | 163.514 | 32.680 |
| cleaner | 2 | ac96213df2187f388 | Cleaner repair #1925 | medium | low | 1 | 4 | 8 | 989 | 0 | 247.836 | 86.030 | 8.591 |
| spec-checker | 3 | a4dff73574e4cedd8 | Spec checker repair #1925 | medium | medium | 1 | 7 | 14 | 2.307 | 196 | 530.842 | 99.930 | 22.924 |
| auditor | 2 | a4b71877947983138 | Auditor repair #1925 | large | high | 1 | 23 | 46 | 19.476 | 7.684 | 2.534.620 | 138.878 | 61.752 |
| validator | 1 | a39915e9d2e0ee886 | Validator #1925 | small | low | 1 | 8 | 16 | 3.192 | 255 | 184.013 | 61.790 | 12.022 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 299 | 630 | 241.677 | 91.083 | 39.918.633 | 1.362.185 |
| medium | 25 | 50 | 8.727 | 727 | 1.792.752 | 411.671 |
| small | 8 | 16 | 3.192 | 255 | 184.013 | 61.790 |
| **Total** | 332 | 696 | 253.596 | 92.065 | 41.895.398 | 1.835.646 |

## Decisions taken

Seven. The one the maintainer may want to see: taking the code-fix node lookup defect (pre-existing on `main` for misaligned literals) into the run as an adjacent fix instead of a follow-up. Also: no change to the diagnostic message or titles (the maintainer had left that to the run), and declaring a tab in front of a raw string's closing quotes unsupported was judged not an option because tab-indented code is commonly written.

## Suggestions

The first skeptic could have caught the wrapper-node lookup gap: its enumeration of positions covered where a literal is reported, not whether the fix is registered for each position. A skeptic or test-implementer checklist item "the fix is offered in every syntactic position the diagnostic reports" would have avoided a repair round.
