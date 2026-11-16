# one-statement-docs

| | |
|---|---|
| Date | 2026-11-16 |
| Run | issues #1470, #1471, #1479, #1544 |
| Pull request | #1793 |

## Questions to the maintainer

None. The run is tooling-lane text and repository tooling; no output question and no escalation arose.

## Cost

No stops. One repair round (the first audit's documentation finding: the text-only proof failed because it treats every `scripts/**` path as behavioral, so the doc fix counted as a repair round). The repair audit found one more member of the same class (a document restating the lane classification), handled as an architecture finding and refused by cause: a sweep of every instruction document so that only the lane-check table in `scripts/README.md` says which paths a lane covers. Auditor and spec checker therefore ran three times each, the spec author twice. The validator ran twice: the first run failed only on `SquadNoteReferenceTests`, which flags the run's own handovers under `sdd/work/` that quoted a note path, and `test.sh` stopped before the Tooling and Playground projects; `sdd/work/` was removed before the second run.

After publishing, `main` gained a check of the same rule as the run's new phase-ownership guard-name test (a parallel PR); the merge kept `main`'s check and dropped the run's test, followed by one more validator run.

## Usage


Session `3380737c-33b7-551a-b9e9-566dcd1669f3`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 9 | 56 | 138 | 21.974 | 2.598 | 8.175.779 | 116.523 |
| test-implementer | 1 | a20a57974979ccb33 | Test implementer for 4 issues | large | medium | 1 | 22 | 44 | 15.839 | 3.569 | 2.489.936 | 138.349 |
| implementer | 1 | ae13b8b3c22feb668 | Implementer for 4 issues | large | medium | 1 | 38 | 74 | 25.220 | 6.446 | 4.919.245 | 109.118 |
| cleaner | 1 | ac072c5325134f03e | Cleaner for 4 issues | medium | low | 1 | 5 | 10 | 1.716 | 41 | 344.169 | 89.384 |
| spec-checker | 1 | a7bf31f4a3834df55 | Spec checker for 4 issues | medium | medium | 1 | 7 | 15 | 5.129 | 723 | 585.964 | 115.878 |
| auditor | 1 | a51acc9328c77c980 | Auditor for 4 issues | large | high | 1 | 32 | 63 | 26.769 | 13.515 | 4.481.249 | 127.839 |
| spec-author | 1 | affc0891f4a69c0a8 | Spec author doc repair | large | medium | 1 | 11 | 21 | 9.278 | 3.289 | 1.136.453 | 66.394 |
| spec-checker | 2 | aee4796c6026404e3 | Spec checker repair round | medium | medium | 1 | 6 | 12 | 2.987 | 683 | 436.023 | 96.561 |
| auditor | 2 | a79e17cd9c809c154 | Auditor on repair round 1 | large | high | 1 | 11 | 21 | 12.025 | 5.988 | 1.065.465 | 61.155 |
| spec-author | 2 | a9b065b4cbc21b1c4 | Spec author: refuse class by cause | large | medium | 1 | 16 | 31 | 11.126 | 3.224 | 1.650.726 | 67.886 |
| spec-checker | 3 | a97355a760895e7c8 | Spec checker after decision 8 | medium | medium | 1 | 7 | 14 | 3.463 | 714 | 631.383 | 49.021 |
| auditor | 3 | ac31b4a2cdedb5a98 | Auditor after decision 8 | large | high | 1 | 16 | 31 | 13.650 | 6.924 | 1.705.546 | 78.432 |
| validator | 1 | a4cadd64c3a654e0a | Validator on final tree | small | low | 2 | 8 | 16 | 3.477 | 494 | 153.036 | 49.064 |
| issue-manager | 1 | a9ecdf31beab973df | Issue manager for follow-ups | medium | medium | 1 | 9 | 20 | 3.890 | 463 | 799.666 | 57.479 |
| validator | 2 | a133959dbc77863b4 | Validator rerun on final tree | small | low | 1 | 8 | 16 | 3.766 | 395 | 183.461 | 56.266 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 202 | 423 | 135.881 | 45.553 | 25.624.399 | 765.696 |
| medium | 34 | 71 | 17.185 | 2.624 | 2.797.205 | 408.323 |
| small | 16 | 32 | 7.243 | 889 | 336.497 | 105.330 |
| **Total** | 252 | 526 | 160.309 | 49.066 | 28.758.101 | 1.279.349 |

## Decisions taken

Ten. Worth a look: dropping #1544's `LineBreakPhase` goal (the row already stated it on `main`); taking `squad/roles/auditor.md` into #1470; refusing the repeated lane-classification class by a sweep of all instruction docs rather than one more line; keeping the path table in the accepted change doc `sdd/changes/2026-10/introduce-sdd.md` as history.

## Suggestions

- Handovers that quote a hypothetical note path (`notes/<date>-x.md`) trip `SquadNoteReferenceTests` while `sdd/work/` exists, which hides later test projects from the validator because `test.sh` stops at the first failing project. Either the test could skip `sdd/work/`, or the validator could run on a tree without `sdd/work/`.
- The text-only proof treats Markdown under `scripts/` as behavioral, so a doc-only fix there always costs a repair round under lead.md section 7.
