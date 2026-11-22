Role: cleaner
Commit: see git log (subject "Correct SplitIntoPascalCaseWords summary [skip ci]")
Verdict: clean

## Formatted
_None._ `scripts/format.sh` reported "Formatted 0 of 1 file(s)".

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| Auditor finding 4 (stale comment) | `src/Reihitsu.Core/CasingUtilities.cs`, summary of `SplitIntoPascalCaseWords` | Now says the first character of a word is upper-cased when it is a letter and kept as written otherwise |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project core`: Passed! Failed: 0, Passed: 884, Total: 884.
- `scripts/verify-text-only.sh --base 6b145e1 --head worktree`: TEXT-ONLY PROOF: PASS — 1 C# comment/layout-only file; xml-doc impact: 1 file.

## Follow-up candidates
_None._
