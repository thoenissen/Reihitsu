# arrow-comment-unsupported

| | |
|---|---|
| Date | 2026-11-17 |
| Run | issue #1732 |
| Pull request | #1839 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | 10 | Declare a comment or directive behind the `=>` of a property, indexer, or method unsupported (confirming the decision the agent-filed issue records) | A, declare | yes |
| 2 | output question | Accessors and local functions belong to the shape | A, include | yes |
| 3 | output question | Directives and disabled text belong to the shape (realistic `#if DEBUG` input) | A, include | yes |
| 4 | output question | A block comment behind the `=>` on one line is refused too | A, include | yes |
| 5 | output question | CLI reason text: "the arrow of an expression body" instead of the issue's backticked wording | A | yes |
| 6 | 10 | Also declare a comment or directive in front of the `=>` unsupported | A, in a change of its own (follow-up #1851) | yes |

## Cost

One stop: the batch before the test implementer (all six questions in one message). One repair round, for a test that asserted only that a fix is offered, not its result (auditor finding, PR-introduced). The spec author was spawned three times: draft, carrying the answers and two wording decisions, and a one-line example fix (a member named like its class) found by the test implementer. The test implementer was the most expensive spawn: it rewrote or moved a large number of existing tests whose inputs held the newly refused shape, and found them by running the suites against a temporary precheck in a scratch worktree. A clean merge of three `main` commits happened before the first audit.

## Usage

Session `8b02d2ba-5679-51fb-a977-65a9ce97362c`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 18 | 63 | 160 | 26.009 | 2.939 | 10.292.974 | 142.371 | 115.520 |
| spec-author | 1 | a1dd790a20395571d | Spec author for #1732 | large | medium | 1 | 67 | 129 | 67.631 | 29.024 | 12.977.551 | 271.311 | 193.250 |
| skeptic | 1 | a7595c287f9be18b6 | Skeptic for #1732 | large | high | 1 | 58 | 118 | 62.084 | 37.342 | 9.543.463 | 236.788 | 162.559 |
| spec-author | 2 | ad772c104d7b88649 | Spec author follow-up #1732 | large | medium | 2 | 16 | 36 | 9.402 | 1.610 | 1.709.327 | 129.008 | 54.000 |
| test-implementer | 1 | aa4c30c7237ab026b | Test implementer for #1732 | large | medium | 1 | 133 | 266 | 117.511 | 31.815 | 36.839.579 | 351.933 | 327.816 |
| implementer | 1 | a22505d1e3c6d21f6 | Implementer for #1732 | large | medium | 2 | 28 | 56 | 20.872 | 3.288 | 3.251.652 | 94.923 | 70.728 |
| spec-author | 3 | a1ebcb721ed3672ee | Spec example fix #1732 | large | medium | 1 | 7 | 14 | 2.783 | 197 | 559.349 | 40.766 | 16.569 |
| cleaner | 1 | aee9057c91fd69a38 | Cleaner for #1732 | medium | low | 1 | 6 | 12 | 1.383 | 17 | 401.528 | 83.068 | 8.390 |
| spec-checker | 1 | abc4a411ee646f4e2 | Spec checker for #1732 | medium | medium | 1 | 12 | 24 | 6.060 | 1.558 | 1.228.411 | 136.521 | 62.312 |
| auditor | 1 | a16d0df12815aca55 | Auditor for #1732 | large | high | 1 | 69 | 138 | 59.995 | 38.013 | 14.646.815 | 290.659 | 216.385 |
| test-implementer | 2 | ae399ce0efb876044 | Test repair round 1 #1732 | large | medium | 1 | 18 | 36 | 10.976 | 1.407 | 1.882.256 | 126.191 | 51.321 |
| cleaner | 2 | ad09c25d295d51b32 | Cleaner repair round 1 #1732 | medium | low | 1 | 7 | 14 | 1.808 | 0 | 524.447 | 91.127 | 16.428 |
| spec-checker | 2 | a314458c7da6fc646 | Spec checker repair round 1 | medium | medium | 1 | 6 | 12 | 1.823 | 35 | 471.686 | 44.683 | 21.266 |
| auditor | 2 | abb49282d2bfb689f | Auditor repair round 1 #1732 | large | high | 1 | 17 | 34 | 8.547 | 2.085 | 1.726.236 | 124.719 | 50.455 |
| validator | 1 | acafe720adcbf1d12 | Validator for #1732 | small | low | 2 | 7 | 14 | 2.118 | 129 | 166.484 | 74.206 | 15.791 |
| issue-manager | 1 | ad41da0aaf2f6562b | Issue manager for #1732 | medium | medium | 1 | 6 | 12 | 7.708 | 1.220 | 573.746 | 73.973 | 49.560 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 476 | 987 | 385.810 | 147.720 | 93.429.202 | 1.808.669 |
| medium | 37 | 74 | 18.782 | 2.830 | 3.199.818 | 429.372 |
| small | 7 | 14 | 2.118 | 129 | 166.484 | 74.206 |
| **Total** | 520 | 1.075 | 406.710 | 150.679 | 96.795.504 | 2.312.247 |

## Decisions taken

Ten. The tenth: `main` gained #1849 after the audit; it overlaps a test file this run changes (`CommentJoinTests.cs`) but only in comments, so it was merged without a second audit or local validator run, relying on CI. Worth a look: dropping the issue's goal of deleting RH5401-004's format-and-compare (it also decides the wrapped expression and the comment in front of the arrow), and keeping `BlocksRewrite`'s directive check so RH3202–RH3208 diagnostics do not change.

## Suggestions

- The issue recorded the maintainer's decision to declare the shape, but carried `agent-filed`; the lead re-asked it as a confirmation. A way to mark a maintainer decision inside an agent-filed issue as already answered would save that question.
