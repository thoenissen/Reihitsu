# unsupported-assignment-arm-comment

| | |
|---|---|
| Date | 2026-11-26 |
| Run | issue #1926 |
| Pull request | #1944 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | Refuse a comment behind a compound assignment operator (`+=` …) too, folding in the arm/anonymous-member shape | B: refuse every assignment operator | yes |
| 2 | output question | Refuse a comment behind a deconstruction declaration's `=` | A: refuse | yes |
| 3 | 10 | Declare the remaining `=` gaps (enum member, parameter default, directive behind a declarator's `=`) | A: in this work | no (B: change of its own) |
| 4 | output question | Wording of the new reason line | delegated to the lead; two reason lines | n/a |
| 5 | output question | Refuse a comment behind an attribute argument's or a using alias's `=` | B: unsupported | no (A: keep supported) |

The second batch needed one ask-back: the first reply ("unsupported") answered only question 5.

## Cost

- Two stops for output-question batches (before the test implementer), plus one ask-back.
- The spec author ran five times: the initial spec, two amendments that carried the two batches' answers (each widened the shape), a small repair of two stale examples found by the test implementer, and the pre-audit merge conflict. The skeptic ran twice: the second time on the amendment that answer 3A caused, which no skeptic had checked. That second skeptic was cut off by a usage limit and resumed four days later.
- The test implementer was the most expensive spawn (about 61M cache read): it built a throwaway prototype outside the repository to find every existing test the change breaks; the prototype found two stale spec examples the earlier roles had missed.
- No repair round; one comment-only audit finding, proved text-only.

## Usage

Session `714123c5-0d55-509c-ab5d-9ae3c7c13cfc`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 22 | 75 | 184 | 35.073 | 4.900 | 15.128.489 | 435.809 | 171.581 |
| spec-author | 1 | a0e89a9b2ecf658bd | Spec author for #1926 | large | medium | 1 | 57 | 110 | 62.972 | 33.090 | 9.866.461 | 236.349 | 157.181 |
| skeptic | 1 | a6a3e3caf90429249 | Skeptic for #1926 | large | high | 1 | 59 | 120 | 67.517 | 41.561 | 9.984.195 | 195.054 | 170.410 |
| spec-author | 2 | ad2264169bd10735e | Spec author amendment for #1926 | large | medium | 1 | 84 | 168 | 92.492 | 43.326 | 16.951.840 | 285.147 | 209.236 |
| skeptic | 2 | afbf9a5cd60e3e971 | Skeptic on spec amendment #1926 | large | high | 2 | 51 | 102 | 48.408 | 28.792 | 9.158.186 | 524.799 | 200.212 |
| spec-author | 3 | a2247cc9e7484249f | Spec author second amendment #1926 | large | medium | 1 | 38 | 76 | 31.541 | 10.963 | 5.264.717 | 175.759 | 98.479 |
| test-implementer | 1 | a748dc60452e1ee17 | Test implementer for #1926 | large | medium | 2 | 184 | 372 | 179.628 | 40.214 | 61.242.119 | 1.758.085 | 442.576 |
| implementer | 1 | a045debffc77a5a61 | Implementer for #1926 | large | medium | 1 | 39 | 78 | 20.351 | 4.825 | 5.545.783 | 318.387 | 112.840 |
| spec-author | 4 | ac044a1d3cc44d9d2 | Spec author: two stale examples #1926 | large | medium | 1 | 11 | 22 | 5.369 | 614 | 1.033.063 | 57.231 | 31.723 |
| test-implementer | 2 | a88dd96fdc3c0cb08 | Test implementer: move pinned test #1926 | large | medium | 1 | 7 | 14 | 2.923 | 43 | 563.198 | 38.425 | 13.043 |
| cleaner | 1 | aed67e8c50c30d825 | Cleaner for #1926 | medium | low | 1 | 6 | 12 | 1.420 | 32 | 431.715 | 89.492 | 12.512 |
| spec-checker | 1 | aa66ac60ef3ccf7f3 | Spec checker for #1926 | medium | medium | 1 | 10 | 20 | 4.759 | 860 | 916.940 | 121.627 | 45.093 |
| spec-author | 5 | ac3f46ebb973f5d6c | Spec author: resolve merge conflicts | large | medium | 1 | 28 | 56 | 12.825 | 2.714 | 3.136.268 | 80.875 | 56.006 |
| auditor | 1 | a93e0a95fde12eb1f | Auditor for #1926 | large | high | 1 | 54 | 110 | 49.513 | 30.913 | 11.023.347 | 294.658 | 218.052 |
| cleaner | 2 | abee4990d15e4dd24 | Cleaner: comment fix #1926 | medium | low | 1 | 6 | 12 | 1.801 | 0 | 409.573 | 84.619 | 7.592 |
| validator | 1 | a2c85cd1de8568648 | Validator for #1926 | small | low | 1 | 7 | 14 | 3.104 | 122 | 162.748 | 59.200 | 13.660 |
| issue-manager | 1 | aa4ae832eee9ae345 | Issue manager for #1926 | medium | medium | 1 | 7 | 16 | 3.987 | 386 | 560.231 | 103.876 | 27.350 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 687 | 1.412 | 608.612 | 241.955 | 148.897.666 | 4.400.578 |
| medium | 29 | 60 | 11.967 | 1.278 | 2.318.459 | 399.614 |
| small | 7 | 14 | 3.104 | 122 | 162.748 | 59.200 |
| **Total** | 723 | 1.486 | 623.683 | 243.355 | 151.378.873 | 4.859.392 |

## Decisions taken

Ten. The one the maintainer may want to see: decision 5 — an equals-value clause formatted on its own counts as holding the whole gap and is refused; no caller formats a bare clause, and it let two refusals go. Decision 7 chose the two reason lines on the maintainer's delegation.

## Suggestions

- Answers that widen a shape (here 1B, 3A, and the second batch's B) each cost a full spec-author amendment and a fresh skeptic. When an escalation 10 for neighbouring gaps is asked in the same batch as the run's own shape, asking in the same message about every sibling `=` (attribute argument, using alias) would have saved the second batch.
