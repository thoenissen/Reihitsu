# rh5112-rh5048-member-access-wording

| | |
|---|---|
| Date | 2026-11-20 |
| Run | issue #897 |
| Pull request | #1898 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | RH5112 title and message wording | "Wrapped chains should keep the first member access on the original line." | yes |
| 2 | output question | RH5048 title and message wording | "Comments must not be placed before the first member access of a wrapped chain" | yes |
| 3 | output question | RH5112 code-fix title | "Keep first wrapped member access on original line" | yes |
| 4 | output question | RH5112 rule-page body text beyond the heading | Reworded to "member access" | yes |
| 5 | output question | RH5048 rule-page "Why" sentence | "belongs on the line its receiver ends on" (skeptic's wording) | yes |

## Cost

One stop (output-question batch before the test implementer), no repair rounds, no escalations. The spec author was spawned twice: once for the change doc, once fresh to apply the maintainer's answers (its first agent's cache was cold). The large-tier skeptic and auditor were the most expensive spawns for a wording-only change; both found nothing beyond what the spec author had already listed, except the skeptic's better RH5048 sentence and the RH5201 follow-up.

## Usage

Session `410a21f0-96c3-5dc2-9c1d-695901fab39a`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 9 | 38 | 104 | 13.813 | 935 | 5.066.098 | 92.836 | 62.890 |
| spec-author | 1 | a7df6555071ba52ae | Spec author for #897 | large | medium | 1 | 22 | 44 | 14.983 | 4.206 | 2.169.730 | 70.357 | 46.417 |
| skeptic | 1 | a5ce72bf87a719abf | Skeptic for #897 | large | high | 1 | 30 | 59 | 17.933 | 6.176 | 3.639.818 | 151.900 | 74.030 |
| spec-author | 2 | a2db3fc100ffad3c4 | Spec author: apply answers | large | medium | 1 | 9 | 17 | 4.656 | 244 | 781.405 | 44.213 | 20.194 |
| test-implementer | 1 | a52dbf19f9f473d71 | Test implementer for #897 | large | medium | 1 | 15 | 29 | 11.032 | 1.661 | 1.601.049 | 76.063 | 52.148 |
| implementer | 1 | a5aeb3602960c1cd2 | Implementer for #897 | large | medium | 1 | 14 | 27 | 5.652 | 141 | 1.314.772 | 50.376 | 25.796 |
| cleaner | 1 | a1d988c2a168c4e71 | Cleaner for #897 | medium | low | 1 | 5 | 10 | 1.400 | 0 | 330.406 | 84.970 | 6.654 |
| spec-checker | 1 | aa24af5a972c3ebd4 | Spec checker for #897 | medium | medium | 1 | 6 | 12 | 2.699 | 259 | 436.781 | 98.232 | 20.374 |
| auditor | 1 | a17a8cb100be6f814 | Auditor for #897 | large | high | 1 | 19 | 37 | 11.834 | 3.269 | 2.228.275 | 148.582 | 70.680 |
| validator | 1 | a48cff03831a812ae | Validator for #897 | small | low | 1 | 12 | 24 | 4.200 | 532 | 255.973 | 93.797 | 11.286 |
| issue-manager | 1 | afa07dd8a655247b1 | Issue manager for #897 | medium | medium | 1 | 7 | 15 | 3.024 | 110 | 626.130 | 55.558 | 31.016 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 147 | 317 | 79.903 | 16.632 | 16.801.147 | 634.327 |
| medium | 18 | 37 | 7.123 | 369 | 1.393.317 | 238.760 |
| small | 12 | 24 | 4.200 | 532 | 255.973 | 93.797 |
| **Total** | 177 | 378 | 91.226 | 17.533 | 18.450.437 | 966.884 |

## Decisions taken

Three: keep a change doc although no requirement changes; drop the issue's "localized resources" goal (none exist); keep the `...FirstCall...` type names (the code-fix equivalence key).

## Suggestions

A wording-only change of diagnostic texts runs the full behavior-change lane, including a large-tier skeptic and auditor. A lighter path for resource-string-only changes, with the visible text still asked as output questions, would cost less with no loss in this run.
