Role: cleaner
Commit: f2be5e6
Verdict: clean

## Formatted
_None._ `scripts/format.sh` on the three changed C# files: formatted 0 of 3.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ (`format.sh` printed S1125 at `src/Reihitsu.Cli/Diff/EditScriptBuilder.cs:91`, a file this round did not change; the Release build reports 0 warnings.)

## Checks
- `scripts/build.sh`: 0 Warning(s), 0 Error(s).
- RH2005 analyzer tests: Passed 94, Failed 0.
- `DeclarationModifierUtilitiesTests`: Passed 23, Failed 0.
