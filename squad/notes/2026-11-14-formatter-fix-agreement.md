# formatter-fix-agreement

| | |
|---|---|
| Date | 2026-11-14 |
| Run | issues #1529, #1543 |
| Pull request | #1697 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question (with escalation 10 as option E) | RH5111 fix vs. whole-document formatting for a wrapped member behind a conditional access: which layout? | B — the fix adopts whole-document formatting's layout; shape not declared unsupported | yes |
| 2 | output question | Blank line between an own-line comment and the brace or chained keyword behind it | A — the formatter removes it for every brace and `else`/`catch`/`finally`/do-`while` | yes |

## Cost

- One stop: a single output-question batch before the test implementer.
- Two repair rounds. Round 1: the change orphaned a node-level formatter fact (the conditional-access-branch flag); deleting it surfaced a pre-existing test that fed that root directly, deleted under *Supported inputs*. Round 2: a comment-only fix of a stale summary, made a round only because `verify-text-only.sh --strict-docs` counts a public property of an internal record as public API documentation.
- Spec author spawned three times (initial, after answers, repair) — each fresh, by the continuation table.
- Most expensive spawns: test implementer (78 responses, 14.4M cache read), auditor 1, skeptic.
- The issue texts were partly stale (#1529's RH6023 half already moot; #1543's own input now refused by FMT-UNSUP-004, one brace already handled); the spec author and skeptic re-derived the cases, which made the questions accurate.

## Usage

Session `5afd75f1-1ecf-536f-864c-6d522067f980`. Responses counted once per message id. Prices are not applied; compute a cost from the tokens and the model.

| Agent | Spawn | Agent ID | Description | Model | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | claude-opus-5-5 | medium | 21 | 65 | 157 | 28.029 | 2.497 | 9.963.892 | 136.548 |
| spec-author | 1 | a0bdf51b10e4158ab | Spec author for #1529 #1543 | claude-opus-5-5 | medium | 1 | 41 | 84 | 33.752 | 13.203 | 5.431.160 | 180.266 |
| skeptic | 1 | ae6546ba60ea1bf4d | Skeptic for #1529 #1543 | claude-opus-5-5 | high | 1 | 54 | 110 | 51.405 | 27.760 | 7.789.971 | 204.486 |
| spec-author | 2 | ad0bc751affe9e08d | Spec author amendment after answers | claude-opus-5-5 | medium | 1 | 44 | 88 | 29.413 | 6.826 | 5.550.845 | 164.210 |
| test-implementer | 1 | a897df6edd5f65c72 | Test implementer #1529 #1543 | claude-opus-5-5 | medium | 1 | 78 | 156 | 61.162 | 17.687 | 14.429.992 | 210.984 |
| implementer | 1 | a0487a9e4cb47389d | Implementer #1529 #1543 | claude-opus-5-5 | medium | 1 | 26 | 52 | 18.532 | 4.035 | 3.055.112 | 100.548 |
| cleaner | 1 | abc0dd76b221a1e3b | Cleaner #1529 #1543 | claude-sonnet-5-5 | low | 1 | 6 | 12 | 2.104 | 72 | 402.260 | 83.949 |
| spec-checker | 1 | a4591398369804864 | Spec checker #1529 #1543 | claude-sonnet-5-5 | medium | 1 | 11 | 22 | 4.832 | 504 | 938.852 | 106.061 |
| auditor | 1 | a8b93a4963eb16128 | Auditor #1529 #1543 | claude-opus-5-5 | high | 1 | 55 | 110 | 64.195 | 40.163 | 10.066.032 | 256.792 |
| spec-author | 3 | a90e180cf1b17e090 | Spec author repair round 1 | claude-opus-5-5 | medium | 1 | 14 | 28 | 6.949 | 1.772 | 1.302.893 | 117.813 |
| implementer | 2 | a4cd2b13fda645d30 | Implementer repair round 1 | claude-opus-5-5 | medium | 1 | 14 | 28 | 8.176 | 1.210 | 1.383.810 | 68.060 |
| test-implementer | 2 | a80787edb05c2d7b4 | Test implementer repair round 1 | claude-opus-5-5 | medium | 1 | 14 | 28 | 6.623 | 721 | 1.288.076 | 56.154 |
| cleaner | 2 | a9873b56cefc5704f | Cleaner repair round 1 | claude-sonnet-5-5 | low | 1 | 4 | 8 | 1.636 | 61 | 236.176 | 82.687 |
| spec-checker | 2 | a1167337be8fb22c6 | Spec checker repair round 1 | claude-sonnet-5-5 | medium | 1 | 6 | 12 | 2.179 | 75 | 451.990 | 100.086 |
| auditor | 2 | ac493728c69099d54 | Auditor repair round 1 | claude-opus-5-5 | high | 1 | 21 | 42 | 15.539 | 6.258 | 2.330.768 | 148.921 |
| cleaner | 3 | ac25629dab0577892 | Cleaner: comment fix after audit | claude-sonnet-5-5 | low | 1 | 7 | 14 | 3.461 | 293 | 479.956 | 84.728 |
| spec-checker | 3 | abb7317a0399f7f8f | Spec checker repair round 2 | claude-sonnet-5-5 | medium | 1 | 5 | 10 | 2.184 | 110 | 384.977 | 38.789 |
| auditor | 3 | ac1a28f62adfb1351 | Auditor repair round 2 | claude-opus-5-5 | high | 1 | 9 | 18 | 5.619 | 1.010 | 746.933 | 47.875 |
| validator | 1 | a3dd1a2de1faac553 | Validator final tree | claude-haiku-5-5 | low | 1 | 6 | 12 | 2.732 | 367 | 321.648 | 167.785 |
| issue-manager | 1 | a8872575b1e2a7199 | Issue manager follow-ups | claude-sonnet-5-5 | medium | 1 | 6 | 14 | 2.453 | 83 | 433.936 | 97.077 |

| Model | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| claude-haiku-5-5 | 6 | 12 | 2.732 | 367 | 321.648 | 167.785 |
| claude-opus-5-5 | 435 | 901 | 329.394 | 123.142 | 63.339.484 | 1.692.657 |
| claude-sonnet-5-5 | 45 | 92 | 18.849 | 1.198 | 3.328.147 | 593.377 |
| **Total** | 486 | 1.005 | 350.975 | 124.707 | 66.989.279 | 2.453.819 |

## Decisions taken

Eight. Worth the maintainer's eye: #4 (the earlier answer in the merged `node-root-detached.md` is superseded in the new change doc only, the merged doc untouched) and #7 (a pre-existing formatter test of a now-unreachable root deleted).

## Suggestions

- `verify-text-only.sh --strict-docs` treats public members of internal types as public API documentation. Since `AGENTS.md` says no assembly is a public API, the strict mode turned a doc-comment fix into a repair round (spec checker + auditor) with no evidence gained; consider limiting "public API documentation" to types that are themselves public, or dropping `--strict-docs` from `lead.md` section 7.
- The auditor's "no test sets the fact" check searched for the type name only; a test reaching a fact through an entry point was missed until the implementer ran the suite. Dead-code findings could ask the auditor to name the test run that proves no test depends on it.
