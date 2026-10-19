Role: cleaner
Commit: (this commit)
Verdict: clean

## Formatted
_None._ `scripts/format.sh` on the 18 changed test files: "Formatted 0 of 18 file(s)".

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ (The format script's own Debug build printed S1125 at `src/Reihitsu.Cli/FormatCommandHandler.cs(506,85)`, a file this run did not change; the Release build reports no warning.)

## Checks
- `scripts/build.sh`: Build succeeded. 0 Warning(s), 0 Error(s).
- Focused analyzer tests (RH0002, RH1001/RH1002, RH200x, RH21xx): Passed! Failed 0, Passed 231.
