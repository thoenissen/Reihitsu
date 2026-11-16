Role: cleaner
Commit: c3121de
Verdict: clean

The prompt carried no intended fix or hoped-for verdict.

## Formatted
_None._

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|

## Unresolved
_None._ (A format-script build printed S1125 at `src/Reihitsu.Cli/FormatCommandHandler.cs:501`, a file this run did not change; the Release build reports 0 warnings.)

## Checks
- `scripts/format.sh` over 23 changed C# files: Formatted 0 of 23.
- `scripts/build.sh`: Build succeeded, 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH4012|FullyQualifiedName~RH8035|FullyQualifiedName~RH8036"`: Passed: 28, Failed: 0.
