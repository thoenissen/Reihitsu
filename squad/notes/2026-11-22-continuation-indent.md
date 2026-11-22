# continuation-indent

| | |
|---|---|
| Date | 2026-11-22 |
| Run | issues #953, #1852, #1896, #1818 |
| Pull request | #1920 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | Which line a continuation inside a right-laid construct is measured from | Neither option: join where nothing stands in the gap (led to 1b–1e) | no |
| 2 | output question | Expression body converted across a blank line | A (join to `return 2;`) | yes |
| 3 | output question | Query clauses outside a list element | A (under `from`) | yes |
| 4 | output question | Wrapped omitted type-argument list | Declare a comment there unsupported (4b A, this run) | no |
| 5 | output question | `return` value on the next line | Join it (folded into 1b) | no |
| 6 | output question | Declarator behind `#endif` | A (continuation indent) | yes |
| 7 | output question | Other applications of the continuation rule | Join the joinable ones (folded into 1b) | no |
| 1b | output question | Join every gap that nothing keeps apart | A | yes |
| 1c | output question | Pattern-chain layout | First: rewrite with `or` under `is`; asked back: A (each combinator under `is`) | yes (after ask-back) |
| 1d | output question / 10 | Comment in a joined gap | Declared unsupported (A, comments and directives, this run) | no (first), yes (scope) |
| 1e | output question | Parenthesized-pattern content | A | yes |
| B2-1 | output question | Join the unnamed neighbouring gaps | B (join, refuse comments) | yes |
| B2-2 | output question | Modifier with no type behind it | B (join) | yes |
| B2-3 | output question / 10 | Directives in declared gaps, incl. conditional accessibility | A (refuse) | no |
| B2-4 | output question | Type test `is⏎string` | B (join) | yes |
| B2-5 | output question | Chain wrapped at only some combinators | B (every combinator starts a line) | no |
| B2-6 | 10 | Comment behind a pattern combinator | B (declare, this run) | yes |
| B2-7 | 10 | Directive inside omitted type arguments | A (refuse) | yes |
| B2-8 | output question | Column of a line left inside an arm/member | Declare unsupported instead | no |
| B2-9 | output question | One reason text per refused gap | A | yes |
| B2-10 | output question | `is` + multi-line parenthesized pattern | A | yes |
| E10 | 10 | Declare remaining comment-held continuation lines unsupported | B (in a change of its own) | no |

## Cost

- Stops: four question rounds (batch 1, ask-back 1b, ask-back 1c, batch 2) before the test implementer, plus one escalation 10 before publishing. The maintainer's batch-1 answers turned an indentation change into a join change, which re-ran the spec author three times and the skeptic once more.
- After batch 2 the maintainer handed later output questions to the lead (standing instruction); about ten such decisions followed.
- Repair rounds: 2 (spec checker minor findings; first audit's four #953 findings, one an architecture finding resolved by a single owner).
- Expensive: the first test implementer (~1.16 M cache write, 73 M cache read) and the first implementer (~58 M cache read), driven by ~110 test files and ~1,400 production lines; the spec author was spawned ten times, mostly for amendments after answers.

## Usage

Session `067a7317-225f-510e-8f9b-6cda6989da93`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 29 | 104 | 256 | 71.947 | 14.211 | 22.647.181 | 498.896 | 227.648 |
| spec-author | 1 | a9122cc330486992c | Spec author for continuation-indent | large | medium | 1 | 79 | 154 | 95.870 | 51.683 | 16.771.596 | 303.035 | 224.430 |
| skeptic | 1 | a13b2f3a8ea6e46ef | Skeptic for continuation-indent | large | high | 1 | 56 | 114 | 67.589 | 41.133 | 9.827.852 | 199.116 | 175.036 |
| spec-author | 2 | ad1f6cf964aaa144e | Spec amendment after skeptic | large | medium | 1 | 31 | 62 | 22.640 | 6.500 | 3.762.859 | 105.193 | 81.104 |
| spec-author | 3 | a7c08358a6865dffe | Spec amendment after maintainer answers | large | medium | 1 | 117 | 234 | 127.668 | 63.058 | 29.078.809 | 362.143 | 287.063 |
| skeptic | 2 | aa983d01fd709e994 | Skeptic on amended spec | large | high | 1 | 58 | 116 | 84.773 | 57.390 | 11.890.996 | 289.259 | 214.707 |
| spec-author | 4 | a91f577c77961198d | Spec amendment after batch 2 | large | medium | 1 | 78 | 156 | 102.904 | 43.516 | 15.884.966 | 283.040 | 207.873 |
| test-implementer | 1 | a4c68c2b1eec963be | Test implementer for continuation-indent | large | medium | 1 | 189 | 380 | 187.325 | 49.885 | 73.063.395 | 1.155.446 | 535.187 |
| spec-author | 5 | a3882a1913144976f | Spec fix for RH5419 and fnptr | large | medium | 1 | 44 | 88 | 25.627 | 7.898 | 5.837.093 | 114.443 | 89.693 |
| test-implementer | 2 | af7d2982b34c85616 | Test update for RH5419/RH5401/RH5201 | large | medium | 1 | 67 | 134 | 45.889 | 15.566 | 11.582.793 | 191.011 | 166.283 |
| implementer | 1 | a409f71bbcee788b3 | Implementer for continuation-indent | large | medium | 1 | 163 | 326 | 155.267 | 67.539 | 57.962.309 | 484.539 | 459.729 |
| spec-author | 6 | a1c6ac56c05041d3e | Spec amendment after implementer | large | medium | 1 | 51 | 102 | 32.425 | 7.802 | 7.825.574 | 145.131 | 120.324 |
| test-implementer | 3 | a9ed77f53e01d3fd6 | Test update after implementer | large | medium | 1 | 110 | 220 | 75.695 | 21.228 | 21.591.151 | 239.936 | 215.099 |
| spec-author | 7 | ac2d3a60b5bb3fd6f | Spec fix FMT-BLANK-018, RH2001-009 | large | medium | 1 | 28 | 56 | 17.395 | 4.865 | 3.212.991 | 86.848 | 62.131 |
| test-implementer | 4 | ab9d8f2bf86abe221 | Add RH2001-009 test | large | medium | 1 | 6 | 12 | 3.790 | 159 | 455.183 | 99.022 | 20.404 |
| cleaner | 1 | a16de13a5b6a6b2f6 | Cleaner for continuation-indent | medium | low | 1 | 7 | 14 | 2.896 | 47 | 513.738 | 90.234 | 15.359 |
| spec-checker | 1 | a55cb89839d942919 | Spec checker for continuation-indent | medium | medium | 1 | 24 | 48 | 12.305 | 3.163 | 2.784.411 | 147.450 | 73.024 |
| test-implementer | 5 | a52655a247d9a0470 | Repair: requirement tag check | large | medium | 1 | 6 | 12 | 2.933 | 300 | 493.427 | 38.960 | 14.837 |
| spec-author | 8 | a642af46c0ebfabd6 | Repair: affected requirements list | large | medium | 1 | 12 | 23 | 6.436 | 1.224 | 1.158.850 | 58.413 | 33.737 |
| spec-checker | 2 | a74b1c88d9f2cdf7a | Spec re-check after repair | medium | medium | 1 | 4 | 8 | 1.866 | 44 | 284.312 | 36.960 | 12.996 |
| spec-author | 9 | a56b22425759196d3 | Resolve merge conflicts in specs | large | medium | 1 | 10 | 20 | 4.430 | 514 | 862.553 | 42.150 | 17.881 |
| cleaner | 2 | a1f9f0e3e3672377e | Cleaner after main merge | medium | low | 1 | 6 | 12 | 1.216 | 30 | 404.795 | 82.380 | 3.899 |
| auditor | 1 | ab40903accb383eef | Auditor for continuation-indent | large | high | 1 | 98 | 196 | 102.969 | 68.102 | 23.037.553 | 339.627 | 265.083 |
| spec-author | 10 | a34d516a92a1f37c7 | Repair round 2: spec | large | medium | 1 | 56 | 107 | 33.224 | 7.712 | 7.374.043 | 167.807 | 89.175 |
| test-implementer | 6 | ad2484856ac37c714 | Repair round 2: tests | large | medium | 1 | 42 | 80 | 28.630 | 5.931 | 5.588.420 | 112.669 | 87.862 |
| implementer | 2 | a54849aa6342b6cbd | Repair round 2: implementation | large | medium | 1 | 23 | 46 | 19.963 | 6.982 | 2.423.397 | 270.934 | 63.806 |
| cleaner | 3 | a5926ae22a9209e7f | Repair round 2: cleaner | medium | low | 1 | 6 | 12 | 2.295 | 76 | 425.056 | 88.687 | 10.136 |
| spec-checker | 3 | a4254cd19f38df0ce | Repair round 2: spec check | medium | medium | 1 | 10 | 19 | 4.007 | 585 | 926.577 | 115.709 | 37.594 |
| auditor | 2 | ab0fa684d3f80ed02 | Audit repair round 2 | large | high | 1 | 35 | 70 | 41.128 | 21.611 | 4.950.679 | 189.025 | 114.419 |
| validator | 1 | a78d0e90d8cdbd040 | Validator for continuation-indent | small | low | 1 | 10 | 20 | 3.956 | 345 | 224.783 | 109.027 | 16.260 |
| issue-manager | 1 | ade6d27cb880a232f | Issue manager for follow-ups | medium | medium | 1 | 8 | 16 | 10.581 | 1.374 | 753.947 | 127.627 | 52.534 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 1.463 | 2.964 | 1.356.517 | 564.809 | 337.283.670 | 5.776.643 |
| medium | 65 | 129 | 35.166 | 5.319 | 6.092.836 | 689.047 |
| small | 10 | 20 | 3.956 | 345 | 224.783 | 109.027 |
| **Total** | 1.538 | 3.113 | 1.395.639 | 570.473 | 343.601.289 | 6.574.717 |

## Decisions taken

33 rows. The ones the maintainer should see (taken under the standing instruction): RH5419 and RH5401 fixes follow the modifier join and are withheld behind a modifier comment; RH5111 collapses a property onto one line; pattern `is`/`and`/`or` and `let` `=` get one-line spacing (`is(` → `is (`); case-label property-pattern braces join their combinator or `not`; FMT-UNSUP-019 also covers initializer members; FMT-UNSUP-019 deletes no special case and rests on the maintainer's answer 8.

## Suggestions

- An issue whose premise depends on a principle the maintainer may revisit (here: indent versus join) could open with one short principle question before the spec author writes a full draft; the first draft and two amendments were written on a premise the first answers replaced.
- Output questions that show many shapes in one block (batch 1 question 1, batch 2 question 1) drew "incomplete example" or per-shape answers; one row per shape with its own options would be read faster.
