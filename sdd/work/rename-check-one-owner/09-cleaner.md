Role: cleaner
Commit: see git log
Verdict: clean

## Formatted
_None._ The formatter changed 0 of 6 changed C# files.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| RH2009 cognitive complexity 16 (limit 15) | `src/Reihitsu.Analyzer.CodeFixes/Base/CasingRenameSafetyUtilities.cs` `FindQualifiedName` | Ancestor walk extracted into private `FindQualifiedNameToken`; same logic |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: Build succeeded, 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~Casing|FullyQualifiedName~RH4103|FullyQualifiedName~RH2001"`: Passed! Failed: 0, Passed: 1108.

## Follow-up candidates
_None._
