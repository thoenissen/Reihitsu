Role: cleaner
Commit: 1e1f7bb
Verdict: clean

## Formatted
_None._ `scripts/format.sh` reported "Formatted 0 of 16 file(s)".

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: Build succeeded, 0 Warning(s), 0 Error(s).
- Focused formatter tests (ArgumentListLineStartTests, ChainLinkLayoutTests, JoinSeparatorPhaseTests) in Release: passed.

## Follow-up candidates
S1125 warnings appear in `src/Reihitsu.Cli/FormatCommandHandler.cs` while format.sh builds the CLI; the file is not changed by the run and the Release build is clean.
