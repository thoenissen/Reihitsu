# message-format-test-literals

| | |
|---|---|
| Date | 2026-11-27 |
| Run | issue #1912 |
| Pull request | #1967 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|

None; the run changes test code only and put no visible output in question.

## Cost

No stops and no repair rounds; every role was spawned once. Lane tests: skeptic, test implementer, cleaner, spec checker, validator, issue manager. The spec author was skipped because the run adds no spec; the implementer and auditor are not part of the tests lane. The skeptic was the most expensive spawn: it re-derived the issue's stale `rg` command (the issue searched `src`, the test projects live under `tests/`) and scanned every resource text as a literal, which found sites the issue missed (interpolated placeholder messages, self-hosting asserts, title literals). The validator's full default test run took about 11 minutes.

## Usage

Session `b32759c7-9208-5953-b7a0-a9f2c9052276`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 4 | 31 | 73 | 8.953 | 703 | 3.980.559 | 86.612 | 52.568 |
| skeptic | 1 | acde84a07f5538488 | Skeptic premise check #1912 | large | high | 1 | 44 | 86 | 29.367 | 10.303 | 5.405.440 | 160.469 | 81.531 |
| test-implementer | 1 | a4220dddf62395009 | Test implementer #1912 | large | medium | 1 | 34 | 66 | 17.894 | 2.306 | 3.780.994 | 80.334 | 54.790 |
| cleaner | 1 | a09740d76d0960fd7 | Cleaner #1912 | medium | low | 1 | 5 | 10 | 1.503 | 0 | 339.420 | 87.771 | 8.394 |
| spec-checker | 1 | afa0a4a46a5c2bed6 | Spec checker #1912 | medium | medium | 1 | 5 | 10 | 1.990 | 39 | 396.154 | 108.262 | 29.303 |
| validator | 1 | ad57ec08f5caa931a | Validator #1912 | small | low | 1 | 10 | 20 | 4.070 | 462 | 281.102 | 78.608 | 18.124 |
| issue-manager | 1 | a91f7dcf0c42cd553 | Issue manager #1912 | medium | medium | 1 | 6 | 14 | 2.564 | 143 | 524.146 | 50.582 | 25.092 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 109 | 225 | 56.214 | 13.312 | 13.166.993 | 327.415 |
| medium | 16 | 34 | 6.057 | 182 | 1.259.720 | 246.615 |
| small | 10 | 20 | 4.070 | 462 | 281.102 | 78.608 |
| **Total** | 135 | 279 | 66.341 | 13.956 | 14.707.815 | 652.638 |

## Decisions taken

Seven, all on the skeptic's recommendations: search `tests/` instead of the issue's `src`; format `MessageFormat` for the placeholder messages of RH2008, RH2009, RH2111 instead of keeping literals; include the self-hosting line-count asserts; delete the two `VerifyTitleNamesFirstMemberAccess` tests (RH5048, RH5112), whose wording is pinned by `AnalyzerPackageMetadataTests`; compare the RH5112 code-fix title with `CodeFixResources.RH5112Title`; no new helper; leave the four CLI argument-array lines the `rg` pattern also matches. The deletion of the two title tests is the one a reviewer might want to see.

## Suggestions

An `agent-filed` issue's search command can go stale when the repository layout changes (#1932 moved the test projects); the skeptic caught it, but issues could state the intent ("no test quotes a resource text") rather than a command.
