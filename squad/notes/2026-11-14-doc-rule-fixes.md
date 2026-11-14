# doc-rule-fixes

| | |
|---|---|
| Date | 2026-11-14 |
| Run | issues #1345, #1346, #1347, #1348, #1270, #1092, #1153, #1617 |
| Pull request | #1700 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | RH8307: are wrapped inline elements (`<b>`) reported? Asked back: only inline excluded (B) or only top-level elements reported (C) | C; nested-element checking becomes a feature issue (#1712) | No (A recommended) |
| 2 | output question | RH8106/RH8306: one diagnostic per reported element or per declaration | A, per element | Yes |
| 3 | 10 | Declare a `///` documentation comment written behind code unsupported | B, not declared | Yes |
| 4 | output question | RH8201/RH8203 on a partial member documented on both parts: once on the defining part, or each part | B, each part | No (A recommended); the maintainer asked back first, then confirmed B |

## Cost

- One stop (output batch 1), with two short ask-backs (Q1 scope, Q4 confirmation). No stop before the auditor.
- The lane widened from bug fix to behavior change after batch 1 (Q1 C and Q2 A changed requirements), so the spec author ran twice and the skeptic twice.
- Two repair rounds: round 1 for a PR-introduced RH8203 regression on partial parts (later comment, `<include>`), round 2 for untested cref branches, a stale comment and a three-way copy of comment selection.
- `main` moved before the second repair audit; merged (ledger only overlap), and the auditor confirmed the merge.
- Most expensive spawns: the first spec author, the test implementer, the implementer, and the first audit (each 60–90 responses over eight issues).

## Usage

Model identifiers are replaced by `model-1` (lead, spec author, skeptic, test implementer, implementer, auditor), `model-2` (cleaner, spec checker, issue manager) and `model-3` (validator), because this environment forbids model identifiers in pushed files; the rows are otherwise unchanged.

Session `ba2fe826-fb0f-5ed5-9b61-789f4316c2f7`. Responses counted once per message id. Prices are not applied; compute a cost from the tokens and the model.

| Agent | Spawn | Agent ID | Description | Model | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | model-1 | medium | 26 | 82 | 200 | 45.467 | 8.163 | 15.614.843 | 182.365 |
| spec-author | 1 | aec083991523cf4ff | Spec author clarifications | model-1 | medium | 1 | 79 | 151 | 83.697 | 41.099 | 16.600.482 | 290.352 |
| skeptic | 1 | a72bad0522d81b4c7 | Skeptic premise check | model-1 | high | 1 | 62 | 128 | 75.448 | 38.607 | 11.367.676 | 207.195 |
| spec-author | 2 | a82187a8b26059f26 | Spec author amendments | model-1 | medium | 2 | 43 | 88 | 38.235 | 10.280 | 5.858.722 | 366.661 |
| skeptic | 2 | a272407b4b3e1f35a | Skeptic on spec amendment | model-1 | high | 1 | 44 | 90 | 45.872 | 26.484 | 6.063.419 | 131.360 |
| test-implementer | 1 | ab4b2318d681d341b | Test implementer for run | model-1 | medium | 1 | 82 | 166 | 85.404 | 15.445 | 19.129.315 | 306.360 |
| implementer | 1 | a1191f93b5a650cc9 | Implementer for run | model-1 | medium | 1 | 87 | 174 | 59.561 | 14.886 | 15.627.549 | 217.888 |
| cleaner | 1 | a8f2d77e0e3f69bf2 | Cleaner for run | model-2 | low | 1 | 6 | 12 | 1.660 | 40 | 466.130 | 36.753 |
| spec-checker | 1 | a7e9629e287a13381 | Spec checker for run | model-2 | medium | 1 | 9 | 18 | 4.588 | 1.091 | 769.127 | 112.570 |
| auditor | 1 | a9f0e0a79b9cade5a | Auditor for run | model-1 | high | 1 | 61 | 122 | 68.724 | 43.280 | 12.837.313 | 302.432 |
| test-implementer | 2 | af8192b2299f095f7 | Repair: RH8203 partial tests | model-1 | medium | 1 | 12 | 24 | 15.444 | 2.761 | 1.094.512 | 121.288 |
| implementer | 2 | a1dd299df1bd34f04 | Repair: RH8203 and cleanup | model-1 | medium | 2 | 28 | 58 | 24.779 | 9.969 | 3.233.977 | 95.695 |
| cleaner | 2 | ab719be3edf00ef2f | Cleaner repair round 1 | model-2 | low | 1 | 7 | 14 | 2.601 | 96 | 491.247 | 86.416 |
| spec-checker | 2 | af1d9a237aaa6318e | Spec checker repair round 1 | model-2 | medium | 1 | 6 | 12 | 2.445 | 301 | 441.568 | 100.275 |
| auditor | 2 | af58931a879a44894 | Auditor repair round 1 | model-1 | high | 1 | 25 | 50 | 33.067 | 19.851 | 3.167.488 | 176.203 |
| test-implementer | 3 | a9d9f09553cb2682f | Repair 2: RH8203 cref tests | model-1 | medium | 1 | 11 | 22 | 6.601 | 586 | 911.862 | 103.169 |
| implementer | 3 | aa3e49797d93af7f3 | Repair 2: shared comment helper | model-1 | medium | 1 | 12 | 24 | 6.593 | 663 | 1.062.229 | 51.518 |
| cleaner | 3 | ada21699cfb3f8d64 | Cleaner repair round 2 | model-2 | low | 1 | 7 | 14 | 2.578 | 116 | 476.224 | 83.176 |
| spec-checker | 3 | a383657892a5bf81c | Spec checker repair round 2 | model-2 | medium | 1 | 5 | 10 | 1.981 | 38 | 339.958 | 91.633 |
| auditor | 3 | ad3b5dcc055200f2a | Auditor repair round 2 | model-1 | high | 1 | 18 | 36 | 11.317 | 3.561 | 1.796.806 | 124.911 |
| validator | 1 | a421b8c50d8a5043e | Validator final tree | model-3 | low | 2 | 7 | 14 | 3.033 | 360 | 412.762 | 171.711 |
| issue-manager | 1 | aa9cf9264e09271f8 | Issue manager follow-ups | model-2 | medium | 1 | 12 | 26 | 12.971 | 2.561 | 1.344.663 | 101.741 |

| Model | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| model-3 | 7 | 14 | 3.033 | 360 | 412.762 | 171.711 |
| model-1 | 646 | 1.333 | 600.209 | 235.635 | 114.366.193 | 2.677.397 |
| model-2 | 52 | 106 | 28.824 | 4.243 | 4.328.917 | 612.564 |
| **Total** | 705 | 1.453 | 632.066 | 240.238 | 119.107.872 | 3.461.672 |

## Decisions taken

26 decisions. Worth the maintainer's eye: #1092 was not implemented (fixing the group alone would only rename RH8002 to RH8001, against the #1037 Q3a answer; linked as `Refs`); RH8401/RH8402 re-parse a `DocumentationMode.None` tree in `Parse` mode; RH8203 builds a small throwaway compilation to expand `<include>` on a partial part, only when the part's own comment holds one; the RH8303 fix for a behind-code comment above a deeper-indented declaration takes the declaration's column for the first line only.

## Suggestions

- Eight issues in one run made every handover long; the first spec author and test implementer were large. A batch of eight is at the limit for one agent per role.
- The triage comments had moved four issues to the bug-fix lane, but two maintainer answers in batch 1 changed requirements anyway; a batch whose issues carry open output cases could be claimed in the behavior-change lane from the start.
- The notes README asks for usage output unchanged, while this environment forbids model identifiers in pushed files; `scripts/usage.sh` could offer a redacted mode.
