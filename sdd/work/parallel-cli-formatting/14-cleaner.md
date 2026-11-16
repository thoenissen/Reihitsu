Role: cleaner
Commit: 57f01e2
Verdict: clean

## Formatted
_None._ (`scripts/format.sh`: "Formatted 0 of 15 file(s).")

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| none | | |

## Unresolved
_None._ The format script's own build printed S1125 notes for `FormatCommandHandler.cs` (636, 647, 658: `== false` comparisons); the Release build reports none, and the repository's style uses `== false`, so nothing was changed.

## Checks
- `scripts/build.sh`: Build succeeded, 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project cli`: Passed! Failed 0, Passed 360, Total 360.

## Follow-up candidates
_None._
