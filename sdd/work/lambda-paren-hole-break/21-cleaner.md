Role: cleaner
Commit: 1589b0a
Verdict: clean

Repair round 2. The prompt carried no intended fix or hoped-for verdict.

## Formatted
_None._ (`scripts/format.sh` over the 13 changed C# files: "Formatted 0 of 13 file(s).")

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ (`scripts/format.sh` prints S1125 at `src/Reihitsu.Cli/FormatCommandHandler.cs` while building the CLI; the run did not change that file, and the Release build is clean.)

## Checks
- `scripts/build.sh`: 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project formatter --filter "HoleAnchorOnUnmovedLineAlignmentTests|LayoutModelTests|AnonymousMethodHoleAlignmentTests|RawString|ParenthesizedLambdaLineStartTests"`: Passed 221, Failed 0.
