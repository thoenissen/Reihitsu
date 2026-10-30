Role: cleaner
Commit: 3e57f05
Verdict: clean

## Formatted
_None._ `scripts/format.sh` reported "Formatted 0 of 3 file(s)." for the three changed C# test files.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ (The formatter's own build of `Reihitsu.Cli` printed S1125 warnings in `FormatCommandHandler.cs`; that file is not changed by this run and the Release build is clean.)

## Checks
- `scripts/build.sh`: Build succeeded, 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project formatter --filter "FullyQualifiedName~SwitchCaseBraceRequirementTests|FullyQualifiedName~ReihitsuFormatterEntryPointTests|FullyQualifiedName~ReihitsuFormatterLanguageVersionTests"`: Passed! Failed: 0, Passed: 78, Total: 78.
- Text-only proof: not applicable (no comment-fix after an audit).
