Role: cleaner
Commit: (this commit)
Verdict: clean

## Formatted
_None._ The formatter changed 0 of the 31 C# files the branch changes against origin/main.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| RH7305 (error) | src/Reihitsu.Analyzer.Test/Ordering/RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests.cs | Test methods stood between `#endregion // Tests` and `#region Methods`, outside every region. Moved the `#endregion // Tests` line behind them so they sit inside the Tests region. No test body, assertion, or input changed. |

## Unresolved
_None._

## Checks
scripts/build.sh: 0 Warning(s), 0 Error(s). scripts/test.sh --project analyzer --filter RH7109 run (see result line in report).
