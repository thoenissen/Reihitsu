# using-ordering-by-value

| | |
|---|---|
| Date | 2026-11-15 |
| Run | issues #1105, #1266, #1267 |
| Pull request | #1735 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | Is a Unicode escape in a using name decoded for ordering and grouping (RH7201, RH7207, formatter)? | Yes, generalized: every name is ordered by its compiler-interpreted value, `@` dropped and escapes decoded | Yes |
| 2 | output question | Is a leading `global::` ignored in the sort key too (RH7203, RH7206, formatter)? | Yes (A) | Yes |

## Cost

Two stops, both output questions: Q1 in the batch before the test implementer, Q2 raised by the first audit. Two repair rounds: round 1 fixed one spec-example marker found by the test implementer and the spec checker; round 2 wrote the Q2 answer into the specs and added tests (no production change). The test implementer ran three times (reproduction; a pre-existing RH7203 test that asserted the old adjacent-only comparison; the `global::` tests). The expensive spawns were the first test implementer (68 responses, 12.7M cache read), the skeptic, and the first audit. `origin/main` moved once before audit 2 without overlap and was merged.

## Usage

Session `b632370d-c346-51cd-a289-5876c06363af`. Responses counted once per message id. Prices are not applied; compute a cost from the tokens and the model.

| Agent | Spawn | Agent ID | Description | Model | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | claude-opus-5-5 | medium | 19 | 73 | 180 | 31.670 | 3.656 | 11.522.796 | 255.458 |
| spec-author | 1 | a1ec2c937cea4c94c | Spec clarification for using ordering | claude-opus-5-5 | medium | 2 | 28 | 57 | 32.489 | 11.326 | 3.441.952 | 196.090 |
| skeptic | 1 | afe24f310c62e786b | Skeptic premise check | claude-opus-5-5 | high | 1 | 41 | 84 | 50.455 | 27.616 | 5.947.319 | 201.606 |
| test-implementer | 1 | ab322f38ebc9f4647 | Tests for using ordering bugs | claude-opus-5-5 | medium | 1 | 68 | 138 | 66.150 | 14.866 | 12.695.092 | 212.218 |
| implementer | 1 | a22599ab153601410 | Implement using ordering fixes | claude-opus-5-5 | medium | 1 | 28 | 56 | 19.987 | 5.966 | 3.576.654 | 106.176 |
| test-implementer | 2 | ab2344f8741d06918 | Fix disputed RH7203 test | claude-opus-5-5 | medium | 1 | 18 | 36 | 8.134 | 1.292 | 1.573.023 | 49.259 |
| cleaner | 1 | a5ac0d4900e227b14 | Format and warning-free build | claude-sonnet-5-5 | low | 1 | 5 | 10 | 1.500 | 33 | 333.656 | 87.227 |
| spec-checker | 1 | a04fa722f1d48578a | Spec check of the run | claude-sonnet-5-5 | medium | 1 | 8 | 16 | 4.415 | 1.298 | 643.220 | 114.467 |
| spec-author | 2 | a5cdfa44ef0d7f693 | Repair RH7201 spec example | claude-opus-5-5 | medium | 1 | 5 | 10 | 1.808 | 53 | 383.113 | 35.388 |
| spec-checker | 2 | ace374514f9908fb6 | Re-check repaired spec | claude-sonnet-5-5 | medium | 1 | 4 | 8 | 1.436 | 0 | 287.697 | 30.531 |
| auditor | 1 | a4ef323866cfc6a6f | Audit the finished tree | claude-opus-5-5 | high | 1 | 32 | 66 | 34.151 | 19.731 | 5.093.564 | 210.312 |
| spec-author | 3 | a7f8f76e235b0e058 | Spec: global:: in sort key | claude-opus-5-5 | medium | 1 | 12 | 23 | 7.488 | 1.751 | 1.151.914 | 121.697 |
| test-implementer | 3 | a43b628281eaee917 | Tests for global:: sort key | claude-opus-5-5 | medium | 1 | 15 | 30 | 11.917 | 1.537 | 1.370.371 | 112.020 |
| cleaner | 2 | ae5b35b1911c54e1b | Clean repair round 2 | claude-sonnet-5-5 | low | 1 | 5 | 10 | 1.697 | 51 | 338.367 | 87.433 |
| spec-checker | 3 | a7f670ad3cee699d3 | Spec check repair round 2 | claude-sonnet-5-5 | medium | 1 | 5 | 10 | 2.168 | 88 | 362.480 | 99.901 |
| auditor | 2 | a627bbadb1e2ffe2d | Audit repair round 2 | claude-opus-5-5 | high | 1 | 16 | 32 | 9.992 | 2.353 | 1.556.160 | 119.935 |
| validator | 1 | a4b27733304f04f86 | Validate final tree | claude-haiku-5-5 | low | 2 | 7 | 14 | 2.587 | 110 | 409.675 | 168.423 |
| issue-manager | 1 | a3533fc17dc0a2f73 | File follow-up issues | claude-sonnet-5-5 | medium | 1 | 7 | 16 | 3.367 | 249 | 546.277 | 103.447 |

| Model | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| claude-haiku-5-5 | 7 | 14 | 2.587 | 110 | 409.675 | 168.423 |
| claude-opus-5-5 | 336 | 712 | 274.241 | 90.147 | 48.311.958 | 1.620.159 |
| claude-sonnet-5-5 | 34 | 70 | 14.583 | 1.719 | 2.511.697 | 523.006 |
| **Total** | 377 | 796 | 291.411 | 91.976 | 51.233.330 | 2.311.588 |

## Decisions taken

Eight. Worth the maintainer's eye: decision 2 (the by-value comparison also changes the formatter's using order, through the shared Core policy), decision 3 (the within-group check of RH7203, RH7204 and RH7206 moved into Core instead of three analyzer loops), and decision 7 (a pre-existing RH7203 test now expects three diagnostics instead of one under SHARED-USING-007). The run used the session-mandated branch name instead of the playbook's `claude/issues-…` name.

## Suggestions

- The implementer called dropping `global::` from the sort key an internal note; only the auditor recognized it as undecided visible output. Evidence: audit 1 finding 1. The implementer's role could require listing every changed key or comparison whose effect reaches a spelling the issue does not name, as a candidate output question.
- Tools decoded literal `\u0053` escapes in Markdown and C# fixtures more than once (skeptic handover lost it; the test implementer restored one). A grep check for intended escapes in prompts helped; a role-file hint for escape-carrying fixtures would make it routine.
