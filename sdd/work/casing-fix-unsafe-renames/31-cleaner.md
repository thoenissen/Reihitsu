Role: cleaner
Commit: see the commit that adds this file
Verdict: clean

## Formatted
_None._ `scripts/format.sh` reported "Formatted 0 of 22 file(s)" (every changed C# file against `origin/main`).

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

The summary of `GetRenameCascadeAsync` in `CasingCodeFixProviderBase{T}.cs` now reads "the symbol itself and at least every symbol declared in source that a rename of it renames with it", which agrees with its remark (the search can reach more than the renamer renames). Comment-only.

## Unresolved
_None._ The S1125 warnings printed while the format tool built (`src/Reihitsu.Cli/FormatCommandHandler.cs`) come from the tool's own non-Release build, not from the run's files, and the Release build has none.

## Checks
- `scripts/build.sh`: Build succeeded. 0 Warning(s) 0 Error(s)
- `TEXT-ONLY PROOF: PASS — base=898f917 head=worktree — 1 changed file(s) (0 non-compiled, 1 C# comment/layout-only); xml-doc impact: 1 file(s), no public API documentation`
- No focused tests run: the edit is text-only.
