# unsupported-link-operator-trivia

| | |
|---|---|
| Date | 2026-11-20 |
| Run | issue #894 |
| Pull request | #1881 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 0 | 10 | Declare a comment/directive/disabled text inside `?.`, `!.`, `!?.` unsupported instead of a new rule (answered in the issue's triage comment before the run) | Declare, in this change | yes |
| 1 | output question | CLI reason text for the new shape | A — `a comment or directive inside a ?. or !. operator` | yes |
| 2 | 10 | Declare a comment between a link's `.` and its member name unsupported | A — in a change of its own (filed as #1888) | yes |

## Cost

One output-question batch (before the test implementer), no other stops. One repair round: the first audit found a missing test for the FMT-UNSUP-004/FMT-UNSUP-015 overlap (the test implementer had wrongly concluded no input exists, because `nameof(s?.Length)` in an attribute is only a semantic error) and a comment wording point. The spec author ran twice (second: status-only acceptance); the implementer was continued once for an adjacent fix. After publishing, CI failed in the self-hosting test: RH2008 (enabled on main shortly before the run) reported the precheck's `FindUnsupportedSyntax` at 66 lines. A CI repair (round 2: implementer, cleaner, auditor, validator) extracted the declaration candidates into a helper; a container restart in the middle cost one implementer re-run. The expensive spawns were the test implementer (205-case matrix), the first audit, and the spec author.

## Usage

Session `a363fcb8-a042-5cef-b077-a327933b6b8c`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 16 | 67 | 165 | 26.124 | 2.427 | 11.371.406 | 145.701 | 117.187 |
| spec-author | 1 | a6321ea29bf0f8a35 | Spec author for #894 | large | medium | 1 | 62 | 120 | 54.213 | 23.567 | 11.353.774 | 250.818 | 172.507 |
| skeptic | 1 | a36b22514a4c53c6b | Skeptic for #894 | large | high | 1 | 38 | 78 | 51.196 | 30.490 | 6.107.859 | 175.343 | 151.574 |
| test-implementer | 1 | a5d0e0abb683c4f99 | Test implementer for #894 | large | medium | 1 | 78 | 156 | 86.065 | 23.816 | 16.240.914 | 254.740 | 230.247 |
| implementer | 1 | ab9dc6a0e41f4162b | Implementer for #894 | large | medium | 2 | 36 | 74 | 30.734 | 9.930 | 5.157.363 | 126.610 | 102.103 |
| spec-author | 2 | a0a6129ffb4f07c59 | Spec author status-only for #894 | large | medium | 1 | 4 | 8 | 1.928 | 127 | 314.718 | 38.444 | 13.749 |
| cleaner | 1 | a5fa10ca9e46a7527 | Cleaner for #894 | medium | low | 1 | 6 | 12 | 1.481 | 12 | 415.028 | 85.977 | 11.296 |
| spec-checker | 1 | a2b61b09557886d4a | Spec checker for #894 | medium | medium | 1 | 7 | 14 | 4.733 | 1.249 | 593.852 | 117.599 | 43.351 |
| auditor | 1 | ad6a95f900ffc6ee4 | Auditor for #894 | large | high | 1 | 56 | 112 | 55.969 | 34.857 | 10.700.223 | 271.021 | 196.780 |
| test-implementer | 2 | aba28504f13fd3b95 | Test repair for #894 | large | medium | 1 | 17 | 32 | 14.689 | 4.116 | 1.916.799 | 139.800 | 61.345 |
| cleaner | 2 | a3fc7fab68007adbf | Cleaner repair for #894 | medium | low | 1 | 5 | 10 | 2.270 | 92 | 340.458 | 90.541 | 15.756 |
| spec-checker | 2 | a53a482099cc26644 | Spec checker repair for #894 | medium | medium | 1 | 5 | 10 | 1.811 | 66 | 318.746 | 88.432 | 14.148 |
| auditor | 2 | afbb08f62bba886f7 | Auditor repair for #894 | large | high | 1 | 12 | 24 | 7.840 | 2.202 | 1.072.172 | 110.144 | 35.814 |
| issue-manager | 1 | aa72fafff2e951808 | Issue manager for #894 | medium | medium | 1 | 6 | 14 | 3.090 | 281 | 481.143 | 48.547 | 24.073 |
| validator | 1 | aeb7d3f1e6ad644b4 | Validator for #894 | small | low | 2 | 8 | 16 | 2.680 | 195 | 167.190 | 60.158 | 9.235 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 370 | 769 | 328.758 | 131.532 | 64.235.228 | 1.512.621 |
| medium | 29 | 60 | 13.385 | 1.700 | 2.149.227 | 431.096 |
| small | 8 | 16 | 2.680 | 195 | 167.190 | 60.158 |
| **Total** | 407 | 845 | 344.823 | 133.427 | 66.551.645 | 2.003.875 |

## Decisions taken

Ten. Worth a look: decision 8 (a new public helper `FluentChain.GetLinkOperatorTokens` in Core as the one definition of link-operator tokens) and decision 10 (adjacent fix: `AttachedPartGapUtilities.StartsLinkOperator` now reads that helper instead of its own copy).

## Suggestions

- The default test set does not run the self-hosting analyzers, so a rule newly enabled on main (here RH2008) surfaced only in CI after publishing. The validator could run the self-hosting `AnalyzerReportsNoViolationsOnOwnSourceCode` test on the changed files before publishing.

- The skeptic flagged the FMT-UNSUP-004 overlap sentence as possibly vacuous and the test implementer accepted that without a probe; the spec checker then found the attribute-argument input. When a spec sentence's reachability is doubted, the test implementer should construct the input from the span definition (attributes are inside a field's declaration span) before declaring it unreachable.
