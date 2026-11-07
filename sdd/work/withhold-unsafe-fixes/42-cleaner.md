Role: cleaner
Commit: see git log
Verdict: clean

## Formatted
_None._ (formatter changed 0 of 3 files)

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| RH5112 (error) | `src/Reihitsu.Analyzer.Test/Clarity/RH3004UseLambdaSyntaxAnalyzerTests.cs:3762` | Raw string assigned to local `template`; `.Replace(...)` moved to a following statement producing `testCode`. Same value, assertion unchanged. |

## Unresolved
_None._

## Checks
scripts/build.sh: Build succeeded, 0 Warning(s), 0 Error(s). Focused RH3004 tests run (see lead summary).
