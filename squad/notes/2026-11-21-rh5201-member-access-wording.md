# rh5201-member-access-wording

| | |
|---|---|
| Date | 2026-11-21 |
| Run | issue #1900 |
| Pull request | #1902 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | RH5201 title, message, and code-fix title wording (asked in the issue, answered at triage before the run) | A: "Member-access chains should be aligned." / "Align member-access chain" | yes |
| 2 | output question | RH5201 rule-page body wording beyond the heading | A: "member-access chain(s)", "chain of member accesses" | yes |

## Cost

One stop (output-question batch before the test implementer, one question), no repair rounds, no escalations. The spec author was spawned twice: once for the change doc, once fresh to apply the maintainer's answer (cold cache). As in the RH5112/RH5048 wording run, the large-tier skeptic and auditor found nothing beyond what the spec author had listed, apart from the skeptic's note that the code-fix title was unpinned.

## Usage

Session `476221c5-c057-502d-af4a-6fd1f980165a`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 11 | 35 | 86 | 10.715 | 573 | 4.625.084 | 92.231 | 61.237 |
| spec-author | 1 | a0fe57084abb9dd67 | Spec author for #1900 | large | medium | 1 | 16 | 33 | 11.000 | 2.365 | 1.517.580 | 115.238 | 36.890 |
| skeptic | 1 | a7240ad75dd8271cf | Skeptic for #1900 | large | high | 1 | 26 | 54 | 12.960 | 3.287 | 2.901.971 | 137.043 | 62.829 |
| spec-author | 2 | aa1a26ea4c58349d0 | Spec author: apply answer | large | medium | 1 | 6 | 12 | 2.821 | 108 | 436.151 | 92.633 | 16.588 |
| test-implementer | 1 | a37fefe0071fb2e16 | Test implementer for #1900 | large | medium | 1 | 13 | 26 | 8.799 | 1.102 | 1.199.402 | 55.178 | 30.735 |
| implementer | 1 | a9656d975df0ba36c | Implementer for #1900 | large | medium | 1 | 11 | 22 | 4.496 | 189 | 907.797 | 39.528 | 15.105 |
| cleaner | 1 | a01c23acae27bd515 | Cleaner for #1900 | medium | low | 1 | 5 | 10 | 1.317 | 0 | 319.108 | 81.985 | 6.067 |
| spec-checker | 1 | a697f8455c6b70ad1 | Spec checker for #1900 | medium | medium | 1 | 5 | 10 | 1.922 | 134 | 333.531 | 90.452 | 14.963 |
| auditor | 1 | acbed83feb6417fae | Auditor for #1900 | large | high | 1 | 16 | 32 | 7.841 | 1.459 | 1.672.095 | 133.613 | 58.112 |
| validator | 1 | af7168052917b0441 | Validator for #1900 | small | low | 1 | 6 | 12 | 3.458 | 444 | 128.455 | 72.814 | 16.181 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 123 | 265 | 58.632 | 9.083 | 13.260.080 | 665.464 |
| medium | 10 | 20 | 3.239 | 134 | 652.639 | 172.437 |
| small | 6 | 12 | 3.458 | 444 | 128.455 | 72.814 |
| **Total** | 139 | 297 | 65.329 | 9.661 | 14.041.174 | 910.715 |

## Decisions taken

One: accept the spec author's internal choices (spec title follows the rule title; type names and the code-fix equivalence key stay; other rules' pages that say "method chain" stay unchanged).

## Suggestions

The RH5112/RH5048 note's suggestion holds again: a resource-string-only wording change ran the full behavior-change lane with large-tier skeptic and auditor and no finding. A lighter path for diagnostic-text-only changes would cost less.
