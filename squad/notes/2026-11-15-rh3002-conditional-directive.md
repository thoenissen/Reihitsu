# rh3002-conditional-directive

| | |
|---|---|
| Date | 2026-11-15 |
| Run | issue #1756 |
| Pull request | #1769 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question (answered in the issue before the run) | Should RH3002 stop reporting parentheses that hold a conditional directive (SHARED-DIAG-008) or keep reporting them (RH3002-009)? | B — keep reporting, no fix; analyzers judge the current build | No (issue recommended A) |
| 2 | scope (answered in the issue before the run) | Align only the spec text in this issue, or reverse SHARED-DIAG-008 generally? | Spec text only; general reversal is #1759 | — |

No question was asked during the run.

## Cost

No stops. One repair round: the spec checker found that the new cross-references between RH3002-010 and SHARED-DIAG-008 were missing from both specs' `Uses` header rows, which the architecture tests enforce; the spec author's first pass did not run the architecture tests. The lane widened from docs to tests because the new requirement needs a covering test. The skeptic (high effort) was the most expensive spawn; it ran nine fixtures through `apply-fix.sh` under LF and CRLF.

## Usage

Session `a6ad34d0-b40c-53bd-ae7d-0bc8f483ebf5`. Responses counted once per message id. Prices are not applied; compute a cost from the tokens and the model.

| Agent | Spawn | Agent ID | Description | Model | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | claude-opus-5-5 | medium | 6 | 42 | 101 | 13.824 | 1.323 | 5.677.488 | 100.501 |
| spec-author | 1 | a47c25628bb00c940 | Spec author for #1756 | claude-opus-5-5 | medium | 2 | 14 | 31 | 9.803 | 2.608 | 1.258.478 | 116.094 |
| skeptic | 1 | a7103cde7dfdfd0bc | Skeptic for #1756 spec diff | claude-opus-5-5 | high | 1 | 30 | 59 | 21.612 | 9.343 | 3.628.731 | 154.230 |
| test-implementer | 1 | a3a850b05d7a57c66 | Test implementer for #1756 | claude-opus-5-5 | medium | 1 | 23 | 44 | 16.171 | 3.473 | 2.390.997 | 69.428 |
| cleaner | 1 | a29a6538b7739e933 | Cleaner for #1756 | claude-sonnet-5-5 | low | 1 | 5 | 10 | 1.702 | 71 | 325.418 | 84.017 |
| spec-checker | 1 | a02956bd5fdc2d085 | Spec checker for #1756 | claude-sonnet-5-5 | medium | 1 | 7 | 13 | 3.473 | 120 | 549.603 | 98.642 |
| spec-author | 2 | aa556b76bb7ebd00e | Spec author repair round 1 | claude-opus-5-5 | medium | 1 | 5 | 10 | 1.863 | 119 | 388.656 | 33.892 |
| spec-checker | 2 | a8fc219a1934c51fd | Spec checker re-check #1756 | claude-sonnet-5-5 | medium | 1 | 5 | 10 | 2.592 | 35 | 399.434 | 42.251 |
| validator | 1 | a93b992a43bac447a | Validator for #1756 | claude-haiku-5-5 | low | 2 | 7 | 14 | 2.429 | 131 | 385.343 | 158.770 |
| issue-manager | 1 | a1b5c86dfbd58157e | Issue manager for #1756 | claude-sonnet-5-5 | medium | 1 | 6 | 14 | 2.952 | 53 | 411.980 | 87.653 |

| Model | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| claude-haiku-5-5 | 7 | 14 | 2.429 | 131 | 385.343 | 158.770 |
| claude-opus-5-5 | 114 | 245 | 63.273 | 16.866 | 13.344.350 | 474.145 |
| claude-sonnet-5-5 | 23 | 47 | 10.719 | 279 | 1.686.435 | 312.563 |
| **Total** | 144 | 306 | 76.421 | 17.276 | 15.416.128 | 945.478 |

## Decisions taken

Six. Worth a look: the SHARED-DIAG-008 wording does not call RH3002 its only exception, because RH2005-005 already reports across an excluded modifier (added to #1759 instead of a new issue).

## Suggestions

The spec author's role file could require `scripts/test.sh --project architecture` after any edit that adds a cross-reference between specs, so `Uses`-row mismatches surface before the spec checker.
