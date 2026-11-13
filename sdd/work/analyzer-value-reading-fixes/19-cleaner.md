Role: cleaner
Commit: see the commit that adds this file (no code changes)
Verdict: clean

## Formatted
_None._ `scripts/format.sh` over the 11 changed C# files: formatted 0 of 11.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ (The format script's tool build prints S1125 in `src/Reihitsu.Cli/Diagnostics/PipelineTraceCommand.cs`, unchanged by this run; not in `scripts/build.sh`.)

## Checks
- `scripts/build.sh`: 0 Warning(s), 0 Error(s).
- Analyzer tests filtered to RH0001, RH2003, RH2109, RH4001, RH4009, RH8402: Passed 225/225.
- Text-only proof: N/A.
