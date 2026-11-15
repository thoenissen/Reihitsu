# analyzer-rule-bug-batch

| | |
|---|---|
| Date | 2026-11-14 |
| Run | issues #1074, #1079, #1276, #1613, #1708 |
| Pull request | #1719 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|

None asked. The new RH5603 expected values rest on the maintainer's decision already posted on #1079 (2026-10-18).

## Cost

No output-question batches and no escalations. One repair round: the auditor's comment-only finding failed `verify-text-only --strict-docs` because a public XML-doc comment changed, which forced a spec-checker and auditor pass over a one-line comment. Roles spawned more than once: spec author (spec example did not compile; RH5404 `Uses` missed SHARED-FIX-001 after its own amendment), test implementer (align the RH5602 example test; drop a `extension(ref int)` test that does not compile). The skeptic, test implementer and auditor were the expensive spawns; five issues over eight rules made each of them large.

## Usage

Session `dc56709a-1929-54d7-b303-039cc25561b2`. Responses counted once per message id. Prices are not applied; compute a cost from the tokens and the model.

| Agent | Spawn | Agent ID | Description | Model | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | claude-opus-5-5 | medium | 17 | 61 | 151 | 25.551 | 3.361 | 9.586.068 | 132.713 |
| spec-author | 1 | a50872ffc842e4664 | Spec author for batch run | claude-opus-5-5 | medium | 2 | 38 | 76 | 35.799 | 15.107 | 4.842.868 | 286.392 |
| skeptic | 1 | ab24e8bfca5f6a1c5 | Skeptic for batch run | claude-opus-5-5 | high | 1 | 64 | 130 | 68.269 | 35.681 | 11.282.382 | 204.869 |
| test-implementer | 1 | af36391e09b457c37 | Test implementer for batch run | claude-opus-5-5 | medium | 1 | 79 | 158 | 95.547 | 21.025 | 15.017.017 | 297.561 |
| spec-author | 2 | a96c44f10d76bac94 | Spec author repair: two spec fixes | claude-opus-5-5 | medium | 1 | 8 | 16 | 3.744 | 409 | 667.887 | 42.961 |
| test-implementer | 2 | af3a2c4889a3d1bf2 | Align RH5602 example test | claude-opus-5-5 | medium | 1 | 12 | 24 | 5.126 | 465 | 1.056.828 | 48.431 |
| implementer | 1 | a7534ff4e4a8c8b12 | Implementer for batch run | claude-opus-5-5 | medium | 1 | 44 | 88 | 29.994 | 6.884 | 6.036.701 | 129.883 |
| test-implementer | 3 | a20fb04f60c8f3381 | Drop non-compiling ref receiver test | claude-opus-5-5 | medium | 1 | 7 | 14 | 3.263 | 53 | 558.256 | 39.254 |
| cleaner | 1 | a2d12a9960ff41f5a | Cleaner for batch run | claude-sonnet-5-5 | low | 1 | 5 | 10 | 1.565 | 40 | 324.309 | 84.220 |
| spec-checker | 1 | a44496122a0ee2dcf | Spec checker for batch run | claude-sonnet-5-5 | medium | 1 | 10 | 20 | 6.420 | 1.356 | 855.016 | 116.463 |
| auditor | 1 | a309ba9864bb7e072 | Auditor for batch run | claude-opus-5-5 | high | 1 | 53 | 108 | 49.437 | 31.362 | 10.198.377 | 213.245 |
| cleaner | 2 | aa561796931750b24 | Cleaner: fix doc comment | claude-sonnet-5-5 | low | 1 | 5 | 10 | 2.033 | 70 | 312.316 | 80.306 |
| auditor | 2 | a7f280f4b70426bf4 | Auditor on repair round 1 | claude-opus-5-5 | high | 2 | 9 | 18 | 3.600 | 345 | 757.368 | 44.749 |
| spec-checker | 2 | a7bbec460cb77c01f | Spec checker on repair round 1 | claude-sonnet-5-5 | medium | 1 | 4 | 8 | 1.557 | 0 | 244.259 | 85.982 |
| validator | 1 | a06a7111a0fc56919 | Validator for batch run | claude-haiku-5-5 | low | 2 | 10 | 20 | 3.729 | 478 | 672.916 | 174.583 |
| issue-manager | 1 | ae708cd0d7e0a595c | Issue manager for follow-ups | claude-sonnet-5-5 | medium | 1 | 7 | 16 | 6.068 | 713 | 625.279 | 63.982 |

| Model | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| claude-haiku-5-5 | 10 | 20 | 3.729 | 478 | 672.916 | 174.583 |
| claude-opus-5-5 | 375 | 783 | 320.330 | 114.692 | 60.003.752 | 1.440.058 |
| claude-sonnet-5-5 | 31 | 64 | 17.643 | 2.179 | 2.361.179 | 430.953 |
| **Total** | 416 | 867 | 341.702 | 117.349 | 63.037.847 | 2.045.594 |

## Decisions taken

15. Worth the maintainer's eye: decision 3 (RH5404 withholds its fix for extension blocks, matching the published rule page, instead of formatting the block) and decision 5 (RH5202's analyzer and fix share one closing-line measurement in Core).

## Suggestions

- A comment-only auditor finding on a public member's XML doc always fails `--strict-docs`, so lead.md section 7 turns every such finding into a full repair round. Evidence: repair round 1 of this run changed one `<param>` line; tokens were identical under the non-strict proof.
- The test implementer should compile each spec example before quoting it verbatim in a test; this run had one non-compiling spec example and one non-compiling test input, each costing an extra spawn.
