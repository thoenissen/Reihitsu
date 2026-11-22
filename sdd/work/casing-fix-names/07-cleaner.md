Role: cleaner
Commit: see the commit that adds this file
Verdict: clean

## Formatted
_None._ `scripts/format.sh --force` over the 30 changed C# files: "Formatted 0 of 30 file(s)."

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|

_None; there were no warnings._

## Unresolved
_None._

## Checks
- `scripts/build.sh`: Build succeeded. 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project core --filter "FullyQualifiedName~Reihitsu.Core.Test.CasingUtilitiesTests"`: Passed 132, Failed 0.
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming"`: Passed 820, Failed 0.
- Text-only proof: not applicable (no comment fix after an audit).

## Follow-up candidates
_None._
