Role: cleaner
Commit: 39475c4
Verdict: clean

## Formatted
_None._ (`scripts/format.sh --force` over the 32 changed C# files: `Formatted 0 of 32 file(s).`)

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: Build succeeded. 0 Warning(s) 0 Error(s)
- `scripts/test.sh --project analyzer --filter "RH8002|RH8032|RH8106|RH8201|RH8203|RH830|RH840"`: Passed! Failed: 0, Passed: 637, Total: 637
- Text-only proof: not applicable (no comment-only fixes).
