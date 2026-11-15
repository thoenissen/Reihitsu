# region-member-rules

| | |
|---|---|
| Date | 2026-11-15 |
| Run | issues #1264, #1333, #1334, #1339, #1337, #1412, #1592 |
| Pull request | #1734 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | RH7309: regions in the `#if` and `#else` branches of one conditional — compared? | A: not reported | yes |
| 1b | output question | RH7309: region in one branch after a region outside every branch | A: reported without fix when the symbol is defined; nothing otherwise (asked back once: first reply skipped it) | yes |
| 2 | output question | RH7409–RH7412: partial member with only some parts in a matching region | A: reported once | yes |
| 3 | output question | RH7105/RH7106: conditional directive behind the last modifier | B: stays reported without fix (judge the active configuration) | no |
| 4 | output question | RH5111: conditional directive inside the value | A: reported, no fix (unchanged) | yes |
| 5 | output question | RH7102/RH7109: fix offered inside the own region when the group's first field is in another region | A: offered | yes |

## Cost

- Stops: one output-question batch (Q1–Q4) plus one ask-back for 1b; one question raised by the first audit (Q5).
- Repair rounds: 1 (three PR-introduced findings: RH7107/RH7108 expression-body `;`, duplicated RH7105/RH7106 span, missing first-modifier boundary test).
- Extra spawns: spec author ×3, test implementer ×4, implementer ×3 — the second spec/test/implementer trio came from new spec examples (RH7102/RH7109) that contradicted unchanged SHARED-ORDER-004, found only by the implementer; the fourth test-implementer spawn resolved a merge conflict.
- One auditor spawn was wasted on a state mismatch: the lead passed a `main` SHA read before the repair, and `main` had moved in between.
- Most expensive: test implementer 1 (106 responses), skeptic (92), implementer 1 (90) — seven issues over ~20 rules.

## Usage

Session `50810c94-4b52-5345-b47f-2a3e174013c3`. Responses counted once per message id. Prices are not applied; compute a cost from the tokens and the model.

| Agent | Spawn | Agent ID | Description | Model | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | claude-opus-5-5 | medium | 16 | 72 | 177 | 39.610 | 6.968 | 12.490.200 | 302.573 |
| spec-author | 1 | ad90cd8427da52f50 | Spec author: clarifications | claude-opus-5-5 | medium | 1 | 54 | 103 | 59.173 | 26.178 | 9.325.836 | 237.158 |
| skeptic | 1 | ae43bb324d8485c46 | Skeptic: premise check | claude-opus-5-5 | high | 1 | 92 | 186 | 87.877 | 38.181 | 19.479.880 | 263.589 |
| spec-author | 2 | a1275b9b23532adc9 | Spec author: amendment round | claude-opus-5-5 | medium | 1 | 37 | 74 | 33.521 | 13.895 | 5.032.896 | 176.745 |
| test-implementer | 1 | a321d7226b1242d62 | Test implementer: reproductions | claude-opus-5-5 | medium | 1 | 106 | 214 | 106.167 | 21.770 | 24.817.891 | 608.759 |
| implementer | 1 | afa17c812bb7a1f4e | Implementer: make tests pass | claude-opus-5-5 | medium | 1 | 90 | 180 | 74.573 | 26.617 | 18.967.678 | 239.813 |
| spec-author | 3 | aa7d08528a6497328 | Spec author: two corrections | claude-opus-5-5 | medium | 1 | 11 | 21 | 5.933 | 974 | 938.464 | 102.859 |
| test-implementer | 2 | a2362fe5987aa5d86 | Test implementer: corrections | claude-opus-5-5 | medium | 1 | 20 | 38 | 13.179 | 1.499 | 2.237.511 | 80.793 |
| implementer | 2 | a3e836e0533967664 | Implementer: remove dead helper | claude-opus-5-5 | medium | 1 | 8 | 16 | 2.981 | 17 | 705.542 | 43.988 |
| cleaner | 1 | a46fed20cfe743341 | Cleaner: format and warnings | claude-sonnet-5-5 | low | 1 | 5 | 10 | 1.580 | 91 | 330.873 | 84.865 |
| spec-checker | 1 | a9a55e6e7c99d0531 | Spec checker: verify diff | claude-sonnet-5-5 | medium | 1 | 9 | 17 | 4.438 | 710 | 800.467 | 114.189 |
| auditor | 1 | a38da37f0babe8120 | Auditor: final quality check | claude-opus-5-5 | high | 1 | 55 | 112 | 66.640 | 42.305 | 11.757.350 | 303.849 |
| test-implementer | 3 | aca272ffeba9fe6bd | Test implementer: repair round 1 | claude-opus-5-5 | medium | 1 | 28 | 53 | 18.161 | 3.452 | 3.418.030 | 97.098 |
| implementer | 3 | a27e35e146660342c | Implementer: repair round 1 | claude-opus-5-5 | medium | 1 | 11 | 21 | 7.168 | 540 | 1.046.737 | 53.214 |
| cleaner | 2 | a38eb20b751838b13 | Cleaner: repair round 1 | claude-sonnet-5-5 | low | 1 | 5 | 10 | 1.320 | 67 | 329.570 | 84.338 |
| spec-checker | 2 | a044b4dcc7206f709 | Spec checker: repair round 1 | claude-sonnet-5-5 | medium | 1 | 6 | 12 | 3.491 | 685 | 496.395 | 109.233 |
| auditor | 2 | a730cb3fcc033f76a | Auditor: repair round 1 | claude-opus-5-5 | high | 1 | 11 | 21 | 8.420 | 3.192 | 926.587 | 104.005 |
| test-implementer | 4 | aac72c4e0a45dd7f9 | Resolve RH5102 merge conflict | claude-opus-5-5 | medium | 1 | 9 | 18 | 5.034 | 668 | 718.021 | 96.390 |
| cleaner | 3 | abb088ca86fe251a3 | Cleaner: after merge | claude-sonnet-5-5 | low | 1 | 5 | 10 | 1.484 | 73 | 341.649 | 88.359 |
| auditor | 3 | aa2573d4c8fd73152 | Auditor: repair round 1 audit | claude-opus-5-5 | high | 1 | 27 | 52 | 17.771 | 6.950 | 3.282.986 | 96.235 |
| validator | 1 | a33583ac4847d6e31 | Validator: final tree | claude-haiku-5-5 | low | 1 | 7 | 14 | 2.680 | 317 | 389.722 | 158.728 |
| issue-manager | 1 | a285de74b1b2f3da4 | Issue manager: follow-ups | claude-sonnet-5-5 | medium | 1 | 8 | 17 | 5.152 | 782 | 710.902 | 116.376 |

| Model | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| claude-haiku-5-5 | 7 | 14 | 2.680 | 317 | 389.722 | 158.728 |
| claude-opus-5-5 | 631 | 1.286 | 546.208 | 193.206 | 115.145.609 | 2.807.068 |
| claude-sonnet-5-5 | 38 | 76 | 17.465 | 2.408 | 3.009.856 | 597.360 |
| **Total** | 676 | 1.376 | 566.353 | 195.931 | 118.545.187 | 3.563.156 |

## Decisions taken

20 rows. Worth the maintainer's look: #14 (RH7101 reports nothing when a conditional directive sits in the initializer of a non-last declarator — literal reading of "between its declarators", no example in the spec), #16 (RH7311 treats a type wholly inside `#if` as one outline), and #3 (#1337 extended to RH5102/RH5109/RH5111 per its triage comment).

## Suggestions

- The lead read `main`'s SHA for the repair audit before the repair ran; lead.md says to read it in the same step as the spawn — also for the repair audit, which is easy to miss.
- A spec author writing a fix example that crosses a region should check it against SHARED-ORDER-004; the conflict surfaced only at the implementer and cost a full spec/test/implementer correction trio.
