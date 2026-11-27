Role: cleaner
Commit: 730053b
Verdict: clean

## Formatted
_None._ (`scripts/format.sh --force` on the 5 changed C# files: Formatted 0 of 5.)

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: Build succeeded, 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~FixQualifies|FullyQualifiedName~FixAllQualifies|FullyQualifiedName~NoFixIsOfferedWhenAnAnonymousObjectMemberTakesItsNameFromTheField"`: Passed! Failed: 0, Passed: 40.

## Follow-up candidates
_None._
