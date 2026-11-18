# line-count-rules

| | |
|---|---|
| Date | 2026-11-18 |
| Run | issues #353, #354 |
| Pull request | #1860 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | Where the limits are configured (`reihitsu.json` `design` vs `.editorconfig`) | A, `reihitsu.json` | yes |
| 2 | output question | Behavior with no limit configured (inactive / built-in defaults / off by default) | B, built-in defaults | no |
| 3 | output question | Nested local functions, lambdas, types count toward the container | A | yes |
| 4 | output question | Rule IDs RH2008/RH2111, titles, messages, locations | A | yes |
| 5 | output question | RH2111 judges unions, not enums | A | yes |
| 6 | output question | Each property accessor judged on its own | A | yes |
| 7 | output question | Lambdas in field initializers not judged | A | yes |
| 8 | output question | Strict whole-number validation of limits by RH0001 | A | yes |
| 9 | output question | Default limits (50/400, 60/500, 75/750) | B, 60/500 | yes |
| 10 | output question | Every line of a multi-line string literal counts | A | yes |
| 11 | output question | This repository's own build: rules off now, follow-up decides | A, plus: also excluded from the self-hosting scans | yes, amended |
| 12 | output question | How test code is recognized (`IsTestProject` / frameworks / attributes / both build properties) | D, both properties; the maintainer asked whether the analyzer can read the second one | yes |
| 13 | output question | Flat test-setting names | A | yes |
| 14 | output question | Test defaults 100/1000 | A | yes |
| 15 | output question | A production limit alone leaves the test default | A | yes |
| 16 | output question | An invalid limit keeps its default | A | yes |

The maintainer also asked, with batch 1, which default values would work for this repository, and, with batch 2, for separate production and test limits — a new requirement that created batch 3.

## Cost

- Three output-question batches. Batch 2 came from the maintainer's 2B answer plus the request to measure the repository; batch 3 from the new production/test requirement. Each new batch cost one spec-author amendment and one fresh skeptic.
- One repair round (a table row out of order in `sdd/specs/README.md`, found by the spec checker); the auditor passed on the first audit.
- One general-purpose measurement agent (method and type line counts of the repository) outside the squad's roles, to answer the maintainer's question about defaults.
- The test implementer was the most expensive spawn (largest context growth), with 172 analyzer test cases plus RH0001, RH8402 and self-hosting tests.

## Usage

Session `1c2d072c-efd3-594a-a5fb-d3e8211b6f71`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 17 | 69 | 170 | 35.982 | 5.421 | 11.539.332 | 325.787 | 123.440 |
| spec-author | 1 | ae2d9c5d0ae664856 | Spec author for line-count rules | large | medium | 1 | 52 | 102 | 46.802 | 16.806 | 8.086.807 | 150.647 | 127.004 |
| skeptic | 1 | a45e1b150126c4c1d | Skeptic for line-count rules | large | high | 1 | 33 | 69 | 39.411 | 22.198 | 4.801.972 | 137.961 | 114.272 |
| general-purpose | 1 | a2dff7ce5464751ab | Measure repo method/type line counts | large | medium | 1 | 11 | 21 | 12.548 | 1.570 | 941.887 | 106.825 | 27.680 |
| spec-author | 2 | aaa912b4d69b30f11 | Spec author amendment: answers + test split | large | medium | 1 | 44 | 84 | 42.431 | 12.109 | 5.981.019 | 182.726 | 103.694 |
| skeptic | 2 | afe3d5fc3698fb6d4 | Skeptic on spec amendment | large | high | 1 | 38 | 73 | 39.374 | 21.253 | 5.459.971 | 186.368 | 108.274 |
| spec-author | 3 | a0946771b68f61c91 | Spec author: batch 3 answers | large | medium | 1 | 21 | 40 | 16.969 | 2.535 | 2.311.776 | 139.632 | 60.985 |
| test-implementer | 1 | a7b25e3967b9a4654 | Test implementer for line-count rules | large | medium | 1 | 70 | 133 | 102.980 | 27.854 | 13.926.928 | 240.265 | 215.740 |
| implementer | 1 | a13767d4b0c09f6fb | Implementer for line-count rules | large | medium | 1 | 42 | 80 | 34.993 | 8.261 | 6.528.363 | 147.259 | 122.747 |
| cleaner | 1 | a0b93e4e7e351e785 | Cleaner for line-count rules | medium | low | 1 | 10 | 19 | 3.993 | 339 | 808.186 | 97.429 | 19.072 |
| spec-checker | 1 | abc9b2865eb19cb8e | Spec checker for line-count rules | medium | medium | 1 | 24 | 46 | 12.523 | 4.965 | 2.910.233 | 158.080 | 80.195 |
| spec-author | 4 | a2018a232f508808d | Spec author repair: README order | large | medium | 1 | 5 | 10 | 1.332 | 31 | 338.583 | 88.641 | 10.373 |
| spec-checker | 2 | ab07b018285833f85 | Spec checker on repair round 1 | medium | medium | 1 | 4 | 8 | 1.493 | 66 | 300.057 | 30.786 | 7.168 |
| auditor | 1 | a50131c386a5f1240 | Auditor for line-count rules | large | high | 1 | 55 | 105 | 50.089 | 26.946 | 9.734.854 | 248.437 | 170.473 |
| validator | 1 | a333129d8971afa92 | Validator for line-count rules | small | low | 2 | 8 | 16 | 2.629 | 147 | 198.202 | 75.547 | 17.868 |
| issue-manager | 1 | ac964adfc61294a93 | Issue manager for follow-ups | medium | medium | 1 | 6 | 12 | 4.998 | 495 | 467.406 | 109.262 | 34.796 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 440 | 887 | 422.911 | 144.984 | 69.651.492 | 1.954.548 |
| medium | 44 | 85 | 23.007 | 5.865 | 4.485.882 | 395.557 |
| small | 8 | 16 | 2.629 | 147 | 198.202 | 75.547 |
| **Total** | 492 | 988 | 448.547 | 150.996 | 74.335.576 | 2.425.652 |

## Decisions taken

Seven. Worth the maintainer's eye: the RH2008/RH2111 `Action="None"` ruleset entries carry out answer 11A, not a warning suppression; the auditor's hints (missing BOM on three new files, two `internal` helpers that could be `private`, no test of a blank line inside an interpolated multi-line string) were left as they are.

## Suggestions

- When the default of a configurable limit is in question, measure the repository's own code before asking: the maintainer asked for that measurement after batch 1, and having it in batch 1 would have saved a round trip.
- The skeptic's build check of the test-project properties (12D) found that `IsTestProject` alone misses Microsoft.Testing.Platform projects; checking an MSBuild-facing claim by building scratch projects was worth its cost.
