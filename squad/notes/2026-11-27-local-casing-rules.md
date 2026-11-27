# local-casing-rules

| | |
|---|---|
| Date | 2026-11-27 |
| Run | issues #1087, #1088, #1978 |
| Pull request | #1987 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | Which rule reports variables that receive a whole element of a positional pattern (`(var a, int b)`, `(int n and > 0, _)`, `(string { … } s, _)`, `({ } x, _)`) | B: RH4117 for every element-receiving variable; RH4115 keeps property-pattern variables | yes |
| 2 | output question | Is a local constant named `_` reported by RH4128 | A: not reported | yes |
| 3 | output question | Casing fixes on a name that is also an anonymous-object member's whole expression | B: write the old member name explicitly (`new { Value = value }`) | yes |
| 4 | output question | Does answer 3 extend to the field, property, event, enum-member, and record-parameter casing fixes | A: yes, every fix using the shared rename | yes |

## Cost

- Stops: two output-question batches before the test implementer (batch 1 with three questions; a one-question follow-up when the spec author read answer 3's rule list as examples rather than a limit).
- Repair rounds: two. Round 1: one change-doc gap from the spec checker (answer 4A missing from *Decisions*). Round 2: two medium audit findings (missing conditional-access test cells; the anonymous-member helper restated the compiler's inferred-name rule, replaced by `SyntaxFacts.TryGetInferredMemberName`).
- Roles spawned more than once: spec author ×3 (amendment after answers, repair round 1), test implementer ×3 (two disputed tests whose test mechanics were wrong, repair round 2), implementer ×2, cleaner ×2, spec checker ×3, auditor ×2.
- Expensive: the first spec author, skeptic, test implementer, implementer, and auditor each grew past 150 thousand context tokens; the run spans four rules plus a shared fix change that touched every casing fix.

## Usage

Session `8560b05a-a108-55b3-9f8b-81ca7c406467`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 20 | 64 | 162 | 32.909 | 3.921 | 8.356.861 | 145.301 | 115.975 |
| spec-author | 1 | a0e2acf37daf60ecd | Spec author: local casing rules | large | medium | 1 | 72 | 144 | 75.188 | 30.962 | 8.290.958 | 177.757 | 160.216 |
| skeptic | 1 | a0a1d8cf432020d40 | Skeptic: local casing rules | large | high | 1 | 51 | 102 | 65.271 | 38.138 | 5.959.890 | 188.221 | 170.679 |
| spec-author | 2 | a9462d8a347390cce | Spec author amendment after answers | large | medium | 1 | 43 | 86 | 44.047 | 15.977 | 4.244.067 | 141.123 | 122.838 |
| test-implementer | 1 | af0d5b20639020b1c | Test implementer: local casing rules | large | medium | 1 | 54 | 108 | 71.907 | 19.924 | 7.237.619 | 201.932 | 184.360 |
| implementer | 1 | a62269fae8d4c6378 | Implementer: local casing rules | large | medium | 1 | 65 | 130 | 56.168 | 18.104 | 8.345.362 | 188.190 | 170.718 |
| test-implementer | 2 | a7a85f57a80f317b1 | Test implementer: fix disputed tests | large | medium | 1 | 18 | 36 | 10.416 | 2.021 | 957.615 | 65.692 | 48.116 |
| cleaner | 1 | a0b53be654f71926b | Cleaner: local casing rules | medium | low | 1 | 6 | 12 | 1.783 | 0 | 177.865 | 39.653 | 15.828 |
| spec-checker | 1 | ab0a31f79360f51b0 | Spec checker: local casing rules | medium | medium | 1 | 13 | 26 | 7.764 | 1.531 | 665.345 | 70.620 | 47.226 |
| spec-author | 3 | a00a25a08e386fc08 | Spec author repair: record 4A | large | medium | 1 | 7 | 14 | 2.690 | 195 | 223.510 | 36.385 | 18.985 |
| spec-checker | 2 | a492b4d6064421da3 | Spec checker re-check | medium | medium | 1 | 4 | 8 | 1.711 | 0 | 95.121 | 28.245 | 10.840 |
| auditor | 1 | ae9718fac3b7af9b5 | Auditor: local casing rules | large | high | 1 | 41 | 82 | 59.115 | 42.005 | 5.996.429 | 231.936 | 207.927 |
| test-implementer | 3 | a3922b93cd14259f2 | Test implementer: repair round 2 | large | medium | 1 | 13 | 26 | 7.730 | 802 | 529.755 | 54.836 | 30.417 |
| implementer | 2 | ada4dff0b008f028d | Implementer: repair round 2 | large | medium | 1 | 12 | 24 | 7.214 | 1.604 | 431.994 | 42.759 | 24.672 |
| cleaner | 2 | a4c3f946c116514e8 | Cleaner: repair round 2 | medium | low | 1 | 5 | 10 | 1.520 | 0 | 110.766 | 29.707 | 5.875 |
| spec-checker | 3 | a0ac1e5fa344ec5a8 | Spec checker: repair round 2 | medium | medium | 1 | 5 | 10 | 1.892 | 110 | 123.835 | 36.204 | 12.791 |
| auditor | 2 | a786ab4de7635cf22 | Auditor: repair round 2 | large | high | 1 | 11 | 22 | 14.167 | 6.457 | 490.771 | 70.169 | 46.147 |
| validator | 1 | a296cd7484b2ef9c8 | Validator: local casing rules | small | low | 1 | 9 | 18 | 4.204 | 640 | 233.914 | 78.734 | 17.147 |
| issue-manager | 1 | a28db6b2838526677 | Issue manager: follow-ups | medium | medium | 1 | 7 | 14 | 3.114 | 247 | 268.062 | 55.692 | 28.357 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 451 | 936 | 446.822 | 180.110 | 51.064.831 | 1.544.301 |
| medium | 40 | 80 | 17.784 | 1.888 | 1.440.994 | 260.121 |
| small | 9 | 18 | 4.204 | 640 | 233.914 | 78.734 |
| **Total** | 500 | 1.034 | 468.810 | 182.638 | 52.739.739 | 1.883.156 |

## Decisions taken

Ten. Worth the maintainer's eye: rule ID RH4128 instead of the retired RH4119 (decision 1); taking the anonymous-member rename defect, which exists on `main` for every casing fix, into the run (decision 3); three new internal helper types counted as the issues' own fix rather than escalation 9 (decision 6).

## Suggestions

- The spec author read a maintainer answer's list of rules as examples and extended it; the playbook's "a case the answer does not name is a new question" caught it, but only after a spec round. An output question that names a list of rules could state explicitly whether the list is exhaustive.
