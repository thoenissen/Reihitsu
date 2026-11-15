# comment-directive-layout-reports

| | |
|---|---|
| Date | 2026-11-15 |
| Run | issues #1147, #1152, #1386, #1387, #1594, #1655, #1736 |
| Pull request | #1753 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 0 | 8 | Split #1736 (behavior change) from the six bug fixes, or one run in the behavior-change lane? | One run, behavior-change lane | yes |
| 1 | output question | RH7304: offer the fix for a region whose first line of code starts with a comment (one-line and multi-line)? | A, offer it | yes |
| 2 | output question | RH5111: stop reporting a comment or `#pragma` in the wrapped `=` gap? | B, keep reporting (syntax is unwanted; formatter should not handle it) | no |
| 3 | output question | RH5111: declarator with a directive-only gap and a comment elsewhere | B, reported | no (follows from 2) |
| 4 | output question | RH5109: lists whose parameters all start on one line (break behind `(`, before `)`, `)` held by a comment) not reported; rule page text | A | yes |
| 5 | output question | RH5109: comment or directive keeps the first parameter off the `(` line | A, not reported | no |

## Cost

- One stop before the claim (lane split, escalation 8) and one output-question batch before the test implementer; none before the auditor.
- Three repair rounds, all used: rounds 1 and 2 were spec-text only (a SHARED-COMMENT-003 span sentence, then a `Uses` violation the first repair introduced — the second round was caused by the first); round 3 repaired three audit findings (RH5201 regression from the shared-helper migration, mode-dependent `///` run count, SHARED-INIT-001 second comment).
- Test implementer spawned four times (main, 11 attribute rules, two disputed tests, repair round 3); spec author four times; spec checker four times; auditor twice.
- Most expensive: the first test implementer (seven issues, ~100 tool uses), the skeptic, and the first auditor. Seven issues across ~40 rules made every handover long.

## Usage

## Usage

Session `004d1ae9-df87-53a5-87fc-40f26da8e264`. Responses counted once per message id. Prices are not applied; compute a cost from the tokens and the model.

| Agent | Spawn | Agent ID | Description | Model | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | claude-opus-5-5 | medium | 23 | 85 | 204 | 45.795 | 7.447 | 15.797.967 | 182.601 |
| spec-author | 1 | a0d52c246d5e758b6 | Spec author for 7-issue run | claude-opus-5-5 | medium | 1 | 57 | 110 | 64.288 | 33.965 | 10.289.115 | 196.846 |
| skeptic | 1 | a23f56e2cba53ea66 | Skeptic for 7-issue run | claude-opus-5-5 | high | 1 | 78 | 158 | 86.879 | 46.334 | 14.416.560 | 216.421 |
| spec-author | 2 | a37b22ca52bf3ce83 | Spec amendments after answers | claude-opus-5-5 | medium | 1 | 33 | 66 | 28.451 | 8.805 | 3.968.904 | 155.628 |
| test-implementer | 1 | aae8bc3c34c88b7ae | Test implementer for 7-issue run | claude-opus-5-5 | medium | 1 | 100 | 202 | 88.990 | 26.957 | 22.564.863 | 289.428 |
| test-implementer | 2 | a4fafa4fe2d5fb723 | Add attribute-rule directive tests | claude-opus-5-5 | medium | 2 | 32 | 66 | 18.853 | 3.218 | 3.668.951 | 169.742 |
| implementer | 1 | a5df1c423bb9cf57c | Implementer for 7-issue run | claude-opus-5-5 | medium | 1 | 77 | 154 | 55.356 | 20.965 | 14.698.917 | 223.971 |
| cleaner | 1 | abe9f540c106bddd5 | Cleaner for 7-issue run | claude-sonnet-5-5 | low | 1 | 7 | 14 | 1.869 | 89 | 506.845 | 87.621 |
| spec-checker | 1 | a002ea586a2df93eb | Spec checker for 7-issue run | claude-sonnet-5-5 | medium | 1 | 14 | 28 | 11.505 | 5.128 | 1.497.737 | 150.387 |
| spec-author | 3 | a4614723ed2d2d80e | Spec repair SHARED-COMMENT-003 | claude-opus-5-5 | medium | 2 | 14 | 29 | 6.623 | 754 | 1.266.724 | 45.175 |
| spec-checker | 2 | a58f9ec2a3e0b5003 | Spec check of repair round 1 | claude-sonnet-5-5 | medium | 1 | 6 | 12 | 2.664 | 104 | 488.581 | 42.345 |
| spec-checker | 3 | aa6b732f10dad12a5 | Spec check of repair round 2 | claude-sonnet-5-5 | medium | 1 | 4 | 8 | 1.328 | 0 | 296.245 | 34.512 |
| auditor | 1 | a136db86c043000c1 | Auditor for 7-issue run | claude-opus-5-5 | high | 1 | 63 | 126 | 81.907 | 54.524 | 14.447.321 | 324.755 |
| spec-author | 4 | a0bb8c1913d161880 | Spec repair round 3 | claude-opus-5-5 | medium | 1 | 16 | 31 | 11.222 | 4.163 | 1.585.019 | 122.528 |
| test-implementer | 3 | a4fb2d9f1990f56ba | Tests for repair round 3 | claude-opus-5-5 | medium | 1 | 34 | 68 | 31.365 | 8.740 | 4.290.285 | 167.948 |
| implementer | 2 | a9ce40f550d4fdab2 | Implementer repair round 3 | claude-opus-5-5 | medium | 1 | 21 | 42 | 12.681 | 2.763 | 2.274.530 | 77.689 |
| cleaner | 2 | a843fc22aaa1ff75f | Cleaner repair round 3 | claude-sonnet-5-5 | low | 1 | 7 | 14 | 2.117 | 177 | 534.815 | 92.384 |
| spec-checker | 4 | a1e4d49ac52dd9e29 | Spec check repair round 3 | claude-sonnet-5-5 | medium | 1 | 5 | 10 | 2.598 | 265 | 412.148 | 43.721 |
| auditor | 2 | a99f5592efb149b6d | Audit repair round 3 | claude-opus-5-5 | high | 1 | 33 | 66 | 27.996 | 15.211 | 4.313.346 | 171.083 |
| validator | 1 | a56c6b7171fcaccfd | Validator final tree | claude-haiku-5-5 | low | 2 | 10 | 20 | 3.143 | 279 | 702.708 | 187.077 |
| issue-manager | 1 | a391b30734ad36cb2 | File run follow-up issues | claude-sonnet-5-5 | medium | 1 | 10 | 22 | 7.854 | 1.157 | 973.782 | 129.800 |

| Model | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| claude-haiku-5-5 | 10 | 20 | 3.143 | 279 | 702.708 | 187.077 |
| claude-opus-5-5 | 643 | 1.322 | 560.406 | 233.846 | 113.582.502 | 2.343.815 |
| claude-sonnet-5-5 | 53 | 108 | 29.935 | 6.920 | 4.710.153 | 580.770 |
| **Total** | 706 | 1.450 | 593.484 | 241.045 | 118.995.363 | 3.111.662 |

## Decisions taken

20. Worth the maintainer's look: decision 4 (one shared helper for the line-start column of SHARED-COMMENT-011, then decision 17 reverting RH5201 to its private copy after the audit found the migration changed its comment-tail behavior); decision 18 (one diagnostic per `///` line of a non-documenting run, in every mode); decision 20 (comment-tail cases in RH5203/RH5110/RH5305 dropped as a rare class rather than filed).

## Suggestions

- Moving a rule onto a shared helper changed its behavior for a shape no test covered (RH5201, comment tail). When a structural decision migrates existing private copies, the test implementer could be asked to pin each migrated rule's current behavior on the helper's edge shapes before the implementer starts.
- A repair round that only edits spec prose could run the architecture tests before the spec checker is spawned; round 2 existed only because round 1 broke `UsesListsExactlyTheReferencedRequirements`.
