# field-split-directives

| | |
|---|---|
| Date | 2026-11-16 |
| Run | issues #1728, #1586 |
| Pull request | #1809 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question (with escalation 10 options C, D) | Split a combined field when a directive (region, active group) or inactive code stands in front of it? | A — split behind a directive; keep whole, fix withheld, behind disabled text | yes |
| 2 | output question (escalation 10 confirmation) | Declare a directive or disabled text inside a multi-declarator field declaration unsupported, in this change? | A — declare it | yes |

## Cost

- One output-question batch (before the test implementer); no other stop.
- Two repair rounds, both on one audit finding: the reason reported when FMT-UNSUP-010 ties with another shape had no test. Round 1 covered one candidate list; audit 2 found the other list's tie still unpinned, so round 2 enumerated every tie member across both lists (decision 3).
- The skeptic caught a safety-rail defect in the first spec draft (disabled text in the leading trivia could be an attribute or modifier of another build), which cost one spec amendment before the output question.
- A container restart lost one spec-checker run; it was re-spawned.
- The test implementer's first spawn was the most expensive (broad matrix over positions, owners, entry points).

## Usage

Session `2d899014-4220-57d1-9cee-959741d550a6`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 24 | 84 | 209 | 30.989 | 4.169 | 14.599.965 | 176.630 | 147.826 |
| spec-author | 1 | a13c7978e6ce132f5 | Spec author for #1728 #1586 | large | medium | 1 | 54 | 105 | 56.809 | 25.162 | 9.168.764 | 176.649 | 152.570 |
| skeptic | 1 | ae8084f670af5faca | Skeptic for #1728 #1586 | large | high | 1 | 34 | 70 | 45.066 | 27.949 | 4.884.270 | 143.690 | 120.414 |
| spec-author | 2 | a4bb31747e6f0503b | Spec amendment after skeptic | large | medium | 1 | 27 | 54 | 25.404 | 7.163 | 3.145.899 | 93.876 | 70.350 |
| spec-author | 3 | afd4a7c7d5b59317a | Record answers in change doc | large | medium | 1 | 6 | 12 | 3.564 | 104 | 482.313 | 45.028 | 21.027 |
| test-implementer | 1 | a6a4672de4b82a323 | Test implementer for #1728 #1586 | large | medium | 1 | 94 | 188 | 102.086 | 32.652 | 22.307.232 | 297.405 | 274.092 |
| implementer | 1 | a0796efdcd9a9f2f2 | Implementer for #1728 #1586 | large | medium | 1 | 22 | 44 | 15.865 | 3.267 | 2.627.134 | 94.079 | 70.774 |
| cleaner | 1 | aec029bab0295a32e | Cleaner for #1728 #1586 | medium | low | 1 | 6 | 12 | 1.571 | 47 | 398.835 | 82.595 | 8.118 |
| spec-checker | 1 | a20fc5030b298da7d | Spec checker for #1728 #1586 | medium | medium | 1 | 11 | 22 | 5.804 | 778 | 1.082.129 | 127.582 | 53.504 |
| auditor | 1 | ad261b103099571e3 | Auditor for #1728 #1586 | large | high | 1 | 52 | 106 | 50.850 | 31.593 | 9.660.043 | 267.471 | 193.316 |
| spec-author | 4 | aa0020b2860ae4eec | Repair: AGENTS.md guard row | large | medium | 1 | 7 | 14 | 3.748 | 683 | 577.511 | 43.313 | 19.318 |
| test-implementer | 2 | a5cfb32eba5ece6ba | Repair: tie-reason tests | large | medium | 2 | 16 | 32 | 10.495 | 1.796 | 1.563.202 | 63.460 | 39.451 |
| cleaner | 2 | a7cd37a3acdafa93e | Cleaner after repair round 1 | medium | low | 1 | 5 | 10 | 1.705 | 0 | 320.693 | 83.508 | 9.003 |
| spec-checker | 2 | a2de6cb72032fae41 | Spec checker after repair 1 | medium | medium | 1 | 6 | 12 | 2.531 | 54 | 415.728 | 95.781 | 21.689 |
| auditor | 2 | a80aecdf00b94271e | Audit repair round 1 | large | high | 1 | 14 | 28 | 13.090 | 5.170 | 1.382.109 | 71.270 | 47.951 |
| test-implementer | 3 | a1fc513d22f5a8f5e | Repair 2: all tie members | large | medium | 1 | 26 | 52 | 26.857 | 9.633 | 3.048.360 | 94.564 | 70.445 |
| spec-author | 5 | a9d62a372831a9a77 | Repair 2: clarify tie wording | large | medium | 1 | 10 | 20 | 4.459 | 805 | 839.812 | 46.739 | 22.714 |
| cleaner | 3 | ab423cee5cb9e1c9c | Cleaner after repair round 2 | medium | low | 1 | 5 | 10 | 1.584 | 0 | 313.042 | 81.188 | 6.686 |
| spec-checker | 3 | a8e2697cc3d3c5aa4 | Spec checker after repair 2 | medium | medium | 1 | 3 | 6 | 437 | 0 | 154.276 | 91.160 | 17.061 |
| spec-checker | 4 | a5bd5c48d0e92160a | Spec checker after repair 2 | medium | medium | 1 | 7 | 14 | 2.416 | 131 | 559.541 | 44.314 | 20.969 |
| auditor | 3 | aa34009395c91c4d8 | Audit repair round 2 | large | high | 1 | 13 | 26 | 14.119 | 5.831 | 1.251.337 | 65.176 | 41.811 |
| issue-manager | 1 | a58df1c4322487ecd | Issue manager follow-ups | medium | medium | 1 | 8 | 18 | 3.032 | 292 | 690.545 | 52.838 | 28.907 |
| validator | 1 | a4662aaba622caa9f | Validator for #1728 #1586 | small | low | 2 | 10 | 20 | 3.746 | 539 | 215.466 | 76.992 | 11.532 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 459 | 960 | 403.401 | 155.977 | 75.537.951 | 1.679.350 |
| medium | 51 | 104 | 19.080 | 1.302 | 3.934.789 | 658.966 |
| small | 10 | 20 | 3.746 | 539 | 215.466 | 76.992 |
| **Total** | 520 | 1.084 | 426.227 | 157.818 | 79.688.206 | 2.415.308 |

## Decisions taken

Four: one change doc for both issues with RH7101-005 changed in place; keep combined fields behind disabled text whole (the skeptic's safety-rail finding, a rare class); treat the repeated tie-reason finding as an architecture signal and cover the whole class with tests rather than restructure the two candidate lists; keep FMT-UNSUP-009's reason for a directive before a local's declarator comma inside a field value (`main`'s output) and clarify the spec. The maintainer may want to see decision 3: the auditor twice hinted that the precheck's two candidate lists could become one.

## Suggestions

- The test implementer's first enumeration of tie members covered only the path its first test happened to take. When a requirement adds a candidate to an ordered list of refusal reasons, the failure-class catalog could name "every ordering path of the precheck" as an axis, so the first round covers both lists.
