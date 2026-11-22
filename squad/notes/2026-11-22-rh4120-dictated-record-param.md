# rh4120-dictated-record-param

| | |
|---|---|
| Date | 2026-11-22 |
| Run | issue #1939 |
| Pull request | #1947 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 0 | output question | (asked on the issue before the run) Is a positional parameter whose generated property implements a referenced-assembly interface property reported? | A — not reported | yes |
| 1 | output question | Is a positional parameter whose generated property overrides a referenced-assembly property reported? | A — not reported | yes |
| 2 | output question | Is a parameter the compiler declares no property for, matching an inherited or declared dictated member, reported? | B — not reported when that member is dictated; not when it merely comes from another assembly | yes |

## Cost

One output-question batch (two questions, raised by the spec author and the skeptic). Two repair rounds: round 1 repaired an accessibility gap in the new member lookup (the compiler skips an inaccessible inherited member; the lookup did not) and pinned an untested field branch; round 2 was test-only — the audit found the accessibility guard covered only for inaccessible and public members, a repeat of round 1's test-gap class, decided as an architecture finding closed by tests because the guard already reads the compiler's result. The three audits were the most expensive spawns; the first one found the real defect by probing.

## Usage

Session `159b922b-a7ab-54be-ab96-eb707ec5e25d`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 20 | 65 | 165 | 25.555 | 2.356 | 10.145.217 | 138.082 | 105.568 |
| spec-author | 1 | ac4af1c14114a5f8d | Spec author for #1939 | large | medium | 1 | 19 | 38 | 15.663 | 5.858 | 2.092.218 | 79.156 | 54.454 |
| skeptic | 1 | a7d958c41966b6fee | Skeptic for #1939 | large | high | 1 | 23 | 48 | 34.303 | 20.980 | 2.831.790 | 163.819 | 88.746 |
| spec-author | 2 | a461d7987a15a60f1 | Spec author follow-up #1939 | large | medium | 1 | 15 | 30 | 15.335 | 4.084 | 1.521.942 | 69.408 | 43.955 |
| test-implementer | 1 | a844b136f5beb2eaa | Test implementer #1939 | large | medium | 1 | 24 | 48 | 24.549 | 5.755 | 2.784.754 | 94.725 | 70.080 |
| implementer | 1 | a81fc61150e4c33ad | Implementer #1939 | large | medium | 1 | 17 | 34 | 8.442 | 2.215 | 1.640.991 | 64.170 | 39.567 |
| cleaner | 1 | ae1493689d0619134 | Cleaner #1939 | medium | low | 1 | 6 | 12 | 1.370 | 0 | 396.410 | 81.174 | 5.725 |
| spec-checker | 1 | a1bc4db45978504db | Spec checker #1939 | medium | medium | 1 | 5 | 10 | 2.695 | 424 | 402.443 | 51.412 | 26.865 |
| auditor | 1 | a90b2cb56bf225fac | Auditor #1939 | large | high | 1 | 29 | 58 | 41.667 | 25.023 | 4.201.342 | 144.559 | 119.950 |
| test-implementer | 2 | a20254baff2e0eba0 | Test repair round 1 #1939 | large | medium | 1 | 10 | 20 | 9.282 | 2.471 | 891.563 | 115.801 | 40.212 |
| implementer | 2 | a3d6d6dba834403c8 | Implementer repair round 1 #1939 | large | medium | 1 | 11 | 22 | 5.621 | 348 | 970.666 | 50.141 | 24.818 |
| cleaner | 2 | ac6229ad1ab15c093 | Cleaner repair round 1 #1939 | medium | low | 1 | 5 | 10 | 1.170 | 0 | 322.285 | 83.084 | 7.628 |
| spec-checker | 2 | aa39873241652bfe1 | Spec checker repair round 1 #1939 | medium | medium | 1 | 6 | 12 | 2.345 | 294 | 443.226 | 97.843 | 22.806 |
| auditor | 2 | a6e4b3d5c9330d641 | Auditor repair round 1 #1939 | large | high | 1 | 16 | 32 | 25.231 | 15.876 | 1.647.480 | 131.879 | 56.715 |
| test-implementer | 3 | a9aff862a3c99ac5f | Test repair round 2 #1939 | large | medium | 1 | 11 | 22 | 8.863 | 1.340 | 1.000.480 | 57.159 | 32.539 |
| cleaner | 3 | a096e02036e70f369 | Cleaner repair round 2 #1939 | medium | low | 1 | 6 | 12 | 1.364 | 0 | 398.658 | 81.742 | 6.296 |
| spec-checker | 3 | aa15777341bfff3a3 | Spec checker repair round 2 #1939 | medium | medium | 1 | 5 | 10 | 1.813 | 0 | 383.404 | 40.253 | 15.669 |
| auditor | 3 | afb294d3cdafb87f3 | Auditor repair round 2 #1939 | large | high | 1 | 8 | 16 | 7.156 | 2.018 | 619.482 | 101.279 | 26.120 |
| validator | 1 | aa7f2b2e9102f4f7c | Validator #1939 | small | low | 1 | 9 | 18 | 3.329 | 190 | 220.816 | 68.644 | 12.293 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 248 | 533 | 221.667 | 88.324 | 30.347.925 | 1.210.178 |
| medium | 33 | 66 | 10.757 | 718 | 2.346.426 | 435.508 |
| small | 9 | 18 | 3.329 | 190 | 220.816 | 68.644 |
| **Total** | 290 | 617 | 235.753 | 89.232 | 32.915.167 | 1.714.330 |

## Decisions taken

Seven. Worth the maintainer's eye: the accessibility guard (row 4) follows answer 2B for parameters behind an inaccessible intermediate member rather than reporting them, and the spec wording "declares or inherits" was kept although a private base member is "inherited" in C# terms (row 7; the Why paragraph resolves it).

## Suggestions

The spec author's first draft decided the "parameter matches an inherited member" boundary (no property declared) as "still reported" without flagging it as an output question; the skeptic caught it. A spec author could list every boundary sentence its draft adds that changes or newly fixes a visible output as a candidate output question.
