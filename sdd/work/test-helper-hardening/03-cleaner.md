Role: cleaner
Commit: 2e06a89
Verdict: clean

## thoenissen/Reihitsu#1804

### Formatted
_None._ `scripts/format.sh` over the 9 changed Formatter.Test files: nothing changed.

### Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| none | | |

### Unresolved
_None._

### Checks
`scripts/test.sh --project formatter` (the 8 touched classes): `Passed: 101, Failed: 0`.

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1838

### Formatted
_None._ Both changed Analyzer.Test files were already formatted.

### Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| none | | |

### Unresolved
_None._

### Checks
`scripts/test.sh --project analyzer` (Formatter parity + `FormatterTestsBaseTests`): `Passed: 488, Failed: 0`.

### Follow-up candidates
_None._

## Whole run

Formatted 0 of 11 files. `scripts/build.sh`: `Build succeeded. 0 Warning(s) 0 Error(s)`. No text-only proof needed (no post-audit comment fix). No code changed by this role.
