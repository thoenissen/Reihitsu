Role: cleaner
Commit: d981368
Verdict: clean

Run: issues #1600, #1150, #1274, #1143, #1673, #1678 (slug `fix-offering-safety`, lane behavior change)

## #1600
Nothing to clean.

## #1150
Nothing to clean.

## #1274
Nothing to clean.

## #1143
Nothing to clean.

## #1673
Nothing to clean.

## #1678
Nothing to clean.

## Whole run

### Formatted
_None._ `scripts/format.sh --force` over all 192 changed C# files: `Formatted 0 of 192 file(s).`

### Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

### Unresolved
_None._ (The S1125 warnings printed while `format.sh` builds the CLI are not Release-build warnings.)

### Checks
- `scripts/build.sh`: Build succeeded, 0 Warning(s), 0 Error(s).
- Formatter `Hidden`: Passed 33/33. Cli `HiddenCode`: Passed 9/9. Analyzer `RH4103`: Passed 89/89.
- Text-only proof: not applicable (no comment fixes).
