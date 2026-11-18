Role: cleaner
Commit: 84843a5 (base of this round; handover committed on top)
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
- `scripts/format.sh` over the 5 changed C# files: Formatted 0 of 5 file(s).
- `scripts/build.sh`: Build succeeded. 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project analyzer` filtered to RH5401 and RH5307 analyzer tests: Passed! Failed: 0, Passed: 133, Total: 133. The two Fix All rows that 09 reported failing also passed in this run (the Release configuration, not 09's Debug); the test was not touched.

## Follow-up candidates
_None._
