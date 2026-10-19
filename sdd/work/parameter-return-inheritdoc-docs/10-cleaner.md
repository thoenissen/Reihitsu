Role: cleaner
Commit: 0edafda
Verdict: clean

## Formatted
_None._

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/format.sh` over the 14 changed C# files (after the merge of origin/main and 2e73c50): `Formatted 0 of 14 file(s).`
- `scripts/build.sh --no-install`: `0 Warning(s) 0 Error(s)`.
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Documentation"`: `Passed! - Failed: 0, Passed: 747, Skipped: 0, Total: 747`.
- Text-only proof: not applicable (no comment fixes).
