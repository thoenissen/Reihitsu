# test-hang-timeout

| | |
|---|---|
| Date | 2026-11-17 |
| Run | issue #1850 |
| Pull request | #1854 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|

None — tooling lane, no visible output in question.

## Cost

No stops, no repair rounds. Every role was spawned once. The run's wall-clock cost was the implementer's hang demonstrations: two real 10-minute aborts (through `scripts/test.sh` and through the SonarCloud command line) plus a full Release run with self-hosting, about 40 minutes in all. The auditor's one text finding (`scripts/README.md`, timing in hosts that run tests in parallel) was fixed by the spec author and proven text-only, so no second audit was needed.

## Usage

Session `8bbcc4eb-74c7-5d0f-87d8-264fb4822576`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 8 | 35 | 86 | 11.663 | 1.480 | 4.470.219 | 85.468 | 54.652 |
| implementer | 1 | a708aee20779b41ea | Implement hang guard #1850 | large | medium | 2 | 30 | 66 | 19.297 | 4.726 | 3.101.748 | 282.788 | 60.873 |
| cleaner | 1 | aa7f296c558e00662 | Clean hang guard change #1850 | medium | low | 1 | 4 | 8 | 950 | 58 | 237.903 | 82.167 | 7.484 |
| spec-checker | 1 | a03d696014cb67df4 | Spec check hang guard #1850 | medium | medium | 1 | 4 | 8 | 1.861 | 213 | 242.307 | 86.926 | 12.697 |
| auditor | 1 | a14eb1997cf4d5a28 | Audit hang guard #1850 | large | high | 1 | 18 | 40 | 16.082 | 7.682 | 1.906.405 | 138.049 | 63.731 |
| spec-author | 1 | a9cc20c4e5dd38e52 | Fix README hang wording #1850 | large | medium | 1 | 5 | 10 | 2.397 | 291 | 380.083 | 35.921 | 11.787 |
| validator | 1 | aefd8073360ea4bfc | Validate final tree #1850 | small | low | 1 | 6 | 12 | 2.190 | 180 | 101.217 | 55.612 | 6.807 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 88 | 202 | 49.439 | 14.179 | 9.858.455 | 542.226 |
| medium | 8 | 16 | 2.811 | 271 | 480.210 | 169.093 |
| small | 6 | 12 | 2.190 | 180 | 101.217 | 55.612 |
| **Total** | 102 | 230 | 54.440 | 14.630 | 10.439.882 | 766.931 |

## Decisions taken

Six. Test implementer skipped (no test project covers `scripts/test.sh` or the workflows; the implementer demonstrated the guard with a temporary, uncommitted test). Two implementer follow-up candidates dropped: other workflows without `timeout-minutes` run no tests, and no caller passes its own `--blame-hang-timeout`. As an adjacent fix, the publish commit also deletes `sdd/work/precheck-comma-linear-scan/`, which an earlier merged PR left on `main` and which makes `WorkingDocumentsDirectoryContainsNoFiles` (self-hosting) fail there. The maintainer may want to see that last one.

## Suggestions

- A run that deletes `sdd/work/<slug>/` in a CI repair after publishing can leave the folder on `main` when the repair commit re-creates it (as `precheck-comma-linear-scan/` shows). `lead.md` section 9 could require the lead to check `git ls-tree HEAD sdd/work` is empty before the final push.
