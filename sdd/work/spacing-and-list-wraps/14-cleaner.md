Role: cleaner
Commit: see git log (this commit)
Verdict: clean

## Formatted
_None._ The formatter changed 0 of 2 files (`LineBreakListRewriter.cs`, `ListWrapByElementsTests.cs`).

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Comment change (decision 18)
The `EnsureSeparatorsHaveEndOfLine` summary in `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakListRewriter.cs` now names the case: a first element that a comment, a directive, or disabled text keeps off the line of the opening delimiter starts its own line, so the list is multi-line, one element per line.

## Unresolved
_None._ The formatter's own build printed S1125 warnings in `src/Reihitsu.Cli/FormatCommandHandler.cs` (lines 490, 501) in a non-Release build; they are untouched by this round and the Release build is clean.

## Checks
- `scripts/build.sh`: Build succeeded. 0 Warning(s) 0 Error(s)
- `scripts/verify-text-only.sh --strict-docs --base 8943764 --head worktree`: `text-only  src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakListRewriter.cs  —  2949 tokens identical (0 literal), directives and disabled text identical, changed trivia: documentation`. The overall verdict is FAIL only because the test implementer's round-2 test addition in `ListWrapByElementsTests.cs` (13) carries compiled code, as intended.
