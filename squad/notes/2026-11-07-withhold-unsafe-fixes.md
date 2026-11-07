# withhold-unsafe-fixes

| | |
|---|---|
| Date | 2026-11-07 |
| Run | issues #1257, #1279, #1281, #1282, #1417, #1422 |
| Pull request | #1599 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | RH3004: lay out a one-line block body as the formatter does, or keep it on one line | A (formatter layout) | yes |
| 2 | output question | RH3106: `var a = delegate () { };` (and `Delegate`/`object` targets) — not reported, or reported without a fix | A (not reported) | yes |
| 3 | output question | RH3101 rule page: make `Derived` `internal` so the example compiles | A | yes |
| 4 | output question | RH3106: argument whose only parameter type is `object` — not reported, or reported without a fix | A (not reported) | yes |
| 5 | output question | RH3201: `return /**/a;` — fix to `return a;`, or withhold | A (`return a;`) | yes |
| 6 | output question | RH3004: several statements in a one-line block — keep them on one line, or one per line | A, on the condition that A needs no special handling (it did not) | yes |
| 7 | output question | RH3004: column-0 `#if` in the moved block — keep column 0, or shift with the block | B (keep column 0); maintainer then asked for no further questions | yes |

After answer 7 the maintainer gave a standing instruction: no further questions; the lead decides on recommendation and sums up in the PR. Escalation 7 (the run still blocked after the narrowing) was decided under that instruction instead of asked.

## Cost

- Stops: four output-question messages (Q1–3, Q4, Q5, Q6–7), then none under the standing instruction.
- Repair rounds: 3, plus architecture changes after audits 2, 3 and 4 (repeated classes), two narrowings after audits 5 and 6, a final repair after audit 7, and a rename after audit 8 — nine audits in total.
- Where it got expensive: the RH3004 moved-code layout (answer 1 made the fix lay out the block like the formatter and move the statements as moved code). Every audit found another line kind or another build configuration in which moved text changed a string's value or broke code — raw strings, interpolated verbatim strings, directives whose chain crosses the anonymous method, `#` lines hidden in strings or comments. It ended only when the fix was withheld for every anonymous method that holds a directive, disabled text, or a string/comment spanning lines. The RH3106 natural-type decision went through position lists, a binding check, and two scope corrections before its last corner was recorded as a gap.
- A container restart lost one implementer mid-work; its uncommitted changes were stashed and reused by the restarted agent.

## Usage

Session `747194af-4ab7-5680-b2fe-a122f62bfe92`. Responses counted once per message id. Prices are not applied; compute a cost from the tokens and the model.

| Agent | Spawn | Agent ID | Description | Model | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | claude-opus-5-5 | medium | 62 | 182 | 485 | 120.214 | 22.813 | 49.897.153 | 367.944 |
| spec-author | 1 | a37062c047adf758d | Spec author: clarifications | claude-opus-5-5 | medium | 4 | 84 | 167 | 93.807 | 48.955 | 14.672.711 | 1.006.664 |
| skeptic | 1 | a793afe2fe7a4e4eb | Skeptic: premise check | claude-opus-5-5 | high | 1 | 73 | 148 | 82.703 | 38.920 | 12.135.749 | 242.997 |
| test-implementer | 1 | a55c84e79f9b0b122 | Test implementer: reproductions | claude-opus-5-5 | medium | 1 | 86 | 172 | 110.050 | 25.025 | 18.468.362 | 316.390 |
| test-implementer | 2 | a08c13d79f7e8cadd | Test implementer: RH3201-007 test | claude-opus-5-5 | medium | 1 | 12 | 23 | 9.418 | 1.510 | 1.183.248 | 67.503 |
| implementer | 1 | aa9300b7f3f4b964a | Implementer: make tests pass | claude-opus-5-5 | medium | 1 | 106 | 212 | 116.637 | 50.897 | 25.690.297 | 349.780 |
| spec-author | 2 | aec7bed63f01aed7d | Spec author: Uses rows repair | claude-opus-5-5 | medium | 1 | 13 | 26 | 5.609 | 860 | 1.067.060 | 44.959 |
| test-implementer | 3 | aa5cf60587914bbf7 | Test implementer: config + Core tests | claude-opus-5-5 | medium | 2 | 24 | 48 | 26.286 | 6.589 | 2.517.293 | 81.037 |
| cleaner | 1 | a3d7e50ea26b5e5b6 | Cleaner: format and warnings | claude-sonnet-5-5 | low | 1 | 7 | 14 | 1.916 | 53 | 458.752 | 79.402 |
| spec-checker | 1 | aa74a6450bd368100 | Spec checker | claude-sonnet-5-5 | medium | 1 | 8 | 16 | 4.326 | 705 | 597.966 | 95.937 |
| auditor | 1 | aefa8a716450f5e3b | Auditor: final quality check | claude-opus-5-5 | high | 1 | 58 | 116 | 65.806 | 36.016 | 9.907.281 | 253.161 |
| spec-author | 3 | a1a9d4f4f8cae0304 | Spec author: repair round 1 | claude-opus-5-5 | medium | 1 | 26 | 50 | 30.468 | 14.660 | 3.074.966 | 155.757 |
| test-implementer | 4 | a4cfa1980161b6fc0 | Test implementer: repair round 1 | claude-opus-5-5 | medium | 1 | 32 | 64 | 38.138 | 10.746 | 4.543.180 | 193.990 |
| implementer | 2 | aa6fd9b3ce83a22a4 | Implementer: repair round 1 | claude-opus-5-5 | medium | 2 | 88 | 176 | 91.363 | 41.496 | 16.241.220 | 226.353 |
| spec-author | 4 | ac2733270eaa881a1 | Spec author: RH3106 example fix | claude-opus-5-5 | medium | 1 | 10 | 20 | 4.664 | 1.297 | 810.765 | 40.613 |
| spec-author | 5 | a5728d092c2958e66 | Spec author: RH3004-006 wording | claude-opus-5-5 | medium | 1 | 18 | 34 | 15.416 | 8.703 | 1.667.241 | 114.875 |
| spec-author | 6 | a2c07c0289f049249 | Spec author: apply answer 7B | claude-opus-5-5 | medium | 1 | 10 | 19 | 5.179 | 1.364 | 782.410 | 94.270 |
| test-implementer | 5 | ac6fa2f41f9f5f80b | Test implementer: rows 21-24 | claude-opus-5-5 | medium | 1 | 30 | 60 | 21.928 | 6.525 | 3.514.241 | 159.523 |
| test-implementer | 6 | a97aa45ec2773b482 | Test implementer: row 25 test | claude-opus-5-5 | medium | 2 | 9 | 18 | 5.803 | 1.056 | 701.040 | 37.830 |
| spec-author | 7 | ae238274cd4f1d4b1 | Spec author: row 25 wording | claude-opus-5-5 | medium | 1 | 9 | 18 | 6.601 | 2.115 | 695.173 | 39.409 |
| implementer | 3 | a440f6aa83ddbf1a9 | Implementer: rows 21-25 | claude-opus-5-5 | medium | 1 | 23 | 46 | 22.065 | 7.304 | 2.643.147 | 97.179 |
| cleaner | 2 | a93ac35d0c11b021d | Cleaner: repair round 1 | claude-sonnet-5-5 | low | 1 | 5 | 10 | 1.697 | 44 | 318.330 | 82.143 |
| spec-checker | 2 | abd766a309f861739 | Spec checker: repair round 1 | claude-sonnet-5-5 | medium | 1 | 13 | 25 | 8.035 | 3.167 | 1.265.926 | 132.219 |
| auditor | 2 | a0f4e3f825572780c | Auditor: repair round 1 | claude-opus-5-5 | high | 1 | 46 | 92 | 74.223 | 48.510 | 8.013.611 | 254.115 |
| spec-author | 8 | a7c4fd2b8647c3628 | Spec author: audit 2 changes | claude-opus-5-5 | medium | 3 | 37 | 74 | 50.590 | 23.993 | 5.429.446 | 185.771 |
| test-implementer | 7 | af619d8851afc4392 | Test implementer: repair round 2 | claude-opus-5-5 | medium | 1 | 44 | 88 | 45.206 | 12.466 | 6.348.292 | 203.649 |
| implementer | 4 | a27cf603ec0c531a5 | Implementer: repair round 2 | claude-opus-5-5 | medium | 1 | 66 | 132 | 92.240 | 42.109 | 13.666.116 | 227.735 |
| test-implementer | 8 | a29281a61d8ef743d | Test implementer: binding helper tests | claude-opus-5-5 | medium | 1 | 14 | 26 | 23.475 | 4.879 | 1.419.565 | 131.331 |
| cleaner | 3 | a6270e5a4006085a8 | Cleaner: repair round 2 | claude-sonnet-5-5 | low | 1 | 6 | 12 | 1.634 | 48 | 396.086 | 81.980 |
| spec-checker | 3 | a4389c2bcf50baa47 | Spec checker: repair round 2 | claude-sonnet-5-5 | medium | 1 | 13 | 25 | 5.114 | 918 | 1.214.002 | 115.860 |
| auditor | 3 | af50bfdc40b07c08c | Auditor: repair round 2 | claude-opus-5-5 | high | 2 | 37 | 74 | 75.055 | 51.356 | 6.189.032 | 252.649 |
| spec-author | 9 | a6caab2383b3bf541 | Spec author: repair round 3 | claude-opus-5-5 | medium | 1 | 17 | 33 | 23.755 | 12.227 | 1.834.509 | 139.253 |
| test-implementer | 9 | a19de30c005469d6c | Test implementer: repair round 3 | claude-opus-5-5 | medium | 1 | 30 | 57 | 27.447 | 4.482 | 3.782.465 | 109.647 |
| implementer | 5 | a0cf8ffb825f2816c | Implementer: repair round 3 | claude-opus-5-5 | medium | 1 | 33 | 66 | 37.727 | 20.082 | 4.204.530 | 167.959 |
| implementer | 6 | ae1b06cb9dc1cd4fa | Implementer: repair round 3 (restart) | claude-opus-5-5 | medium | 1 | 35 | 70 | 32.022 | 15.791 | 4.901.451 | 128.400 |
| cleaner | 4 | a5f72a5166d98d544 | Cleaner: repair round 3 | claude-sonnet-5-5 | low | 1 | 5 | 10 | 1.598 | 0 | 311.419 | 80.054 |
| spec-checker | 4 | adcb2ade07baf245f | Spec checker: repair round 3 | claude-sonnet-5-5 | medium | 1 | 9 | 17 | 4.021 | 270 | 755.157 | 112.332 |
| auditor | 4 | a83db6c4f477896ef | Auditor: repair round 3 | claude-opus-5-5 | high | 1 | 27 | 54 | 63.574 | 46.329 | 4.194.190 | 227.995 |
| spec-author | 10 | ad8fbdcbf88816f77 | Spec author: row 41 wording | claude-opus-5-5 | medium | 1 | 14 | 28 | 17.467 | 8.626 | 1.235.696 | 114.637 |
| test-implementer | 10 | a3c1cd6251431986c | Test implementer: audit 4 tests | claude-opus-5-5 | medium | 2 | 39 | 80 | 35.417 | 6.205 | 4.787.569 | 172.569 |
| implementer | 7 | a919a98ac91ebf309 | Implementer: audit 4 architecture | claude-opus-5-5 | medium | 1 | 25 | 50 | 33.796 | 15.661 | 3.149.281 | 106.862 |
| spec-author | 11 | a9fb160a5035f37fd | Spec author: RH3004-008 sentence | claude-opus-5-5 | medium | 2 | 7 | 14 | 2.530 | 305 | 510.925 | 31.481 |
| cleaner | 5 | a46949caa00303788 | Cleaner: audit 4 changes | claude-sonnet-5-5 | low | 1 | 5 | 10 | 1.491 | 0 | 295.292 | 75.834 |
| spec-checker | 5 | a8162cbe26ded87f7 | Spec checker: audit 4 changes | claude-sonnet-5-5 | medium | 1 | 6 | 12 | 3.536 | 421 | 449.995 | 109.454 |
| auditor | 5 | a965dea78b52e1044 | Auditor: audit 4 architecture | claude-opus-5-5 | high | 1 | 37 | 74 | 88.053 | 67.269 | 6.119.732 | 245.077 |
| spec-author | 12 | af3164970bc572b25 | Spec author: narrowing | claude-opus-5-5 | medium | 1 | 18 | 34 | 19.735 | 9.663 | 1.797.253 | 133.772 |
| test-implementer | 11 | a23a52fbac151c20f | Test implementer: narrowing | claude-opus-5-5 | medium | 1 | 19 | 37 | 20.201 | 3.765 | 2.032.616 | 80.666 |
| implementer | 8 | aa45ee904bb4a8109 | Implementer: narrowing | claude-opus-5-5 | medium | 1 | 15 | 29 | 9.369 | 1.897 | 1.471.778 | 64.435 |
| cleaner | 6 | acf3ccb0cf502060e | Cleaner: narrowing round | claude-sonnet-5-5 | low | 1 | 5 | 10 | 1.285 | 0 | 312.644 | 80.451 |
| spec-checker | 6 | a214a67702dd2d313 | Spec checker: narrowing round | claude-sonnet-5-5 | medium | 1 | 9 | 18 | 4.469 | 682 | 701.466 | 103.968 |
| auditor | 6 | a0f49bc277e3e47ba | Auditor: narrowing round | claude-opus-5-5 | high | 1 | 35 | 70 | 53.493 | 32.616 | 4.896.309 | 192.149 |
| spec-author | 13 | a10236085632d59cd | Spec author: class narrowing | claude-opus-5-5 | medium | 1 | 32 | 61 | 35.324 | 15.788 | 3.958.049 | 155.494 |
| test-implementer | 12 | ad2b3407ab4863ae7 | Test implementer: class narrowing | claude-opus-5-5 | medium | 1 | 37 | 72 | 46.562 | 12.560 | 5.638.805 | 161.116 |
| implementer | 9 | ac12d1da5fdf4c837 | Implementer: class narrowing | claude-opus-5-5 | medium | 1 | 26 | 50 | 23.933 | 6.484 | 3.351.685 | 104.808 |
| cleaner | 7 | a8a79fc82fcf20136 | Cleaner: class narrowing | claude-sonnet-5-5 | low | 1 | 5 | 10 | 1.890 | 177 | 314.172 | 81.147 |
| spec-checker | 7 | a4aa8480d04ae3537 | Spec checker: class narrowing | claude-sonnet-5-5 | medium | 1 | 14 | 27 | 7.396 | 2.179 | 1.353.936 | 122.313 |
| auditor | 7 | a722aaae0886bfa43 | Auditor: class narrowing | claude-opus-5-5 | high | 1 | 37 | 74 | 55.396 | 34.997 | 6.093.909 | 231.473 |
| spec-author | 14 | a9b295d2685bb42c6 | Spec author: row 50 wording | claude-opus-5-5 | medium | 1 | 17 | 33 | 19.606 | 7.606 | 1.665.782 | 128.912 |
| test-implementer | 13 | a0b93789d9e83ee4a | Test implementer: row 50 tests | claude-opus-5-5 | medium | 1 | 14 | 27 | 14.193 | 1.758 | 1.397.650 | 67.994 |
| implementer | 10 | a90dd84c659c77060 | Implementer: rows 50 and 52 | claude-opus-5-5 | medium | 1 | 20 | 42 | 9.648 | 1.539 | 2.035.006 | 63.199 |
| cleaner | 8 | a1f2481df9ec595cd | Cleaner: rows 50/52 | claude-sonnet-5-5 | low | 1 | 5 | 10 | 1.373 | 64 | 313.694 | 80.378 |
| spec-checker | 8 | a0aa7da94fde39f32 | Spec checker: rows 50/52 | claude-sonnet-5-5 | medium | 1 | 8 | 15 | 4.637 | 832 | 644.962 | 106.094 |
| auditor | 8 | a6cc1fe0be9e43c23 | Auditor: rows 50/52 | claude-opus-5-5 | high | 1 | 25 | 50 | 37.751 | 23.415 | 3.078.090 | 172.038 |
| cleaner | 9 | ae463e5fef5e5acef | Cleaner: rename finding 25 | claude-sonnet-5-5 | low | 1 | 5 | 10 | 1.986 | 63 | 324.475 | 84.985 |
| auditor | 9 | ac3dfdbf9a0fa9448 | Auditor: rename check | claude-opus-5-5 | high | 1 | 7 | 14 | 4.164 | 740 | 501.832 | 89.377 |
| validator | 1 | ae306b8d5f25b8386 | Validator: final tree | claude-haiku-5-5 | low | 1 | 7 | 14 | 3.189 | 586 | 519.088 | 91.371 |
| issue-manager | 1 | a46797662f65c6ad1 | Issue manager: follow-ups | claude-sonnet-5-5 | medium | 1 | 11 | 22 | 13.512 | 1.826 | 1.182.594 | 144.409 |

| Model | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| claude-haiku-5-5 | 7 | 14 | 3.189 | 586 | 519.088 | 91.371 |
| claude-opus-5-5 | 1.703 | 3.507 | 1.944.107 | 859.969 | 283.917.711 | 8.234.797 |
| claude-sonnet-5-5 | 139 | 273 | 69.916 | 11.449 | 11.210.868 | 1.768.960 |
| **Total** | 1.849 | 3.794 | 2.017.212 | 872.004 | 295.647.667 | 10.095.128 |

## Decisions taken

55 rows in the run's decision table. Those the maintainer should see first: row 47 (reverses answer 7 — a block holding any directive now withholds the RH3004 fix), rows 45/48 (RH3106 array-element positions stay reported without a fix, departing from answer 2's principle for that class), row 51 (the safety-rail defect on `main`, filed as #1600), and row 46 (escalation 7 decided under the standing instruction).

## Suggestions

- An output answer that makes a code fix lay out code it moves (answer 1) opens a large failure surface across build configurations; a lead could ask, together with such an output question, whether the fix may be withheld for blocks holding directives or multi-line literals, before the first audit rather than after the sixth.
- The hidden-`#`-line class (a `#` line inside a string or comment that is a directive in another build) recurred in four audits; a shared "same code in every build" check (#1600) would let fixes and the formatter ask one owner instead of each guarding locally.
- Batching six issues let the cheap ones (#1257, #1282, #1417, #1422) finish after round 1 but kept them waiting for nine audits of #1279 and #1281; splitting an issue out of a batch once it alone keeps blocking would shorten such runs.
