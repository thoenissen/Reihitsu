Role: cleaner
Commit: 61ead44
Verdict: clean

## Formatted
_None._ `scripts/format.sh` over the 10 changed C# files: `Formatted 0 of 10 file(s).`

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/build.sh` (Release): `0 Warning(s)`, `0 Error(s)`.
- `scripts/test.sh --project cli`: `Passed!  - Failed: 0, Passed: 355, Skipped: 0, Total: 355`.

## Follow-up candidates
S1125 in `FormatCommandHandler.cs` (`ConfirmLargeRunIfRequiredAsync`, `CollectFiles`; lines 593, 604, 615) appears in Debug builds only, on lines the run did not change. Release is clean.
