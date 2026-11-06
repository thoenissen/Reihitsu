Role: cleaner
Commit: see the commit that adds this file
Verdict: clean

## Formatted
_None._ (formatter changed 0 of 22 changed C# files)

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| S3267 (error) | `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs`, `GetOldNames` loop | Loop iterates the selected names; the `Remove` result is held in a local before the `if`. Same logic. |

## Unresolved
_None._

## Checks
- scripts/build.sh: 0 Warning(s), 0 Error(s)
- RH4105 and RH4121 analyzer tests: Passed: 52, Failed: 0
