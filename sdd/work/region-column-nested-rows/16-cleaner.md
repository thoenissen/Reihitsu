Role: cleaner
Commit: (see git log, subject ending [skip ci])
Verdict: clean

## Formatted
- `src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.RegionInsideMember.cs` (after the warning fix below). The other 44 existing changed C# files since 7dd086b were already formatted (one listed path, `ApplyFixWrapperFixtureDirectoryScope.cs`, no longer exists).

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| RH2009 (cognitive complexity 17), S3267 | `PrecheckPhase.RegionInsideMember.cs`, `FindRegionDirectiveInsideMember` | The inner trivia loop became a LINQ `Where`/`FirstOrDefault` over the leading trivia; the nested `if`/`break` became one condition (token skipped when a member-position exception applies); same result |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: 0 Warning(s), 0 Error(s).
- Formatter filter `UnsupportedRegionInsideMember|ArrayInitializerBraceLine`: Passed 140/140.

## Follow-up candidates
_None._
