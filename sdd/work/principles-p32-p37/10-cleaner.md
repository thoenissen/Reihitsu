Role: cleaner
Commit: 17229a5
Verdict: clean

## Formatted
_None._ The formatter reported `Formatted 0 of 8 file(s).` for the eight C# files changed since `34ca7c3`.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: `Build succeeded.` `0 Warning(s)` `0 Error(s)`
- `scripts/test.sh --project analyzer` (RH5201, RH5301, RH5303, RH5304, RH5405, RH5406, RH5417): `Passed!  - Failed: 0, Passed: 260, Skipped: 0, Total: 260`
- `scripts/test.sh --project formatter` (`ReihitsuFormatterTests`): `Passed!  - Failed: 0, Passed: 48, Skipped: 0, Total: 48`
- Note: `scripts/format.sh` prints S1125 warnings from building the CLI tool (`Reihitsu.Cli` sources, none in the changed files); the repository Release build is warning-free.
