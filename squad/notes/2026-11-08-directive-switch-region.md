# directive-switch-region

| | |
|---|---|
| Date | 2026-11-08 |
| Run | issues #1385, #1398, #1239 |
| Pull request | #1657 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | Directive directly between a `break`-ending section and the next section: no blank line (A) or a blank line behind the directive (B)? | A | yes |
| 2 | output question | Remove whitespace between `#` and `region`/`endregion` on rewritten region lines (A) or keep it (B)? | A | yes |

## Cost

One output-question batch (before the test implementer), no escalations. Answer 1A changed FMT-BLANK-006, so the lane widened from bug fix to behavior change after the spec author and skeptic had already run; the spec author was continued once to write the change doc. The test implementer ran twice: the implementer disputed one pre-existing line-ending test that pinned the old blank-line placement and that the first test pass had not found. One repair round: a post-audit doc-comment fix on a `public` member of an internal class failed `verify-text-only.sh --strict-docs`, which forced a spec checker and a repair audit for a comment-only change. The most expensive spawns were the test implementer, the skeptic, and the auditor (each about 7–10 million cache-read tokens).

## Usage

Session `223bf957-c4f1-5f0a-96d0-c1e7db545864`. Responses counted once per message id. Prices are not applied; compute a cost from the tokens and the model.

| Agent | Spawn | Agent ID | Description | Model | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | claude-opus-5-5 | medium | 17 | 63 | 145 | 22.075 | 3.414 | 9.740.485 | 125.207 |
| spec-author | 1 | af2fea907d2b7430c | Spec author for run | claude-opus-5-5 | medium | 2 | 37 | 78 | 34.929 | 13.130 | 4.088.701 | 283.951 |
| skeptic | 1 | ad8766c659faa7cb3 | Skeptic premise check | claude-opus-5-5 | high | 1 | 53 | 108 | 52.238 | 26.103 | 8.116.273 | 214.010 |
| test-implementer | 1 | afa1be118f527bd70 | Test implementer for run | claude-opus-5-5 | medium | 1 | 63 | 126 | 55.110 | 11.967 | 9.623.771 | 161.673 |
| implementer | 1 | ae56d086726421599 | Implementer for run | claude-opus-5-5 | medium | 1 | 26 | 52 | 16.894 | 2.725 | 2.851.008 | 85.085 |
| test-implementer | 2 | a4528490526a8b5b8 | Fix disputed test | claude-opus-5-5 | medium | 1 | 7 | 14 | 4.234 | 750 | 571.637 | 46.948 |
| cleaner | 1 | a9d8607cafd27f10b | Cleaner for run | claude-sonnet-5-5 | low | 1 | 5 | 10 | 1.706 | 83 | 312.243 | 81.415 |
| spec-checker | 1 | a9e057c01c7499330 | Spec checker for run | claude-sonnet-5-5 | medium | 1 | 9 | 18 | 5.128 | 1.315 | 733.643 | 113.532 |
| auditor | 1 | a3e1d44341f9ed485 | Auditor for run | claude-opus-5-5 | high | 1 | 44 | 90 | 39.740 | 18.544 | 7.018.877 | 223.443 |
| cleaner | 2 | a5fa9b95f1ed13b8c | Cleaner fixes doc comments | claude-sonnet-5-5 | low | 2 | 10 | 24 | 4.318 | 100 | 769.168 | 89.622 |
| spec-checker | 2 | a51f77745227e17f3 | Spec checker repair round | claude-sonnet-5-5 | medium | 1 | 5 | 10 | 2.182 | 50 | 310.623 | 83.811 |
| auditor | 2 | aa37625d80c07d012 | Auditor repair round | claude-opus-5-5 | high | 1 | 9 | 18 | 5.392 | 926 | 729.145 | 46.501 |
| validator | 1 | a093b45b8bce5fc4d | Validator final tree | claude-haiku-5-5 | low | 1 | 8 | 16 | 3.112 | 174 | 544.842 | 80.976 |
| issue-manager | 1 | a12d017f724861cbe | Issue manager follow-ups | claude-sonnet-5-5 | medium | 1 | 10 | 22 | 6.248 | 978 | 938.246 | 123.738 |

| Model | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| claude-haiku-5-5 | 8 | 16 | 3.112 | 174 | 544.842 | 80.976 |
| claude-opus-5-5 | 302 | 631 | 230.612 | 77.559 | 42.739.897 | 1.186.818 |
| claude-sonnet-5-5 | 39 | 84 | 19.582 | 2.526 | 3.063.923 | 492.118 |
| **Total** | 349 | 731 | 253.306 | 80.259 | 46.348.662 | 1.759.912 |

## Decisions taken

Seven. The branch used the session-mandated name instead of the playbook's pattern. The RH7301 code fix was kept out of the #1239 change. FMT-BLANK-016 got a wording clarification. Answer 1A was ruled to need a change doc and widen the lane, while answer 2A was treated as a clarification of FMT-REGION-007 with no change doc; the maintainer may want to see that split, because #1239 changes visible output without a change doc. There was no second skeptic run on the change doc. The disputed test was judged wrong against FMT-BLANK-016. The strict-docs failure was handled as a repair round.

## Suggestions

- `--strict-docs` treats `public` members of `internal` types as public API documentation, although no assembly here is a public API (`AGENTS.md`, *Supported inputs*). A comment-only fix on such a member cost a full repair round. Consider scoping `--strict-docs` to effectively public members.
- The test implementer's first pass should search existing tests for assertions on the output the spec diff changes, not only the tests the handovers name. One such test surfaced only at the implementer.
