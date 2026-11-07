Role: cleaner
Commit: 6a8a260b
Verdict: clean

## Formatted
_None._ (formatter changed 0 of 1 files)

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| S2583 | DocumentOrderFixAllProvider.cs GetFixedDocumentAsync | Callback only collects matching actions; the first one is taken with FirstOrDefault and null-tested |

## Unresolved
_None._

## Checks
scripts/build.sh: 0 Warning(s), 0 Error(s). Analyzer tests: Passed 7005, Failed 0.
