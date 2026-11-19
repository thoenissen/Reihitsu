# get-only-expression-body

| | |
|---|---|
| Date | 2026-11-19 |
| Run | issues #849, #850 |
| Pull request | #1868 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | Two rules RH3209/RH3210 as drafted (span, fix, not-reported shapes, version gates, pages) | A | yes |
| 2 | output question | Comment the formatter keeps the accessor list for: not reported, or reported without fix | B (reported, no fix) | no |
| 3 | output question | Comment/directive the formatter refuses next to `=>`: reported without fix | A | yes |
| 4 | output question | Fix keeps the rest of the signature as written | A | yes |
| 5 | 10 | Declare a comment/directive inside a get-only accessor list unsupported | not declared | yes |
| 6 | 9 | Move the get-only predicate and conversion into one new Core type | A | yes |
| 7 | output question | Directive the formatter keeps the accessor list for: not reported, or reported without fix | B (reported, no fix) | no |

## Cost

- Stops: two — the batch of questions 1–6 before the test implementer, and question 7 alone after the spec author's amendment surfaced it. Question 7 was a neighbour of question 2 (comments vs. directives) and should have been in the first batch.
- Repair rounds: one (audit 11: shared-line handling of the fix, a missing test, code owners). Audit 21 found a repeat of the shared-line class (documentation comments in the sibling gap), handled as an architecture finding by refusing the class by its cause, not as a round.
- Roles spawned more than once: spec author 8 times (answers, Q7, wording clarification, two owner repairs, two shared-line spec changes) — small follow-ups, each fresh; test implementer 5 times; implementer 3 times (one interrupted by a usage limit and resumed); auditor 3 times.
- Expensive: the first test implementer, implementer, and auditor spawns (each 160–190k context growth); the main session's long context across two days.
- The implementer's first spawn copied the RH3202 family's splice instead of reusing it, which the audit caught as drift; a shared helper name in the skeptic's handover could have avoided the round.

## Usage

## Usage

Session `7461094d-f6eb-516d-a73f-9e374f86c67e`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 33 | 98 | 247 | 49.245 | 7.955 | 19.359.735 | 362.074 | 183.407 |
| spec-author | 1 | a3757d085eb8b8d1f | Spec author for #849 #850 | large | medium | 1 | 45 | 87 | 46.697 | 21.209 | 6.895.898 | 218.103 | 136.877 |
| skeptic | 1 | a7ad1c8b67552585b | Skeptic for #849 #850 | large | high | 1 | 49 | 100 | 56.599 | 34.491 | 7.487.259 | 163.728 | 140.078 |
| spec-author | 2 | a82c0a776b3e5817a | Spec amendments after answers | large | medium | 1 | 20 | 40 | 27.058 | 9.740 | 2.342.755 | 94.008 | 69.935 |
| spec-author | 3 | a02a444d29a3b9925 | Spec amendment for Q7 | large | medium | 1 | 10 | 20 | 10.163 | 2.498 | 937.796 | 58.483 | 34.784 |
| test-implementer | 1 | a899ace0090a28099 | Tests for RH3209 RH3210 | large | medium | 1 | 64 | 128 | 77.534 | 17.328 | 11.721.327 | 206.664 | 183.007 |
| implementer | 1 | a23e2b907092ae73b | Implement RH3209 RH3210 | large | medium | 1 | 52 | 104 | 53.946 | 14.821 | 9.531.384 | 190.257 | 166.540 |
| spec-author | 4 | ada371d3216b7ae4b | Clarify SHARED-GETONLY-003 wording | large | medium | 2 | 18 | 36 | 11.359 | 2.934 | 1.937.916 | 73.574 | 49.841 |
| test-implementer | 2 | a39e356bea4efe2d9 | Fix disputed CRLF test | large | medium | 1 | 8 | 16 | 4.365 | 263 | 651.465 | 37.236 | 13.572 |
| cleaner | 1 | aeb56d8927ed1bd05 | Cleaner for #849 #850 | medium | low | 1 | 5 | 10 | 1.283 | 0 | 394.277 | 36.001 | 12.509 |
| spec-checker | 1 | a07435db66af362fb | Spec checker for #849 #850 | medium | medium | 1 | 9 | 18 | 4.563 | 569 | 840.010 | 62.555 | 39.032 |
| auditor | 1 | a2dcf67b44a874e59 | Auditor for #849 #850 | large | high | 1 | 54 | 108 | 53.510 | 30.040 | 10.282.123 | 215.045 | 191.471 |
| test-implementer | 3 | ae0b5611f7efee974 | Repair tests round 1 | large | medium | 2 | 26 | 52 | 23.722 | 6.409 | 3.302.823 | 116.009 | 92.305 |
| spec-author | 5 | a0ac0e97742a96391 | Repair code owners round 1 | large | medium | 1 | 8 | 16 | 3.923 | 288 | 696.676 | 47.361 | 23.726 |
| test-implementer | 4 | a41d971b34132d7e5 | Tests for decisions 14 15 | large | medium | 2 | 22 | 44 | 16.265 | 2.399 | 2.317.259 | 77.561 | 53.823 |
| spec-author | 6 | a2f7f60140e4ae5a9 | Spec amend decisions 14 15 | large | medium | 1 | 14 | 28 | 10.377 | 3.079 | 1.406.803 | 70.174 | 46.453 |
| implementer | 2 | a320e9c5abfbe1455 | Repair fix round 1 | large | medium | 2 | 24 | 48 | 22.201 | 5.953 | 2.861.273 | 257.067 | 87.748 |
| cleaner | 2 | ae44acb07756bd55f | Cleaner repair round 1 | medium | low | 1 | 9 | 18 | 2.636 | 127 | 657.692 | 86.086 | 10.679 |
| spec-checker | 2 | aec2b2f9749ee9dba | Spec checker repair round 1 | medium | medium | 1 | 8 | 16 | 5.269 | 802 | 640.213 | 110.583 | 35.590 |
| spec-author | 7 | a41f87ae056b135e0 | Fix code owners finding | large | medium | 1 | 10 | 20 | 5.786 | 1.018 | 851.213 | 44.636 | 20.991 |
| auditor | 2 | abc5c98aaf14cefd6 | Audit repair round 1 | large | high | 2 | 40 | 80 | 42.656 | 23.743 | 6.653.066 | 231.513 | 156.421 |
| spec-checker | 3 | a1ac73047af1eb398 | Spec checker re-check | medium | medium | 1 | 6 | 12 | 2.486 | 164 | 491.531 | 43.502 | 19.905 |
| spec-author | 8 | a55755c77be77506e | Spec wording for decision 18 | large | medium | 1 | 15 | 30 | 9.435 | 2.480 | 1.409.613 | 62.681 | 38.304 |
| test-implementer | 5 | a24c14d626acce20d | Tests for decision 18 | large | medium | 2 | 16 | 32 | 12.193 | 2.346 | 1.654.523 | 72.856 | 49.114 |
| implementer | 3 | a03f1c6ad4e0c6730 | Implement decision 18 | large | medium | 1 | 13 | 26 | 6.778 | 329 | 1.227.585 | 57.755 | 34.084 |
| cleaner | 3 | abcc02d49ddd7dbc7 | Cleaner after decision 18 | medium | low | 1 | 5 | 10 | 1.298 | 0 | 383.747 | 34.491 | 10.953 |
| auditor | 3 | a76f4714c8b71733d | Audit decision 18 delta | large | high | 2 | 21 | 42 | 17.355 | 6.499 | 2.683.393 | 114.121 | 90.413 |
| spec-checker | 4 | a2903b615bfd0e5e5 | Spec checker after decision 18 | medium | medium | 1 | 7 | 14 | 3.422 | 236 | 582.881 | 49.569 | 25.976 |
| validator | 1 | a59df0ad7e9f745ac | Validate final tree | small | low | 2 | 8 | 16 | 2.898 | 180 | 160.000 | 58.076 | 8.421 |
| issue-manager | 1 | a86cffa70b79ded0c | File follow-up issues | medium | medium | 1 | 10 | 22 | 8.025 | 1.433 | 1.002.624 | 79.115 | 54.845 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 627 | 1.304 | 561.167 | 195.822 | 96.211.885 | 2.770.906 |
| medium | 59 | 120 | 28.982 | 3.331 | 4.992.975 | 501.902 |
| small | 8 | 16 | 2.898 | 180 | 160.000 | 58.076 |
| **Total** | 694 | 1.440 | 593.047 | 199.333 | 101.364.860 | 3.330.884 |

## Decisions taken

19 decisions taken by the lead. Worth the maintainer's eye: decision 12 (the RH3202-family splice and the get-only fix now share `ExpressionBodyCodeFixUtilities.ReplaceBodyAsync`), decision 15 (a member in a type written on one line is converted in place, adopting the RH3202-002 / RH5417-003 exception), and decision 18 (the fix is withheld for any non-whitespace gap to a sibling member on the same line).

## Suggestions

- When an output question's answer covers comments, ask the same question for directives and disabled text in the same batch; the neighbour rule in `lead.md` section 5 names this, and the run still split it into a second stop.
- When a new code fix mirrors an existing family's mechanism, the skeptic's architecture finding could name the code-fix helpers to reuse, not only the formatter owner; here the audit found the copied splice.
