Role: cleaner
Commit: see git log
Verdict: clean

## Formatted
_None._ `scripts/format.sh` over the 14 changed C# files: `Formatted 0 of 14 file(s)`.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ (format.sh's tool build prints S1125 in `src/Reihitsu.Cli/FormatCommandHandler.cs`, a file this run did not change.)

## Checks
`scripts/build.sh`: Build succeeded, 0 Warning(s), 0 Error(s). Focused formatter tests (DelimitedDocumentation, UsingDirective): passed.
