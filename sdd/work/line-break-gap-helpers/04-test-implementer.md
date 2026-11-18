Role: test implementer
Commit: ee6230b (test adjusted and run on this head; committed on top of it)
Verdict: tests written

Repair round for decision 9. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

`LineBreakRewriterTests.SplitsMixedLineArgumentsUsingConfiguredLineFeed` ran the line-break phase alone and asserted `Call("a",\n`, which locked the absence of whitespace in front of the line break the phase inserts. Under decision 4 (FMT-BREAK-029, FMT-CLEAN-001) that whitespace is the cleanup phase's, so the assertion no longer applies. The test keeps its purpose, that the inserted separator break uses the context's LF:

- The first assertion is now `Assert.MatchesRegex(new Regex("Call\\(\"a\",[ ]*\n[ ]*\"b\",", RegexOptions.None, Regex.InfiniteMatchTimeout), result, …)`. It allows spaces in front of the break, still requires an LF with no CR directly behind them, and also checks that `"b"` starts the next line (the split the test name describes), which the old assertion did not.
- The second assertion (`DoesNotContain("\r\n", …)`) is unchanged.
- `using System.Text.RegularExpressions;` added. The timeout is `InfiniteMatchTimeout`: the pattern is a fixed literal over test output, not untrusted input (`AGENTS.md`, Key conventions).

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `LineBreakRewriterTests.SplitsMixedLineArgumentsUsingConfiguredLineFeed` (changed) | none (untagged before and after); FC-010 (line ending of an inserted break) | `ExecuteLineBreakPhase` (phase alone, LF context) | pass |

## Scenarios (bug fix)
_None._ No reproduction exists for this refactor (see `02-test-implementer.md`).

## Not covered, and why
- No CRLF variant added: the test's point is that an LF context yields LF; the existing `DoesNotContain("\r\n")` assertion and the regex both fail if the phase falls back to CRLF.
- The final output is covered unchanged by the full-pipeline tests; this test asserts one phase's output only (decision 9).

## Commands
```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.LineBreaks.LineBreakRewriterTests.SplitsMixedLineArgumentsUsingConfiguredLineFeed"
scripts/test.sh --project formatter
```
Result on this head with the change: formatter project 5781 passed, 0 failed.

## Follow-up candidates
_None._
