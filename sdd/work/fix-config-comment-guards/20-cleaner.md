Role: cleaner
Commit: see git log (subject ends in [skip ci])
Verdict: clean

## Formatted
_None._ `scripts/format.sh` over the 11 changed non-test C# files: "Formatted 0 of 11".

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| S3267 | `OrderingDeclarationUtilities.cs` `TryGetOrderedMembers` member loop | `if` moved into `.Where(...)` |
| S3267 | `OrderingDeclarationUtilities.cs` `GetConditionalRegions` member loop | loop iterates `.Select(member => member.FullSpan)` |
| S3267 | `OrderingDeclarationUtilities.cs` `TryAddOrderedMembersOfRegion` incomplete-member loop | `if` moved into `.Where(...)` |
| S1168 x2 | `OrderingDeclarationUtilities.cs` both `GetConfigurations` overloads | "over the cap" is an empty list instead of `null` (a valid result always holds at least one configuration); the three callers test `Count == 0` |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project analyzer --filter RH7109|RH7103|RH7309|RH7102|RH2001`: Passed 449, Failed 0.
- `scripts/test.sh --project core`: Passed 660, Failed 0.
