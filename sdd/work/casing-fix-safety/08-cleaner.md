Role: cleaner
Commit: see git log (cleaner commit on top of 457db3f plus lead notes)
Verdict: clean

## Formatted
_None._ `scripts/format.sh --force` over the 38 changed C# files: `Formatted 0 of 38 file(s).`

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| RH7110 (member order) | `src/Reihitsu.Analyzer/Base/CasingAnalyzerBase.cs` | Moved `internal bool IsValidName` (with its doc comment) above `protected abstract GetLocations`; no body changed |

## Unresolved
_None._

## Checks
- `scripts/build.sh` (Release): `0 Warning(s)`, `0 Error(s)`.
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~Naming"`: `Passed!  - Failed: 0, Passed: 648, Skipped: 0, Total: 648`.
