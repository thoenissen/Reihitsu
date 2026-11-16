# union-with-analyzer-rules

| | |
|---|---|
| Date | 2026-11-16 |
| Run | issue #1767 |
| Pull request | #1787 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | Do unions join the existing type-kind rules (RH2004, RH5404, RH51xx list rules, RH71xx ordering, RH73xx/RH74xx regions)? | A — yes, like a struct | yes |
| 2 | output question | Do RH5507/RH5508 report a union's attribute lists (struct target)? | A — yes | yes |
| 3 | output question, escalation 5 | New rule RH5418 "Empty unions should use semicolon declarations"? | A — yes, with fix | yes |
| 4 | output question, escalation 5 | New rule RH2110 "Nested unions should not be used"? | A — yes, no fix | yes |

The maintainer added that unions are treated like other types such as class or struct.

## Cost

- One stop for the output-question batch (Q1–Q4, answered together).
- Two repair rounds: the spec checker found the change doc still marked Draft (round 1); the first audit found the adjacent crash fix in the attribute-placement code fixes tested only for unions, and an RH5106 rule page gap (round 2).
- The spec author ran six times: the first spec, a skeptic-caused amendment, the specs for the two new rules the answers created, three small wording or status repairs. The skeptic ran twice, the second time on the new-rule specs only.
- The most expensive spawns were the first spec author, the first test implementer (86 tests over ~25 rules), the first skeptic, and the first audit.

## Usage

## Usage

Session `eae177c8-7efe-5a37-b009-6be6c6737890`. Responses counted once per message id. Prices are not applied; compute a cost from the tokens and the model.

| Agent | Spawn | Agent ID | Description | Model | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | claude-opus-5-5 | medium | 17 | 67 | 170 | 33.088 | 4.293 | 10.871.429 | 153.127 |
| spec-author | 1 | a9db2bc927a1f36e2 | Spec author for #1767 | claude-opus-5-5 | medium | 2 | 91 | 175 | 69.007 | 24.720 | 17.955.655 | 284.194 |
| skeptic | 1 | a5978bf14724dda03 | Skeptic for #1767 | claude-opus-5-5 | high | 1 | 65 | 132 | 53.775 | 26.847 | 11.231.639 | 238.263 |
| spec-author | 2 | aa8bbcf6378ada887 | Spec amendment for #1767 | claude-opus-5-5 | medium | 1 | 14 | 28 | 7.487 | 1.282 | 1.334.065 | 121.075 |
| spec-author | 3 | a057aa64d4b41981f | Specs for maintainer answers #1767 | claude-opus-5-5 | medium | 1 | 30 | 60 | 22.183 | 6.102 | 3.339.602 | 141.290 |
| skeptic | 2 | a5d6d5b98f89187f7 | Skeptic on new rule specs | claude-opus-5-5 | high | 1 | 53 | 106 | 38.782 | 16.519 | 7.816.167 | 194.786 |
| spec-author | 4 | a12e5dec1c8339662 | Spec wording edits #1767 | claude-opus-5-5 | medium | 1 | 8 | 16 | 3.730 | 327 | 617.628 | 96.447 |
| test-implementer | 1 | ac14aeb30db403a4e | Test implementer for #1767 | claude-opus-5-5 | medium | 1 | 83 | 166 | 76.203 | 25.217 | 17.751.664 | 259.801 |
| implementer | 1 | a40b2129b0610b3cd | Implementer for #1767 | claude-opus-5-5 | medium | 1 | 48 | 96 | 24.860 | 3.862 | 6.607.715 | 126.138 |
| cleaner | 1 | aa1c2f38dae0d5262 | Cleaner for #1767 | claude-sonnet-5-5 | low | 1 | 5 | 10 | 1.335 | 130 | 312.550 | 80.557 |
| spec-checker | 1 | a2901adcc635d68ce | Spec checker for #1767 | claude-sonnet-5-5 | medium | 1 | 11 | 22 | 5.539 | 1.127 | 1.016.163 | 121.296 |
| spec-author | 5 | a859804ffa40148ac | Repair: change doc status | claude-opus-5-5 | medium | 1 | 6 | 12 | 1.891 | 39 | 424.390 | 88.891 |
| spec-checker | 2 | a4236f491c89dee8f | Spec checker repair re-check | claude-sonnet-5-5 | medium | 1 | 3 | 6 | 1.214 | 0 | 160.737 | 85.993 |
| auditor | 1 | ab57cad88f4d83f75 | Auditor for #1767 | claude-opus-5-5 | high | 1 | 67 | 134 | 45.693 | 21.192 | 13.238.061 | 275.961 |
| test-implementer | 2 | abcff13c205bf77ec | Repair: crash-fix tests | claude-opus-5-5 | medium | 2 | 12 | 24 | 10.863 | 1.744 | 1.099.161 | 118.044 |
| spec-author | 6 | a0feef73fc558a724 | Repair: RH5106 rule page | claude-opus-5-5 | medium | 1 | 6 | 12 | 1.869 | 107 | 420.516 | 91.261 |
| cleaner | 2 | abc34033dadd235d3 | Cleaner after repair 2 | claude-sonnet-5-5 | low | 1 | 4 | 8 | 1.089 | 45 | 236.820 | 80.752 |
| spec-checker | 3 | a192c4bada6fb137c | Spec checker after repair 2 | claude-sonnet-5-5 | medium | 1 | 7 | 14 | 2.763 | 212 | 522.869 | 96.237 |
| auditor | 2 | a327b5a03aea38da4 | Auditor on repair 2 | claude-opus-5-5 | high | 1 | 12 | 23 | 8.729 | 2.258 | 1.110.527 | 116.763 |
| validator | 1 | a2d09d59fefc89a38 | Validator for #1767 | claude-haiku-5-5 | low | 1 | 6 | 12 | 2.755 | 301 | 320.433 | 167.303 |
| issue-manager | 1 | ab3f4288a51f573e3 | Issue manager for #1767 | claude-sonnet-5-5 | medium | 1 | 10 | 21 | 6.895 | 847 | 892.336 | 119.086 |

| Model | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| claude-haiku-5-5 | 6 | 12 | 2.755 | 301 | 320.433 | 167.303 |
| claude-opus-5-5 | 562 | 1.154 | 398.160 | 134.509 | 93.818.219 | 2.306.041 |
| claude-sonnet-5-5 | 40 | 81 | 18.835 | 2.361 | 3.141.475 | 583.921 |
| **Total** | 608 | 1.247 | 419.750 | 137.171 | 97.280.127 | 3.057.265 |

## Decisions taken

19 decisions. Worth the maintainer's eye:

- The adjacent fix in `TargetAttributePlacementCodeFixProviderBase.HasUnusableEnclosingScope` ends a `main` crash of the RH5505, RH5507, and RH5521 fixes for any semicolon-declared type after other code, not only for unions.
- The empty-type rules' reporting of a conditional directive in the body was left as is and recorded on #1759.
- Branch name followed the session's mandated branch instead of `claude/issue-<N>-<slug>`.

## Suggestions

- The spec author's first draft named a union `Value`, which does not compile because every union has a `Value` property; the role file could ask that every spec example be compiled once, as the skeptic did.
- The lead's "accept the change doc" step does not say who flips its `Status` field; it cost a repair round here.
