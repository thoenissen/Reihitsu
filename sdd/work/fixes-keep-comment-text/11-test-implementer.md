Role: test implementer
Commit: ccd65f2
Verdict: tests written (repair round 2: one test removed)

The prompt carried no intended fix, suspected cause, or hoped-for verdict. This round carries out `00-lead.md` decision 13 on auditor finding 1 (`10-auditor.md`).

## Change

- Removed `RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzerTests.VerifyFixKeepsTextOfEnclosingRegionDirectives` (tagged `SHARED-FIX-003`, listed in `04-test-implementer.md`). As the auditor showed, it could not fail on the mechanism it claimed to check: the target's leading trivia (`#region legacy`) is written back verbatim, and `#endregion` lies outside the target. FMT-PIPE-028 stays covered by `ReihitsuFormatterNodeRegionTests`. A shape that could fail needs both directives inside the target, and its output carries blank lines that thoenissen/Reihitsu#1140 owns, so no replacement is written.
- The run had added only this test to the file and no `using`, so removing it puts the file back to `main`'s content exactly (`git diff main -- <file>` is empty). No `using` is left unused.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5412…Tests.VerifyFixKeepsTextOfEnclosingRegionDirectives` | removed (was SHARED-FIX-003) | – | – |

## Scenarios (bug fix)
| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| RH5412 `#region legacy⏎class A⏎{⏎}⏎#endregion` | LF | code fix | not reproduced; no test kept | see `04-test-implementer.md`; the characterization was dropped per decision 13 |

## Not covered, and why
- Region text kept by a node-level fix when both directives are inside the target (RH5417 accessor-list region): its output carries blank-line decisions that thoenissen/Reihitsu#1140 owns; FMT-PIPE-028 is covered by `ReihitsuFormatterNodeRegionTests`.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5412"
```
Result on this head: 29 passed, 0 failed.
