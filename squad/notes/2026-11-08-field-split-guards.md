# field-split-guards

| | |
|---|---|
| Date | 2026-11-08 |
| Run | issues #963, #1384 |
| Pull request | #1660 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | Own-line comment in front of the first declarator: split as today with the comment on the first field only (A), or keep the declaration whole (B)? | A | yes |
| 2 | output question | `///` or `/** */` behind the type on its line (Parse/Diagnose, owned by the first declarator): keep the declaration whole (A) or split as on main (B)? | B | yes |

## Cost

Two stops, one output question each: before the test implementer, and after the first audit. One repair round (audit finding #1, which needed question 2 because the literal-spec repair was not idempotent), then one structural change: the second audit's finding repeated #1's class (the spec listed comment kinds and positions while the code decides by trivia ownership), so FMT-STRUCT-030 was rewritten to state the boundary once in ownership terms instead of adding another list entry. The third audit passed with a wording-only finding, proven text-only. The test implementer ran four times (initial, a disputed Fix All markup configuration, repair round, structural change); its first spawn was the most expensive of the run (about 16 million cache-read tokens), followed by the first audit and the skeptic.

## Usage

Session `97d9a81b-01a7-58f5-89e4-4ec128033620`. Responses counted once per message id. Prices are not applied; compute a cost from the tokens and the model.

| Agent | Spawn | Agent ID | Description | Model | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | claude-opus-5-5 | medium | 26 | 83 | 206 | 38.235 | 4.449 | 13.679.485 | 272.995 |
| spec-author | 1 | a4980f70348d4854c | Spec author: field split guards | claude-opus-5-5 | medium | 2 | 32 | 65 | 37.905 | 16.401 | 3.948.648 | 325.657 |
| skeptic | 1 | a8ceface9ed9f42c4 | Skeptic: field split guards | claude-opus-5-5 | high | 1 | 43 | 88 | 44.215 | 24.116 | 6.296.855 | 202.969 |
| test-implementer | 1 | a3a8262eaa275b638 | Test implementer: field split guards | claude-opus-5-5 | medium | 1 | 78 | 156 | 78.215 | 26.568 | 15.932.578 | 307.082 |
| implementer | 1 | a14a0d2c41459025f | Implementer: field split guards | claude-opus-5-5 | medium | 1 | 26 | 52 | 18.626 | 4.084 | 3.056.873 | 94.464 |
| test-implementer | 2 | a9bab0eac38738912 | Test implementer: fix disputed test | claude-opus-5-5 | medium | 1 | 9 | 18 | 5.260 | 537 | 721.510 | 42.139 |
| cleaner | 1 | a1075e5be38869fe7 | Cleaner: field split guards | claude-sonnet-5-5 | low | 1 | 5 | 10 | 1.819 | 54 | 322.257 | 84.772 |
| spec-checker | 1 | a747d1ffe85bfffe3 | Spec checker: field split guards | claude-sonnet-5-5 | medium | 1 | 7 | 14 | 3.437 | 388 | 539.379 | 109.023 |
| auditor | 1 | a0674572a4eaa3c4d | Auditor: field split guards | claude-opus-5-5 | high | 1 | 43 | 88 | 51.575 | 32.227 | 7.455.125 | 247.877 |
| spec-author | 2 | a262a5a677b128eb2 | Spec author: repair round 1 | claude-opus-5-5 | medium | 1 | 19 | 38 | 12.915 | 4.283 | 1.830.111 | 120.569 |
| test-implementer | 3 | a3364ecede2c79f19 | Test implementer: repair round 1 | claude-opus-5-5 | medium | 1 | 21 | 42 | 18.935 | 3.823 | 2.477.357 | 97.371 |
| implementer | 2 | ac8308c5a744eda49 | Implementer: repair round 1 | claude-opus-5-5 | medium | 1 | 14 | 28 | 7.046 | 379 | 1.269.117 | 56.876 |
| cleaner | 2 | addcd752944b093c9 | Cleaner: repair round 1 | claude-sonnet-5-5 | low | 1 | 5 | 10 | 2.661 | 193 | 314.260 | 84.812 |
| spec-checker | 2 | a3cbfaa3b99813b13 | Spec checker: repair round 1 | claude-sonnet-5-5 | medium | 1 | 8 | 16 | 3.449 | 302 | 604.593 | 99.285 |
| auditor | 2 | a96a542b6efc11d7a | Auditor: repair round 1 | claude-opus-5-5 | high | 1 | 14 | 28 | 17.404 | 7.273 | 1.361.830 | 130.746 |
| spec-author | 3 | a6b91e9f19cb642f4 | Spec author: structural boundary | claude-opus-5-5 | medium | 1 | 25 | 50 | 20.459 | 7.491 | 2.649.011 | 129.498 |
| test-implementer | 4 | a9ddba0347fb10242 | Test implementer: structural boundary | claude-opus-5-5 | medium | 1 | 31 | 62 | 30.077 | 5.901 | 3.808.465 | 113.652 |
| cleaner | 3 | afe02488aae638351 | Cleaner: structural boundary comments | claude-sonnet-5-5 | low | 1 | 11 | 22 | 6.833 | 359 | 858.170 | 96.832 |
| spec-checker | 3 | af4c9b0b5e1ca11bf | Spec checker: structural boundary | claude-sonnet-5-5 | medium | 1 | 8 | 16 | 4.096 | 350 | 622.397 | 101.906 |
| auditor | 3 | a58ff0d562d917705 | Auditor: structural change | claude-opus-5-5 | high | 1 | 27 | 54 | 30.204 | 16.246 | 3.406.626 | 171.969 |
| spec-author | 4 | a60193c81190fde1f | Spec author: wording fix #3 | claude-opus-5-5 | medium | 1 | 11 | 22 | 7.125 | 1.866 | 923.585 | 102.127 |
| validator | 1 | a59c608de937d3e4f | Validator: field split guards | claude-haiku-5-5 | low | 2 | 7 | 14 | 3.587 | 433 | 389.620 | 161.534 |
| issue-manager | 1 | a2e9290a81231b8ad | Issue manager: follow-ups | claude-sonnet-5-5 | medium | 1 | 10 | 22 | 6.127 | 1.234 | 899.714 | 119.804 |

| Model | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| claude-haiku-5-5 | 7 | 14 | 3.587 | 433 | 389.620 | 161.534 |
| claude-opus-5-5 | 476 | 997 | 418.196 | 155.644 | 68.817.176 | 2.415.991 |
| claude-sonnet-5-5 | 54 | 110 | 28.422 | 2.880 | 4.160.770 | 696.434 |
| **Total** | 537 | 1.121 | 450.205 | 158.957 | 73.367.566 | 3.273.959 |

## Decisions taken

Sixteen. The branch used the session-mandated name instead of the playbook's pattern. The architecture finding about the RH7101 fix's private copy of the documentation-comment check was resolved in the run: the fix now asks the formatter's split predicate alone. The maintainer may want to see decision 14: the spec boundary for the field-split comment guard was restated as trivia ownership without asking, on the grounds that the outputs it newly names (for example `private int /** c */ /* d */ _first, _second;`, split) are today's main outputs and follow answer B.

## Suggestions

- When a spec boundary is about comments in front of a syntax node, the spec author should state it in Roslyn trivia-ownership terms from the start, with the derived list only as explanation. Listing comment kinds and positions missed members twice in this run (doc-style comment behind the type; ordinary comment behind such a comment), and each miss cost an audit.
- The test implementer's first pass used `{|RH7101:…|}` in a fixed source without enabling markup handling for the fixed states; a note in the analyzer test guidance on when `MarkupMode.Allow` is needed would have saved a spawn.
