# link-dot-name-trivia

| | |
|---|---|
| Date | 2026-11-21 |
| Run | issue #1888 |
| Pull request | #1903 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | CLI reason text for the new unsupported shape | A — `a comment or directive between a . and its member name` | yes |
| 2 | output question | Is the one-line form (`t. /* c */ Length`) refused too? | A — refused, the shape is a place on any line | yes |

The declaration itself was answered before the run (recorded in the change doc of the FMT-UNSUP-015 run), so no escalation 10 was asked for the run's own shape.

## Cost

- One output-question batch, before the test implementer; no escalations, no repair rounds.
- The spec author ran three times: the initial spec, recording the answers (status-only), and verifying a candidate special case the implementer named (`MethodChainAlignmentContributor`'s same-line condition, kept). The test implementer ran twice: the initial tests, and rewriting one existing test the first search pattern missed (a `.` behind `}`) plus a regression test for the kept condition.
- The auditor passed with one doc-comment finding, fixed by the cleaner with a text-only proof; no second audit.
- Most expensive: the test implementer's first spawn (a large class matrix of link kinds × trivia kinds × enclosing scopes) and the auditor.

## Usage

Session `e476d049-56d0-5657-be40-0969700ea99f`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 10 | 56 | 136 | 20.492 | 1.920 | 8.683.502 | 127.846 | 98.008 |
| spec-author | 1 | a7f8273e5aeb5c758 | Spec author for #1888 | large | medium | 1 | 47 | 92 | 42.506 | 17.189 | 7.252.955 | 152.739 | 128.783 |
| skeptic | 1 | aea9295510a382622 | Skeptic for #1888 | large | high | 1 | 33 | 65 | 40.085 | 24.036 | 4.542.352 | 192.114 | 114.231 |
| spec-author | 2 | a17013db1d40aec4f | Spec author: record answers | large | medium | 1 | 8 | 16 | 3.644 | 278 | 659.076 | 105.038 | 25.240 |
| test-implementer | 1 | a038af87d685dd62b | Test implementer for #1888 | large | medium | 1 | 62 | 118 | 73.992 | 15.723 | 12.237.099 | 492.380 | 200.386 |
| implementer | 1 | a436e40d2515d7c5a | Implementer for #1888 | large | medium | 1 | 28 | 53 | 14.152 | 2.733 | 3.202.934 | 213.805 | 61.755 |
| spec-author | 3 | af792ca0cc45ec85a | Spec author: special case check | large | medium | 1 | 19 | 36 | 15.116 | 3.683 | 1.907.379 | 60.700 | 35.874 |
| test-implementer | 2 | a267bfde0ab546765 | Test implementer: disputed test | large | medium | 1 | 21 | 40 | 13.018 | 3.327 | 2.262.378 | 78.622 | 53.738 |
| cleaner | 1 | a70196eaae5a88595 | Cleaner for #1888 | medium | low | 1 | 4 | 8 | 1.127 | 0 | 250.582 | 86.374 | 6.740 |
| spec-checker | 1 | a465d03b11f0331b5 | Spec checker for #1888 | medium | medium | 1 | 13 | 25 | 5.014 | 1.194 | 1.264.779 | 132.765 | 53.602 |
| auditor | 1 | a20e66d3ca3cb5f43 | Auditor for #1888 | large | high | 1 | 57 | 109 | 40.266 | 21.748 | 10.709.818 | 251.205 | 172.005 |
| cleaner | 2 | a5af330688a3d024e | Cleaner: doc comment fix | medium | low | 1 | 5 | 10 | 1.746 | 0 | 336.537 | 86.851 | 7.082 |
| validator | 1 | a1c5990b404e4471d | Validator for #1888 | small | low | 1 | 7 | 14 | 2.287 | 51 | 128.312 | 53.954 | 5.351 |
| issue-manager | 1 | af3f9d07adf0fb586 | Issue manager for #1888 | medium | medium | 1 | 7 | 15 | 4.288 | 339 | 577.397 | 110.092 | 30.932 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 331 | 665 | 263.271 | 90.637 | 51.457.493 | 1.674.449 |
| medium | 29 | 58 | 12.175 | 1.533 | 2.429.295 | 416.082 |
| small | 7 | 14 | 2.287 | 51 | 128.312 | 53.954 |
| **Total** | 367 | 737 | 277.733 | 92.221 | 54.015.100 | 2.144.485 |

## Decisions taken

Five. Worth a look: not asking escalation 10 for the qualified-name dot gap (declaring it would delete no special case; filed as #1905 with an output question instead), and keeping the `GetChainAnchorColumn` same-line condition the implementer suggested deleting (a multi-line verbatim index still reaches it).

## Suggestions

- The test implementer's search for existing tests that assert output for a newly refused shape should match any token in front of the refused gap; a `.` behind `}` slipped through the first pattern and was caught only by the implementer's full run.
