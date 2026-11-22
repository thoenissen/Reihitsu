# casing-fix-names

| | |
|---|---|
| Date | 2026-11-22 |
| Run | issues #971, #1072, #1451, #1434 |
| Pull request | #1934 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | Leading acronym in camelCase / `_camelCase` targets (`HTTPClient` → `_httpClient`, `URLs` → `_urLs`) | A | yes |
| 2 | output question | RH4005: existing `I`/`i` prefix counts; fix withheld for `I`, `i`, `i2Factory` | A | yes |
| 3 | output question | RH4121: existing `T_`/`t_` prefix counts; `T_` → `T` | A | yes |
| 4 | output question | `Café` (U+0301) and `ClassⅫ` no longer reported; Mn/Mc/Nl/Pc kept in the word | A | yes |
| 5 | output question | `CAFÉ_NAME` (U+0301) → `CaféName` | A | yes |
| 6 | output question | Fix withheld when the rename would write `@` (reserved keyword, `await` in async) | A | yes |
| 7 | output question | Names dictated by a referenced assembly not reported, through the whole override chain | A | yes |
| 8 | output question | Override of a method from another project of the solution not reported | A | yes |
| 9 | output question | RH4104: `[DllImport]` local function without `EntryPoint` not reported | A | yes |
| 10 | output question | Acronym run taken from the first word only (`m_HTTPClient` → `_mHTTPClient`) | A | yes |
| 11 | output question | Base-class method that implements an interface only through a derived class stays reported | A | yes |
| 12 | output question | Tuple names supplied by a type argument reported where written, not in the dictated signature | A | yes |

## Cost

One stop (one batch of twelve output questions, answered "as recommended"). Three repair rounds plus two structural repairs: a spec-checker code-owner finding (round 1); audit 1's RH4116 operator/event and `ToPascalCaseWord` findings (round 2); audit 2's record-positional-parameter finding repeated audit 1's class and became a structural repair (RH4116 decides by declared symbol instead of a syntax-kind list); audit 3's record `Deconstruct` finding repeated it again and became a second structural repair, with a duplicated-mapping finding and missing tests as round 3. Four audits in total. The expensive part was the RH4116 exemption: the first implementation chose the dictated signature by syntax kind, and each audit found the next uncovered member kind. Deciding by the compiler's declared symbol from the start would have saved two audits and two structural repairs.

## Usage

Session `0c2e8da6-d36b-54b9-836d-2f92577bdd98`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 32 | 103 | 254 | 47.552 | 4.217 | 19.636.180 | 198.720 | 169.959 |
| spec-author | 1 | a7c3a23fe5a1c3915 | Spec author: casing fixes | large | medium | 1 | 61 | 117 | 89.577 | 40.871 | 13.320.141 | 256.222 | 231.421 |
| skeptic | 1 | a7509f95772642126 | Skeptic: casing fixes | large | high | 1 | 46 | 94 | 61.699 | 36.686 | 7.543.033 | 182.995 | 158.338 |
| spec-author | 2 | a35534a0f9a5b12ff | Spec author: record answers | large | medium | 1 | 19 | 38 | 15.576 | 3.018 | 2.026.653 | 82.413 | 57.617 |
| test-implementer | 1 | a66762088e966cb9e | Test implementer: casing fixes | large | medium | 1 | 90 | 180 | 96.098 | 23.309 | 20.371.664 | 286.352 | 261.679 |
| implementer | 1 | a101b83c652cc5af7 | Implementer: casing fixes | large | medium | 1 | 67 | 134 | 62.856 | 20.351 | 13.367.471 | 476.354 | 199.586 |
| test-implementer | 2 | a334589484a518e93 | Test implementer: disputed test | large | medium | 1 | 17 | 34 | 9.342 | 2.061 | 1.544.441 | 57.904 | 32.469 |
| cleaner | 1 | a4dfa0d9422b5e504 | Cleaner: casing fixes | medium | low | 1 | 7 | 14 | 1.789 | 0 | 520.504 | 90.723 | 15.244 |
| spec-checker | 1 | ac3e664f79c6729e6 | Spec checker: casing fixes | medium | medium | 1 | 9 | 18 | 5.358 | 869 | 780.883 | 111.257 | 36.185 |
| spec-author | 3 | af3dd312a81c19551 | Spec author: code-owner repair | large | medium | 1 | 9 | 18 | 3.429 | 325 | 750.975 | 42.253 | 16.954 |
| spec-checker | 2 | a0dfe2e30724451d8 | Spec checker: repair check | medium | medium | 1 | 4 | 8 | 1.649 | 0 | 297.445 | 37.485 | 12.865 |
| auditor | 1 | ae6e2de0d0f5f1ee2 | Auditor: casing fixes | large | high | 1 | 44 | 88 | 65.061 | 41.582 | 7.907.960 | 266.284 | 191.145 |
| test-implementer | 3 | ae2e9125bb2fa7b2b | Test implementer: audit repair | large | medium | 1 | 28 | 56 | 23.440 | 5.693 | 3.044.403 | 83.970 | 59.271 |
| implementer | 2 | a16c87cb874e165e6 | Implementer: audit repair | large | medium | 1 | 16 | 32 | 8.894 | 1.562 | 1.404.777 | 108.494 | 30.639 |
| cleaner | 2 | a5603ba1d350f35b4 | Cleaner: repair round 2 | medium | low | 1 | 5 | 10 | 1.220 | 64 | 319.700 | 82.034 | 6.537 |
| spec-checker | 3 | a0d5c82b7f382f0b1 | Spec checker: repair round 2 | medium | medium | 1 | 9 | 18 | 3.466 | 571 | 700.101 | 100.795 | 25.693 |
| auditor | 2 | aa6420133bd0761af | Auditor: repair round 2 | large | high | 1 | 24 | 48 | 27.828 | 13.776 | 2.921.499 | 165.367 | 90.193 |
| test-implementer | 4 | a86a22caa16f05b7c | Test implementer: finding 3 | large | medium | 1 | 15 | 30 | 13.811 | 3.702 | 1.405.505 | 118.826 | 43.167 |
| implementer | 3 | a17d6bcf78a86b21f | Implementer: structural repair | large | medium | 1 | 13 | 26 | 13.939 | 6.662 | 1.129.960 | 170.322 | 37.831 |
| cleaner | 3 | ab8baedd96973b53e | Cleaner: structural repair | medium | low | 1 | 5 | 10 | 2.158 | 44 | 327.160 | 86.032 | 10.450 |
| spec-checker | 4 | a7d8444e44bd6eddb | Spec checker: structural repair | medium | medium | 1 | 4 | 8 | 2.265 | 99 | 265.854 | 97.307 | 22.215 |
| auditor | 3 | a657d2f99a54cc9a5 | Auditor: structural repair | large | high | 1 | 29 | 58 | 41.612 | 23.897 | 3.811.013 | 179.290 | 104.100 |
| test-implementer | 5 | a7197bbbe666b3e19 | Test implementer: findings 5, 7 | large | medium | 1 | 14 | 28 | 14.327 | 1.940 | 1.329.151 | 120.511 | 44.799 |
| implementer | 4 | a45a804722a5507f4 | Implementer: round 3 | large | medium | 1 | 18 | 36 | 12.513 | 3.077 | 1.636.645 | 180.774 | 44.073 |
| cleaner | 4 | abc040208516dcde6 | Cleaner: round 3 | medium | low | 1 | 6 | 12 | 1.478 | 41 | 403.853 | 83.099 | 7.614 |
| spec-checker | 5 | a392873f67269a768 | Spec checker: round 3 | medium | medium | 1 | 6 | 12 | 3.712 | 478 | 457.160 | 104.662 | 29.542 |
| spec-author | 4 | ae7ab9ff2f458ab16 | Spec author: code-owner list | large | medium | 1 | 6 | 12 | 2.772 | 233 | 468.217 | 39.410 | 14.108 |
| spec-checker | 6 | a0b41bad83b866e0f | Spec checker: owner repair | medium | medium | 1 | 4 | 8 | 1.474 | 0 | 300.290 | 38.795 | 14.195 |
| auditor | 4 | ab9362833f36814c8 | Auditor: repair round 3 | large | high | 1 | 19 | 38 | 20.977 | 9.185 | 2.171.757 | 149.636 | 74.418 |
| validator | 1 | a9549340520c63a25 | Validator: final tree | small | low | 2 | 12 | 24 | 4.773 | 417 | 369.315 | 80.709 | 19.363 |
| issue-manager | 1 | ac5ad51ee9c77ccc5 | Issue manager: follow-ups | medium | medium | 1 | 8 | 20 | 3.818 | 643 | 670.119 | 111.729 | 36.679 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 638 | 1.321 | 631.303 | 242.147 | 105.791.445 | 3.166.097 |
| medium | 67 | 138 | 28.387 | 2.809 | 5.043.069 | 943.918 |
| small | 12 | 24 | 4.773 | 417 | 369.315 | 80.709 |
| **Total** | 717 | 1.483 | 664.463 | 245.373 | 111.203.829 | 4.190.724 |

## Decisions taken

Nine. Worth a look: decision 1 kept the four issues in one run although #1434 changes reporting while the others change name derivation (all share the casing component and spec); decisions 4 and 6 restructured RH4116 to decide by the compiler's declared symbols (including the record's generated property and `Deconstruct`) rather than refusing the class, because keeping the names reported would contradict SHARED-CASING-014.

## Suggestions

- An analyzer exemption keyed on "the member that owns this signature" should be decided from the semantic model from the first implementation (FC-044 already says so for fixes); the role files could state it for analyzer exemptions too. Evidence: three audits each found one more member kind the syntax-kind list missed.
- The round-2 implementer used `git stash` to compare against old code; no other role was running, but the rule forbids it. A worktree hint in the implementer role file would help.
