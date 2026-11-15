# casing-rename-call-sites

| | |
|---|---|
| Date | 2026-11-15 |
| Run | issue #1676 |
| Pull request | #1751 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | RH4103 (all casing rules): withhold the fix when the rename would capture an existing reference to another symbol (e.g. `values.ToList()` → `Enumerable.ToList(values)`), or keep rewriting? | A — withhold | Yes |

## Cost

- One stop: the output-question batch before the test implementer.
- Three repair rounds (the maximum) plus three architecture-finding rounds that did not count as repair rounds. Five of the eight audit findings were the same class — whether a rename changes which member implements an interface member — found member by member: own type (F2), derived type listing the interface (F4), other cascade members (F5), then the refusal's own kind filter catching constructors (F7) and local functions (F6). Each round cost a spec author, test implementer, implementer, cleaner, spec checker, and auditor spawn.
- The class only ended once the check became a name-based refusal over the whole cascade with an allow-list of kinds. Starting from that shape at decision 7, instead of counting interface mappings on one type, would likely have saved three rounds.
- The validator's first full run was killed by the sandbox's out-of-memory killer during the analyzer tests; a second validator ran the projects one at a time and passed.

## Usage

Session `da3cd0c0-38e5-5273-9a8c-dac1cb5c4d5f`. Responses counted once per message id. Prices are not applied; compute a cost from the tokens and the model.

| Agent | Spawn | Agent ID | Description | Model | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | claude-opus-5-5 | medium | 21 | 95 | 246 | 49.981 | 5.110 | 17.195.738 | 194.897 |
| skeptic | 1 | a68d2ec038bb2b8ac | Skeptic premise check #1676 | claude-opus-5-5 | high | 1 | 27 | 53 | 38.489 | 19.019 | 3.783.619 | 134.727 |
| spec-author | 1 | a92ca9a1bdd3b58dd | Spec clarification #1676 | claude-opus-5-5 | medium | 1 | 14 | 27 | 10.039 | 2.277 | 1.465.366 | 66.047 |
| test-implementer | 1 | a5a303d24c0a27ac4 | Tests for #1676 | claude-opus-5-5 | medium | 1 | 43 | 82 | 48.550 | 13.068 | 6.355.983 | 156.232 |
| implementer | 1 | a967b46a258d197f4 | Implement fix for #1676 | claude-opus-5-5 | medium | 1 | 19 | 37 | 21.048 | 10.036 | 2.096.680 | 219.243 |
| cleaner | 1 | ad660b92e06fe5a89 | Clean up for #1676 | claude-sonnet-5-5 | low | 1 | 4 | 8 | 1.228 | 82 | 249.509 | 85.831 |
| spec-checker | 1 | aff3f013331d1ed74 | Spec check for #1676 | claude-sonnet-5-5 | medium | 1 | 6 | 12 | 3.026 | 343 | 453.998 | 100.491 |
| auditor | 1 | a27c85cc1b4939149 | Audit for #1676 | claude-opus-5-5 | high | 1 | 29 | 55 | 55.401 | 38.546 | 4.388.862 | 201.253 |
| spec-author | 2 | a91a735c77587796c | Spec repair round 1 #1676 | claude-opus-5-5 | medium | 1 | 20 | 38 | 14.953 | 5.327 | 2.132.625 | 129.461 |
| test-implementer | 2 | ab44bb31ee0f5e3e8 | Tests repair round 1 #1676 | claude-opus-5-5 | medium | 1 | 29 | 55 | 23.057 | 4.509 | 3.546.608 | 90.878 |
| implementer | 2 | a9063d6058e094b74 | Implement repair round 1 #1676 | claude-opus-5-5 | medium | 1 | 15 | 28 | 14.491 | 5.019 | 1.623.341 | 70.816 |
| cleaner | 2 | a3c19f83740eb07b4 | Clean repair round 1 #1676 | claude-sonnet-5-5 | low | 1 | 6 | 12 | 1.593 | 63 | 421.036 | 86.710 |
| spec-checker | 2 | acd9c31f4136dcb1a | Spec check round 1 #1676 | claude-sonnet-5-5 | medium | 1 | 5 | 10 | 2.076 | 88 | 357.758 | 98.387 |
| auditor | 2 | a0862e582f8563d21 | Audit repair round 1 #1676 | claude-opus-5-5 | high | 1 | 20 | 38 | 23.742 | 12.680 | 2.439.866 | 159.017 |
| spec-author | 3 | a250adcfd5818df4d | Spec decision 10 #1676 | claude-opus-5-5 | medium | 1 | 9 | 18 | 10.524 | 4.041 | 785.049 | 112.052 |
| test-implementer | 3 | a35c916d587c69b28 | Tests decision 10 #1676 | claude-opus-5-5 | medium | 1 | 19 | 36 | 17.036 | 4.116 | 2.019.367 | 74.878 |
| implementer | 3 | a2be98a22afdece27 | Implement decision 10 #1676 | claude-opus-5-5 | medium | 1 | 15 | 29 | 11.004 | 2.932 | 1.308.010 | 166.772 |
| cleaner | 3 | a5897478975d24ca0 | Clean decision 10 #1676 | claude-sonnet-5-5 | low | 1 | 6 | 12 | 1.806 | 180 | 425.563 | 87.908 |
| spec-checker | 3 | ac951b2a6ce43edea | Spec check decision 10 #1676 | claude-sonnet-5-5 | medium | 1 | 7 | 14 | 2.659 | 194 | 561.107 | 102.684 |
| auditor | 3 | ac82cafdb78199950 | Audit decision 10 repair #1676 | claude-opus-5-5 | high | 1 | 28 | 54 | 26.644 | 12.956 | 3.373.899 | 154.554 |
| spec-author | 4 | a7fc4bd6bad4991ef | Spec decisions 12-13 #1676 | claude-opus-5-5 | medium | 1 | 13 | 25 | 8.508 | 1.673 | 1.204.816 | 112.697 |
| test-implementer | 4 | a0ee20fb174ddba3b | Tests decisions 12-13 #1676 | claude-opus-5-5 | medium | 1 | 17 | 32 | 14.496 | 2.553 | 1.759.462 | 67.548 |
| implementer | 4 | abcaebeec8bfd2c0f | Implement decisions 12-13 #1676 | claude-opus-5-5 | medium | 1 | 13 | 25 | 8.884 | 530 | 1.214.415 | 174.015 |
| cleaner | 4 | a490dfc921e328a9a | Clean repair round 2 #1676 | claude-sonnet-5-5 | low | 1 | 5 | 10 | 1.531 | 58 | 338.684 | 87.287 |
| spec-checker | 4 | a0fbe117f297d200a | Spec check round 2 #1676 | claude-sonnet-5-5 | medium | 1 | 6 | 12 | 2.721 | 205 | 454.676 | 103.345 |
| auditor | 4 | aefad001be8eb5e6c | Audit repair round 2 #1676 | claude-opus-5-5 | high | 1 | 21 | 40 | 22.912 | 11.783 | 2.409.387 | 145.856 |
| spec-author | 5 | afa74326ff4497bfc | Spec decisions 14-15 #1676 | claude-opus-5-5 | medium | 1 | 9 | 18 | 7.947 | 2.406 | 825.170 | 51.992 |
| test-implementer | 5 | a157e8e03fa9cf262 | Tests decisions 14-15 #1676 | claude-opus-5-5 | medium | 1 | 15 | 29 | 9.224 | 331 | 1.421.192 | 55.927 |
| implementer | 5 | a0d7be47bc9634f2e | Implement decisions 14-15 #1676 | claude-opus-5-5 | medium | 1 | 14 | 27 | 6.838 | 312 | 1.287.469 | 46.678 |
| cleaner | 5 | a2494cc62867bdbe9 | Clean repair round 3 #1676 | claude-sonnet-5-5 | low | 1 | 6 | 12 | 1.668 | 48 | 476.087 | 29.995 |
| spec-checker | 5 | a08427b80197189ae | Spec check round 3 #1676 | claude-sonnet-5-5 | medium | 1 | 6 | 12 | 2.319 | 252 | 484.076 | 110.145 |
| auditor | 5 | a0d43548bfd5aba6b | Audit repair round 3 #1676 | claude-opus-5-5 | high | 1 | 12 | 23 | 17.537 | 9.714 | 1.209.445 | 133.264 |
| spec-author | 6 | ad8c53b11ff5464a7 | Spec wording fix #1676 | claude-opus-5-5 | medium | 1 | 5 | 10 | 2.723 | 438 | 351.899 | 93.926 |
| validator | 1 | a5b67af89c0cc8c92 | Validate final tree #1676 | claude-haiku-5-5 | low | 1 | 10 | 20 | 5.856 | 1.379 | 831.504 | 100.281 |
| validator | 2 | a177fc07f69841315 | Validate per project #1676 | claude-haiku-5-5 | low | 1 | 8 | 18 | 4.046 | 337 | 718.153 | 43.644 |
| issue-manager | 1 | a1e53583e3a988112 | File follow-ups #1676 | claude-sonnet-5-5 | medium | 1 | 7 | 16 | 3.486 | 624 | 584.631 | 110.031 |

| Model | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| claude-haiku-5-5 | 18 | 38 | 9.902 | 1.716 | 1.549.657 | 143.925 |
| claude-opus-5-5 | 501 | 1.025 | 464.028 | 169.376 | 64.198.268 | 2.812.730 |
| claude-sonnet-5-5 | 64 | 130 | 24.113 | 2.137 | 4.807.125 | 1.002.814 |
| **Total** | 583 | 1.193 | 498.043 | 173.229 | 70.555.050 | 3.959.469 |

## Decisions taken

17. Ones the maintainer may want to see: decision 2 (pulled the silent rebinding through inherited-member hiding into the run, which `main` also gets wrong); decisions 10, 12 and 14 (the interface refusal is by name, wider than strictly necessary); decision 8 (two fixes `main` offered — `values.toList()` → `Extensions.ToList(values)` and `box.write(1)` → `box.Write((object)1)` — are now withheld); decisions 9 and 11 (production diff +155/−6 in one file, about 40 lines of it XML docs, judged under escalation 9's threshold).

## Suggestions

- When a finding concerns a semantic mapping the compiler computes from the whole hierarchy (interface implementation, overload resolution), refuse by a name-based check over the whole rename cascade from the first finding, with an allow-list of symbol kinds rather than a deny-list. Evidence: F2 → F4 → F5 → F6/F7 in this run.
- The validator role could run test projects one at a time by default in the cloud sandbox; the all-projects run hit the memory limit here.
