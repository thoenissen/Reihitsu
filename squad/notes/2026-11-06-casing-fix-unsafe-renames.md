# casing-fix-unsafe-renames

| | |
|---|---|
| Date | 2026-11-06 |
| Run | issues #1070, #1073 |
| Pull request | #1596 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|

_None._ Every withheld fix was decided by SHARED-FIX-003 and the principle decisions P1/P2 on #912; every offered fix keeps `main`'s output.

## Cost

- Stops: none — no output-question batch and no escalation.
- Repair rounds: 2 of 3 (spec checker finding on SHARED-CASING-009; auditor finding 9, a test that could not fail for its stated reason). Three structural changes on one repeated class (decisions 13, 17, 19): the excluded-code guard predicted the renamer's reach — first per check, then by relation kind, then by spelling — and each audit found the next member.
- Roles spawned more than once: spec author 5×, test implementer 6×, implementer 4×, cleaner 5×, spec checker 7×, auditor 5× — the three structural changes and the two repair rounds.
- Expensive: the skeptic (cross-component class enumeration), the first test implementer (43 reproductions with a compiled-reference helper), and the auditors, which built scratch harnesses that ran the real fix across documents and projects.

## Usage

Session `f3a4f4d5-2eae-5be2-bb46-c5ca05533a2e`. Responses counted once per message id. Prices are not applied; compute a cost from the tokens and the model.

| Agent | Spawn | Agent ID | Description | Model | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | claude-opus-5-5 | medium | 26 | 92 | 238 | 50.653 | 5.297 | 16.146.910 | 181.004 |
| skeptic | 1 | a3dae97f3af949722 | Skeptic premise check | claude-opus-5-5 | high | 1 | 47 | 91 | 56.172 | 32.601 | 7.128.235 | 232.737 |
| spec-author | 1 | a97a1c79029ebb9b5 | Spec author clarifications | claude-opus-5-5 | medium | 1 | 27 | 56 | 28.225 | 14.968 | 3.266.255 | 160.755 |
| test-implementer | 1 | a32a519e86587ad5e | Test implementer reproductions | claude-opus-5-5 | medium | 1 | 50 | 100 | 56.876 | 14.219 | 7.675.107 | 172.473 |
| implementer | 1 | ae9657930dabe732e | Implementer makes tests pass | claude-opus-5-5 | medium | 1 | 42 | 86 | 44.623 | 21.878 | 6.500.766 | 149.113 |
| test-implementer | 2 | a0680ceb584f64b22 | Test implementer corrects two tests | claude-opus-5-5 | medium | 1 | 10 | 20 | 5.315 | 404 | 803.410 | 42.784 |
| cleaner | 1 | a99489b2ef8e8a6dc | Cleaner formats and warnings | claude-sonnet-5-5 | low | 1 | 6 | 12 | 1.349 | 60 | 373.959 | 77.185 |
| spec-checker | 1 | acc2037b5a93c3f20 | Spec checker verifies | claude-sonnet-5-5 | medium | 1 | 7 | 14 | 4.551 | 887 | 526.430 | 104.544 |
| spec-author | 2 | af7113dbdc5ac2bfd | Spec author repair round 1 | claude-opus-5-5 | medium | 1 | 6 | 12 | 4.016 | 1.014 | 454.293 | 40.779 |
| spec-checker | 2 | a1c0b378d56b5b4ca | Spec checker re-check round 1 | claude-sonnet-5-5 | medium | 1 | 5 | 10 | 1.843 | 15 | 362.901 | 33.725 |
| auditor | 1 | a264dc651da8e9380 | Auditor final check | claude-opus-5-5 | high | 1 | 39 | 80 | 46.251 | 23.946 | 6.054.775 | 217.352 |
| test-implementer | 3 | a85e553adbc0aa6cb | Test implementer structural repair | claude-opus-5-5 | medium | 1 | 29 | 58 | 35.683 | 8.843 | 3.457.065 | 164.324 |
| implementer | 2 | a278337818421b6b2 | Implementer structural repair | claude-opus-5-5 | medium | 1 | 17 | 34 | 18.690 | 5.023 | 1.841.255 | 91.882 |
| cleaner | 2 | a1cacde71f52629e2 | Cleaner after structural repair | claude-sonnet-5-5 | low | 1 | 5 | 10 | 1.414 | 38 | 296.950 | 76.355 |
| spec-checker | 3 | acbc1b25547d6aa42 | Spec checker after structural repair | claude-sonnet-5-5 | medium | 1 | 9 | 18 | 4.582 | 794 | 686.202 | 104.816 |
| spec-author | 3 | a5c37c949eb147889 | Spec author cascade wording | claude-opus-5-5 | medium | 1 | 9 | 18 | 5.581 | 1.079 | 679.648 | 94.493 |
| spec-checker | 4 | ac714050bc272b37e | Spec checker re-verify wording | claude-sonnet-5-5 | medium | 1 | 8 | 16 | 3.107 | 103 | 584.361 | 90.242 |
| auditor | 2 | abe1559c22e5d4cd8 | Auditor on structural repair | claude-opus-5-5 | high | 1 | 28 | 56 | 44.414 | 26.476 | 3.605.831 | 176.140 |
| spec-author | 4 | a0fe113c086e19f29 | Spec author single cascade statement | claude-opus-5-5 | medium | 1 | 13 | 25 | 11.715 | 3.559 | 1.133.600 | 111.058 |
| test-implementer | 4 | a54622389c835a95c | Test implementer partial-method cascade | claude-opus-5-5 | medium | 1 | 13 | 26 | 9.890 | 1.208 | 1.062.791 | 104.455 |
| implementer | 3 | aa2508136277f4041 | Implementer reads cascade from Roslyn | claude-opus-5-5 | medium | 2 | 14 | 28 | 7.380 | 1.491 | 1.308.989 | 60.510 |
| cleaner | 3 | a2a43a372342c796d | Cleaner after second structural change | claude-sonnet-5-5 | low | 1 | 4 | 8 | 930 | 0 | 244.400 | 85.981 |
| spec-checker | 5 | ab9107c9afefbde91 | Spec checker after second structural change | claude-sonnet-5-5 | medium | 1 | 6 | 12 | 2.975 | 368 | 426.182 | 94.299 |
| auditor | 3 | a167f7491f669caca | Auditor on second structural change | claude-opus-5-5 | high | 1 | 30 | 60 | 47.919 | 26.895 | 3.915.095 | 179.043 |
| spec-author | 5 | ac86396de2fba7efc | Spec author old-name set | claude-opus-5-5 | medium | 1 | 8 | 16 | 7.874 | 2.646 | 615.082 | 98.713 |
| test-implementer | 5 | a13941b05e774a73f | Test implementer old-name set | claude-opus-5-5 | medium | 1 | 16 | 31 | 13.678 | 2.616 | 1.476.142 | 57.162 |
| implementer | 4 | a73e2f6aa4407b165 | Implementer old-name set | claude-opus-5-5 | medium | 2 | 27 | 58 | 18.549 | 6.536 | 2.528.476 | 116.213 |
| cleaner | 4 | aa625b5771141972d | Cleaner after decision 19 | claude-sonnet-5-5 | low | 1 | 9 | 18 | 2.450 | 301 | 638.438 | 82.985 |
| spec-checker | 6 | a55b5b060a9639191 | Spec checker after decision 19 | claude-sonnet-5-5 | medium | 1 | 5 | 10 | 2.905 | 665 | 329.794 | 91.350 |
| auditor | 4 | abea15aaccea9769c | Auditor on decision 19 change | claude-opus-5-5 | high | 1 | 22 | 44 | 39.067 | 21.972 | 2.541.678 | 163.692 |
| test-implementer | 6 | aed5c986c861e993f | Test implementer finding 9 | claude-opus-5-5 | medium | 1 | 7 | 14 | 4.328 | 551 | 513.251 | 93.473 |
| cleaner | 5 | a04cc29daf642fadf | Cleaner repair round 2 | claude-sonnet-5-5 | low | 1 | 6 | 12 | 2.315 | 47 | 402.110 | 83.644 |
| spec-checker | 7 | a2aa195bcab7426cd | Spec checker repair round 2 | claude-sonnet-5-5 | medium | 1 | 3 | 6 | 1.878 | 60 | 159.448 | 87.351 |
| auditor | 5 | a5e5311bd3eed1cec | Auditor repair round 2 | claude-opus-5-5 | high | 1 | 11 | 22 | 7.164 | 1.562 | 945.114 | 52.495 |
| validator | 1 | a57d39db4db2ba3dc | Validator full run | claude-haiku-5-5 | low | 1 | 9 | 18 | 4.194 | 242 | 642.173 | 85.879 |
| issue-manager | 1 | af2f9f88d2ed42f7f | Issue manager follow-ups | claude-sonnet-5-5 | medium | 1 | 8 | 15 | 5.970 | 703 | 687.100 | 115.257 |

| Model | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| claude-haiku-5-5 | 9 | 18 | 4.194 | 242 | 642.173 | 85.879 |
| claude-opus-5-5 | 557 | 1.173 | 564.063 | 224.784 | 73.653.768 | 2.760.650 |
| claude-sonnet-5-5 | 81 | 161 | 36.269 | 4.041 | 5.718.275 | 1.127.734 |
| **Total** | 647 | 1.352 | 604.526 | 229.067 | 80.014.216 | 3.974.263 |

## Decisions taken

21, recorded in the PR's run report. The maintainer may want to see: decision 2 (the DllImport case taken as decided by P1 rather than asked), decisions 13/17/19 (three structural changes in one run on the same class), and decision 20 (the implementer's departure from decision 19's literal wording).

## Suggestions

- When a guard must cover "what a Roslyn rename renames", the first structural fix should read the renamer's result (before/after) rather than model its reach. This run modeled it three times (per check, by relation kind, by spelling) before reading it, and each model failed one audit.
- The skeptic's premise check derived one compile error (CS0542 on an interface) without compiling it, which cost a disputed test. Deriving a compiler error without a compile is a cheap thing for the skeptic's role file to forbid.
