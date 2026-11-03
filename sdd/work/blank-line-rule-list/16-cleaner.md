Role: cleaner
Commit: f21b1915
Verdict: clean

## Formatted
_None._ (`scripts/format.sh`: "Formatted 0 of 3 file(s)." for the three changed test files.)

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ (The format script's own Cli build printed S1125 warnings in `FormatCommandHandler.cs`, an unchanged file outside this run; the Release build reports none.)

## Checks
- `scripts/build.sh`: Build succeeded. 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project formatter` (BlankLineRegionDirectiveRewriterTests, BlankLineTriviaBoundaryRewriterTests, RegionDirectiveLineAboveTests): Passed! Failed: 0, Passed: 29, Total: 29.
