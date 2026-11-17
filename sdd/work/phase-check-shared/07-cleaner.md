Role: cleaner
Commit: 4b2f4ad (head before this repair)
Verdict: clean

Repair round 1, item (b) (finding #1). My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Formatted
_None._ `scripts/format.sh` reported 0 of 2 files changed (the base class and the RH6001 test).

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| Finding #1 (stale XML summaries) | `src/Reihitsu.Analyzer.Test/Base/FormatterTestsBase{TAnalyzer}.cs`, private `VerifyFormatter(string, string, string, Action, DiagnosticResult[])` and `VerifyFormatterFixCore` | First summary now states one line ending, the second pass, and the per-phase check unless the input was refused as unsupported. Second summary now says it asserts the output equals the expected fixed text. |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatter.Formatting.RH6001"`: Passed 4 of 4.
- `scripts/verify-text-only.sh --base 091e588 --head worktree`: `text-only  src/Reihitsu.Analyzer.Test/Base/FormatterTestsBase{TAnalyzer}.cs  —  781 tokens identical (7 literal), directives and disabled text identical, changed trivia: documentation`. The overall verdict is FAIL only because the range also holds the RH6001 test change (item a, dc6c41f), which is intended compiled behavior and not mine.

## Follow-up candidates
_None._
