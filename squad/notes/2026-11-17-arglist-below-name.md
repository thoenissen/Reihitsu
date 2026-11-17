# arglist-below-name

| | |
|---|---|
| Date | 2026-11-17 |
| Run | issue #1786 |
| Pull request | #1813 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | Argument list written below its name: join, join and collapse collections, or leave | A (join; multi-line collection keeps one element per line) | yes |
| 2 | output question | Constructor initializer and primary-constructor base type below keyword/type: join or leave | A (join) | yes |
| 3 | output question | Conditional with such a branch: judged after the join (one line) or as written | A (after the join) | yes |
| 4 | output question | Dictionary element initializer with such an element: one element per line or one line | A (one element per line) | yes |
| 5 | output question | Call whose only break was such a gap inside an interpolation hole / later query clause / array or stackalloc initializer: one line or one argument per line | A (one line) | yes |

## Cost

- Two stops: the output-question batch before the test implementer (questions 1–4), and question 5 raised by the first audit as undecided output.
- Repair rounds: one for the spec checker's code-owner finding (text only), one for the audit's undecided-output finding (spec, change doc, tests; no production change). The second audit passed with a text-only finding, proven with `verify-text-only.sh`.
- The spec author was spawned six times: initial spec, skeptic amendment, status-only acceptance, code-owner repair, audit repair, text cleanup — each a fresh agent because the earlier one was large or the follow-up was another stage.
- The first auditor (spawn 1) was lost to a container restart and re-run from scratch; that spawn is pure waste in the usage table.
- `origin/main` moved during the audit repair; the merge conflicted in two analyzer test files, resolved by an extra test-implementer spawn.
- The expensive spawns were the first test implementer (broad failure-class coverage over six argument-list owners and five code fixes) and the auditors (corpus runs of 190 fixtures against a `main` worktree).

## Usage

Session `92285af5-ff9d-59c4-b2f4-02549ff3d5f3`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 16 | 69 | 170 | 29.113 | 3.296 | 11.354.648 | 276.918 | 124.120 |
| spec-author | 1 | a8bef0cada9c2fb14 | Spec author for #1786 | large | medium | 1 | 47 | 91 | 44.530 | 22.876 | 6.383.859 | 124.920 | 101.462 |
| skeptic | 1 | aab95b57fff592c82 | Skeptic for #1786 | large | high | 1 | 31 | 64 | 42.168 | 25.526 | 3.939.109 | 114.136 | 90.858 |
| spec-author | 2 | aee1941e18047d118 | Spec author amendment #1786 | large | medium | 1 | 22 | 44 | 25.952 | 9.333 | 2.369.544 | 138.213 | 63.529 |
| spec-author | 3 | ab43f58be11cee9fd | Spec author accept change doc | large | medium | 1 | 5 | 10 | 2.467 | 55 | 399.555 | 41.975 | 17.914 |
| test-implementer | 1 | aaabc0efc78af087d | Test implementer #1786 | large | medium | 1 | 79 | 158 | 67.532 | 20.671 | 13.860.172 | 199.132 | 175.832 |
| implementer | 1 | aae3c07ea618deed2 | Implementer #1786 | large | medium | 1 | 21 | 42 | 18.270 | 6.145 | 2.520.883 | 93.958 | 70.688 |
| cleaner | 1 | a7e6d0970d98866ea | Cleaner #1786 | medium | low | 1 | 4 | 8 | 1.301 | 65 | 248.675 | 86.507 | 8.729 |
| spec-checker | 1 | a02899a821d9436ad | Spec checker #1786 | medium | medium | 1 | 6 | 12 | 3.609 | 563 | 481.274 | 112.167 | 34.840 |
| spec-author | 4 | a7eabc66c9dc041b2 | Spec author repair round 1 | large | medium | 1 | 5 | 10 | 1.531 | 0 | 336.956 | 87.929 | 10.170 |
| spec-checker | 2 | abe436cd98dd83d30 | Spec checker re-run #1786 | medium | medium | 1 | 4 | 8 | 1.270 | 0 | 306.167 | 36.744 | 13.425 |
| auditor | 1 | a293534b9ae02c5b1 | Auditor #1786 | large | high | 1 | 28 | 56 | 12.187 | 7.361 | 3.876.815 | 138.951 | 115.718 |
| auditor | 2 | a6af5cb389ff49247 | Auditor #1786 (restart) | large | high | 1 | 38 | 76 | 41.223 | 25.228 | 6.059.499 | 172.708 | 149.378 |
| spec-author | 5 | afd0f462319303e87 | Spec author audit repair | large | medium | 1 | 25 | 48 | 19.635 | 6.856 | 2.684.449 | 128.715 | 50.748 |
| test-implementer | 2 | a24945525b2455885 | Test implementer audit repair | large | medium | 1 | 27 | 54 | 18.304 | 3.985 | 2.983.330 | 133.149 | 58.521 |
| test-implementer | 3 | a25bc93ccb019f108 | Resolve merge conflicts in tests | large | medium | 1 | 9 | 18 | 3.772 | 148 | 741.017 | 37.068 | 12.690 |
| cleaner | 2 | a4d35b1d6ad3b11b9 | Cleaner after audit repair | medium | low | 1 | 6 | 12 | 1.903 | 113 | 411.415 | 85.019 | 7.165 |
| spec-checker | 3 | a164dd7fa87c45bfb | Spec checker after audit repair | medium | medium | 1 | 9 | 17 | 4.210 | 452 | 776.247 | 113.666 | 36.254 |
| auditor | 3 | a6bf1881708b6df0a | Auditor on repair #1786 | large | high | 1 | 37 | 74 | 33.693 | 15.723 | 4.521.664 | 156.493 | 82.307 |
| spec-author | 6 | a44a69b68977b679b | Spec author text cleanup | large | medium | 1 | 8 | 16 | 3.412 | 357 | 682.794 | 41.698 | 18.290 |
| validator | 1 | a34b957455a2146d9 | Validator #1786 | small | low | 2 | 5 | 10 | 2.564 | 268 | 83.756 | 51.642 | 7.719 |
| issue-manager | 1 | a87b9f3e1e51a1308 | Issue manager follow-ups | medium | medium | 2 | 15 | 32 | 8.083 | 967 | 1.423.378 | 116.820 | 42.740 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 451 | 931 | 363.789 | 147.560 | 62.714.294 | 1.885.963 |
| medium | 44 | 89 | 20.376 | 2.160 | 3.647.156 | 550.923 |
| small | 5 | 10 | 2.564 | 268 | 83.756 | 51.642 |
| **Total** | 500 | 1.030 | 386.729 | 149.988 | 66.445.206 | 2.488.528 |

## Decisions taken

Eight. The one the maintainer might want to see: decision 3, making one requirement (FMT-BREAK-039) and one rewriter the single owner of the gap in front of an argument list's `(`, taking it away from the chain and list-element joins. That reordering is what moved chain argument lists ahead of the list decision and produced question 5.

## Suggestions

- When a decision moves a join from one rewriter to another that runs earlier, the skeptic or spec author should enumerate which containers decide their layout between the two positions; here that enumeration came only from the auditor, which cost a stop and a repair round.
