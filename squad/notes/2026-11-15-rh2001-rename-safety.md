# rh2001-rename-safety

| | |
|---|---|
| Date | 2026-11-15 |
| Run | issue #1757 |
| Pull request | #1770 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | RH2001 fix when the derived field name is already used by an inherited member (fix rewrote the read to `base._value`): keep or withhold? | B — withhold | Yes |
| 2 | 9 | RH2001 fix when the property is an element of a tuple a `switch` matches on (and the rest of the refused tuple class): withhold or keep `main`'s fix? | A — withhold | Yes |

## Cost

- Two stops: one output-question batch before the test implementer, one escalation 9 after the second repair audit.
- Lane widened from bug fix to behavior change after answer 1 (new RH2001-011, change doc).
- The skeptic's measured table for implicitly typed lambdas and the extension call did not reproduce on the test inputs; the implementer disputed 5 tests and a fresh test implementer found the real failing shape (lambda in a block body, delegate-typed property). One extra test-implementer and spec-author round before the first audit.
- Three repair rounds plus a final narrowing round. Round 1 (inferred anonymous/tuple names, receiver text, crefs); round 2 (generic constructions refused as a repeated class; tuple guard narrowed); round 3 (tuple guard refused as a repeated class after answer 2; no code change); narrowing round (offered-side tests for non-tuple targets, text fixes).
- Most expensive: the auditor (four audits, each tracing many fixtures on head and `main`) and the RH2001 binding-check mechanism, whose boundaries the audits kept finding one class at a time.
- The analyzer test project was OOM-killed in single runs during implementation; the validator's single run completed (9963 tests).

## Usage

## Usage

Session `4271dfb1-87ee-51f4-9b4e-ea7505ebffbd`. Responses counted once per message id. Prices are not applied; compute a cost from the tokens and the model.

| Agent | Spawn | Agent ID | Description | Model | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | claude-opus-5-5 | medium | 34 | 119 | 300 | 52.088 | 9.754 | 23.376.187 | 211.849 |
| skeptic | 1 | aaab4972bb196672b | Skeptic for issue 1757 | claude-opus-5-5 | high | 1 | 43 | 84 | 43.488 | 20.867 | 5.704.093 | 184.911 |
| spec-author | 1 | adcbb1c521e493396 | Spec author for issue 1757 | claude-opus-5-5 | medium | 3 | 23 | 48 | 21.343 | 4.985 | 2.367.012 | 416.361 |
| test-implementer | 1 | aa11cca06a59d349c | Test implementer for issue 1757 | claude-opus-5-5 | medium | 1 | 17 | 34 | 26.606 | 4.947 | 1.931.483 | 156.608 |
| implementer | 1 | a8ea4afef07644bab | Implementer for issue 1757 | claude-opus-5-5 | medium | 1 | 42 | 84 | 46.985 | 21.277 | 5.974.869 | 141.570 |
| test-implementer | 2 | a7629043e5c479261 | Resolve disputed tests 1757 | claude-opus-5-5 | medium | 3 | 35 | 72 | 35.815 | 7.486 | 4.424.923 | 111.974 |
| cleaner | 1 | a40115a32cf569aa2 | Cleaner for issue 1757 | claude-sonnet-5-5 | low | 1 | 6 | 12 | 1.749 | 48 | 400.111 | 82.952 |
| spec-checker | 1 | a84dfc08828670077 | Spec checker for issue 1757 | claude-sonnet-5-5 | medium | 1 | 8 | 16 | 4.110 | 796 | 667.342 | 112.594 |
| auditor | 1 | a89318eb31e2a3ca9 | Auditor for issue 1757 | claude-opus-5-5 | high | 1 | 38 | 78 | 43.257 | 26.149 | 5.902.581 | 211.268 |
| spec-author | 2 | a6d86350d590407ce | Spec repair round 1 for 1757 | claude-opus-5-5 | medium | 2 | 12 | 26 | 9.090 | 2.239 | 986.510 | 158.104 |
| test-implementer | 3 | a4a51e337a7b690de | Tests repair round 1 for 1757 | claude-opus-5-5 | medium | 2 | 25 | 50 | 22.295 | 3.496 | 2.694.126 | 140.047 |
| implementer | 2 | afeed2cf29c43ad0f | Implementer repair round 1 for 1757 | claude-opus-5-5 | medium | 1 | 15 | 30 | 14.814 | 5.385 | 1.514.907 | 73.697 |
| cleaner | 2 | a44d5a55ace7ec3c9 | Cleaner repair round 1 for 1757 | claude-sonnet-5-5 | low | 1 | 6 | 12 | 1.683 | 0 | 403.540 | 83.572 |
| spec-checker | 2 | a6be040656ae6cca0 | Spec checker repair round 1 for 1757 | claude-sonnet-5-5 | medium | 1 | 7 | 14 | 3.277 | 247 | 523.702 | 100.400 |
| auditor | 2 | a4157a51519612235 | Auditor repair round 1 for 1757 | claude-opus-5-5 | high | 1 | 26 | 52 | 38.364 | 22.218 | 3.358.286 | 177.353 |
| test-implementer | 4 | adb409a5e4ec4d439 | Tests repair round 2 for 1757 | claude-opus-5-5 | medium | 2 | 26 | 52 | 15.531 | 2.077 | 2.651.706 | 123.597 |
| spec-author | 3 | a54ca6595a18f9851 | Spec repair round 2 for 1757 | claude-opus-5-5 | medium | 1 | 11 | 22 | 17.264 | 6.856 | 950.090 | 113.307 |
| implementer | 3 | a26f964324c403f45 | Implementer repair round 2 for 1757 | claude-opus-5-5 | medium | 1 | 25 | 50 | 14.652 | 2.400 | 2.663.602 | 205.305 |
| cleaner | 3 | afda3e610f9af608a | Cleaner repair round 2 for 1757 | claude-sonnet-5-5 | low | 1 | 5 | 10 | 1.659 | 70 | 312.477 | 80.744 |
| spec-checker | 3 | a033354c6ce309cd3 | Spec checker repair round 2 for 1757 | claude-sonnet-5-5 | medium | 1 | 6 | 12 | 2.631 | 181 | 452.310 | 103.761 |
| auditor | 3 | a5fff5e3be8245940 | Auditor repair round 2 for 1757 | claude-opus-5-5 | high | 1 | 34 | 68 | 39.454 | 21.053 | 4.585.492 | 178.781 |
| spec-author | 4 | a8ba630f2877377d9 | Spec repair round 3 for 1757 | claude-opus-5-5 | medium | 1 | 19 | 38 | 19.810 | 6.969 | 2.025.393 | 143.334 |
| test-implementer | 5 | a97ae2234f5bb2aa7 | Tests repair round 3 for 1757 | claude-opus-5-5 | medium | 2 | 22 | 46 | 13.487 | 715 | 2.068.213 | 117.446 |
| cleaner | 4 | a49c73523b74da00c | Cleaner repair round 3 for 1757 | claude-sonnet-5-5 | low | 1 | 5 | 10 | 2.556 | 210 | 318.928 | 84.742 |
| spec-checker | 4 | abc0e000e77e7054b | Spec checker repair round 3 for 1757 | claude-sonnet-5-5 | medium | 1 | 8 | 16 | 3.118 | 258 | 609.459 | 95.773 |
| auditor | 4 | aa350bf2e7670aac6 | Auditor repair round 3 for 1757 | claude-opus-5-5 | high | 1 | 24 | 48 | 25.160 | 13.354 | 2.875.108 | 155.719 |
| test-implementer | 6 | a6f3b292e3135ebaa | Narrowing-round tests for 1757 | claude-opus-5-5 | medium | 2 | 14 | 28 | 7.567 | 407 | 1.169.643 | 97.458 |
| spec-author | 5 | a50b04076c9c26e82 | Narrowing-round spec text for 1757 | claude-opus-5-5 | medium | 1 | 6 | 12 | 4.912 | 307 | 434.483 | 97.856 |
| cleaner | 5 | a5f2f030d145e77da | Cleaner narrowing round for 1757 | claude-sonnet-5-5 | low | 1 | 5 | 10 | 1.642 | 106 | 323.803 | 85.039 |
| auditor | 5 | ac19c9620a96df995 | Auditor narrowing round 1757 | claude-opus-5-5 | high | 2 | 14 | 28 | 9.416 | 2.191 | 1.473.946 | 79.619 |
| spec-checker | 5 | a09e0fc09d36773f1 | Spec checker narrowing round 1757 | claude-sonnet-5-5 | medium | 1 | 6 | 12 | 2.563 | 311 | 431.847 | 97.433 |
| issue-manager | 1 | ad5e3d1a983959b16 | Issue manager for 1757 | claude-sonnet-5-5 | medium | 1 | 6 | 14 | 5.117 | 561 | 543.574 | 70.151 |
| validator | 1 | acb91b689ea4e40ce | Validator for issue 1757 | claude-haiku-5-5 | low | 2 | 9 | 18 | 3.688 | 303 | 570.744 | 170.540 |

| Model | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| claude-haiku-5-5 | 9 | 18 | 3.688 | 303 | 570.744 | 170.540 |
| claude-opus-5-5 | 590 | 1.250 | 521.398 | 185.132 | 79.132.653 | 3.296.164 |
| claude-sonnet-5-5 | 68 | 138 | 30.105 | 2.788 | 4.987.093 | 997.161 |
| **Total** | 667 | 1.406 | 555.191 | 188.223 | 84.690.490 | 4.463.865 |

## Decisions taken

26 decisions. Ones the maintainer may want to see: decision 3 (narrow RH2001-specific rename-binding check instead of extracting the casing fixes' private check; unification filed as #1771), decision 22 (the guard is ~145 code lines in one provider, judged not a structural change for escalation 9), and decisions 19 and 23 (refusing rare generic-construction and tuple classes after repeated audit findings).

## Suggestions

- The skeptic measured outputs on fixtures it then deleted; the test implementer rebuilt different inputs and the disagreement cost a round. Keeping the skeptic's fixtures (inputs inline in the handover) would have let the test implementer reproduce exactly the failing shape.
- A semantic "does the rename keep every binding" guard attracts one audit finding per unmodelled input class; refusing by class earlier (after the first repeat) would have saved a round.
