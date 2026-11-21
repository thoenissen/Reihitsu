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
| 3 | maintainer intervention after the CI repair (not an escalation) | Why does a text change need new tests? Keep, slim, or remove the new wording tests | A: remove the new test class; keep only the one-line message update of an existing test | yes |

## Cost

One output-question stop, two repair rounds, no escalations. The run was published once, then CI failed: the self-hosting RH2111 row reported the RH5201 analyzer test class at 3015 lines (limit 3000), pushed over by three new wording tests. The default local test run excludes `Reihitsu.SelfHosting.Test`, so only CI saw it. Round 1 moved the tests into their own class (test implementer, cleaner, spec checker, auditor, validator), plus a merge of `main`. The maintainer then asked why a text change needs tests at all; two of the three tests duplicated `AnalyzerPackageMetadataTests`, and round 2 removed the new class (test implementer, cleaner, auditor, validator). The maintainer also asked for a process issue, filed as #1906 by the issue manager.

Where it got expensive: the full behavior-change lane for a five-line text change (large-tier skeptic and auditor found nothing), the skeptic requesting tests by precedent without checking existing coverage, and the CI-only self-hosting failure that cost a full repair round. Every pushed `[skip ci]` handover commit also produced a check-suite event the lead had to read.

## Usage

Session `476221c5-c057-502d-af4a-6fd1f980165a`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 39 | 119 | 314 | 29.460 | 3.050 | 20.958.453 | 161.880 | 134.801 |
| spec-author | 1 | a0fe57084abb9dd67 | Spec author for #1900 | large | medium | 1 | 16 | 33 | 11.000 | 2.365 | 1.517.580 | 115.238 | 36.890 |
| skeptic | 1 | a7240ad75dd8271cf | Skeptic for #1900 | large | high | 1 | 26 | 54 | 12.960 | 3.287 | 2.901.971 | 137.043 | 62.829 |
| spec-author | 2 | aa1a26ea4c58349d0 | Spec author: apply answer | large | medium | 1 | 6 | 12 | 2.821 | 108 | 436.151 | 92.633 | 16.588 |
| test-implementer | 1 | a37fefe0071fb2e16 | Test implementer for #1900 | large | medium | 1 | 13 | 26 | 8.799 | 1.102 | 1.199.402 | 55.178 | 30.735 |
| implementer | 1 | a9656d975df0ba36c | Implementer for #1900 | large | medium | 1 | 11 | 22 | 4.496 | 189 | 907.797 | 39.528 | 15.105 |
| cleaner | 1 | a01c23acae27bd515 | Cleaner for #1900 | medium | low | 1 | 5 | 10 | 1.317 | 0 | 319.108 | 81.985 | 6.067 |
| spec-checker | 1 | a697f8455c6b70ad1 | Spec checker for #1900 | medium | medium | 1 | 5 | 10 | 1.922 | 134 | 333.531 | 90.452 | 14.963 |
| auditor | 1 | acbed83feb6417fae | Auditor for #1900 | large | high | 1 | 16 | 32 | 7.841 | 1.459 | 1.672.095 | 133.613 | 58.112 |
| validator | 1 | af7168052917b0441 | Validator for #1900 | small | low | 1 | 6 | 12 | 3.458 | 444 | 128.455 | 72.814 | 16.181 |
| test-implementer | 2 | a18d6cf9ba6c38021 | Test implementer: CI repair | large | medium | 3 | 22 | 48 | 9.061 | 1.249 | 1.838.347 | 322.440 | 34.318 |
| cleaner | 2 | ac26d6e4faad72b91 | Cleaner: CI repair | medium | low | 1 | 5 | 10 | 1.279 | 0 | 320.711 | 82.492 | 6.567 |
| spec-checker | 2 | af71c6260ef7d061f | Spec checker: CI repair | medium | medium | 2 | 3 | 6 | 1.322 | 78 | 158.455 | 83.481 | 7.992 |
| auditor | 2 | a51c625474564971e | Auditor: CI repair | large | high | 2 | 14 | 28 | 8.925 | 2.458 | 1.216.919 | 105.354 | 29.803 |
| validator | 2 | af443fbf25604143c | Validator: CI repair | small | low | 2 | 9 | 18 | 3.165 | 871 | 202.337 | 66.468 | 13.690 |
| test-implementer | 3 | ad9e31d0230d4d713 | Test implementer: remove tests | large | medium | 3 | 6 | 12 | 2.760 | 162 | 417.279 | 88.338 | 12.323 |
| issue-manager | 1 | aa95731ba5107a9ec | Issue manager for #1900 | medium | medium | 1 | 6 | 12 | 3.299 | 148 | 450.040 | 99.418 | 24.001 |
| cleaner | 3 | a6bd82ff58cf800ab | Cleaner: test removal | medium | low | 2 | 5 | 10 | 1.202 | 0 | 321.316 | 82.639 | 6.698 |
| validator | 3 | a1d9b732b1da0a8fe | Validator: final tree | small | low | 3 | 12 | 24 | 5.356 | 887 | 339.936 | 118.666 | 21.099 |
| auditor | 3 | a1a0b8b82acd8cb7c | Auditor: test removal | large | high | 2 | 11 | 22 | 7.024 | 1.431 | 907.888 | 102.714 | 27.166 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 260 | 603 | 105.147 | 16.860 | 33.973.882 | 1.353.959 |
| medium | 29 | 58 | 10.341 | 360 | 1.903.161 | 520.467 |
| small | 27 | 54 | 11.979 | 2.202 | 670.728 | 257.948 |
| **Total** | 316 | 715 | 127.467 | 19.422 | 36.547.771 | 2.132.374 |

## Decisions taken

Two: accept the spec author's internal choices (spec title follows the rule title; type names and the code-fix equivalence key stay; other rules' pages that say "method chain" stay unchanged); route the CI failure as a test-implementer repair that moves the tests into their own class (later superseded by the maintainer's answer 3).

## Suggestions

Filed as #1906 at the maintainer's request: a short lane for diagnostic-text-only changes; check existing coverage before requesting or writing a test; run the self-hosting RH2111 row locally when a run grows a test file; push handover commits less often.
