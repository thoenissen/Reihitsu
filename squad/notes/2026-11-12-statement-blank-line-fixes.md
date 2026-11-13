# statement-blank-line-fixes

| | |
|---|---|
| Date | 2026-11-12 |
| Run | issues #946, #972, #1096, #1076, #1626, #1625 |
| Pull request | #1683 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | RH5011: keep reporting a `break` followed by a `///` comment (drop the never-implemented spec clause)? | A — keep reporting | yes |
| 2 | output question | Statement blank-line fixes inside an initializer after a multi-line `case … when` label: column as written vs. predicted formatter column | A — as written | yes |
| 3 | output question | RH3208 output for `int L() => 1; /* a⏎b */ L();` (blank line + block indentation / issue text / withhold / no move) | A — blank line, block indentation | yes |

## Cost

One output-question batch (three questions), no escalation. One standard repair round after the first audit (findings #1–#3), then three architecture decisions for repeated classes (#4 → #26, #5 → #28, #9 → #35) and a second repair round for test gaps (#7, #8). Six audits in total; repair rounds used: 2.

Where it got expensive: the first pass (six issues over one shared code base, ~20 rules, 48 C# files, 120+ new tests) took about 45% of the cache reads. About a third went to one rare shape — the column of a moved statement inside an object initializer (two statements on one line, an RH5103 violation). The lead fixed it narrowly first (decision #9), refused only the enumerated cell at the first repeat (#26), and widened the refusal only at the second repeat (#28). The resulting precise guard predicate then drew three rounds of mutant-based test-gap findings (#6, #7–#8, #9), each costing a full cleaner → spec checker → auditor chain. The root cause in the source: the code fix computes the moved statement's column itself instead of delegating layout to the formatter (follow-up #1684).

Roles spawned more than once: spec author ×7, test implementer ×8, implementer ×5, cleaner ×6, spec checker ×6, auditor ×6 — mostly repair and architecture follow-ups. Several follow-ups (spec author 24/26, test implementer 32/36) should have continued the previous agent under the continue-or-fresh table but were spawned fresh; about 5–6 M cache reads.

## Usage

## Usage

Session `f796f9fb-a679-5b49-a87a-cd08a0ee701d`. Responses counted once per message id. Prices are not applied; compute a cost from the tokens and the model.

| Agent | Spawn | Agent ID | Description | Model | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | claude-opus-5-5 | medium | 33 | 130 | 342 | 68.099 | 10.344 | 28.620.653 | 246.781 |
| spec-author | 1 | ac8338d55c586b8b5 | Spec author clarifications | claude-opus-5-5 | medium | 1 | 70 | 135 | 59.324 | 29.706 | 10.831.369 | 215.272 |
| skeptic | 1 | ac5efc809a358ef8b | Skeptic premise check | claude-opus-5-5 | high | 1 | 61 | 124 | 83.525 | 44.946 | 10.589.602 | 264.518 |
| spec-author | 2 | a254fcf57b947321c | Spec amendment after answers | claude-opus-5-5 | medium | 1 | 37 | 74 | 31.300 | 10.672 | 4.574.907 | 161.238 |
| test-implementer | 1 | abb343dc18b85ac79 | Write reproduction tests | claude-opus-5-5 | medium | 1 | 88 | 176 | 89.094 | 20.244 | 17.387.045 | 238.831 |
| implementer | 1 | aff3240df49d2ebaf | Make failing tests pass | claude-opus-5-5 | medium | 1 | 69 | 138 | 60.762 | 24.027 | 12.950.512 | 213.945 |
| test-implementer | 2 | afda414bccb80261e | Fix disputed test inputs | claude-opus-5-5 | medium | 1 | 25 | 50 | 16.708 | 4.943 | 2.680.217 | 79.451 |
| implementer | 2 | af5f0faa2a386b69a | Implementer follow-up cleanup | claude-opus-5-5 | medium | 2 | 24 | 48 | 15.502 | 3.183 | 2.404.698 | 66.669 |
| spec-author | 3 | ac894e9af4202ec53 | Spec Code owners update | claude-opus-5-5 | medium | 1 | 17 | 34 | 13.777 | 5.485 | 1.801.878 | 76.477 |
| cleaner | 1 | ab172bcbe406c4d8a | Format and warning cleanup | claude-sonnet-5-5 | low | 1 | 7 | 14 | 2.226 | 144 | 512.279 | 89.121 |
| spec-checker | 1 | aa01506af4729cdce | Spec check of diff | claude-sonnet-5-5 | medium | 1 | 12 | 23 | 6.549 | 1.614 | 1.155.924 | 125.493 |
| auditor | 1 | a3100ae480e39e3d3 | Audit finished tree | claude-opus-5-5 | high | 1 | 57 | 108 | 80.480 | 54.212 | 11.867.990 | 311.575 |
| test-implementer | 3 | afdfc6cd2936e8782 | Repair: withheld-fix tests | claude-opus-5-5 | medium | 1 | 12 | 24 | 6.449 | 913 | 1.047.867 | 107.556 |
| spec-author | 4 | ac72e8e382c3f8261 | Repair: spec text findings | claude-opus-5-5 | medium | 2 | 21 | 42 | 14.244 | 4.393 | 2.281.199 | 132.231 |
| implementer | 3 | a32db0de6affb92bc | Repair: withhold fix and summaries | claude-opus-5-5 | medium | 2 | 19 | 38 | 13.771 | 2.104 | 2.017.703 | 71.713 |
| cleaner | 2 | acbd881868124c6f3 | Repair round cleaner | claude-sonnet-5-5 | low | 1 | 6 | 12 | 1.696 | 93 | 443.525 | 92.201 |
| spec-checker | 2 | a8d4b9ea0da707884 | Repair round spec check | claude-sonnet-5-5 | medium | 1 | 11 | 21 | 4.624 | 683 | 967.268 | 113.967 |
| auditor | 2 | aaf05d4a4936133fc | Audit repair round 1 | claude-opus-5-5 | high | 1 | 31 | 59 | 34.786 | 21.287 | 4.395.815 | 194.024 |
| test-implementer | 4 | aad75f4b9d72f8af8 | RH3208 refused-class tests | claude-opus-5-5 | medium | 2 | 23 | 46 | 13.228 | 2.006 | 2.315.024 | 123.579 |
| spec-author | 5 | ae319b85ee7682e3e | Record RH3208 refused class | claude-opus-5-5 | medium | 1 | 10 | 20 | 7.071 | 1.861 | 813.434 | 103.166 |
| implementer | 4 | abc337e8d380eabf1 | Refuse class for RH3208 | claude-opus-5-5 | medium | 1 | 25 | 50 | 22.153 | 6.039 | 2.934.777 | 92.828 |
| cleaner | 3 | a6c34a48cb3ad85d6 | Cleaner after decision 26 | claude-sonnet-5-5 | low | 1 | 6 | 12 | 1.692 | 82 | 448.197 | 93.362 |
| spec-checker | 3 | a0a1d32aa3b38927b | Spec check decision 26 | claude-sonnet-5-5 | medium | 1 | 6 | 12 | 2.973 | 132 | 490.044 | 110.421 |
| auditor | 3 | a772164e7ace0e0de | Audit decision 26 repair | claude-opus-5-5 | high | 1 | 38 | 72 | 42.270 | 24.455 | 5.851.890 | 210.461 |
| test-implementer | 5 | a672edcbf0f526e82 | Tests for widened refusal | claude-opus-5-5 | medium | 2 | 34 | 68 | 30.180 | 9.200 | 4.018.596 | 160.118 |
| spec-author | 6 | a66b0046f21a4433d | Record widened refusal | claude-opus-5-5 | medium | 1 | 18 | 36 | 18.402 | 9.711 | 1.662.009 | 118.651 |
| spec-author | 7 | afba9d9b35a620065 | Narrow refusal wording | claude-opus-5-5 | medium | 1 | 12 | 23 | 11.838 | 3.575 | 1.102.309 | 119.320 |
| implementer | 5 | a1d70c21857331ab6 | Implement widened refusal | claude-opus-5-5 | medium | 1 | 18 | 36 | 20.627 | 8.773 | 2.043.335 | 92.458 |
| test-implementer | 6 | a2b67deb8954db181 | Convert disputed test to no-fix | claude-opus-5-5 | medium | 1 | 12 | 23 | 5.996 | 421 | 1.058.225 | 107.311 |
| cleaner | 4 | a0d0ea303a2234acf | Cleaner after decision 28 | claude-sonnet-5-5 | low | 1 | 6 | 12 | 1.856 | 43 | 458.845 | 96.126 |
| spec-checker | 4 | a28576d1b5c5c9d67 | Spec check decision 28 | claude-sonnet-5-5 | medium | 1 | 9 | 17 | 4.095 | 695 | 739.570 | 107.617 |
| auditor | 4 | a44bd10afd89db76d | Audit widened refusal | claude-opus-5-5 | high | 1 | 33 | 62 | 55.792 | 36.197 | 5.573.011 | 237.693 |
| test-implementer | 7 | adaba4720c0c066c3 | Repair round 2 tests | claude-opus-5-5 | medium | 1 | 12 | 23 | 9.171 | 1.406 | 1.145.230 | 120.265 |
| spec-checker | 5 | a5ce0c52ef2119fe2 | Spec check repair round 2 | claude-sonnet-5-5 | medium | 2 | 6 | 12 | 2.213 | 133 | 423.409 | 96.102 |
| cleaner | 5 | ad1ed19fbe6318576 | Cleaner repair round 2 | claude-sonnet-5-5 | low | 1 | 5 | 10 | 1.549 | 61 | 340.166 | 89.386 |
| auditor | 5 | a1eb91c3192c9f38e | Audit repair round 2 | claude-opus-5-5 | high | 1 | 20 | 38 | 17.308 | 6.621 | 2.245.239 | 137.779 |
| test-implementer | 8 | af1e3bce895a0738d | Tests closing fallback axes | claude-opus-5-5 | medium | 1 | 10 | 19 | 8.155 | 282 | 835.469 | 102.538 |
| cleaner | 6 | aa6e92a694817018e | Cleaner after decision 35 | claude-sonnet-5-5 | low | 2 | 5 | 10 | 1.595 | 38 | 340.419 | 89.500 |
| spec-checker | 6 | afec79d728e5b5212 | Spec check decision 35 | claude-sonnet-5-5 | medium | 1 | 5 | 10 | 1.726 | 55 | 352.774 | 94.144 |
| auditor | 6 | aae6be60a57f4816b | Final limited audit | claude-opus-5-5 | high | 1 | 13 | 25 | 8.889 | 1.764 | 1.242.398 | 56.599 |
| validator | 1 | af7b59da7a1cba360 | Validate final tree | claude-haiku-5-5 | low | 2 | 7 | 14 | 2.981 | 381 | 442.827 | 183.545 |
| issue-manager | 1 | ac24fc9372fdf4002 | File follow-up issues | claude-sonnet-5-5 | medium | 1 | 8 | 17 | 7.768 | 976 | 765.263 | 133.680 |

| Model | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| claude-haiku-5-5 | 7 | 14 | 2.981 | 381 | 442.827 | 183.545 |
| claude-opus-5-5 | 939 | 1.933 | 858.901 | 348.770 | 146.292.401 | 4.163.047 |
| claude-sonnet-5-5 | 92 | 182 | 40.562 | 4.749 | 7.437.683 | 1.331.120 |
| **Total** | 1.038 | 2.129 | 902.444 | 353.900 | 154.172.911 | 5.677.712 |

## Decisions taken

37 decisions. Ones the maintainer may want to see: #4 (the object-initializer limitation paragraph removed from 20 shared-base rule pages, on #1096's decision and the rule-page convention); #6 (RH5026/RH5027 region exemption taken in as an adjacent fix, by analogy with the #1076 answer for RH5024/RH5025); #23/#26/#28/#30/#33 (code fixes withheld inside initializers for the rare class where the moved statement's column cannot be read from a line started by a statement or label of its own list); #36 (four follow-up candidates dropped as rare-class only).

## Suggestions

- **Refuse at the first repeat, broadly.** At the first audit finding that repeats a rare class, refuse the whole class the skeptic named (here: any moved statement inside an initializer whose column is not trivially readable), not the cell the audit enumerated. Evidence: #4, #5 were members of the class #1 already belonged to; refusing the cell cost two further architecture rounds.
- **Stop rule for test gaps on a refusal guard.** A test-gap finding (no defect, code correct) on a guard that only serves a rare class should not start a full repair chain; it could go to review notes or a follow-up. Evidence: #6–#9 were all correct-code test gaps found by mutation probing, ~15 M cache reads.
- **Continue-or-fresh threshold.** "About 100 k cache read per response" is close to the baseline every agent reaches just by reading its role file, `squad/README.md`, handovers and specs (~80–90 k), so nearly every agent counts as large. Growth since spawn or the response count alone would be a better signal.
- **Issue intake for rare classes.** A question like #1096 Q1 about an input that already violates another rule could carry "withhold the fix" as the recommended option before it is decided as a bug; once decided as a bug with an output, the run must build the column machinery *Special cases do not grow the source* is meant to prevent.
