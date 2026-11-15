Role: cleaner
Commit: (this commit)
Verdict: clean

## Formatted
_None._ `scripts/format.sh` reported "Formatted 0 of 3 file(s)".

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ The format script's own CLI build printed S1125 warnings in `src/Reihitsu.Cli/FormatCommandHandler.cs`, a file this run did not change; the Release build is clean.

## Checks
- `scripts/build.sh`: 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project formatter --filter UsingDirectiveHeaderBlankLineTests`: see commit; no code changed.
