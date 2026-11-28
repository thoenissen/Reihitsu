Role: cleaner
Commit: (see git log, subject ending [skip ci])
Verdict: clean

## Formatted
- `tests/Reihitsu.SelfHosting.Test/Analyzers/AnalyzerViolationReportTests.cs` (reformatted by FMT-BREAK-046, as 06 predicted; not in the run's diff, formatted on the lead's instruction). The other 44 changed C# files were already formatted.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| RH2111 (type over 500 lines, 543) | `src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs` | `FindRegionDirectiveInsideMember` and `StandsBetweenMembers` moved unchanged into the new partial file `PrecheckPhase.RegionInsideMember.cs`; class made `partial` |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: 0 Warning(s), 0 Error(s).
- Formatter filter `UnsupportedRegionInsideMemberTests|ArrayInitializerBraceLineTests`: Passed 129/129. Architecture: Passed 221/221.

## Follow-up candidates
_None._
