Role: cleaner
Commit: e3104df
Verdict: #1680: clean; #1742: clean

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## thoenissen/Reihitsu#1680

### Formatted
_None._ `scripts/format.sh` over the 23 changed C# files: `Formatted 0 of 23 file(s).`

### Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

### Unresolved
_None._

## thoenissen/Reihitsu#1742

No production change in this issue; nothing to format or resolve. Its characterization tests are among the focused tests below.

## Whole run

### Checks
- `scripts/build.sh`: `0 Warning(s)`, `0 Error(s)`.
- `scripts/test.sh --project analyzer` with a filter on the twelve changed test classes (RH2004, RH4122, RH7302, RH7306, RH7307, RH7310, RH8204, RH8304, RH8305, RH8307, RH8308, RH8309): `Passed!  - Failed: 0, Passed: 368, Skipped: 0, Total: 368`. The number is lower than the implementer's 419 because my filter was broader-named but not identical (no `PrimaryConstructor`-name filter on other classes); no failure.
- Text-only proof: not applicable (no comment-only fix).
- Note: `scripts/format.sh` prints S1125 warnings from `Reihitsu.Cli` files this run did not change; the Release build reports none.
