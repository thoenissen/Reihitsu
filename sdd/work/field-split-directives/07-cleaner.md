Role: cleaner
Commit: 5e052e1
Verdict: clean

## Formatted
_None._ (`scripts/format.sh` on the 16 changed C# files: formatted 0 of 16.)

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._ The formatter run printed S1125 warnings from `src/Reihitsu.Cli/FormatCommandHandler.cs`, a file this run did not change; the Release build reports none.

## Checks
- `scripts/build.sh`: Build succeeded. 0 Warning(s), 0 Error(s)
- formatter focused (UnsupportedFieldDeclarationDirective, FieldDeclarationSplit, SplitsFieldDeclaration): Passed! Failed: 0, Passed: 179
- analyzer focused (RH7101): Passed! Failed: 0, Passed: 156

## Follow-up candidates
Check whether S1125 in `FormatCommandHandler.cs` (lines 490, 501, 555) appears in a non-Release build of Reihitsu.Cli only.
