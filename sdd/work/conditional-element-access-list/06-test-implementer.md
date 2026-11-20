Role: test implementer
Commit: 54ef8e33 (head before this commit)
Verdict: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decisions 6, 7, 8 (`00-lead.md`), which I carried out as stated. Production code was not touched.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5307…AnalyzerTests.VerifyFixJoinsConditionalElementAccessListsAndKeepsTheCodeAround` (decision 6) — `VerifyCodeFixWithEndOfLine` replaced by `Verify(ConvertEndOfLine(testCode, endOfLine), ConvertEndOfLine(fixedCode, endOfLine))`, the form `VerifyFixesJoinEveryReportedListWhateverItsReceiverOrContainer` uses; markup and expected fixed text unchanged | RH5307-001, RH5307-003, RH5307-004, SHARED-FIX-006 (tags unchanged) | `Verify(source, fixedSource)` (re-analysis, iterations, Fix All) | passes |
| `RH5307…AnalyzerTests.VerifyFixesJoinTheListsOfTwoConditionalElementAccesses` (decision 6) — same switch; markup and expected fixed text unchanged | RH5307-001, RH5307-003, RH5307-004 (tags unchanged) | `Verify(source, fixedSource)` | passes |
| `RH5307…AnalyzerTests.VerifyFixAndIndexAccessFixConvergeInEitherOrder` (decision 7) — RH5115 markup in `wrappedForGap` and `listJoined` moved from `{\|RH5115:[\|}` to `c{\|RH5115:?\|}`; every other step unchanged | RH5307-001, RH5307-004 (tags unchanged) | `Verify` + `VerifyIndexAccessFix` | passes |
| `SyntaxNodeUtilitiesTests.IsIndexArgumentListReturnsTrueForIndexLists` (decision 8; rows `m[1, 2]`, `d![1, 2]`, `c?[1, 2]`, `new Dictionary<int, int> { [1] = 2 }`) | FMT-BREAK-014 | direct call on the single `BracketedArgumentListSyntax` parsed inside an `unsafe struct` (new private `GetBracketedArgumentList`) | passes (4 rows) |
| `SyntaxNodeUtilitiesTests.IsIndexArgumentListReturnsFalseForFixedSizeBufferSize` (`public fixed int b[3];`) | FMT-BREAK-014 | same | passes |

RH5115's location (decision 7) was derived from `main`, not from the patched code: `git diff origin/main -- src/Reihitsu.Analyzer/Rules/Layout/RH5115*Analyzer.cs` is empty; `RH5115…Analyzer.TryGetFirstToken` maps a `ConditionalAccessExpressionSyntax` to its `OperatorToken` and reports there (line 67, 114); the existing RH5115 tests mark the same shape as `n{|RH5115:?|}⏎[0]` (e.g. lines 46, 260, 522 of the RH5115 test file).

The Core tests are tagged FMT-BREAK-014 because its second paragraph states exactly the three index parent kinds and the excluded fixed-size buffer the predicate decides. They exercise existing behavior on this head, so they pass; the fixed-buffer row would fail if the predicate admitted a `VariableDeclaratorSyntax` parent, and each true row would fail if its parent kind were dropped (the `c?[` row is the omission the run fixes).

## Scenarios (bug fix)

_N/A_ — repair round on existing tests.

## Not covered, and why

- No new behavior: decisions 6 and 7 correct helpers and a location that unchanged requirements and unchanged RH5115 behavior decide; expected fixed text is unchanged in all three tests. Single-application behavior on multi-diagnostic sources (one fix changes only its own list, RH5307-004) is already pinned by `VerifyEachFixJoinsItsOwnListWhenTheReceiverIsAnElementBinding`.
- Failure classes: unchanged from `04-test-implementer.md`.

## Commands

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzerTests"   # Passed 84, Failed 0
scripts/test.sh --configuration Debug --project core --filter "FullyQualifiedName~Reihitsu.Core.Test.SyntaxNodeUtilitiesTests.IsIndexArgumentList"   # Passed 5, Failed 0
```

`scripts/format.sh` over both changed test files: `Formatted 0 of 2 file(s).`

## Follow-up candidates

_None._
