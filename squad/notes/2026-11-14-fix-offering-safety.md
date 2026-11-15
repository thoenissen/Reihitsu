# fix-offering-safety

| | |
|---|---|
| Date | 2026-11-14 |
| Run | issues #1600, #1150, #1274, #1143, #1673, #1678 |
| Pull request | #1718 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | Whole-document formatting of a multi-line string/comment that hides code in another build: declare / skip silently / keep | B (skip, like a syntax error) | yes |
| 2 | output question | String opened only by an excluded branch: record gap / refuse such files | A (record gap) | yes |
| 3 | 9 | How the code fixes reach the new check: call per provider / common base type / formatter-delegating fixes only | B (base type), after asking back; plus a follow-up to reword the escalation-9 rule | no (A recommended) |
| 4 | output question + 10 | RH5308 with a comment behind `?`/`:`: break line / not reported / declare unsupported | C (declare, in this work), confirmed after asking back | no (B recommended) |
| 5 | output question | #1678 partial layout part only in excluded code: keep gap / withhold in all partial types / withhold when file has disabled text | A (keep gap; analyzers judge the current build) | no (C recommended) |
| 6 | 10 | Declare a directive in a multi-declarator field unsupported | B (declare, own change), after asking back (answered with a nonexistent label first) | no (A recommended) |
| 7 | 10 | Declare a directive between statement header and unbraced body unsupported | B (declare, own change) | no (A recommended) |
| 8 | output question | CLI/Playground report for a file skipped as hiding code | B (reported as skipped, own message) | no (A recommended) |
| 9 | output question | Refuse inline `/* */` between `?`/`:` and branch too | A (refuse every comment there) | no (B recommended) |
| 10 | 10 | Declare a comment between a binary operator and its operand unsupported | A (declare, own change) | yes |

## Cost

- Stops: two output-question batches (Q1–7, Q8–10) and two ask-backs (Q3, Q4/Q6).
- Repair rounds: 3. Round 1 was a change-doc gap; round 2 an architecture finding (two Fix All delegates bypassed the new base) plus a spec clarification; round 3 only because `verify-text-only.sh --strict-docs` classifies XML-doc edits of a public class as behavioral.
- Spec author spawned five times: the maintainer's answers came in two batches, the second skeptic stage raised new output questions, and two repairs were text.
- Expensive: the first test implementer (~42M cache read, 142 responses) and implementer (~31M, 127 responses) — six issues and a 105-provider mechanical move in one agent each; the first skeptic (~19M).

## Usage


Session `7a182cc1-a102-55a9-9339-aa4d5207f27b`. Responses counted once per message id. Prices are not applied; compute a cost from the tokens and the model.

| Agent | Spawn | Agent ID | Description | Model | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | claude-opus-5-5 | medium | 29 | 90 | 223 | 59.915 | 9.351 | 18.267.750 | 217.752 |
| spec-author | 1 | ace0d988c3fbdd8b3 | Spec author for 6-issue run | claude-opus-5-5 | medium | 1 | 36 | 70 | 35.214 | 20.035 | 4.693.794 | 181.752 |
| skeptic | 1 | ad12ed623d3e2d379 | Skeptic premise check, 6 issues | claude-opus-5-5 | high | 1 | 97 | 196 | 89.401 | 42.195 | 18.845.205 | 303.693 |
| spec-author | 2 | ad920a7daac6b4bd1 | Spec author: change doc and specs | claude-opus-5-5 | medium | 1 | 56 | 112 | 66.459 | 33.356 | 9.879.220 | 202.149 |
| skeptic | 2 | a7e7dd7ebfdd66b25 | Skeptic on spec diff | claude-opus-5-5 | high | 1 | 65 | 130 | 87.045 | 58.379 | 12.386.777 | 283.639 |
| spec-author | 3 | ad1a9cf858389acfc | Spec author: batch-2 answers | claude-opus-5-5 | medium | 1 | 40 | 80 | 34.587 | 9.042 | 5.266.145 | 170.529 |
| test-implementer | 1 | ae64681f3e73aa4d8 | Test implementer, 6 issues | claude-opus-5-5 | medium | 1 | 142 | 286 | 137.071 | 33.055 | 42.183.906 | 393.321 |
| implementer | 1 | ae7fc47164c1c6b93 | Implementer, 6 issues | claude-opus-5-5 | medium | 1 | 127 | 254 | 85.491 | 29.653 | 30.946.828 | 352.284 |
| test-implementer | 2 | a1cb047109cb52ea3 | Fix two disputed test setups | claude-opus-5-5 | medium | 1 | 11 | 22 | 6.009 | 441 | 922.779 | 43.679 |
| cleaner | 1 | a6a2cd60eaef49384 | Cleaner, 6-issue run | claude-sonnet-5-5 | low | 1 | 8 | 16 | 2.095 | 123 | 601.776 | 89.729 |
| spec-checker | 1 | af009164364de16e8 | Spec checker, 6-issue run | claude-sonnet-5-5 | medium | 1 | 23 | 46 | 13.086 | 5.191 | 2.846.196 | 168.585 |
| spec-author | 4 | a1afb050064f30547 | Repair change doc finding | claude-opus-5-5 | medium | 1 | 6 | 12 | 4.829 | 635 | 494.564 | 48.028 |
| spec-checker | 2 | a9a536652ac8303a1 | Spec checker re-run after repair | claude-sonnet-5-5 | medium | 1 | 5 | 10 | 2.621 | 317 | 402.475 | 44.409 |
| auditor | 1 | aeeeddcd1a11b1d39 | Auditor, 6-issue run | claude-opus-5-5 | high | 1 | 74 | 148 | 69.794 | 37.534 | 16.229.824 | 315.298 |
| test-implementer | 3 | ab743a56baa6767f0 | Repair round 2 tests | claude-opus-5-5 | medium | 1 | 40 | 80 | 23.633 | 5.904 | 5.172.566 | 161.717 |
| implementer | 2 | aa6587ee778f62192 | Repair round 2 implementation | claude-opus-5-5 | medium | 1 | 25 | 50 | 14.944 | 2.565 | 2.875.316 | 87.604 |
| test-implementer | 4 | a529b49e95ddf077f | Architecture test for Fix All | claude-opus-5-5 | medium | 1 | 12 | 24 | 8.747 | 2.169 | 1.092.927 | 55.928 |
| cleaner | 2 | a974e08fe22dd28dc | Cleaner, repair round 2 | claude-sonnet-5-5 | low | 1 | 7 | 14 | 2.291 | 196 | 493.899 | 85.866 |
| spec-checker | 3 | a03165e95a7270e3d | Spec checker, repair round 2 | claude-sonnet-5-5 | medium | 1 | 11 | 22 | 5.666 | 989 | 882.974 | 107.226 |
| spec-author | 5 | a781398e1eca528be | Clarify Fix All in specs | claude-opus-5-5 | medium | 1 | 7 | 14 | 3.512 | 371 | 556.797 | 39.679 |
| spec-checker | 4 | a4363d66d4437a9e4 | Spec checker re-check round 2 | claude-sonnet-5-5 | medium | 1 | 5 | 10 | 2.693 | 153 | 406.071 | 47.493 |
| auditor | 2 | a62d2870d15c09064 | Audit repair round 2 | claude-opus-5-5 | high | 1 | 27 | 54 | 26.038 | 13.477 | 3.740.688 | 177.477 |
| cleaner | 3 | a45621d08830657f9 | Fix two XML summaries | claude-sonnet-5-5 | low | 1 | 7 | 14 | 3.176 | 64 | 476.946 | 82.664 |
| auditor | 3 | a095b0670ef963b59 | Audit doc-comment repair | claude-opus-5-5 | high | 1 | 12 | 24 | 7.907 | 2.132 | 1.082.876 | 53.329 |
| validator | 1 | a20c734af5072da36 | Validator on final tree | claude-haiku-5-5 | low | 1 | 48 | 96 | 10.898 | 1.407 | 4.342.608 | 218.113 |
| issue-manager | 1 | ab92208d6a4041b76 | File follow-up issues | claude-sonnet-5-5 | medium | 1 | 12 | 24 | 14.956 | 1.804 | 1.257.408 | 138.548 |

| Model | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| claude-haiku-5-5 | 48 | 96 | 10.898 | 1.407 | 4.342.608 | 218.113 |
| claude-opus-5-5 | 867 | 1.779 | 760.596 | 300.294 | 174.637.962 | 3.087.858 |
| claude-sonnet-5-5 | 78 | 156 | 46.584 | 8.837 | 7.367.745 | 764.520 |
| **Total** | 993 | 2.031 | 818.078 | 310.538 | 186.348.315 | 4.070.491 |

## Decisions taken

29 decisions. Worth a look: decision 9 (whole-document withholding of fixes in a file with a hiding carrier, inline-comment refusal), decision 14 (`--check` exit code no longer counts a skipped hidden-code file), decision 21 (base type owns the per-document decision for every Fix All path), and decision 26 (strict text-only proof failing on doc comments counted as a repair round).

## Suggestions

- `verify-text-only.sh --strict-docs` treats XML-doc edits of any `public` type as public API documentation, although no assembly is a public API (AGENTS.md *Supported inputs*) and the package ships no XML docs; this cost a repair round for two comments. Consider dropping the public-API classification or scoping it to shipped documentation.
- Output questions with option letters that shift between drafts (spec author's C vs. the lead's C for #1274) caused an ask-back; keep a question's labels stable once a role has drafted them.
- Six issues with very different mechanisms in one run made the single test-implementer and implementer agents large; batching only issues that share a mechanism would keep agents small.
